using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Collections;
using Utility;

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

            Debug.Log($"[NpcAI] Initialized for {npc.name} at cell {mover.currentCell}");
        }

        private IEnumerator EvaluateCardCoroutine(
            CardScript card,
            int stamina,
            Vector3Int virtualPosition,
            Action<ScoredCard> onComplete)
        {
            // Step 0: quick validation
            if (card == null || card.cardData == null)
            {
                Debug.Log("[NpcAI][EvaluateCard] Invalid card or missing cardData - aborting evaluation.");
                onComplete?.Invoke(null);
                yield break;
            }

            // Timing
            var cardName = card.cardData.cardName;
            float tStart = Time.realtimeSinceStartup;
            float tPrev = tStart;
            Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Starting evaluation at {tStart:0.000}s");

            // Step 1: valid targets (cheap)
            var validTargets = TargetingUtility.GetValidTargets(card.cardData);
            //yield return null;
            float tAfterStep1 = Time.realtimeSinceStartup;
            Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Step 1 (validTargets) took {tAfterStep1 - tPrev:0.000}s");
            tPrev = tAfterStep1;
            if (validTargets == null || validTargets.Count == 0)
            {
                Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' - no valid targets, aborting after {Time.realtimeSinceStartup - tStart:0.000}s");
                onComplete?.Invoke(null);
                yield break;
            }

            // Step 2: create targeting mode
            var targetingMode = TargetingModeFactory.Create(card);
            //yield return null;

            float tAfterStep2 = Time.realtimeSinceStartup;
            Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Step 2 (create targeting mode) took {tAfterStep2 - tPrev:0.000}s");
            tPrev = tAfterStep2;
            if (targetingMode == null)
            {
                Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' - no targeting mode, aborting after {Time.realtimeSinceStartup - tStart:0.000}s");
                onComplete?.Invoke(null);
                yield break;
            }

            // Step 3: generate templates (may be heavier); yield after call
            var templates = targetingMode.GetTargetingData(card, npcScript);
            //yield return null;

            float tAfterStep3 = Time.realtimeSinceStartup;
            Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Step 3 (generate templates) took {tAfterStep3 - tPrev:0.000}s");
            tPrev = tAfterStep3;
            if (templates == null || templates.Count == 0)
            {
                Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' - no templates, aborting after {Time.realtimeSinceStartup - tStart:0.000}s");
                onComplete?.Invoke(null);
                yield break;
            }

            // Step 4: compute reachable candidates via coroutine (batched pathfinding)
            List<(PathData, TargetingModeData)> reachableMoves = null;
            yield return CoroutineRunner.Instance.StartCoroutineManaged(TargetingUtility.GetReachableCandidatesCoroutine(card, templates, stamina, virtualPosition, 4, (res) => reachableMoves = res));
            
            float tAfterStep4 = Time.realtimeSinceStartup;
            Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Step 4 (reachable candidates/pathfinding) of {templates.Count} _Templates took {tAfterStep4 - tPrev:0.000}s");
            tPrev = tAfterStep4;
            if (reachableMoves == null || reachableMoves.Count == 0)
            {
                Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' - no reachable moves, aborting after {Time.realtimeSinceStartup - tStart:0.000}s");
                onComplete?.Invoke(null);
                yield break;
            }

            // Step 5: evaluate movement and aim (cheap in comparison)
            var best = EvaluateMovementAndAim(card, reachableMoves);
            float tAfterStep5 = Time.realtimeSinceStartup;
            Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Step 5 (evaluate movement & aim) took {tAfterStep5 - tPrev:0.000}s");
            Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' evaluation complete in {Time.realtimeSinceStartup - tStart:0.000}s (best score {best?.score})");

            onComplete?.Invoke(best);
            yield break;
        }

        #region Core Turn Planning

        public void BuildTurnPlan(Action<List<PlannedAction>> callback)
        {
            npcScript.StartCoroutine(BuildTurnPlanCoroutine(callback));
        }

        public IEnumerator BuildTurnPlanCoroutine(Action<List<PlannedAction>> onComplete)
        {
            float start = Time.realtimeSinceStartup;

            // Build plan without executing actions
            List<PlannedAction> plan = new();

            Draw();

            Vector3Int virtualPosition = mover.currentCell;
            int virtualStamina = npcScript.entityStats.CurrentStamina;

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

                var repositionCandidate = TryGetRepositionCandidate(virtualPosition);

                if (repositionCandidate != null && repositionCandidate.score > 0)
                    actionCandidates.Add(repositionCandidate);

                if (actionCandidates.Count == 0)
                {
                    var chaseCandidate = TryGetChaseCandidate(virtualPosition);

                    if (chaseCandidate != null)
                        actionCandidates.Add(chaseCandidate);
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

            float duration = Time.realtimeSinceStartup - start;
            Debug.Log($"[NpcAI] BuildTurnPlan for {npcScript.name} took {duration:0.000}s");

            Debug.Log($"[NpcAI] Turn plan complete. Total actions planned: {plan.Count}");
            for (int i = 0; i < plan.Count; i++)
            {
                var action = plan[i];
                Debug.Log($"[NpcAI]   Action {i + 1}: {action.Type} - {action.Name}");
            }

            onComplete?.Invoke(plan);
        }

        private void Draw()
        {
            int cardsToDraw = npcScript.entityStats.Wisdom.Value() / 2;

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

                if (score > best.score)
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
            var castingPosition = movePath?.End ?? mover.currentCell;

            // Pre-compute visible tiles as HashSet for O(1) lookup
            HashSet<Vector3Int> visibleTiles = null;
            if (cardData.targetingData.TargetingUsesVision)
            {
                var tilesToCheck = new List<Vector3Int>();
                foreach (var target in targets)
                {
                    var targetEntityOnMap = target.GetComponent<EntityOnMap>();
                    if (targetEntityOnMap != null)
                        tilesToCheck.Add(targetEntityOnMap.currentCell);
                }
                if (tilesToCheck.Count > 0)
                    visibleTiles = new HashSet<Vector3Int>(VisionUtility.GetVisibleTiles(castingPosition, tilesToCheck));
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
                    if (targetEntityOnMap == null || !visibleTiles.Contains(targetEntityOnMap.currentCell))
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

        private ScoredCard SelectBestActionCandidate(List<ScoredCard> candidates, int stamina)
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

        private ScoredCard TryGetRepositionCandidate(
            Vector3Int virtualPosition
            )
        {
            if (npcScript.entityStats.IsRooted) 
                return null;

            ScoredCard moveOption = null;

            switch (npcAIBias.RepositionCondition)
            {
                case RepositionCondition.lowHealth:

                    if (npcScript.entityStats.CurrentHealth <
                        npcScript.entityStats.MaxHealth.Value() * 0.3f)
                    {
                        moveOption = DetermineRepositionTarget(
                            virtualPosition,
                            npcScript);

                        if (moveOption != null)
                        {
                            moveOption.score = 1;
                            moveOption.pseudoName = "Low Health Flee";
                        }
                    }

                    break;
            }

            return moveOption;
        }

        private ScoredCard TryGetChaseCandidate(Vector3Int virtualPosition)
        {
            if (npcScript.entityStats.IsRooted)
                return null;

            var allEntities = TargetingUtility.AllEntitiesCache();

            var enemies = allEntities
                .Where(e => e.entityAffiliation != npcScript.entityAffiliation)
                .ToList();

            if (enemies.Count == 0)
                return null;

            var nearestEnemy = enemies
                .OrderBy(e =>
                    Vector3Int.Distance(
                        e.GetComponent<EntityOnMap>().currentCell,
                        virtualPosition))
                .FirstOrDefault();

            if (nearestEnemy == null)
                return null;

            var targetCell = nearestEnemy.GetComponent<EntityOnMap>().currentCell;

            var pathData = MovementUtility.FindPath(
                virtualPosition,
                targetCell,
                walkClose: true,
                movementCostModifier: npcScript.entityStats.MovementCostModifier);

            if (pathData == null)
                return null;

            if (pathData.PathCost <= npcScript.entityStats.CurrentStamina)
            {
                return new ScoredCard
                {
                    pseudoName = "Chase",
                    targetingModeData = new() { castingPosition = targetCell },
                    pathData = pathData,
                    score = 10
                };
            }

            return null;
        }

        private ScoredCard DetermineRepositionTarget(Vector3Int virtualPosition, EntityScript entity)
        {
            var allEntities = TargetingUtility.AllEntitiesCache();
            var enemies = allEntities.Where(e => e.entityAffiliation != entity.entityAffiliation).ToList();

            if (enemies.Count == 0)
                return CreateStayInPlaceMove(virtualPosition, "No Enemies Move");

            // Cap flee distance by number of hostiles (but not above stamina)
            int maxFleeDistance = Mathf.Min(entity.entityStats.CurrentStamina, enemies.Count);

            // Delegate the search and scoring
            ScoredCard bestTarget = EvaluateRepositionCandidates(virtualPosition, entity, maxFleeDistance, enemies);

            return bestTarget ?? CreateStayInPlaceMove(virtualPosition, "No Move");
        }

        private ScoredCard CreateStayInPlaceMove(Vector3Int position, string name)
        {
            return new ScoredCard
            {
                pseudoName = name,
                targetingModeData = new() { castingPosition = position },
                pathData = new PathData
                {
                    Start = position,
                    End = position,
                    PathCost = 0,
                    Path = new List<Vector3Int> { position }
                },
                score = 0
            };
        }
        private ScoredCard EvaluateRepositionCandidates(Vector3Int virtualPosition, EntityScript entity, int maxFleeDistance, List<EntityScript> enemies)
        {
            ScoredCard bestTarget = null;
            float bestScore = float.MinValue;

            for (int dx = -maxFleeDistance; dx <= maxFleeDistance; dx++)
            {
                for (int dy = -maxFleeDistance; dy <= maxFleeDistance; dy++)
                {
                    Vector3Int candidate = new Vector3Int(virtualPosition.x + dx, virtualPosition.y + dy, virtualPosition.z);

                    // Skip current position or invalid tiles
                    if (candidate == virtualPosition || !TilemapUtilityScript.BaseTilemap.HasTile(candidate))
                        continue;

                    // Pathfinding
                    var pathData = MovementUtility.FindPath(virtualPosition, candidate, movementCostModifier: npcScript.entityStats.MovementCostModifier);
                    if (pathData == null || pathData.Path == null || pathData.Path.Count == 0)
                        continue;

                    int moveCost = pathData.PathCost;
                    if (moveCost == 0 || moveCost > entity.entityStats.CurrentStamina)
                        continue;

                    // Determine minimum distance to any hostile
                    float minEnemyDist = float.MaxValue;
                    foreach (var e in enemies)
                    {
                        var enemyPos = e.GetComponent<EntityOnMap>();
                        if (enemyPos != null)
                        {
                            float distToEnemy = Vector3Int.Distance(enemyPos.currentCell, candidate);
                            if (distToEnemy < minEnemyDist)
                                minEnemyDist = distToEnemy;
                        }
                    }

                    // Scoring formula
                    float score = (minEnemyDist * 2f) - moveCost;

                    // Penalize being too close
                    if (minEnemyDist < 2f)
                        score -= 100f;

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