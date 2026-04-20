using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Collections;

namespace facingfate
{
    public class NpcAIController
    {
        private readonly NonPlayerScript npcScript;
        private readonly NpcAiBias npcAIBias;
        private readonly EntityOnMap mover;

        public Transform deck;
        public Transform discard;

        public List<CardScript> hand = new();

        public NpcAIController(NonPlayerScript npc, NpcData npcData)
        {
            npcScript = npc;
            npcAIBias = npcData.aiBias;
            mover = npc.GetComponent<EntityOnMap>();
        }

        private IEnumerator EvaluateCardCoroutine(
            CardScript card,
            float stamina,
            Vector3 virtualPosition,
            Action<ScoredCard> onComplete)
        {
            // Step 0: quick validation
            if (card == null || card.cardData == null)
            {
                //Debug.Log("[NpcAI][EvaluateCard] Invalid card or missing cardData - aborting evaluation.");
                onComplete?.Invoke(null);
                yield break;
            }

            var cardName = card.cardData.cardName;

            // Cache expensive computed properties to avoid repeated Stat evaluations
            int cardCost = card.cardData.Cost;
            float cardRange = card.cardData.Range;

            // Early-out: no point evaluating a card we cannot afford
            if (cardCost > stamina)
            {
                onComplete?.Invoke(null);
                yield break;
            }

            float tStart = Time.realtimeSinceStartup;

            // Step 1: Use TargetingModeFactory to get the card's targeting mode and generate optimal targeting data
            var targetingMode = TargetingModeFactory.Create(card);
            var targetingModeResults = targetingMode.GetTargetingData(card, npcScript);

            if (targetingModeResults == null || targetingModeResults.Count == 0)
            {
                onComplete?.Invoke(null);
                yield break;
            }

            // Step 2: For each targeting mode result, find movement paths to the casting position
            var reachableMoves = new List<(PathData pathData, TargetingModeData targetingData)>();

            // Track visited cast positions to avoid redundant pathfinding and double-scoring
            var seenCastPositions = new HashSet<Vector3> { virtualPosition };

            // Always include current position (no movement)
            reachableMoves.Add((
                new PathData { PathCost = 0, Start = virtualPosition, End = virtualPosition },
                targetingModeResults[0]  // Use first result's base targeting as fallback for current position
            ));

            // Explore movement to other targeting positions if we have stamina and range allows
            if (cardRange > 0 && stamina > cardCost)
            {
                int movementBudget = Mathf.RoundToInt(stamina - cardCost);

                // Try to path to each targeting result's suggested casting position
                foreach (var targetingResult in targetingModeResults)
                {
                    Vector3 suggestedCastPos = targetingResult.castingPosition;

                    if (!seenCastPositions.Add(suggestedCastPos))
                        continue;

                    var pathData = MovementUtility.FindPath(
                        virtualPosition,
                        suggestedCastPos,
                        entityStats: npcScript.entityStats);

                    if (pathData != null && pathData.Path != null && pathData.Path.Count > 0 && pathData.PathCost <= movementBudget)
                    {
                        reachableMoves.Add((pathData, targetingResult));
                    }
                }

                // Also explore nearby positions within movement budget for additional targeting options
                var nearbyPositions = GetNearbyPositionsForCard(virtualPosition, movementBudget, cardRange);
                foreach (var (pathData, castPos) in nearbyPositions)
                {
                    // Skip positions already covered by targeting mode results
                    if (!seenCastPositions.Add(castPos))
                        continue;

                    // Generate fresh targeting data for this nearby position
                    var nearbyTargetingData = TargetingUtility.GetAffected(
                        card,
                        castPos,
                        npcScript,
                        card.cardData.targetingData.EffectUsesVision,
                        null,
                        true);

                    if (nearbyTargetingData?.targetedEntities != null && nearbyTargetingData.targetedEntities.Count > 0)
                    {
                        reachableMoves.Add((pathData, nearbyTargetingData));
                    }
                }
            }

            // Step 3: Evaluate all reachable moves and find best
            var evaluation = EvaluateMovementAndAim(card, reachableMoves);

            Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' evaluation complete in {Time.realtimeSinceStartup - tStart:0.000}s (score: {evaluation?.score})");
            onComplete?.Invoke(evaluation);
            yield break;
        }

        /// <summary>
        /// Gets nearby positions to evaluate for card casting within movement budget.
        /// </summary>
        private List<(PathData, Vector3)> GetNearbyPositionsForCard(Vector3 currentPos, int movementBudget, float cardRange)
        {
            var positions = new List<(PathData, Vector3)>();
            int searchRadius = Mathf.Min(movementBudget, Mathf.RoundToInt(cardRange));

            for (float x = currentPos.x - searchRadius; x <= currentPos.x + searchRadius; x++)
            {
                for (float z = currentPos.z - searchRadius; z <= currentPos.z + searchRadius; z++)
                {
                    Vector3 candidate = new Vector3(x, currentPos.y, z);
                    if (candidate == currentPos)
                        continue;

                    // Try to find a path to this position
                    var pathData = MovementUtility.FindPath(currentPos, candidate, entityStats: npcScript.entityStats);
                    if (pathData != null && pathData.Path != null && pathData.Path.Count > 0 && pathData.PathCost <= movementBudget)
                    {
                        positions.Add((pathData, candidate));
                    }
                }
            }

            return positions;
        }

        #region Core Turn Planning

        public void BuildTurnPlan(Action<List<PlannedAction>> callback)
        {
            npcScript.StartCoroutine(BuildTurnPlanCoroutine(callback));
        }

        public IEnumerator BuildTurnPlanCoroutine(Action<List<PlannedAction>> onComplete)
        {
            List<PlannedAction> plan = new();
            var playedCards = new HashSet<CardScript>();

            DrawCard();

            // Execute the turn planning
            yield return ExecuteTurnPlanning(plan, playedCards);

            onComplete?.Invoke(plan);
        }

        private IEnumerator ExecuteTurnPlanning(List<PlannedAction> plan, HashSet<CardScript> playedCards)
        {
            float startTime = Time.realtimeSinceStartup;
            Vector3 virtualPosition = mover.transform.position;
            float virtualStamina = npcScript.entityStats.CurrentStamina;
            bool hasRepositioned = false;
            bool hasChased = false;

            // Continue selecting best actions until no valid action remains
            while (virtualStamina > 0)
            {
                var actionCandidates = new List<ScoredCard>();

                // Evaluate hand cards concurrently via coroutines, wait for all to finish.
                var handCount = hand.Count;
                var remaining = handCount;
                var results = new ScoredCard[handCount];

                if (handCount > 0)
                {
                    for (int i = 0; i < handCount; i++)
                    {
                        int idx = i;
                        // Skip cards that have already been played this turn
                        if (playedCards.Contains(hand[idx]))
                        {
                            results[idx] = null;
                            remaining--;
                            continue;
                        }

                        CoroutineRunner.Instance.StartCoroutineManaged(EvaluateCardCoroutine(hand[idx], virtualStamina, virtualPosition, (res) =>
                        {
                            results[idx] = res;
                            remaining--;
                        }));
                        // Slight yield to spread coroutine starts across frames
                        yield return null;
                    }

                    // Wait for all evaluations to complete
                    while (remaining > 0)
                        yield return null;

                    var handCandidates = new List<ScoredCard>();
                    for (int i = 0; i < handCount; i++)
                    {
                        var r = results[i];
                        if (r != null && r.score > 0)
                            handCandidates.Add(r);
                    }

                    if (handCandidates.Count > 0)
                        actionCandidates.AddRange(handCandidates);
                }

                // Only offer reposition if we haven't repositioned this turn
                if (!hasRepositioned)
                {
                    var repositionCandidate = TryGetRepositionCandidate(virtualPosition, virtualStamina);

                    if (repositionCandidate != null && repositionCandidate.score > 0)
                        actionCandidates.Add(repositionCandidate);
                }

                if (actionCandidates.Count == 0)
                {
                    // Only offer chase if we haven't chased this turn
                    if (!hasChased)
                    {
                        var chaseCandidate = TryGetChaseCandidate(virtualPosition, virtualStamina);

                        if (chaseCandidate != null)
                            actionCandidates.Add(chaseCandidate);
                        else
                            break;
                    }
                    else
                        break;
                }

                var bestAction = SelectBestActionCandidate(actionCandidates, virtualStamina);

                if (bestAction == null || bestAction.score <= 0)
                {
                    Debug.Log($"[NpcAI] No valid action found. Breaking turn planning.");
                    break;
                }

                Debug.Log($"[NpcAI] Action chosen: {bestAction.pseudoName} (score: {bestAction.score})");

                int moveCost = bestAction.pathData?.PathCost ?? 0;
                int cardCost = bestAction.card?.cardData?.Cost ?? 0;
                int totalCost = moveCost + cardCost;

                if (totalCost > virtualStamina || totalCost <= 0)
                    break;

                // Detect movement-only actions (chase/reposition/flee)
                bool isMovementOnlyAction = bestAction.card == null && bestAction.pathData != null && bestAction.pathData.PathCost > 0;

                if (isMovementOnlyAction)
                {
                    if (bestAction.pseudoName.Contains("Chase"))
                        hasChased = true;
                    else if (bestAction.pseudoName.Contains("Flee") || bestAction.pseudoName.Contains("Reposition"))
                        hasRepositioned = true;
                }

                // Record movement action if any
                if (bestAction.pathData != null && bestAction.pathData.Path != null && bestAction.pathData.Path.Count > 0 && bestAction.pathData.PathCost > 0)
                {
                    plan.Add(new PlannedAction
                    {
                        Type = PlannedAction.ActionType.Move,
                        Name = $"Move_for_{bestAction.pseudoName}",
                        TargetingModeData = bestAction.targetingModeData,
                        PathData = bestAction.pathData
                    });

                    // Update virtual position immediately (predictive)
                    virtualPosition = bestAction.pathData.End;
                }

                // Record card action if chosen
                if (bestAction.executionOption == CardExecutionOption.PlayCard && bestAction.card != null)
                {
                    // Track card as played to prevent re-selection
                    playedCards.Add(bestAction.card);

                    // Remove card from hand immediately to avoid re-selection
                    hand.Remove(bestAction.card);

                    plan.Add(new PlannedAction
                    {
                        Type = PlannedAction.ActionType.PlayCard,
                        Name = bestAction.card.name,
                        Card = bestAction.card,
                        TargetingModeData = bestAction.targetingModeData,
                        PathData = bestAction.pathData
                    });
                }

                // Deduct stamina immediately (predictive)
                virtualStamina -= totalCost;
            }

            float duration = Time.realtimeSinceStartup - startTime;
            Debug.Log($"[NpcAI] BuildTurnPlan for {npcScript.name} took {duration:0.000}s");

            Debug.Log($"[NpcAI] Turn plan complete. Total actions planned: {plan.Count}");
            for (int i = 0; i < plan.Count; i++)
            {
                var action = plan[i];
                Debug.Log($"[NpcAI]   Action {i + 1}: {action.Type} - {action.Name}");
            }

            yield break;
        }

        public void DrawCard()
        {
            int cardsToDraw = Mathf.RoundToInt(npcScript.entityStats.CurrentWisdom / 2);

            for (int i = 0; i < cardsToDraw; i++)
            {
                if (deck.childCount == 0)
                {
                    // Get all cards from discard, shuffle, and move back to deck
                    List<Transform> discardCards = new();
                    int discardCount = discard.childCount;

                    for (int j = 0; j < discardCount; j++)
                    {
                        discardCards.Add(discard.GetChild(j));
                    }
                    // Shuffle discard cards
                    System.Random rng = new();
                    discardCards = discardCards.OrderBy(a => rng.Next()).ToList();

                    foreach (var card in discardCards)
                    {
                        card.SetParent(deck);
                    }
                }

                if (deck.childCount > 0)
                {
                    Transform cardObj = deck.GetChild(0);
                    CardScript cardScript = cardObj.GetComponent<CardScript>();

                    hand.Add(cardScript);

                    cardObj.SetParent(discard);
                }
            }
        }
        public void DiscardCard(int toDiscard)
        {
            for (int i = 0; i < toDiscard; i++)
            {
                if (hand.Count == 0)
                    break;

                // Discard random card from hand
                System.Random rng = new();
                int idx = rng.Next(hand.Count);
                CardScript cardToDiscard = hand[idx];
                hand.RemoveAt(idx);

                // Move card object to discard pile
                cardToDiscard.transform.SetParent(discard);
            }
        }

        #endregion

        #region Hand Evaluation

        private ScoredCard EvaluateMovementAndAim(
            CardScript card,
            List<(PathData, TargetingModeData)> reachableMoves)
        {
            ScoredCard best = new()
            {
                card = card,
                score = 0,
                pathData = new PathData { PathCost = int.MaxValue },
            };

            foreach (var move in reachableMoves)
            {
                if (move.Item2?.targetedEntities == null || move.Item2.targetedEntities.Count == 0)
                    continue;

                // Filter targets based on card's targeting data (affiliation, vision, etc.)
                var validTargets = FilterValidTargets(card, move.Item2.targetedEntities, move.Item1);

                if (validTargets.Count == 0)
                    continue;

                int score = EvaluateCardScore(card, validTargets, move.Item1.PathCost);

                bool betterScore = score > best.score;
                bool equalScoreCheaperPath = score == best.score && score > 0 && move.Item1.PathCost < best.pathData.PathCost;

                if (betterScore || equalScoreCheaperPath)
                {
                    best.score = score;
                    best.pseudoName = $"Play {card.cardData.cardName}";
                    best.targets = validTargets
                        .Take(card.cardData.MaxTarget > 0 ? card.cardData.MaxTarget : validTargets.Count)
                        .ToList();

                    best.targetingModeData = move.Item2;
                    best.pathData = move.Item1;
                    best.executionOption = CardExecutionOption.PlayCard;
                }
            }

            return best;
        }

        private List<EntityScript> FilterValidTargets(CardScript card, List<EntityScript> targets, PathData movePath)
        {
            if (targets == null || targets.Count == 0)
                return new List<EntityScript>();

            var validTargets = new List<EntityScript>();
            var cardData = card.cardData;
            var castingPosition = movePath?.End ?? mover.transform.position;

            // Pre-compute visible tiles as HashSet for O(1) lookup
            HashSet<Vector3> visibleTiles = null;
            if (cardData.targetingData.TargetingUsesVision)
            {
                var tilesToCheck = new List<Vector3>();
                foreach (var target in targets)
                {
                    var targetEntityOnMap = target.GetComponent<EntityOnMap>();
                    if (targetEntityOnMap != null)
                        tilesToCheck.Add(targetEntityOnMap.transform.position);
                }
                if (tilesToCheck.Count > 0)
                    visibleTiles = new HashSet<Vector3>(VisionUtility.GetVisibleTiles(castingPosition, tilesToCheck));
            }

            foreach (var target in targets)
            {
                if (target == null)
                    continue;

                // Check affiliation match
                if (!IsValidTargetAffiliation(target, cardData))
                    continue;

                // Check vision requirements if applicable
                if (visibleTiles != null)
                {
                    var targetEntityOnMap = target.GetComponent<EntityOnMap>();
                    if (targetEntityOnMap == null || !visibleTiles.Contains(targetEntityOnMap.transform.position))
                        continue;
                }

                validTargets.Add(target);
            }

            return validTargets;
        }

        private bool IsValidTargetAffiliation(EntityScript target, CardData cardData)
        {
            if (target == null || cardData == null)
                return false;

            var targetingAffiliation = cardData.targetingData.CardTargetAffiliation;

            return targetingAffiliation switch
            {
                CardTargetAffiliation.Self => target == npcScript,
                CardTargetAffiliation.Ally => target.entityAffiliation == npcScript.entityAffiliation && target != npcScript,
                CardTargetAffiliation.AllyNeutral => (target.entityAffiliation == npcScript.entityAffiliation || target.entityAffiliation == EntityAffiliation.Neutral) && target != npcScript,
                CardTargetAffiliation.Enemy => target.entityAffiliation != npcScript.entityAffiliation && target.entityAffiliation != EntityAffiliation.Neutral,
                CardTargetAffiliation.EnemyNeutral => target.entityAffiliation != npcScript.entityAffiliation,
                CardTargetAffiliation.AllyEnemy => target != npcScript,
                CardTargetAffiliation.All => true,
                CardTargetAffiliation.None => true,
                _ => true
            };
        }

        #endregion

        #region Helpers

        private ScoredCard SelectBestActionCandidate(List<ScoredCard> candidates, float stamina)
        {
            if (candidates == null || candidates.Count == 0)
                return null;

            // Find the highest-scoring affordable candidate in single pass
            ScoredCard best = null;
            int bestScore = 0;

            foreach (var candidate in candidates)
            {
                int totalCost = (candidate.pathData?.PathCost ?? 0) +
                                (candidate.card?.cardData?.Cost ?? 0);

                if (totalCost <= stamina && candidate.score > bestScore)
                {
                    bestScore = candidate.score;
                    best = candidate;
                }
            }

            return best;
        }

        private int EvaluateCardScore(CardScript card, List<EntityScript> targets, int moveCost)
        {
            if (card?.cardData == null)
                return 0;

            if (card.cardData.MaxTarget > 0)
                targets = targets.Take(card.cardData.MaxTarget).ToList();

            int throughput =
                card.cardData.CardAiBias?.ThroughputOverride(
                    npcAIBias,
                    card.cardData,
                    targets) ?? 0;

            int cost = Mathf.Max(1, card.cardData.Cost + moveCost);

            return (throughput * 100) / cost;
        }

        #endregion

        #region Flee / Chase

        private ScoredCard TryGetRepositionCandidate(Vector3 virtualPosition, float remainingStamina)
        {
            if (npcScript.entityStats.IsRooted)
                return null;

            if (npcAIBias == null)
                return null;

            ScoredCard moveOption = null;

            switch (npcAIBias.RepositionCondition)
            {
                case RepositionCondition.lowHealth:

                    if (npcScript.entityStats.CurrentHealth <
                        npcScript.entityStats.MaxHealth * 0.3f)
                    {
                        // Use virtual position and stamina for consistent turn planning
                        moveOption = DetermineRepositionTarget(
                            virtualPosition,
                            npcScript,
                            remainingStamina);

                        // Only accept actual movement repositions, not "stay in place" actions
                        if (moveOption != null && moveOption.pathData != null && moveOption.pathData.PathCost > 0)
                        {
                            moveOption.score = 1;
                            moveOption.pseudoName = "Low Health Flee";
                        }
                        else
                        {
                            moveOption = null;
                        }
                    }

                    break;
            }

            return moveOption;
        }

        private ScoredCard TryGetChaseCandidate(Vector3 virtualPosition, float remainingStamina)
        {
            if (npcScript.entityStats.IsRooted)
                return null;

            var allEntities = TargetingUtility.AllEntitiesCache();

            var enemies = allEntities
                .Where(e => e.entityAffiliation != npcScript.entityAffiliation)
                .ToList();

            if (enemies.Count == 0)
                return null;

            ScoredCard bestChaseTarget = null;
            int bestScore = 0;

            // Evaluate multiple enemies to find the best chase target
            foreach (var enemy in enemies)
            {
                var targetPosition = enemy.transform.position;
                float distToEnemy = Vector3.Distance(virtualPosition, targetPosition);

                var pathData = MovementUtility.FindPath(
                    virtualPosition,
                    targetPosition,
                    walkClose: true,
                    entityStats: npcScript.entityStats);

                if (pathData == null || pathData.PathCost > remainingStamina)
                    continue;

                // Calculate chase score based on distance and movement efficiency
                // Closer enemies are higher priority, but account for movement cost
                // Score formula: (maxDistance - distToEnemy) / (pathCost + 1) * baseMultiplier
                float maxDistance = remainingStamina * 1.5f;
                float distScore = Mathf.Max(0, maxDistance - distToEnemy);
                float efficiency = distScore / Mathf.Max(1, pathData.PathCost);
                int score = Mathf.RoundToInt(efficiency * 10f);

                // Bonus if chase is affordable and leaves stamina for actions
                if (pathData.PathCost < remainingStamina * 0.5f)
                    score += 5;

                // Prefer closer enemies with reasonable path costs
                if (score > bestScore)
                {
                    bestScore = score;
                    bestChaseTarget = new ScoredCard
                    {
                        pseudoName = "Chase",
                        targetingModeData = new() { castingPosition = new Vector3Int((int)targetPosition.x, (int)targetPosition.y, (int)targetPosition.z) },
                        pathData = pathData,
                        score = score
                    };
                }
            }

            return bestChaseTarget;
        }

        private ScoredCard DetermineRepositionTarget(Vector3 realPosition, EntityScript entity, float realStamina)
        {
            var allEntities = TargetingUtility.AllEntitiesCache();
            var enemies = allEntities.Where(e => e.entityAffiliation != entity.entityAffiliation).ToList();

            if (enemies.Count == 0)
                return CreateStayInPlaceMove(realPosition, "No Enemies Move");

            // Cap flee distance by number of hostiles (but not above remaining stamina)
            int maxFleeDistance = Mathf.Min(Mathf.RoundToInt(realStamina), enemies.Count);

            // Delegate the search and scoring
            ScoredCard bestTarget = EvaluateRepositionCandidates(realPosition, entity, maxFleeDistance, enemies, realStamina);

            return bestTarget ?? CreateStayInPlaceMove(realPosition, "No Move");
        }

        private ScoredCard CreateStayInPlaceMove(Vector3 position, string name)
        {
            return new ScoredCard
            {
                pseudoName = name,
                targetingModeData = new() { castingPosition = new Vector3Int((int)position.x, (int)position.y, (int)position.z) },
                pathData = new PathData
                {
                    Start = position,
                    End = position,
                    PathCost = 0,
                    Path = new List<Vector3> { position }
                },
                score = 0
            };
        }
        private ScoredCard EvaluateRepositionCandidates(Vector3 realPosition, EntityScript entity, int maxFleeDistance, List<EntityScript> enemies, float realStamina)
        {
            ScoredCard bestTarget = null;
            float bestScore = float.MinValue;

            // Expand search radius for better target discovery
            int searchRadius = Mathf.Max(maxFleeDistance, Mathf.RoundToInt(realStamina));

            for (int dx = -searchRadius; dx <= searchRadius; dx++)
            {
                for (int dy = -searchRadius; dy <= searchRadius; dy++)
                {
                    Vector3 candidate = new Vector3(realPosition.x + dx, realPosition.y + dy, realPosition.z);

                    // Skip current position or invalid tiles
                    if (candidate == realPosition)
                        continue;

                    // Pathfinding
                    var pathData = MovementUtility.FindPath(realPosition, candidate, entityStats: npcScript.entityStats);
                    if (pathData == null || pathData.Path == null || pathData.Path.Count == 0)
                        continue;

                    int moveCost = pathData.PathCost;
                    if (moveCost == 0 || moveCost > realStamina)
                        continue;

                    // Calculate average and minimum distance to hostiles for better evaluation
                    float totalEnemyDist = 0f;
                    float minEnemyDist = float.MaxValue;
                    int validEnemyCount = 0;

                    foreach (var e in enemies)
                    {
                        var enemyPos = e.GetComponent<EntityOnMap>();
                        if (enemyPos != null)
                        {
                            float distToEnemy = Vector3.Distance(enemyPos.transform.position, candidate);
                            totalEnemyDist += distToEnemy;
                            validEnemyCount++;

                            if (distToEnemy < minEnemyDist)
                                minEnemyDist = distToEnemy;
                        }
                    }

                    if (validEnemyCount == 0)
                        continue;

                    // Scoring formula prioritizing average distance and minimum safety distance
                    float averageEnemyDist = totalEnemyDist / validEnemyCount;
                    float score = (averageEnemyDist * 1.5f) + (minEnemyDist * 1f) - (moveCost * 0.5f);

                    // Heavy penalty for being too close to any enemy
                    if (minEnemyDist < 2f)
                        score -= 150f;

                    // Moderate penalty for being somewhat close
                    else if (minEnemyDist < 3f)
                        score -= 50f;

                    // Bonus for finding a position with good spacing from multiple enemies
                    if (averageEnemyDist > minEnemyDist + 2f)
                        score += 30f;

                    // Track best-scoring candidate
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestTarget = new ScoredCard
                        {
                            pseudoName = "Flee",
                            targetingModeData = new() { castingPosition = candidate },
                            pathData = pathData,
                            score = Mathf.RoundToInt(score)
                        };
                    }
                }
            }

            return bestTarget;
        }
        #endregion
    }

    #region Types

    public class ScoredCard
    {
        public CardScript card;
        public string pseudoName;
        public List<EntityScript> targets;
        public int score;
        public PathData pathData;
        public TargetingModeData targetingModeData = new();
        public CardExecutionOption executionOption;
    }

    public enum CardExecutionOption
    {
        PlayCard,
        MoveOnly,
        Flee,
        Reposition
    }

    public class PlannedAction
    {
        public enum ActionType { Move, PlayCard }

        public string Name;
        public ActionType Type;
        public CardScript Card;
        public TargetingModeData TargetingModeData = new();
        public PathData PathData;
    }

    #endregion
}