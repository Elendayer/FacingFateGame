using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
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
            // Validation
            if (card == null || card.cardData == null)
            {
                Debug.LogWarning("[NpcAI][EvaluateCard] Invalid card or missing cardData");
                onComplete?.Invoke(null);
                yield break;
            }

            var cardName = card.cardData.cardName;
            var cardCost = card.cardData.Cost;
            var cardRange = card.cardData.Range;

            if (cardCost > stamina)
            {
                Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' REJECTED: Cost {cardCost} > stamina {stamina}");
                onComplete?.Invoke(null);
                yield break;
            }

            float tStart = Time.realtimeSinceStartup;
            var reachableMoves = new List<(NavMeshPathData, TargetingModeData)>();
            float movementBudget = stamina - cardCost;

            // Phase 1: Try casting from current virtual position
            var targetingAtCurrent = GetTargetingFromPosition(card, virtualPosition);
            if (targetingAtCurrent?.targetedEntities != null && targetingAtCurrent.targetedEntities.Count > 0)
            {
                reachableMoves.Add((
                    new NavMeshPathData { PathCost = 0, Start = virtualPosition, End = virtualPosition },
                    targetingAtCurrent
                ));
                Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Phase 1 SUCCESS - Found {targetingAtCurrent.targetedEntities.Count} targets at virtual position");
            }
            else
            {
                Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Phase 1 FAILED - No targets at current position (distance too far for range {cardRange:F2})");
            }

            // Phase 2: If card has range and we have movement budget, explore better positions
            if (cardRange > 0 && movementBudget > 0)
            {
                Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' - Entering Phase 2 & 3 (Range: {cardRange:F2}, MoveBudget: {movementBudget:F0})");

                // Try nearby positions for better coverage
                var nearbyPositions = GetNearbyPositionsForCard(virtualPosition, movementBudget, cardRange);
                Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Phase 2 - Found {nearbyPositions.Count} nearby positions to explore");

                foreach (var (pathData, castPos) in nearbyPositions)
                {
                    var targetingAtNearby = GetTargetingFromPosition(card, castPos);
                    if (targetingAtNearby?.targetedEntities != null && targetingAtNearby.targetedEntities.Count > 0)
                    {
                        reachableMoves.Add((pathData, targetingAtNearby));
                        Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Phase 2 - Found {targetingAtNearby.targetedEntities.Count} targets at nearby position");
                    }
                }
                Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Phase 2 Complete - reachableMoves count: {reachableMoves.Count}");

                // Phase 3: Try moving into range of enemies (search independently)
                // This handles cases where targets are out of current range but can be reached via movement
                var allEntities = UnityEngine.Object.FindObjectsByType<EntityScript>(0);
                var potentialEnemies = new List<EntityScript>();

                Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Phase 3 - Starting independent enemy search (found {allEntities.Length} total entities)");

                foreach (var entity in allEntities)
                {
                    // Skip self and allies
                    if (entity == npcScript || entity.entityAffiliation == npcScript.entityAffiliation)
                        continue;

                    // Check if target is valid for this card
                    if (IsValidTargetAffiliation(entity, card.cardData) && TargetingUtility.IsTargetValid(card.cardData, entity))
                    {
                        potentialEnemies.Add(entity);
                    }
                }

                Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Phase 3 - Found {potentialEnemies.Count} valid potential enemies to path to");

                int pathsFound = 0;
                int pathsFailed = 0;

                foreach (var targetEntity in potentialEnemies)
                {
                    var targetPos = targetEntity.GetComponent<EntityOnMap>()?.transform.position ?? targetEntity.transform.position;
                    var distToTarget = Vector3.Distance(virtualPosition, targetPos);

                    // If target is within card range, we already evaluated it in Phase 1/2
                    // Only try to path to targets that are beyond current range
                    if (distToTarget > cardRange)
                    {
                        Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Phase 3 - Attempting path to '{targetEntity.name}' at distance {distToTarget:F2} (range: {cardRange:F2})");

                        var rangeResult = TargetingUtility.FindPathIntoRange(virtualPosition, targetPos, cardRange, npcScript.entityStats, movementBudget);


                        if (rangeResult.HasValue)
                        {
                            var castPos = rangeResult.Value.castPosition;

                            // Validate that the specific target entity is still reachable and in range from the cast position
                            float distFromCastPos = Vector3.Distance(castPos, targetPos);
                            const float RANGE_TOLERANCE = 0.1f; // Allow tolerance for floating-point precision and NavMesh snapping
                            if (distFromCastPos <= cardRange + RANGE_TOLERANCE)
                            {
                                // For Single target cards, verify the target is valid from this position
                                // GetTargetingFromPosition might find a different target, so we need to check explicitly
                                var targetingAtRange = GetTargetingFromPosition(card, castPos);

                                // Check if our intended target is in the resulting targets
                                bool targetFoundInResults = targetingAtRange?.targetedEntities != null && targetingAtRange.targetedEntities.Contains(targetEntity);

                                if (targetFoundInResults)
                                {
                                    reachableMoves.Add((rangeResult.Value.pathData, targetingAtRange));
                                    pathsFound++;
                                    Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Phase 3 SUCCESS - Path found to '{targetEntity.name}', {targetingAtRange.targetedEntities.Count} targets at cast position");
                                }
                                else if (targetingAtRange?.targetedEntities != null && targetingAtRange.targetedEntities.Count > 0)
                                {
                                    // Found other targets even if not the intended one - still valid
                                    reachableMoves.Add((rangeResult.Value.pathData, targetingAtRange));
                                    pathsFound++;
                                    Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Phase 3 SUCCESS - Path found, targeting different entity. Found {targetingAtRange.targetedEntities.Count} targets at cast position");
                                }
                                else
                                {
                                    Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Phase 3 - Path to '{targetEntity.name}' found but target not reachable from cast position (distance from cast: {distFromCastPos:F2})");
                                }
                            }
                            else
                            {
                                Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Phase 3 - FindPathIntoRange returned position outside range of target (distance: {distFromCastPos:F2} > {cardRange + RANGE_TOLERANCE:F2} [range + tolerance])");
                            }
                        }
                        else
                        {
                            pathsFailed++;
                            Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Phase 3 - No path found to '{targetEntity.name}' (distance: {distToTarget:F2}, budget: {movementBudget})");
                        }
                    }
                    else
                    {
                        Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Phase 3 - Target '{targetEntity.name}' already in range ({distToTarget:F2} <= {cardRange:F2}), skipping");
                    }
                }
                Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' Phase 3 Complete - Paths found: {pathsFound}, Paths failed: {pathsFailed}, Total reachableMoves: {reachableMoves.Count}");
            }
            else if (cardRange <= 0 || movementBudget <= 0)
            {
                Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' - SKIPPING Phase 2&3 (Range: {cardRange:F2}, MoveBudget: {movementBudget})");
            }

            // If no valid positions found, return zero score
            if (reachableMoves.Count == 0)
            {
                Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' REJECTED: No valid targeting positions found");
                onComplete?.Invoke(null);
                yield break;
            }

            // Evaluate best move
            var evaluation = EvaluateMovementAndAim(card, reachableMoves);
            var elapsed = Time.realtimeSinceStartup - tStart;

            if (evaluation == null)
            {
                Debug.LogError($"[NpcAI][EvaluateCard] '{cardName}' EvaluateMovementAndAim returned null");
                onComplete?.Invoke(null);
            }
            else if (evaluation.score == 0)
            {
                Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' - No viable moves (score: 0) [{elapsed:0.000}s]");
                onComplete?.Invoke(evaluation);
            }
            else
            {
                Debug.Log($"[NpcAI][EvaluateCard] '{cardName}' - Score: {evaluation.score} [{elapsed:0.000}s]");
                onComplete?.Invoke(evaluation);
            }

            yield break;
        }

        /// <summary>
        /// Gets targeting data from a specific casting position without relying on the NPC's current position.
        /// Temporarily simulates the NPC being at castPos for targeting calculations.
        /// </summary>
        private TargetingModeData GetTargetingFromPosition(CardScript card, Vector3 castPos)
        {
            if (card?.cardData == null)
                return null;

            var cardData = card.cardData;
            var entities = new List<EntityScript>();

            // Determine entities in targeting range based on card's targeting mode
            switch (cardData.targetingData.cardTargetingMode)
            {
                case CardTargetingMode.Cone:
                    // For cone, aim towards primary enemies
                    var coneTarget = FindPrimaryTarget(castPos);
                    if (coneTarget != null)
                    {
                        var direction = (coneTarget.transform.position - castPos).normalized;
                        entities = TargetingUtility.GetEntitiesInPhysicsCone(castPos, direction, cardData.Range, cardData.Area, cardData);
                    }
                    break;

                case CardTargetingMode.LineSelf:
                    // For line, aim towards primary enemies
                    var lineTarget = FindPrimaryTarget(castPos);
                    if (lineTarget != null)
                    {
                        var direction = (lineTarget.transform.position - castPos).normalized;
                        entities = TargetingUtility.GetEntitiesInPhysicsLine(castPos, direction, cardData.Range, cardData.Area, cardData);
                    }
                    break;

                case CardTargetingMode.Sphere:
                    // For radius, center on self
                    entities = TargetingUtility.GetEntitiesInPhysicsSphere(castPos, cardData.Radius, cardData);
                    break;

                case CardTargetingMode.Ring:
                    // For ring, center on self
                    entities = TargetingUtility.GetEntitiesInPhysicsRing(castPos, cardData.Radius, cardData.Area, cardData);
                    break;

                case CardTargetingMode.Single:
                    // For single target, find nearest valid enemy within range
                    var singleTarget = FindPrimaryTarget(castPos);
                    if (singleTarget != null)
                    {
                        float distToTarget = Vector3.Distance(castPos, singleTarget.transform.position);
                        // Only add target if it's within card range (with tolerance for floating-point precision)
                        const float RANGE_TOLERANCE = 0.1f;
                        if (distToTarget <= cardData.Range + RANGE_TOLERANCE)
                        {
                            // Directly add the target we found, rather than using OverlapSphere which might miss it
                            entities = new List<EntityScript> { singleTarget };
                        }
                    }
                    break;

                default:
                    // For other modes, use GetAffected as fallback
                    var result = TargetingUtility.GetAffected(card, castPos, npcScript, false, null, true);
                    return result;
            }

            // Filter to valid targets based on card affiliation requirements
            var validTargets = entities.FindAll(e => IsValidTargetAffiliation(e, cardData) && TargetingUtility.IsTargetValid(cardData, e));

            // Apply vision filtering if needed
            if (cardData.targetingData.TargetingUsesVision)
            {
                validTargets = validTargets.FindAll(e => TargetingUtility.HasPhysicsLineOfSight(castPos, e.transform.position));
            }

            var primaryTarget = FindPrimaryTarget(castPos);
            var targetingData = new TargetingModeData
            {
                castingPosition = castPos,
                aimPosition = primaryTarget != null ? primaryTarget.transform.position : castPos,
                targetedEntities = validTargets,
                targetedPositions = validTargets.Select(e => e.transform.position).ToList()
            };

            return targetingData;
        }

        /// <summary>
        /// Finds the nearest enemy to target for directional targeting modes (cone, line, single).
        /// Returns null if no valid enemy is found.
        /// </summary>
        private EntityScript FindPrimaryTarget(Vector3 fromPosition)
        {
            EntityScript nearest = null;
            float nearestDist = float.MaxValue;

            var allEntities = UnityEngine.Object.FindObjectsByType<EntityScript>(0);

            foreach (var entity in allEntities)
            {
                // Skip self and allies
                if (entity == npcScript || entity.entityAffiliation == npcScript.entityAffiliation)
                {
                    continue;
                }

                // Skip neutrals
                if (entity.entityAffiliation == EntityAffiliation.Neutral)
                {
                    continue;
                }

                var dist = Vector3.Distance(fromPosition, entity.transform.position);

                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = entity;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Gets nearby positions to evaluate for card casting within movement budget.
        /// Uses spiral search pattern to efficiently find tactical positions without grid explosion.
        /// Instead of testing (2*radius+1)^2 positions, tests ~12-16 positions per distance ring.
        /// </summary>
        private List<(NavMeshPathData, Vector3)> GetNearbyPositionsForCard(Vector3 currentPos, float movementBudget, float cardRange)
        {
            var positions = new List<(NavMeshPathData, Vector3)>();
            const int maxPositions = 16;  // Limit total positions evaluated
            const int anglesPerRing = 12; // 12 angles = 30° increments

            // Start with small radius and spiral outward
            for (float distance = 1f; distance <= movementBudget && positions.Count < maxPositions; distance += 1f)
            {
                // Test 12 angles around the circle at this distance
                for (int angleIndex = 0; angleIndex < anglesPerRing; angleIndex++)
                {
                    if (positions.Count >= maxPositions)
                        break;

                    float angle = (angleIndex * 360f / anglesPerRing) * Mathf.Deg2Rad;
                    Vector3 candidate = currentPos + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;

                    // Snap to navmesh
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(candidate, out hit, 2f, NavMesh.AllAreas))
                    {
                        candidate = hit.position;

                        // Try to find a path to this position
                        var pathData = MovementUtility.FindPath(currentPos, candidate, entityStats: npcScript.entityStats);
                        if (pathData != null && pathData.CachedNavMeshPath != null && pathData.CachedNavMeshPath.corners.Length > 0 && pathData.PathCost <= movementBudget)
                        {
                            positions.Add((pathData, candidate));
                        }
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

            Debug.Log($"[NpcAI][ExecuteTurnPlanning] Starting turn planning for {npcScript.name}");
            Debug.Log($"[NpcAI][ExecuteTurnPlanning] Initial stamina: {virtualStamina}, Position: {virtualPosition}, Hand: {hand.Count} cards");
            if (hand.Count > 0)
            {
                Debug.Log($"[NpcAI][ExecuteTurnPlanning] Cards in hand: {string.Join(", ", hand.Select(c => c?.cardData?.cardName ?? "NULL"))}");
            }

            // Continue selecting best actions until no valid action remains
            while (virtualStamina > 0)
            {
                var actionCandidates = new List<ScoredCard>();

                // Evaluate hand cards concurrently via coroutines, wait for all to finish.
                var handCount = hand.Count;
                var remaining = handCount;
                var results = new ScoredCard[handCount];

                Debug.Log($"[NpcAI][HandEvaluation] Evaluating {handCount} cards in hand (stamina: {virtualStamina}, position: {virtualPosition})");

                if (handCount > 0)
                {
                    for (int i = 0; i < handCount; i++)
                    {
                        int idx = i;
                        // Skip cards that have already been played this turn
                        if (playedCards.Contains(hand[idx]))
                        {
                            Debug.Log($"[NpcAI][HandEvaluation] Card {idx} ({hand[idx].cardData.cardName}) already played, skipping");
                            results[idx] = null;
                            remaining--;
                            continue;
                        }

                        Debug.Log($"[NpcAI][HandEvaluation] Starting evaluation coroutine for card {idx}: {hand[idx].cardData.cardName}");

                        CoroutineRunner.Instance.StartCoroutineManaged(EvaluateCardCoroutine(hand[idx], virtualStamina, virtualPosition, (res) =>
                        {
                            if (res != null)
                            {
                                Debug.Log($"[NpcAI][HandEvaluation] Card {idx} ({hand[idx].cardData.cardName}) evaluation complete. Score: {res.score}");
                            }
                            else
                            {
                                Debug.Log($"[NpcAI][HandEvaluation] Card {idx} ({hand[idx].cardData.cardName}) evaluation returned NULL");
                            }
                            results[idx] = res;
                            remaining--;
                        }));
                        // Slight yield to spread coroutine starts across frames
                        yield return null;
                    }

                    // Wait for all evaluations to complete
                    while (remaining > 0)
                    {
                        yield return null;
                    }

                    var handCandidates = new List<ScoredCard>();
                    int nullCount = 0;
                    int zeroScoreCount = 0;

                    Debug.Log($"[NpcAI][HandEvaluation] Processing {handCount} evaluation results...");

                    for (int i = 0; i < handCount; i++)
                    {
                        var r = results[i];
                        if (r == null)
                        {
                            nullCount++;
                            Debug.Log($"[NpcAI][HandEvaluation] Result {i}: NULL (card not viable - no stamina, no targets, or aborted)");
                            // Null results are expected - card wasn't viable (no stamina, no targets, etc.)
                        }
                        else if (r.score > 0)
                        {
                            handCandidates.Add(r);
                            Debug.Log($"[NpcAI][HandEvaluation] Result {i}: ADDED TO CANDIDATES - Card: {r.card.cardData.cardName}, Score: {r.score}");
                        }
                        else
                        {
                            zeroScoreCount++;
                            Debug.Log($"[NpcAI][HandEvaluation] Result {i}: REJECTED - Score is 0 (Card: {r.card.cardData.cardName})");
                        }
                    }

                    Debug.Log($"[NpcAI][HandEvaluation] Results summary - NULL: {nullCount}, Zero Score: {zeroScoreCount}, Valid candidates: {handCandidates.Count}");

                    if (handCandidates.Count > 0)
                    {
                        actionCandidates.AddRange(handCandidates);
                        Debug.Log($"[NpcAI][HandEvaluation] Added {handCandidates.Count} hand candidates to action candidates list");
                    }
                    else
                    {
                        Debug.LogWarning($"[NpcAI][HandEvaluation] NO HAND CANDIDATES! All {handCount} cards were rejected or had null results");
                    }
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

                // Record movement action if any movement is needed (movement-only OR movement before card cast)
                if (bestAction.pathData != null && bestAction.pathData.CachedNavMeshPath != null && bestAction.pathData.CachedNavMeshPath.corners.Length > 0 && bestAction.pathData.PathCost > 0)
                {
                    // For movement-only actions (chase/reposition), record the full action with cost
                    // For card actions with movement, record the movement separately
                    bool isCardAction = bestAction.executionOption == CardExecutionOption.PlayCard && bestAction.card != null;

                    plan.Add(new PlannedAction
                    {
                        Type = PlannedAction.ActionType.Move,
                        Name = $"Move_for_{bestAction.pseudoName}",
                        TargetingModeData = bestAction.targetingModeData,
                        PathData = bestAction.pathData,
                        PathCost = bestAction.pathData.PathCost,
                        CardCost = 0  // Movement action doesn't consume card cost
                    });

                    Debug.Log($"[NpcAI] Recorded movement: cost={bestAction.pathData.PathCost}, position={bestAction.pathData.End}");

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
                        PathData = bestAction.pathData,
                        PathCost = 0,  // Path cost was already recorded in movement action if needed
                        CardCost = cardCost
                    });

                    Debug.Log($"[NpcAI] Recorded card play: card={bestAction.card.cardData.cardName}, cost={cardCost}, casting_pos={bestAction.targetingModeData.castingPosition}");
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
                Debug.Log($"[NpcAI] Action {i + 1}: {action.Type} - {action.Name}, with Cost: {action.Cost}");
            }

            yield break;
        }

        public void DrawCard()
        {
            int cardsToDraw = Mathf.RoundToInt(npcScript.entityStats.CurrentWisdom / 2);
            Debug.Log($"[NpcAI][DrawCard] NPC: {npcScript.name}, Current Wisdom: {npcScript.entityStats.CurrentWisdom}, Cards to draw: {cardsToDraw}");

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
                    Debug.Log($"[NpcAI][DrawCard] Drew card: {cardScript.cardData.cardName}, Hand size now: {hand.Count}");

                    cardObj.SetParent(discard);
                }
                else
                {
                    Debug.LogWarning($"[NpcAI][DrawCard] Tried to draw card #{i + 1} but deck is empty!");
                }
            }

            Debug.Log($"[NpcAI][DrawCard] Draw complete. Final hand size: {hand.Count} cards");
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
            List<(NavMeshPathData, TargetingModeData)> reachableMoves)
        {
            ScoredCard best = new()
            {
                card = card,
                score = 0,
                pathData = new NavMeshPathData { PathCost = int.MaxValue },
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

        private List<EntityScript> FilterValidTargets(CardScript card, List<EntityScript> targets, NavMeshPathData movePath)
        {
            if (targets == null || targets.Count == 0)
            {
                Debug.Log($"[FilterValidTargets] '{card?.cardData?.cardName}' - Input targets NULL or empty");
                return new List<EntityScript>();
            }

            var validTargets = new List<EntityScript>();
            var cardData = card.cardData;
            var castingPosition = movePath?.End ?? mover.transform.position;

            Debug.Log($"[FilterValidTargets] '{cardData.cardName}' - Input: {targets.Count} targets, Casting from: {castingPosition}");

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

            int rejectedCount = 0;

            foreach (var target in targets)
            {
                if (target == null)
                {
                    rejectedCount++;
                    continue;
                }

                // Check affiliation match
                if (!IsValidTargetAffiliation(target, cardData))
                {
                    Debug.Log($"[FilterValidTargets] '{cardData.cardName}' - Rejected '{target.name}': Invalid affiliation");
                    rejectedCount++;
                    continue;
                }

                // Check vision requirements if applicable
                if (visibleTiles != null)
                {
                    var targetEntityOnMap = target.GetComponent<EntityOnMap>();
                    if (targetEntityOnMap == null || !visibleTiles.Contains(targetEntityOnMap.transform.position))
                    {
                        Debug.Log($"[FilterValidTargets] '{cardData.cardName}' - Rejected '{target.name}': No line of sight");
                        rejectedCount++;
                        continue;
                    }
                }

                validTargets.Add(target);
                Debug.Log($"[FilterValidTargets] '{cardData.cardName}' - ACCEPTED '{target.name}'");
            }

            Debug.Log($"[FilterValidTargets] '{cardData.cardName}' - Final: {validTargets.Count} valid targets (rejected: {rejectedCount})");
            return validTargets;
        }

        private bool IsValidTargetAffiliation(EntityScript target, CardData cardData)
        {
            if (target == null || cardData == null)
                return false;

            var targetingAffiliation = cardData.targetingData.CardTargetAffiliation;

            return targetingAffiliation switch
            {
                CardTargetAffiliation.Self =>           target == npcScript,
                CardTargetAffiliation.Ally =>           target.entityAffiliation == npcScript.entityAffiliation,
                CardTargetAffiliation.Enemy =>          target.entityAffiliation != npcScript.entityAffiliation,
                CardTargetAffiliation.All => true,
                CardTargetAffiliation.None => false,
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

            // Get throughput - CardAiBias is optional, defaults to 0 if null
            int throughput = card.cardData.CardAiBias?.ThroughputOverride(npcAIBias, card.cardData, targets) ?? 0;

            // If CardAiBias is null, use a default throughput based on card damage
            if (throughput == 0 && card.cardData.CardAiBias == null)
            {
                // Fallback: base throughput on damage * number of targets
                throughput = card.cardData.Damage * targets.Count;
                Debug.Log($"[CardScore] '{card.cardData.cardName}' - NO CardAiBias, using fallback throughput: {throughput} (damage: {card.cardData.Damage}, targets: {targets.Count})");
            }
            else if (throughput == 0)
            {
                Debug.Log($"[CardScore] '{card.cardData.cardName}' - CardAiBias returned 0 throughput!");
            }

            int cost = Mathf.Max(1, card.cardData.Cost + moveCost);
            int score = (throughput * 100) / cost;

            Debug.Log($"[CardScore] '{card.cardData.cardName}' - Targets: {targets.Count}, Throughput: {throughput}, Cost: {cost}, Score: {score}");

            return score;
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

            // Check if this NPC prefers ranged combat with no reposition available
            bool preferRangedNoReposition = npcAIBias.RepositionCondition == RepositionCondition.preferRanged;

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

                // If preferring ranged combat, prioritize maintaining distance over closing in
                if (preferRangedNoReposition)
                {
                    // Keep range: prefer positions that maintain or increase distance
                    float distanceAfterMove = Vector3.Distance(pathData.End, targetPosition);

                    // Only consider moves that don't reduce distance below current, or moves that increase distance
                    if (distanceAfterMove < distToEnemy && distToEnemy > 2f)
                        continue; // Skip moves that would close distance to an already-ranged target

                    // Score based on maintained distance and stamina efficiency
                    float distanceBonus = distanceAfterMove * 2f; // Higher distance = higher score
                    float staminaEfficiency = (remainingStamina - pathData.PathCost) / Mathf.Max(1, pathData.PathCost);
                    int score = Mathf.RoundToInt((distanceBonus + staminaEfficiency * 5f) * 10f);

                    // Bonus for having stamina left after movement for card plays
                    if (pathData.PathCost < remainingStamina * 0.5f)
                        score += 15;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestChaseTarget = new ScoredCard
                        {
                            pseudoName = "Chase",
                            targetingModeData = new() { castingPosition = new Vector3Int((int)pathData.End.x, (int)pathData.End.y, (int)pathData.End.z) },
                            pathData = pathData,
                            score = score
                        };
                    }
                }
                else
                {
                    // Normal chase behavior: calculate score based on distance and movement efficiency
                    // Closer enemies are higher priority, but account for movement cost
                    // Score formula accounts for remaining stamina after movement
                    float remainingStaminaAfterMove = remainingStamina - pathData.PathCost;
                    float distScore = Mathf.Max(0, remainingStamina - distToEnemy);
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
                targetingModeData = new() { castingPosition = new Vector3(position.x, position.y, position.z) },
                pathData = MovementUtility.FindPath(position, position, npcScript.entityStats),
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
                    if (pathData == null || pathData.CachedNavMeshPath == null || pathData.CachedNavMeshPath.corners.Length == 0)
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
        public NavMeshPathData pathData;
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
        public NavMeshPathData PathData;

        public int PathCost;
        public int CardCost;
        public int Cost => PathCost + CardCost;
    }

    #endregion
}