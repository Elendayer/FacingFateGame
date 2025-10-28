using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility;

public class NpcAIController
{
    private readonly NonPlayerScript npc;
    private readonly EntityOnMap mover;
    private List<EntityScript> allEntities => Object.FindObjectsByType<EntityScript>(FindObjectsSortMode.None).ToList();
    public List<CardScript> hand = new();
    public NpcAIController(NonPlayerScript npc)
    {
        this.npc = npc;
        mover = npc.GetComponent<EntityOnMap>();
        Debug.Log($"[NpcAI] Initialized for {npc.name} at cell {mover.currentCell}");
    }

    public List<PlannedAction> BuildTurnPlan()
    {
        List<PlannedAction> plan = new();
        int stamina = npc.entityStats.CurrentStamina.Value;
        Vector3Int virtualPosition = mover.currentCell; // Simulated NPC position

        Debug.Log($"[NpcAI] {npc.name} starting turn. Stamina: {npc.entityStats.CurrentStamina.Value}, virtualPosition: {virtualPosition}");
        hand = GetHandCards();

        // Loop until stamina is too low to do anything
        while (npc.entityStats.CurrentStamina.Value > 0)
        {
            var actionCandidates = new List<ScoredCard>();

            // Evaluate hand cards
            actionCandidates.AddRange(EvaluateHandActions(hand, npc.entityStats.CurrentStamina.Value, virtualPosition));

            // Add flee option if applicable
            var fleeCandidate = TryGetFleeCandidate(virtualPosition, allEntities);
            if (fleeCandidate != null && fleeCandidate.Score > 0)
                actionCandidates.Add(fleeCandidate);

            if (actionCandidates.Count == 0)
            {
                // Add chase/reposition option if applicable
                var chaseCandidate = TryGetChaseCandidate(virtualPosition, allEntities);
                if (chaseCandidate != null && chaseCandidate.Score > 0)
                    actionCandidates.Add(chaseCandidate);
            }

            Debug.Log($"[NpcAI] Evaluated {actionCandidates.Count} action candidates.");
            var bestAction = SelectBestActionCandidate(actionCandidates, npc.entityStats.CurrentStamina.Value);
            if (bestAction == null || bestAction.Score <= 0)
            {
                Debug.Log("[NpcAI] No good actions found.");
                break;
            }

            int totalCost = (bestAction.MovementCost) + (bestAction.Card?.cardData.Cost ?? 0);
            if (totalCost > npc.entityStats.CurrentStamina.Value)
            {
                Debug.Log($"[NpcAI] Skipping best action: not enough stamina (need {totalCost}, have {npc.entityStats.CurrentStamina.Value})");
                break;
            }

            // Apply move portion (if any)
            ApplyMoveActionToPlan(plan, bestAction, ref virtualPosition, npc);

            // Apply card portion (if any)
            ApplyCardActionToPlan(plan, bestAction, npc);

            // If stamina is now too low to play any card or move, break
            bool canDoAnything = false;
            var nextHand = GetHandCards();
            foreach (var card in nextHand)
            {
                if (card.cardData != null && card.cardData.Cost <= npc.entityStats.CurrentStamina.Value)
                {
                    canDoAnything = true;
                    break;
                }
            }
            // Check if can move (flee/chase) with remaining stamina
            if (!canDoAnything)
            {
                var flee = TryGetFleeCandidate(virtualPosition, allEntities);
                var chase = TryGetChaseCandidate(virtualPosition, allEntities);
                if ((flee != null && flee.Score > 0 && (flee.MovementCost <= npc.entityStats.CurrentStamina.Value)) ||
                    (chase != null && chase.Score > 0 && (chase.MovementCost <= npc.entityStats.CurrentStamina.Value)))
                {
                    canDoAnything = true;
                }
            }
            if (!canDoAnything)
                break;
        }

        Debug.Log($"[NpcAI] Finished turn plan. Total actions: {plan.Count}");
        return plan;
    }

    private List<ScoredCard> EvaluateHandActions(List<CardScript> hand, int stamina, Vector3Int virtualPosition)
    {
        var actionCandidates = new List<ScoredCard>();
        foreach (var card in hand)
        {
            var scoredCard = EvaluateCardForTurn(card, stamina, virtualPosition);
            Debug.Log($"[NpcAI] {scoredCard.Targets.Count} Targets and {scoredCard.Score} Score");
            if (scoredCard.Score <= 0) continue;

            if (scoredCard != null && scoredCard.Score > 0)
            {
                actionCandidates.Add(scoredCard);
            }
        }
        return actionCandidates;
    }

    private ScoredCard TryGetFleeCandidate(Vector3Int virtualPosition, List<EntityScript> allEntities)
    {
        ScoredCard moveOption_Flee = null;
        switch (npc.npcAIBias.RepositionCondition)
        {
            case RepositionCondition.surrounded:
                if (TargetingUtility.GetEntitiesFromTiles(TilemapUtilityScript.GetTilesInRadius(virtualPosition, 1), allEntities).Count >= 3)
                {
                    moveOption_Flee = DetermineFleeTarget(virtualPosition, allEntities, npc);
                    if (moveOption_Flee != null)
                    {
                        moveOption_Flee.Score = 1;
                        moveOption_Flee.pseudoName = "Surrounded Flee";
                    }
                }
                break;
            case RepositionCondition.lowHealth:
                if (npc.entityStats.CurrentHealth.Value < npc.entityStats.MaxHealth.Value * 0.3f)
                {
                    moveOption_Flee = DetermineFleeTarget(virtualPosition, allEntities, npc);
                    if (moveOption_Flee != null)
                    {
                        moveOption_Flee.Score = 1;
                        moveOption_Flee.pseudoName = "Low Health Flee";
                    }
                }
                break;
            case RepositionCondition.preferRanged:
                var closeEnemies = allEntities
                    .Where(e => e.entityAffiliation != npc.entityAffiliation)
                    .Where(e => Vector3Int.Distance(e.GetComponent<EntityOnMap>().currentCell, virtualPosition) < 2)
                    .ToList();
                if (closeEnemies.Count > 0)
                {
                    moveOption_Flee = DetermineFleeTarget(virtualPosition, allEntities, npc);
                    if (moveOption_Flee != null)
                    {
                        moveOption_Flee.Score = 1;
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
        var enemies = allEntities.Where(e => e.entityAffiliation != npc.entityAffiliation).ToList();
        if (enemies == null || enemies.Count == 0)
            return null;

        var nearestEnemy = enemies
            .OrderBy(e => Vector3Int.Distance(e.GetComponent<EntityOnMap>().currentCell, virtualPosition))
            .FirstOrDefault();

        if (nearestEnemy == null)
            return null;

        var targetCell = nearestEnemy.GetComponent<EntityOnMap>().currentCell;
        var pathData = TilemapUtilityScript.FindPath(virtualPosition, targetCell);

        if (pathData == null)
        {
            Debug.Log($"[NpcAI] No path to nearest enemy {nearestEnemy.name} at {targetCell}");
            return null;
        }

        Debug.Log($"[NpcAI] Nearest enemy is {nearestEnemy.name} at {targetCell}, distance {pathData.Path.Count}");

        if (pathData.Path.Count > 1 && pathData.Path.Count <= npc.entityStats.CurrentStamina.Value)
        {
            return new ScoredCard
            {
                Card = null,
                pseudoName = "Reposition",
                TargetCell = targetCell,
                MovementPath = pathData.Path,
                MovementCost = pathData.PathCost,
                Score = 1 // scoring can be improved later
            };
        }

        return null;
    }

    private ScoredCard SelectBestActionCandidate(List<ScoredCard> actionCandidates, int stamina)
    {
        if (actionCandidates == null || actionCandidates.Count == 0)
            return null;

        // Order by score descending
        var ordered = actionCandidates.OrderByDescending(a => a.Score).ToList();

        foreach (var candidate in ordered)
        {
            int totalCost = (candidate.MovementCost) + (candidate.Card?.cardData.Cost ?? 0);
            if (totalCost <= stamina)
                return candidate;
        }

        return null;
    }

    private void ApplyMoveActionToPlan(List<PlannedAction> plan, ScoredCard bestAction,
                                       ref Vector3Int virtualPosition, EntityScript entity)
    {
        if (bestAction?.MovementPath != null && bestAction.MovementPath.Count > 1)
        {
            plan.Add(new PlannedAction
            {
                Type = PlannedAction.ActionType.Move,
                Name = bestAction.pseudoName + "_ Move",
                TargetCell = bestAction.TargetCell,
                Path = bestAction.MovementPath
            });
            Debug.Log($"[NpcAI] -> Move Name {bestAction.pseudoName}, MOVE to {bestAction.TargetCell} (cost {bestAction.MovementCost}) ");
            virtualPosition = bestAction.MovementPath.Last();

            // only subtract stamina if there's an actual card with a cost
            int staminaCost = bestAction.Card?.cardData?.Cost ?? bestAction.MovementCost;
            entity.entityStats.CurrentStamina.AddModifier(
                new StatModifier(-staminaCost, ModifierScaling.Flat, name: "BaseValue"),
                ModifierMergeStrategy.Merge);
        }
    }

    private void ApplyCardActionToPlan(List<PlannedAction> plan, ScoredCard bestAction, EntityScript entity)
    {
        if (bestAction?.Card != null)
        {
            plan.Add(new PlannedAction
            {
                Type = PlannedAction.ActionType.PlayCard,
                Name = bestAction.Card.name,
                Card = bestAction.Card,
                Targets = bestAction.Targets,
                TargetCell = bestAction.TargetCell,
                Path = bestAction.MovementPath
            });
            Debug.Log($"[NpcAI] -> PLAY {bestAction.Card.cardData.cardName} on {string.Join(", ", bestAction.Targets.Select(t => t.name))}");

            entity.entityStats.CurrentStamina.AddModifier(new StatModifier(-bestAction.Card.cardData.Cost, ModifierScaling.Flat, name: "BaseValue"), ModifierMergeStrategy.Merge);

            hand.Remove(bestAction.Card); // Assume card is played and removed from hand
        }
    }

    private ScoredCard DetermineFleeTarget(Vector3Int virtualPosition, List<EntityScript> allEntities, EntityScript entity)
    {
        // Count hostiles
        int hostileCount = allEntities.Count(e => e.entityAffiliation != entity.entityAffiliation);
        if (hostileCount == 0)
        {
            // No enemies — stay in place
            return new ScoredCard
            {
                TargetCell = virtualPosition,
                pseudoName = "No Enemies Move",
                MovementPath = new List<Vector3Int> { virtualPosition },
                MovementCost = 0,
                Score = 0
            };
        }

        // Cap flee distance by number of hostiles (but not above stamina)
        int maxFleeDistance = Mathf.Min(entity.entityStats.CurrentStamina.Value, hostileCount);

        // Delegate the search and scoring
        ScoredCard bestTarget = EvaluateFleeCandidates(virtualPosition, allEntities, entity, maxFleeDistance);

        // Fallback if no valid move found
        return bestTarget ?? new ScoredCard
        {
            pseudoName = "No Move",
            TargetCell = virtualPosition,
            MovementPath = new List<Vector3Int> { virtualPosition },
            MovementCost = 0,
            Score = 0
        };
    }
    private ScoredCard EvaluateFleeCandidates(Vector3Int virtualPosition, List<EntityScript> allEntities, EntityScript entity, int maxFleeDistance)
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
                var pathData = TilemapUtilityScript.FindPath(virtualPosition, candidate);
                if (pathData == null || pathData.Path == null || pathData.Path.Count == 0)
                    continue;

                int moveCost = pathData.PathCost;
                if (moveCost == 0 || moveCost > entity.entityStats.CurrentStamina.Value)
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
                        TargetCell = candidate,
                        MovementPath = pathData.Path,
                        MovementCost = moveCost,
                        Score = Mathf.RoundToInt(score)
                    };
                }
            }
        }

        return bestTarget;
    }

    // --- Hand retrieval ---
    private List<CardScript> GetHandCards()
    {
        if (DeckManager.Instance == null) return new List<CardScript>();
        if (!DeckManager.Instance.DeckManagement.ContainsKey(npc)) return new List<CardScript>();

        Transform dock = DeckManager.Instance.DeckManagement[npc];
        return dock.GetComponentsInChildren<CardScript>(true).Take(5).ToList();
    }

    // --- Card evaluation for the current turn ---
    private ScoredCard EvaluateCardForTurn(CardScript card, int availableStamina, Vector3Int fromPosition)
    {
        Debug.Log($"[NpcAI] Evaluating {card.cardData.cardName} from {fromPosition}");

        var potentialTargets = TargetingUtility.GetValidTargets(card, npc, allEntities);

        if (potentialTargets.Count == 0)
        {
            Debug.Log($"[NpcAI] No valid targets for {card.cardData.cardName}");
            return new ScoredCard { Card = card, Score = 0, pseudoName = card.cardData.cardName };
        }

        // Step 1: Get reachable candidate positions
        var candidatePositions = GetReachablePositions(card, potentialTargets, availableStamina, fromPosition);

        if (candidatePositions.Count == 0)
        {
            Debug.Log($"[NpcAI] No reachable positions for {card.cardData.cardName}");
            return new ScoredCard { Card = card, Score = 0, pseudoName = card.cardData.cardName };
        }

        // Step 2: Evaluate score from positions
        return GetBestPositionScore(card, candidatePositions, allEntities);
    }

    private List<PathData> GetReachablePositions(CardScript card, List<EntityScript> targets, int stamina, Vector3Int currentCell)
    {
        Debug.Log($"[NpcAI] GetReachablePositions START card={(card?.cardData?.cardName ?? "null")} stamina={stamina} from={currentCell} targets={(targets?.Count ?? 0)}");

        var candidatePathdata = new List<PathData>();

        if (card == null || card.cardData == null)
        {
            Debug.LogWarning("[NpcAI] GetReachablePositions: card or card.cardData is null.");
            return candidatePathdata;
        }

        // Determine area radius from card targeting data
        int areaRadius = card.cardData?.Area ?? 0;

        // Collect frequency counts for tiles that are within areaRadius of each target
        var tileFrequency = new Dictionary<Vector3Int, int>();

        if (areaRadius > 1)
        {
            // Early exit when no targets
            if (targets == null || targets.Count == 0)
            {
                Debug.Log("[NpcAI] No targets provided to GetReachablePositions.");
                return candidatePathdata;
            }

            // For each target, gather tiles in radius and increment frequency
            for (int tIndex = 0; tIndex < targets.Count; tIndex++)
            {
                var target = targets[tIndex];
                Vector3Int targetCell = target.GetComponent<EntityOnMap>().currentCell;

                List<Vector3Int> tilesInRange;

                // If areaRadius <= 0, still consider the exact target cell
                if (areaRadius <= 0)
                {
                    tilesInRange = new() { targetCell };
                }
                else
                {
                    // Use existing utility to get tiles in radius; fall back to single tile if util returns null
                    try
                    {
                        tilesInRange = TilemapUtilityScript.GetTilesInRadius(targetCell, areaRadius) ?? new() { targetCell };
                    }
                    catch
                    {
                        tilesInRange = new() { targetCell };
                    }
                }

                foreach (var tile in tilesInRange)
                {
                    // Only consider tiles that exist on base map
                    if (!TilemapUtilityScript.BaseTilemap.HasTile(tile))
                        continue;

                    if (tileFrequency.TryGetValue(tile, out int existing))
                        tileFrequency[tile] = existing + 1;
                    else
                        tileFrequency[tile] = 1;
                }
            }
        }
        else
        {
            foreach (var t in targets)
            {
                tileFrequency.Add(t.GetComponent<EntityOnMap>().currentCell, 10);
            }
        }

        int cardCost = card?.cardData?.Cost ?? 0;
        Debug.Log(cardCost);

        // For each unique candidate tile, compute path and determine affected targets and keep metrics
        var entries = new List<(Vector3Int Tile, PathData PathData, int Frequency, float Distance)>();

        foreach (var kvp in tileFrequency)
        {
            var tile = kvp.Key;
            int frequency = kvp.Value;

            // Pathfinding from currentCell to candidate tile
            PathData pathData = new();
            if (card.cardData.Range == 1)
            {
                pathData = TilemapUtilityScript.FindPath(currentCell, tile, walkClose: true);
            }
            else
            {
                pathData = TilemapUtilityScript.FindPath(currentCell, tile);
            }
            if (pathData == null)
            {
                Debug.Log($"[NpcAI] No path to tile {tile}");
                continue;
            }

            int totalCost = pathData.PathCost + cardCost;
            Debug.Log($"{pathData.PathCost} : {cardCost} ");

            if (totalCost > stamina)
            {
                Debug.Log("[NpcAI] Path cost exceeds available stamina");
                continue;
            }

            // Determine which targets would be affected from this tile position
            var affectedTargets = TargetingUtility.GetEntitiesFromPosition(card, tile, allEntities, npc);
            if (affectedTargets == null || affectedTargets.Count == 0)
            {
                Debug.Log($"[NpcAI] No affected targets from tile {tile}");
                continue;
            }

            float distance = Vector3Int.Distance(currentCell, tile);

            // Build PathData record (preserve Path list)
            var pd = new PathData
            {
                Start = currentCell,
                End = tile,
                Path = pathData.Path,
                PathCost = pathData.PathCost
            };

            entries.Add((Tile: tile, PathData: pd, Frequency: frequency, Distance: distance));
        }

        if (entries.Count == 0)
        {
            Debug.Log("[NpcAI] No valid path entries found.");
            return candidatePathdata;
        }

        int maxFrequency = entries.Max(e => e.Frequency);

        // Filter entries within 1 of max frequency
        var ordered = entries
            .Where(e => e.Frequency >= maxFrequency - 1)
            .OrderByDescending(e => e.Frequency)      // frequency desc
            .ThenBy(e => e.PathData.PathCost)         // path cost asc
            .ThenBy(e => e.Distance)                  // distance asc
            .ToList();

        // Convert ordered entries to PathData list, ensuring unique tile entries (first one wins)
        var seenTiles = new HashSet<Vector3Int>();
        foreach (var e in ordered)
        {
            if (seenTiles.Add(e.Tile))
            {
                candidatePathdata.Add(e.PathData);
            }
        }

        Debug.Log($"[NpcAI] GetReachablePositions END -> candidatePathdata count = {candidatePathdata.Count}");
        return candidatePathdata;
    }

    private ScoredCard GetBestPositionScore(CardScript card, List<PathData> candidatePositions, List<EntityScript> allEntities)
    {
        Vector3Int virtualPosition = mover.currentCell;
        int availableStamina = npc.entityStats.CurrentStamina.Value;
        int cardCost = card?.cardData?.Cost ?? 0;

        var orderedCandidates = candidatePositions;

        ScoredCard bestScoredCard = new ScoredCard
        {
            Card = card,
            pseudoName = card.cardData.cardName,
            Targets = new List<EntityScript>(),
            TargetCell = Vector3Int.zero,
            MovementPath = null,
            MovementCost = int.MaxValue,
            Score = 0
        };

        foreach (var candidate in orderedCandidates)
        {
            Vector3Int npcPosition = candidate.End;
            var affectedTargets = TargetingUtility.GetEntitiesFromPosition(card, npcPosition, allEntities, npc);

            if (affectedTargets.Count == 0) continue;
            if (card.cardData.targetingData.SelectionType == CardTargetSelection.Single)
                affectedTargets = new List<EntityScript> { affectedTargets.First() };

            int score = EvaluateCardScore(card, affectedTargets, candidate.PathCost);

            if (score > bestScoredCard.Score ||
                (score == bestScoredCard.Score && candidate.PathCost < bestScoredCard.MovementCost + cardCost))
            {
                bestScoredCard.Card = card;
                bestScoredCard.Targets = affectedTargets;
                bestScoredCard.TargetCell = npcPosition;
                bestScoredCard.MovementPath = candidate.Path;
                bestScoredCard.MovementCost = candidate.PathCost;
                bestScoredCard.Score = score;

                Debug.Log($"[NpcAI] New best for {card.cardData.cardName}: score={score}, moveCost={candidate.PathCost}, targets=[{string.Join(", ", affectedTargets.Select(t => t.name))}] at {npcPosition}");
            }
        }

        return bestScoredCard;
    }

    // --- Card scoring ---
    private int EvaluateCardScore(CardScript card, List<EntityScript> targets, int moveCost)
    {
        if (card == null || card.cardData == null)
        {
            Debug.LogWarning("[NpcAI] EvaluateCardScore: card or card.cardData is null.");
            return 0;
        }
        if (card.cardData.CardAiBias == null)
        {
            Debug.LogWarning($"[NpcAI] EvaluateCardScore: CardAiBias is null for card {card.cardData.cardName}.");
            return 0;
        }
        if (targets == null)
        {
            Debug.LogWarning("[NpcAI] EvaluateCardScore: targets is null.");
            return 0;
        }

        List<EntityScript> vettedEntities = TargetingUtility.GetValidTargets(card, card.cardData.Owner ,targets) ?? new List<EntityScript>();

        int cardPower = card.cardData.Power + card.cardData.Damage + card.cardData.Healing;
        int repeats = Mathf.Max(1, card.cardData.Repeats);

        int throughput = (cardPower * repeats) * vettedEntities.Count;
        int cost = Mathf.Max(1, card.cardData.Cost + moveCost);
        int efficiency = throughput / cost;

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