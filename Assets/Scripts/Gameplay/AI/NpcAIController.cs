using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility;

public class NpcAIController
{
    private readonly NonPlayerScript npc;
    private readonly EntityOnMap mover;
    private List<EntityScript> allEntities => Object.FindObjectsByType<EntityScript>(FindObjectsSortMode.None).ToList();

    public NpcAIController(NonPlayerScript npc)
    {
        this.npc = npc;
        mover = npc.GetComponent<EntityOnMap>();
        Debug.Log($"[NpcAI] Initialized for {npc.name} at cell {mover.currentCell}");
    }

    public List<PlannedAction> BuildTurnPlan()
    {
        List<PlannedAction> plan = new();
        int stamina = npc.CurrentStamina.Value;
        Vector3Int virtualPosition = mover.currentCell;

        Debug.Log($"[NpcAI] {npc.name} starting turn. Stamina: {stamina}, virtualPosition: {virtualPosition}");

        var hand = GetHandCards();
        var actionCandidates = new List<ScoredCard>();

        // Evaluate all cards in hand
        foreach (var card in hand)
        {
            var scoredCard = EvaluateCardForTurn(card, stamina, virtualPosition);
            if (scoredCard.Score > 0)
            {
                scoredCard.pseudoName = "Action";
                actionCandidates.Add(scoredCard);
            }
        }

        // Add movement as a "pseudo-card" option
        ScoredCard moveOption = new();
        switch (npc.npcAIBias.ReposiitionCondition)
        {
            case FleeCondition.surrounded:
                if (TargetingUtility.GetEntitiesFromTiles(TilemapUtilityScript.GetTilesInRadius(virtualPosition, 1), allEntities).Count >= 3)
                    moveOption = DetermineFleeTarget(virtualPosition, allEntities, stamina);
                moveOption.pseudoName = "Surrounded Flee";
                break;
            case FleeCondition.lowHealth:
                if (npc.CurrentHealth.Value < npc.MaxHealth.Value * 0.3f)
                    moveOption = DetermineFleeTarget(virtualPosition, allEntities, stamina);
                moveOption.pseudoName = "Low Health Flee";

                break;
            case FleeCondition.preferRanged:
                var closeEnemies = allEntities
                    .Where(e => e.entityAffiliation != npc.entityAffiliation)
                    .Where(e => Vector3Int.Distance(e.GetComponent<EntityOnMap>().currentCell, virtualPosition) < 2)
                    .ToList();
                if (closeEnemies.Count > 0)
                    moveOption = DetermineFleeTarget(virtualPosition, allEntities, stamina);
                moveOption.pseudoName = "Prefer-Range Flee";

                break;
        }
                // Default: Move closer to the nearest enemy
                var enemies = allEntities.Where(e => e.entityAffiliation != npc.entityAffiliation).ToList();
        if (enemies.Count > 0)
        {
            var nearest = enemies.OrderBy(e => Vector3Int.Distance(e.GetComponent<EntityOnMap>().currentCell, virtualPosition)).First();
            var targetCell = nearest.GetComponent<EntityOnMap>().currentCell;
            var PathData = TilemapUtilityScript.FindPath(virtualPosition, targetCell);

            if (PathData != null && PathData.Path.Count > 1 && PathData.Path.Count <= stamina)
            {
                moveOption = new ScoredCard
                {
                    Card = null,
                    pseudoName = "Reposition",
                    Targets = new List<EntityScript> { nearest },
                    TargetCell = PathData.Path[1],
                    MovementPath = PathData.Path.Take(2).ToList(),
                    MovementCost = PathData.PathCost,
                    Score = 1 // You can improve this scoring logic as needed
                };
            }
        }
        if (moveOption != null && moveOption.Score > 0)
            actionCandidates.Add(moveOption);

        // Pick the best action
        var bestAction = actionCandidates.OrderByDescending(a => a.Score).FirstOrDefault();
        if (bestAction == null || bestAction.Score <= 0)
        {
            Debug.Log("[NpcAI] No good actions found.");
            return plan;
        }

        int totalCost = (bestAction.MovementCost) + (bestAction.Card?.cardData.Cost ?? 0);
        if (totalCost > stamina)
        {
            Debug.Log($"[NpcAI] Skipping best action: not enough stamina (need {totalCost}, have {stamina})");
            return plan;
        }

        // If the best action is a move, add it
        if (bestAction.MovementPath?.Count > 1)
        {
            plan.Add(new PlannedAction
            {
                Type = PlannedAction.ActionType.Move,
                Name = bestAction.pseudoName,
                TargetCell = bestAction.MovementPath.Last(),
                Path = bestAction.MovementPath
            });
            Debug.Log($"[NpcAI] -> {bestAction.pseudoName} MOVE to {bestAction.TargetCell} (cost {bestAction.MovementCost}) ");
            virtualPosition = bestAction.MovementPath.Last();
            stamina -= bestAction.MovementCost;
        }

        // If the best action is a card, play it
        if (bestAction.Card != null)
        {
            plan.Add(new PlannedAction
            {
                Type = PlannedAction.ActionType.PlayCard,
                Name = bestAction.Card.name,
                Card = bestAction.Card,
                Targets = bestAction.Targets,
                TargetCell = bestAction.TargetCell
            });
            Debug.Log($"[NpcAI] -> PLAY {bestAction.Card.cardData.cardName} on {string.Join(", ", bestAction.Targets.Select(t => t.name))}");
            stamina -= bestAction.Card.cardData.Cost;
        }

        Debug.Log($"[NpcAI] Finished turn plan. Total actions: {plan.Count}");
        return plan;
    }

    private ScoredCard DetermineFleeTarget(Vector3Int virtualPosition, List<EntityScript> allEntities, int stamina)
    {
        ScoredCard bestTarget = null;
        float bestScore = float.MinValue;

        // Count how many hostiles we want to flee from
        int hostileCount = allEntities.Count(e => e.entityAffiliation != npc.entityAffiliation);
        if (hostileCount == 0)
        {
            // no enemies, stay in place
            return new ScoredCard()
            {
                TargetCell = virtualPosition,
                MovementPath = new List<Vector3Int>() { virtualPosition },
                MovementCost = 0,
                Score = 0
            };
        }

        // Cap flee distance by number of hostiles (but not above stamina)
        int maxFleeDistance = Mathf.Min(stamina, hostileCount);

        // Generate candidate positions within flee range
        for (int dx = -maxFleeDistance; dx <= maxFleeDistance; dx++)
        {
            for (int dy = -maxFleeDistance; dy <= maxFleeDistance; dy++)
            {
                Vector3Int candidate = new Vector3Int(
                    virtualPosition.x + dx,
                    virtualPosition.y + dy,
                    virtualPosition.z
                );

                // Skip current position
                if (candidate == virtualPosition)
                    continue;

                // Skip if tile does not exist on basemap
                if (!TilemapUtilityScript.BaseTilemap.HasTile(candidate))
                    continue;

                // Pathfinding
                var pathData = TilemapUtilityScript.FindPath(virtualPosition, candidate);
                if (pathData == null) continue;

                int moveCost = pathData.PathCost;
                if (moveCost > stamina || moveCost == 0) continue;

                // Evaluate candidate against hostiles
                float minEnemyDist = float.MaxValue;
                foreach (var entity in allEntities)
                {
                    if (entity.entityAffiliation != npc.entityAffiliation)
                    {
                        float distToEnemy = Vector3Int.Distance(
                            entity.GetComponent<EntityOnMap>().currentCell,
                            candidate
                        );
                        if (distToEnemy < minEnemyDist)
                            minEnemyDist = distToEnemy;
                    }
                }

                // Score system:
                // - Prefer being farther from enemies
                // - Slightly prefer closer paths
                float score = (minEnemyDist * 2f) - moveCost;

                // Penalize if too close
                if (minEnemyDist < 2)
                    score -= 100f;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = new ScoredCard()
                    {
                        TargetCell = candidate,
                        MovementPath = pathData.Path,
                        MovementCost = pathData.PathCost,
                        Score = Mathf.RoundToInt(score)
                    };
                }
            }
        }

        // Fallback: stay in place
        return bestTarget ?? new ScoredCard()
        {
            TargetCell = virtualPosition,
            MovementPath = new List<Vector3Int>() { virtualPosition },
            MovementCost = 0,
            Score = 0
        };
    }

    // --- Hand retrieval ---
    private List<CardScript> GetHandCards()
    {
        if (DeckManager.Instance == null) return new List<CardScript>();
        if (!DeckManager.Instance.DeckManagement.ContainsKey(npc)) return new List<CardScript>();

        Transform dock = DeckManager.Instance.DeckManagement[npc];
        return dock.GetComponentsInChildren<CardScript>(true).ToList();
    }

    // --- Card evaluation for the current turn ---
    private ScoredCard EvaluateCardForTurn(CardScript card, int availableStamina, Vector3Int fromPosition)
    {
        Debug.Log($"[NpcAI] Evaluating {card.cardData.cardName} from {fromPosition}");

        var potentialTargets = TargetingUtility.GetValidTargets(card, npc, allEntities);

        if (potentialTargets.Count == 0)
        {
            Debug.Log($"[NpcAI] No valid targets for {card.cardData.cardName}");
            return new ScoredCard { Card = card, Score = 0 };
        }

        // Step 1: Get reachable candidate positions
        var candidatePositions = GetReachablePositions(card, potentialTargets, availableStamina, fromPosition);

        // Step 2: Evaluate score from positions
        return GetBestPositionScore(card, candidatePositions, allEntities);
    }

    // --- Step 1: Compute reachable tiles for all targets ---
    private List<PathData> GetReachablePositions(CardScript card, List<EntityScript> targets, int stamina, Vector3Int currentCell)
    {
        List<PathData> candidatePathdata = new();

        Vector3Int? triangulatedTile = null;
        if (card.cardData.targetingData.CardTargetType == CardTargetType.CombatTile && targets.Count > 0)
            triangulatedTile = ComputeTriangulatedTile(targets);

        int maxAttemptsWithoutImprovement = 6;

        foreach (var target in targets)
        {
            Vector3Int targetCell = target.GetComponent<EntityOnMap>().currentCell;
            var candidateTiles = TargetingUtility.GetCandidateTilesForTarget(card, targetCell)
                .OrderBy(tile => Vector3Int.Distance(currentCell, tile))
                .ToList();

            if (triangulatedTile.HasValue && !candidateTiles.Contains(triangulatedTile.Value))
                candidateTiles.Add(triangulatedTile.Value);

            int attemptsWithoutImprovement = 0;

            foreach (var tile in candidateTiles)
            {
                var path = TilemapUtilityScript.FindPath(currentCell, tile)?.Path;
                if (path == null || path.Count > stamina) continue;

                // Use the last tile as NPC's position to check for targets
                var finalTile = path.Last();
                var affectedTargets = TargetingUtility.GetTargetsFromPosition(card, finalTile, allEntities, npc);

                candidatePathdata.Add(new() 
                {
                    Start= currentCell,
                    End= tile,
                    Path= path,
                    PathCost= path.Count,
                });

                attemptsWithoutImprovement++;

                if (attemptsWithoutImprovement >= maxAttemptsWithoutImprovement)
                    break;
            }
        }

        return candidatePathdata;
    }

    // --- Step 2: Score candidate positions ---
    private ScoredCard GetBestPositionScore(CardScript card, List<PathData> candidatePositions, List<EntityScript> allEntities)
    {
        int bestScore = 0;
        List<EntityScript> bestTargets = new();
        List<Vector3Int> bestPath = null;
        Vector3Int bestTile = Vector3Int.zero;
        int bestMoveCost = int.MaxValue;

        foreach (var pathData in candidatePositions)
        {
            if (pathData == null || pathData.Path == null || pathData.Path.Count == 0) continue;

            Vector3Int npcPosition = pathData.End; // use where NPC will stand
            var affectedTargets = TargetingUtility.GetTargetsFromPosition(card, npcPosition, allEntities, npc);

            Debug.Log($"[NpcAI] Candidate tile {npcPosition} affects {affectedTargets.Count} targets");

            if (affectedTargets.Count == 0) continue;
            if (card.cardData.targetingData.areaType == CardTargetArea.Single)
                affectedTargets = new List<EntityScript> { affectedTargets.First() };

            int score = EvaluateCardScore(card, affectedTargets);

            if (score > bestScore || (score == bestScore && pathData.PathCost < bestMoveCost))
            {
                bestScore = score;
                bestTargets = affectedTargets;
                bestPath = pathData.Path;
                bestTile = npcPosition;
                bestMoveCost = pathData.PathCost;

                Debug.Log($"[NpcAI] New best option: score {bestScore}, moveCost {bestMoveCost}, targets {string.Join(", ", bestTargets.Select(t => t.name))}");
            }
        }

        return new ScoredCard
        {
            Card = card,
            Targets = bestTargets,
            TargetCell = bestTile,
            MovementPath = bestPath,
            MovementCost = bestMoveCost,
            Score = bestScore
        };
    }


    // --- Compute triangulated position for clustered targets ---
    private Vector3Int ComputeTriangulatedTile(List<EntityScript> targets)
    {
        if (targets.Count == 0) return Vector3Int.zero;

        Vector3 center = Vector3.zero;
        float totalWeight = 0f;

        for (int i = 0; i < targets.Count; i++)
        {
            Vector3 posI = targets[i].GetComponent<EntityOnMap>().currentCell;
            float weight = 1f;

            for (int j = 0; j < targets.Count; j++)
            {
                if (i == j) continue;
                Vector3 posJ = targets[j].GetComponent<EntityOnMap>().currentCell;
                float dist = Vector3.Distance(posI, posJ);
                weight *= 1f / (1f + dist); // penalize far apart targets
            }

            center += posI * weight;
            totalWeight += weight;
        }

        center /= Mathf.Max(1f, totalWeight);
        return Vector3Int.RoundToInt(center);
    }

    // --- Card scoring ---
    private int EvaluateCardScore(CardScript card, List<EntityScript> targets)
    {
        List<EntityScript> vettedEntities = TargetingUtility.VetTargetsEntities(card, npc, targets);

        // --- Base throughput (power * repeats) ---
        int throughput = (card.cardData.Power * Mathf.Max(1, card.cardData.Repeats))* vettedEntities.Count;

        // --- Apply CardAiBias throughput overrides ---
        if (card.cardData.CardAiBias != null)
        {
            throughput += card.cardData.CardAiBias.ThroughputOverride(throughput, card.cardData.GameplayReferences);
        }

        // --- Apply NpcAiBias directly to throughput ---
        if (npc.npcAIBias != null)
        {
            throughput += npc.npcAIBias.BiasCalc(card.cardData);
        }

        // --- Efficiency: biased throughput / cost ---
        int efficiency = throughput / Mathf.Max(1, card.cardData.Cost);

        return efficiency;
    }

    // --- Helper class for internal evaluation ---
    private class ScoredCard
    {
        public CardScript Card;
        public string pseudoName;
        public List<EntityScript> Targets;
        public int Score;
        public int MovementCost;
        public List<Vector3Int> MovementPath;
        public Vector3Int TargetCell; // Tile to move to / aim at
    }
}

[System.Serializable]
public class PlannedAction
{
    public enum ActionType { Move, PlayCard }
    public string Name;
    public ActionType Type;
    public CardScript Card;
    public List<EntityScript> Targets;
    public Vector3Int TargetCell; // used if Type == Move or aim point for card
    public List<Vector3Int> Path;
}
