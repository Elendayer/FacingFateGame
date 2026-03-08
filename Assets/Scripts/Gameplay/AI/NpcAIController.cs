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

        #region Core Turn Planning

        public void BuildTurnPlan(Action<List<PlannedAction>> callback)
        {
            npcScript.StartCoroutine(BuildTurnPlanCoroutine(callback));
        }

        public IEnumerator BuildTurnPlanCoroutine(Action<List<PlannedAction>> onComplete)
        {
            float start = Time.realtimeSinceStartup;

            List<PlannedAction> plan = new();

            // Cache entities once
            var allEntities = UnityEngine.Object
                .FindObjectsByType<EntityScript>(FindObjectsSortMode.None)
                .ToList();

            Draw();

            Vector3Int virtualPosition = mover.currentCell;
            int virtualStamina = npcScript.entityStats.CurrentStamina;

            while (virtualStamina > 0)
            {
                var actionCandidates = new List<ScoredCard>();

                // Evaluate hand cards in small batches to avoid long single-frame stalls.
                var handCandidates = new List<ScoredCard>();

                const int batchSize = 1; // tuneable: number of cards to evaluate per frame

                for (int i = 0; i < hand.Count; i++)
                {
                    var scored = EvaluateCard(hand[i], virtualStamina, virtualPosition, allEntities);
                    if (scored != null && scored.score > 0)
                    {
                        handCandidates.Add(scored);
                    }
                    
                    // Yield periodically so heavy targeting calculations don't stall the frame.
                    if ((i % batchSize) == batchSize - 1)
                        yield return null;
                }

                if (handCandidates.Count > 0)
                    actionCandidates.AddRange(handCandidates);

                var repositionCandidate = TryGetRepositionCandidate(virtualPosition, allEntities);

                if (repositionCandidate != null && repositionCandidate.score > 0)
                    actionCandidates.Add(repositionCandidate);

                if (actionCandidates.Count == 0)
                {
                    var chaseCandidate = TryGetChaseCandidate(virtualPosition, allEntities);

                    if (chaseCandidate != null)
                        actionCandidates.Add(chaseCandidate);
                    else
                        break;
                }

                var bestAction = SelectBestActionCandidate(actionCandidates, virtualStamina);

                if (bestAction == null || bestAction.score <= 0)
                    break;

                int moveCost = bestAction.pathData?.PathCost ?? 0;
                int cardCost = bestAction.card?.cardData?.Cost ?? 0;
                int totalCost = moveCost + cardCost;

                if (totalCost > virtualStamina || totalCost <= 0)
                    break;

                ApplyActionToPlan(plan, bestAction, ref virtualPosition, ref virtualStamina);

                yield return null;
            }

            float duration = Time.realtimeSinceStartup - start;
            Debug.Log($"[NpcAI] BuildTurnPlan for {npcScript.name} took {duration:0.000}s");

            onComplete?.Invoke(plan);
        }

        private void Draw()
        {
            int cardsToDraw = npcScript.entityStats.Wisdom.Value() / 2;

            for (int i = 0; i < cardsToDraw; i++)
            {
                if (deck.childCount == 0)
                {
                    foreach (Transform card in discard)
                        card.SetParent(deck);
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

        private List<ScoredCard> EvaluateHandActions(
            List<CardScript> hand,
            int stamina,
            Vector3Int virtualPosition,
            List<EntityScript> allEntities)
        {
            var candidates = new List<ScoredCard>();

            foreach (var card in hand)
            {
                var scored = EvaluateCard(card, stamina, virtualPosition, allEntities);

                if (scored != null && scored.score > 0)
                    candidates.Add(scored);
            }

            return candidates;
        }

        private ScoredCard EvaluateCard(
            CardScript card,
            int stamina,
            Vector3Int virtualPosition,
            List<EntityScript> allEntities)
        {
            if (card == null || card.cardData == null)
                return null;

            var validTargets = TargetingUtility.GetValidTargets(card.cardData, allEntities);

            if (validTargets.Count == 0)
                return null;

            var targetingMode = TargetingModeFactory.Create(card);

            if (targetingMode == null)
                return null;

            var templates = targetingMode.GetTargetingData(card, validTargets, npcScript);

            if (templates == null || templates.Count == 0)
                return null;

            var reachableMoves = TargetingUtility.GetReachableCandidates(
                card,
                templates,
                stamina,
                virtualPosition
            );

            if (reachableMoves == null || reachableMoves.Count == 0)
                return null;

            return EvaluateMovementAndAim(card, reachableMoves);
        }

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

                int score = EvaluateCardScore(card, move.Item2.targetedEntities, move.Item1.PathCost);

                if (score > best.score)
                {
                    best.score = score;
                    best.pseudoName = $"Play {card.cardData.cardName}";
                    best.targets = move.Item2.targetedEntities
                        .Take(card.cardData.MaxTarget > 0 ? card.cardData.MaxTarget : move.Item2.targetedEntities.Count)
                        .ToList();

                    best.targetingModeData = move.Item2;
                    best.pathData = move.Item1;
                    best.executionOption = CardExecutionOption.PlayCard;
                }
            }

            return best;
        }

        #endregion

        #region Apply Actions

        private void ApplyActionToPlan(
            List<PlannedAction> plan,
            ScoredCard bestAction,
            ref Vector3Int virtualPosition,
            ref int virtualStamina)
        {
            if (bestAction == null)
                return;

            if (bestAction.pathData != null && bestAction.pathData.Path.Count > 0)
            {
                plan.Add(new PlannedAction
                {
                    Type = PlannedAction.ActionType.Move,
                    Name = $"Move_for_{bestAction.pseudoName}",
                    TargetingModeData = bestAction.targetingModeData,
                    PathData = bestAction.pathData
                });

                virtualPosition = bestAction.pathData.End;
                virtualStamina -= bestAction.pathData.PathCost;
            }

            if (bestAction.executionOption == CardExecutionOption.PlayCard && bestAction.card != null)
            {
                plan.Add(new PlannedAction
                {
                    Type = PlannedAction.ActionType.PlayCard,
                    Name = bestAction.card.name,
                    Card = bestAction.card,
                    TargetingModeData = bestAction.targetingModeData,
                    PathData = bestAction.pathData
                });

                virtualStamina -= bestAction.card.cardData.Cost;
                hand.Remove(bestAction.card);
            }
        }

        #endregion

        #region Helpers

        private ScoredCard SelectBestActionCandidate(List<ScoredCard> candidates, int stamina)
        {
            if (candidates == null || candidates.Count == 0)
                return null;

            var ordered = candidates.OrderByDescending(a => a.score);

            foreach (var candidate in ordered)
            {
                int totalCost = (candidate.pathData?.PathCost ?? 0) +
                                (candidate.card?.cardData?.Cost ?? 0);

                if (totalCost <= stamina)
                    return candidate;
            }

            return null;
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

            return throughput / cost;
        }

        #endregion

        #region Flee / Chase

        private ScoredCard TryGetRepositionCandidate(
            Vector3Int virtualPosition,
            List<EntityScript> allEntities)
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
                            allEntities,
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

        private ScoredCard TryGetChaseCandidate(
            Vector3Int virtualPosition,
            List<EntityScript> allEntities)
        {
            if (npcScript.entityStats.IsRooted)
                return null;

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

        private ScoredCard DetermineRepositionTarget(Vector3Int virtualPosition, List<EntityScript> allEntities, EntityScript entity)
        {
            // Count hostiles
            int hostileCount = allEntities.Count(e => e.entityAffiliation != entity.entityAffiliation);
            if (hostileCount == 0)
            {
                // No enemies — stay in place
                return new ScoredCard
                {
                    pseudoName = "No Enemies Move",
                    targetingModeData = new() { castingPosition = virtualPosition },
                    pathData = new PathData
                    {
                        Start = virtualPosition,
                        End = virtualPosition,
                        PathCost = 0,
                        Path = new List<Vector3Int> { virtualPosition }
                    },
                    score = 0
                };
            }
            // Cap flee distance by number of hostiles (but not above stamina)

            int maxFleeDistance = Mathf.Min(entity.entityStats.CurrentStamina, hostileCount);
            // Delegate the search and scoring
            ScoredCard bestTarget = EvaluateRepositionCandidates(virtualPosition, allEntities, entity, maxFleeDistance);
            // Fallback if no valid move found
            return bestTarget ?? new ScoredCard
            {
                pseudoName = "No Move",
                targetingModeData = new() { castingPosition = virtualPosition },
                pathData = new PathData { Start = virtualPosition, End = virtualPosition, PathCost = 0, Path = new List<Vector3Int> { virtualPosition } },
                score = 0
            };
        }
        private ScoredCard EvaluateRepositionCandidates(Vector3Int virtualPosition, List<EntityScript> allEntities, EntityScript entity, int maxFleeDistance)
        {
            ScoredCard bestTarget = null; float bestScore = float.MinValue; for (int dx = -maxFleeDistance; dx <= maxFleeDistance; dx++)
            {
                for (int dy = -maxFleeDistance; dy <= maxFleeDistance; dy++)
                {
                    Vector3Int candidate = new Vector3Int(virtualPosition.x + dx, virtualPosition.y + dy, virtualPosition.z);

                    // Skip current position or invalid tiles
                    if (candidate == virtualPosition || !TilemapUtilityScript.BaseTilemap.HasTile(candidate)) continue;

                    // Pathfinding
                    var pathData = MovementUtility.FindPath(virtualPosition, candidate, movementCostModifier: npcScript.entityStats.MovementCostModifier);
                    if (pathData == null || pathData.Path == null || pathData.Path.Count == 0) continue; int moveCost = pathData.PathCost; if (moveCost == 0 || moveCost > entity.entityStats.CurrentStamina) continue;

                    // Determine minimum distance to any hostile
                    float minEnemyDist = float.MaxValue; foreach (var e in allEntities)
                    {
                        if (e.entityAffiliation == entity.entityAffiliation) continue;
                        float distToEnemy = Vector3Int.Distance(e.GetComponent<EntityOnMap>().currentCell, candidate); if (distToEnemy < minEnemyDist) minEnemyDist = distToEnemy;
                    }

                    // Scoring formula
                    float score = (minEnemyDist * 2f) - moveCost;

                    // Penalize being too close
                    if (minEnemyDist < 2f) score -= 100f;

                    // Track best-scoring candidate
                    if (score > bestScore) { bestScore = score; bestTarget = new ScoredCard { pseudoName = "Flee", targetingModeData = new() { castingPosition = candidate }, pathData = pathData, score = Mathf.RoundToInt(score) }; }
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