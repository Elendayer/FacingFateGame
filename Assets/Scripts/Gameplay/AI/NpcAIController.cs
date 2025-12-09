using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility;

public class NpcAIController
{
    private readonly NonPlayerScript npcScript;
    private readonly NpcAiBias npcAIBias;
    private readonly EntityOnMap mover;
    private List<EntityScript> allEntities => Object.FindObjectsByType<EntityScript>(FindObjectsSortMode.None).ToList();
    public List<CardScript> hand = new();

    public NpcAIController(NonPlayerScript npc, NpcData npcData)
    {
        npcScript = npc;
        npcAIBias = npcData.aiBias;
        mover = npc.GetComponent<EntityOnMap>();
        Debug.Log($"[NpcAI] Initialized for {npc.name} at cell {mover.currentCell}");
    }

    #region Core Turn Planning
    public List<PlannedAction> BuildTurnPlan()
    {
        List<PlannedAction> plan = new();
        hand = GetHandCards();
        Vector3Int virtualPosition = mover.currentCell;

        while (npcScript.entityStats.CurrentStamina > 0)
        {
            var actionCandidates = new List<ScoredCard>();

            // Evaluate hand cards
            actionCandidates.AddRange(EvaluateHandActions(hand, npcScript.entityStats.CurrentStamina, virtualPosition));

            // Evaluate flee or reposition
            var repositionCandidate = TryGetRepositionCandidate(virtualPosition, allEntities);
            if (repositionCandidate != null && repositionCandidate.score > 0)
            {
                actionCandidates.Add(repositionCandidate);
            }

            ScoredCard bestAction;
            int totalCost;

            // If no actions found, consider chasing
            if (actionCandidates.Count == 0)
            {
                Debug.Log($"[NpcAI] No action candidates found, considering chase for NPC {npcScript.name}");
                var chaseCandidate = TryGetChaseCandidate(virtualPosition, allEntities);
                if (chaseCandidate != null)
                {
                    actionCandidates = new List<ScoredCard> { chaseCandidate };

                    // Select best action within stamina
                    Debug.Log($"[NpcAI] Found {actionCandidates.Count} action candidates for NPC {npcScript.name}");
                     bestAction = SelectBestActionCandidate(actionCandidates, npcScript.entityStats.CurrentStamina);
                    if (bestAction == null || bestAction.score <= 0) break;

                    Debug.Log($"[NpcAI] Best action selected: {bestAction.pseudoName} with score {bestAction.score} for NPC {npcScript.name}");
                     totalCost = (bestAction.movementCost) + (bestAction.card?.cardData.Cost ?? 0);
                    if (totalCost > npcScript.entityStats.CurrentStamina) break;

                    ApplyActionToPlan(plan, bestAction, ref virtualPosition, npcScript);
                    break;
                }
                else
                {
                    Debug.Log($"[NpcAI] No chase candidate found for NPC {npcScript.name}, ending turn planning");
                    break;
                }
            }

            // Select best action within stamina
            Debug.Log($"[NpcAI] Found {actionCandidates.Count} action candidates for NPC {npcScript.name}");
            bestAction = SelectBestActionCandidate(actionCandidates, npcScript.entityStats.CurrentStamina);
            if (bestAction == null || bestAction.score <= 0) break;

            Debug.Log($"[NpcAI] Best action selected: {bestAction.pseudoName} with score {bestAction.score} for NPC {npcScript.name}");
            totalCost = (bestAction.movementCost) + (bestAction.card?.cardData.Cost ?? 0);
            if (totalCost > npcScript.entityStats.CurrentStamina) break;

            ApplyActionToPlan(plan, bestAction, ref virtualPosition, npcScript);
        }
        return plan;
    }
    #endregion

    #region Hand Evaluation
    private List<ScoredCard> EvaluateHandActions(List<CardScript> hand, int stamina, Vector3Int virtualPosition)
    {
        var candidates = new List<ScoredCard>();
        foreach (var card in hand)
        {
            var scored = EvaluateCard(card, stamina, virtualPosition);
            if (scored != null && scored.score > 0)
                candidates.Add(scored);
        }
        return candidates;
    }

    private ScoredCard EvaluateCard(CardScript card, int stamina, Vector3Int virtualPosition)
    {
        // STEP 1 — Get final list of valid targets
        var validTargets = TargetingUtility.GetValidTargets(card, npcScript, allEntities);
        if (validTargets.Count == 0)
            return null;

        // STEP 2 — Produce TARGETING TEMPLATES
        var targetingMode = TargetingModeFactory.Create(card);
        var templates = targetingMode.GetTargetingData(card, validTargets, npcScript);
        if (templates == null || templates.Count == 0)
            return null;


        // STEP 3 — Pathfinding only to those tiles (fast)
        var reachableMoves = TargetingUtility.GetReachableCandidates(
            card,
            templates,
            stamina,
            virtualPosition
        );

        if (reachableMoves == null || reachableMoves.Count == 0)
            return null;

        // STEP 5 — Evaluate (movement + aim) using templates + reachable paths
        return EvaluateMovementAndAim(card, reachableMoves);
    }

    private ScoredCard EvaluateMovementAndAim(CardScript card, List<(PathData,TargetingModeData)> reachableMoves)
    {
        ScoredCard best = new()
        {
            card = card,
            score = 0,
            movementCost = int.MaxValue
        };

        foreach (var move in reachableMoves)
        {
            if (move.Item2.targetedEntities == null || move.Item2.targetedEntities.Count == 0) continue;
            
            int score = EvaluateCardScore(card, move.Item2.targetedEntities, move.Item1.PathCost);

            if (score > best.score)
            {
                best.score = score;
                best.pseudoName = $"Play {card.cardData.cardName}";
                best.targets = move.Item2.targetedEntities;
                best.targetingModeData = move.Item2;
                best.movementPath = move.Item1.Path;
                best.movementCost = move.Item1.PathCost;
                best.executionOption = CardExecutionOption.PlayCard;
            }
        }
        Debug.Log($"[NpcAI] Best action for card {card.cardData.cardName} is score {best.score} with movement cost {best.movementCost}, at {best.targetingModeData.castingPosition} for NPC {npcScript.name}");
        return best;
    }
    #endregion

    #region Apply Actions
    private void ApplyActionToPlan(List<PlannedAction> plan, ScoredCard bestAction, ref Vector3Int virtualPosition, EntityScript entity)
    {
        if (bestAction == null) return;

        switch (bestAction.executionOption)
        {
            case CardExecutionOption.PlayCard:
                ApplyMoveActionToPlan(plan, bestAction, ref virtualPosition, entity);
                ApplyCardActionToPlan(plan, bestAction, entity);
                break;

            case CardExecutionOption.MoveOnly:
            case CardExecutionOption.Reposition:
            case CardExecutionOption.Flee:
                ApplyMoveActionToPlan(plan, bestAction, ref virtualPosition, entity);
                break;
        }
    }

    private void ApplyMoveActionToPlan(List<PlannedAction> plan, ScoredCard bestAction, ref Vector3Int virtualPosition, EntityScript entity)
    {
        if (bestAction?.movementPath == null || bestAction.movementPath.Count <= 0) return;

        plan.Add(new PlannedAction
        {
            Type = PlannedAction.ActionType.Move,
            Name =  $"Move_for_{bestAction.pseudoName}",
            TargetingModeData = bestAction.targetingModeData,
            Path = bestAction.movementPath
        });

        virtualPosition = bestAction.movementPath.Last();

        entity.entityStats.CurrentStamina -= bestAction.movementCost;
    }

    private void ApplyCardActionToPlan(List<PlannedAction> plan, ScoredCard bestAction, EntityScript entity)
    {
        if (bestAction?.card == null) return;

        plan.Add(new PlannedAction
        {
            Type = PlannedAction.ActionType.PlayCard,
            Name = bestAction.card.name,
            Card = bestAction.card,
            TargetingModeData = bestAction.targetingModeData,
            Path = bestAction.movementPath
        });

        entity.entityStats.CurrentStamina -= bestAction.card.cardData.Cost;
        hand.Remove(bestAction.card);
    }
    #endregion

    #region Helpers
    private ScoredCard SelectBestActionCandidate(List<ScoredCard> actionCandidates, int stamina)
    {
        if (actionCandidates == null || actionCandidates.Count == 0) return null;

        var ordered = actionCandidates.OrderByDescending(a => a.score).ToList();

        foreach (var candidate in ordered)
        {
            int totalCost = candidate.movementCost + (candidate.card?.cardData.Cost ?? 0);
            if (totalCost <= stamina) return candidate;
        }

        return null;
    }

    private int EvaluateCardScore(CardScript card, List<EntityScript> targets, int moveCost)
    {
        if (card == null || card.cardData == null) return 0;

        int throughput = card.cardData.CardAiBias?.ThroughputOverride(npcAIBias, card.cardData, targets) ?? 0;
        int cost = Mathf.Max(1, card.cardData.Cost + moveCost);

        return throughput / cost;
    }

    private List<CardScript> GetHandCards()
    {
        if (DeckManager.Instance == null || !DeckManager.Instance.DeckManagement.ContainsKey(npcScript)) return new();
        Transform dock = DeckManager.Instance.DeckManagement[npcScript];
        return dock.GetComponentsInChildren<CardScript>(true).Take(5).ToList();
    }
    #endregion

    #region Flee / Chase
    private ScoredCard TryGetRepositionCandidate(Vector3Int virtualPosition, List<EntityScript> allEntities)
    {
        if (npcScript.entityStats.IsRooted) { return null; }

        ScoredCard moveOption_Flee = null;
        switch (npcScript.npcAIController.npcAIBias.RepositionCondition)
        {
            case RepositionCondition.surrounded:
                if (TargetingUtility.GetEntitiesFromTiles(TilemapUtilityScript.GetTilesInRadius(virtualPosition, 1), allEntities).Where(e => e.entityAffiliation != npcScript.entityAffiliation).Count() > 3)
                {
                    moveOption_Flee = DetermineRepositionTarget(virtualPosition, allEntities, npcScript);
                    if (moveOption_Flee != null)
                    {
                        moveOption_Flee.score = 1;
                        moveOption_Flee.pseudoName = "Surrounded Flee";
                    }
                }
                break;
            case RepositionCondition.lowHealth:
                if (npcScript.entityStats.CurrentHealth < npcScript.entityStats.MaxHealth.Value() * 0.3f)
                {
                    moveOption_Flee = DetermineRepositionTarget(virtualPosition, allEntities, npcScript);
                    if (moveOption_Flee != null)
                    {
                        moveOption_Flee.score = 1;
                        moveOption_Flee.pseudoName = "Low Health Flee";
                    }
                }
                break;
            case RepositionCondition.preferRanged:
                var closeEnemies = allEntities
                    .Where(e => e.entityAffiliation != npcScript.entityAffiliation)
                    .Where(e => Vector3Int.Distance(e.GetComponent<EntityOnMap>().currentCell, virtualPosition) < 2)
                    .ToList();
                if (closeEnemies.Count > 0)
                {
                    moveOption_Flee = DetermineRepositionTarget(virtualPosition, allEntities, npcScript);
                    if (moveOption_Flee != null)
                    {
                        moveOption_Flee.score = 1;
                        moveOption_Flee.pseudoName = "Prefer-Range Flee";
                    }
                }
                break;
        }

        // Ensure returned object is non-null for caller convenience
        return moveOption_Flee;
    }

    private ScoredCard TryGetChaseCandidate(Vector3Int virtualPosition, List<EntityScript> allEntities)
    {
        if (npcScript.entityStats.IsRooted) { return null; }

        Debug.Log($"[NpcAI] Trying to find chase candidate for NPC {npcScript.name} at position {virtualPosition}");
        var enemies = allEntities.Where(e => e.entityAffiliation != npcScript.entityAffiliation).ToList();

        if (enemies == null || enemies.Count == 0)
        {
            Debug.Log($"[NpcAI] No enemies found for NPC {npcScript.name} to chase");
            return null;
        }

        var nearestEnemy = enemies
            .OrderBy(e => Vector3Int.Distance(e.GetComponent<EntityOnMap>().currentCell, virtualPosition))
            .FirstOrDefault();

        if (nearestEnemy == null)
        {
            Debug.Log($"[NpcAI] Nearest enemy is null for NPC {npcScript.name}");
            return null;
        }

        var targetCell = nearestEnemy.GetComponent<EntityOnMap>().currentCell;
        var pathData = MovementUtility.FindPath(virtualPosition, targetCell, walkClose:true, movementCostModifier: npcScript.entityStats.MovementCostModifier);

        if (pathData == null)
        {
            Debug.Log($"[NpcAI] Path to nearest enemy is null for NPC {npcScript.name}");
            return null;
        }

        Debug.Log($"[NpcAI] Path to nearest enemy for NPC {npcScript.name} has cost {pathData.PathCost}");
        if (pathData.PathCost <= npcScript.entityStats.CurrentStamina)
        {
            Debug.Log($"[NpcAI] Chase candidate found for NPC {npcScript.name} towards enemy at {targetCell}");
            return new ScoredCard
            {
                card = null,
                pseudoName = "Reposition",
                targetingModeData = new() { castingPosition = targetCell },
                movementPath = pathData.Path,
                movementCost = pathData.PathCost,
                score = 10 // scoring can be improved later
            };
        }

        Debug.Log($"[NpcAI] No valid chase move found for NPC {npcScript.name}");
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
                targetingModeData = new() {castingPosition = virtualPosition },
                movementPath = new List<Vector3Int> { virtualPosition },
                movementCost = 0,
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
            targetingModeData = new() {castingPosition = virtualPosition },
            movementPath = new List<Vector3Int> { virtualPosition },
            movementCost = 0,
            score = 0
        };
    }
    private ScoredCard EvaluateRepositionCandidates(Vector3Int virtualPosition, List<EntityScript> allEntities, EntityScript entity, int maxFleeDistance)
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
                foreach (var e in allEntities)
                {
                    if (e.entityAffiliation == entity.entityAffiliation)
                        continue;

                    float distToEnemy = Vector3Int.Distance(e.GetComponent<EntityOnMap>().currentCell, candidate);
                    if (distToEnemy < minEnemyDist)
                        minEnemyDist = distToEnemy;
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
                        movementPath = pathData.Path,
                        movementCost = moveCost,
                        score = Mathf.RoundToInt(score)
                    };
                }
            }
        }

        return bestTarget;
    }
    #endregion
}

#region Additonal Types
public class ScoredCard
{
    public CardScript card;
    public string pseudoName;
    public List<EntityScript> targets;
    public int score;
    public int movementCost;
    public List<Vector3Int> movementPath;
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

    public List<Vector3Int> Path;
}
    #endregion