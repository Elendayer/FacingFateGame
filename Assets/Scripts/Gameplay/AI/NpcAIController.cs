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
        Vector3Int virtualPosition = mover.currentCell; // Simulated NPC position

        Debug.Log($"[NpcAI] {npc.name} starting turn. Stamina: {stamina}, virtualPosition: {virtualPosition}");

        switch (npc.npcAIBias.fleeCondition)
        { 
            case FleeCondition.none: break;
            case FleeCondition.surrounded:
                if (TargetingUtility.GetEntitiesFromTiles(TilemapUtilityScript.GetTilesInRadius(virtualPosition, 1), allEntities).Count >= 3)
                {
                    ScoredCard FakeFleeCard = DetermineFleeTarget(virtualPosition, allEntities, stamina);
                    plan.Add(new PlannedAction
                    {
                        Type = PlannedAction.ActionType.Move,
                        TargetCell = FakeFleeCard.TargetCell,
                    });
                    Debug.Log($"[NpcAI] -> MOVE to {FakeFleeCard.TargetCell} (cost {FakeFleeCard.MovementCost})");
                    virtualPosition = FakeFleeCard.MovementPath.Last();
                    stamina -= FakeFleeCard.MovementCost; 
                }
                break;
        }

        var hand = GetHandCards();
        if (hand.Count == 0) return plan;

        foreach (var card in hand)
        {
            var scoredCard = EvaluateCardForTurn(card, stamina, virtualPosition);
            Debug.Log($"{scoredCard.Targets.Count} Targets and {scoredCard.Score} Score");
            if (scoredCard.Score <= 0) continue;

            int totalCost = scoredCard.MovementCost + scoredCard.Card.cardData.Cost;
            if (totalCost > stamina)
            {
                Debug.Log($"[NpcAI] Skipping {card.cardData.cardName}: not enough stamina (need {totalCost}, have {stamina})");
                continue;
            }

            // Add movement
            if (scoredCard.MovementPath?.Count > 1)
            {
                plan.Add(new PlannedAction
                {
                    Type = PlannedAction.ActionType.Move,
                    TargetCell = scoredCard.MovementPath.Last()
                });
                Debug.Log($"[NpcAI] -> MOVE to {scoredCard.TargetCell} (cost {scoredCard.MovementCost})");
                virtualPosition = scoredCard.MovementPath.Last();
                stamina -= scoredCard.MovementCost;
            }

            // Add card play
            plan.Add(new PlannedAction
            {
                Type = PlannedAction.ActionType.PlayCard,
                Card = scoredCard.Card,
                Targets = scoredCard.Targets,
                TargetCell = scoredCard.TargetCell
            });
            Debug.Log($"[NpcAI] -> PLAY {card.cardData.cardName} on {string.Join(", ", scoredCard.Targets.Select(t => t.name))}");
            stamina -= scoredCard.Card.cardData.Cost;

            Debug.Log($"[NpcAI] Remaining stamina: {stamina}, virtualPosition: {virtualPosition}");
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
                var path = GetPathToTile(virtualPosition, candidate);
                if (path == null) continue;

                int moveCost = path.Count;
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
                        MovementPath = path,
                        MovementCost = moveCost,
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
    private List<(Vector3Int tile, List<Vector3Int> path)> GetReachablePositions(CardScript card, List<EntityScript> targets, int stamina, Vector3Int fromPosition)
    {
        List<(Vector3Int tile, List<Vector3Int> path)> candidatePositions = new();

        Vector3Int? triangulatedTile = null;
        if (card.cardData.targetingData.CardTargetType == CardTargetType.CombatTile && targets.Count > 0)
            triangulatedTile = ComputeTriangulatedTile(targets);

        int maxAttemptsWithoutImprovement = 5;

        foreach (var target in targets)
        {
            Vector3Int targetCell = target.GetComponent<EntityOnMap>().currentCell;
            var candidateTiles = TargetingUtility.GetCandidateTilesForTarget(card, targetCell)
                .OrderBy(tile => Vector3Int.Distance(fromPosition, tile))
                .ToList();

            if (triangulatedTile.HasValue && !candidateTiles.Contains(triangulatedTile.Value))
                candidateTiles.Add(triangulatedTile.Value);

            int attemptsWithoutImprovement = 0;

            foreach (var tile in candidateTiles)
            {
                var path = GetPathToTile(fromPosition, tile);
                if (path == null || path.Count > stamina) continue;

                // Use the last tile as NPC's position to check for targets
                var finalTile = path.Last();
                var affectedTargets = TargetingUtility.GetTargetsFromPosition(card, finalTile, allEntities, npc);

                candidatePositions.Add((tile, path));
                attemptsWithoutImprovement++;

                if (attemptsWithoutImprovement >= maxAttemptsWithoutImprovement)
                    break;
            }
        }

        return candidatePositions;
    }

    // --- Step 2: Score candidate positions ---
    private ScoredCard GetBestPositionScore(CardScript card, List<(Vector3Int tile, List<Vector3Int> path)> candidatePositions, List<EntityScript> allEntities)
    {
        int bestScore = 0;
        List<EntityScript> bestTargets = new();
        List<Vector3Int> bestPath = null;
        Vector3Int bestTile = Vector3Int.zero;
        int bestMoveCost = int.MaxValue;

        foreach (var candidate in candidatePositions)
        {
            var path = candidate.path;
            if (path == null || path.Count == 0) continue;

            Vector3Int npcPosition = path.Last(); // use where NPC will stand
            var affectedTargets = TargetingUtility.GetTargetsFromPosition(card, npcPosition, allEntities, npc);

            Debug.Log($"[NpcAI] Candidate tile {npcPosition} affects {affectedTargets.Count} targets");

            if (affectedTargets.Count == 0) continue;
            if (card.cardData.targetingData.areaType == CardTargetArea.Single)
                affectedTargets = new List<EntityScript> { affectedTargets.First() };

            int score = EvaluateCardScore(card, affectedTargets);

            if (score > bestScore || (score == bestScore && path.Count < bestMoveCost))
            {
                bestScore = score;
                bestTargets = affectedTargets;
                bestPath = path;
                bestTile = npcPosition;
                bestMoveCost = path.Count;

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

    // --- Pathfinding ---
    private List<Vector3Int> GetPathToTile(Vector3Int fromPosition, Vector3Int tile)
    {
        return TilemapUtilityScript.FindPath(fromPosition, tile);
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
    public ActionType Type;
    public CardScript Card;
    public List<EntityScript> Targets;
    public Vector3Int TargetCell; // used if Type == Move or aim point for card
}
