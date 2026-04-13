using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using facingfate;

namespace Utility
{
    public static class TargetingUtility
    {

        #region Entity Validation
        public static List<EntityScript> GetValidTargets(CardData card)
        {
            var candidates = AllEntitiesCache();

            if (card == null || card.Owner == null || candidates == null) return new List<EntityScript>();
    
            var owner = card.Owner;
            var results = new List<EntityScript>(candidates.Count);
            for (int i = 0; i < candidates.Count; i++)
            {
                var target = candidates[i];
                if (target != null && IsTargetValid(card, target))
                {
                    results.Add(target);
                }
            }
            return results;
        }
        public static bool IsTargetValid(CardData cardData, EntityScript target)
        {
            if (target == null) return false;
            if (target.enabled == false) return false;

            EntityScript owner = cardData.Owner;

            var targeting = cardData.targetingData;
            var targetAff = target.entityAffiliation;
            var ownerAff = owner.entityAffiliation;

            if (target.HasReference(GameplayRef.untargetableByAll).found) return false;

            bool baseValid = targeting.CardTargetAffiliation switch
            {
                CardTargetAffiliation.Ally => targetAff == ownerAff,
                CardTargetAffiliation.Enemy => targetAff != ownerAff,
                CardTargetAffiliation.Self => target == owner,
                CardTargetAffiliation.All => true,
                CardTargetAffiliation.AllyNeutral => targetAff == ownerAff || targetAff == EntityAffiliation.Neutral,
                CardTargetAffiliation.EnemyNeutral => targetAff != ownerAff || targetAff == EntityAffiliation.Neutral,
                CardTargetAffiliation.AllyEnemy => targetAff != EntityAffiliation.Neutral,
                _ => false
            };

            if (!baseValid) return false;

            bool isEnemyTarget = targetAff != ownerAff && targetAff != EntityAffiliation.Neutral;
            bool isAllyTarget = targetAff == ownerAff && targetAff != EntityAffiliation.Neutral;

            if (isEnemyTarget && target.HasReference(GameplayRef.untargetableByEnemies).found) return false;

            if (isAllyTarget && target.HasReference(GameplayRef.untargetableByAllies).found) return false;

            return true;
        }
        #endregion

        #region Entity / Tile Conversion
        public static List<EntityScript> AllEntitiesCache()
        {
            List<EntityScript> allEntities = UnityEngine.Object.FindObjectsByType<EntityScript>(0).ToList();

            return allEntities;
        }
        public static List<EntityScript> GetEntitiesFromTiles(List<Vector3Int> tiles)
        {
            List<EntityScript> allEntities = AllEntitiesCache();

            if (tiles == null || allEntities == null) return new List<EntityScript>();

            var tileSet = new HashSet<Vector3Int>(tiles);
            var results = new List<EntityScript>();

            for (int i = 0; i < allEntities.Count; i++)
            {
                var e = allEntities[i];
                if (e == null) continue;

                var onMap = e.GetComponent<EntityOnMap>();
                if (onMap == null) continue;

                if (tileSet.Contains(onMap.currentCell))
                {
                    results.Add(e);
                }
            }

            return results;
        }
        public static EntityScript GetEntitiesFromTile (Vector3Int tile)
        {
            List<EntityScript> allEntities = AllEntitiesCache();

            if (allEntities == null) return null;
            for (int i = 0; i < allEntities.Count; i++)
            {
                var e = allEntities[i];
                if (e == null) continue;
                var onMap = e.GetComponent<EntityOnMap>();
                if (onMap == null) continue;
                if (onMap.currentCell == tile)
                {
                    return e;
                }
            }
            return null;
        }
        public static Vector3Int? GetHoveredTile(PointerEventData eventData)
        {
            foreach (GameObject hoveredObject in eventData.hovered)
            {
                if (hoveredObject.TryGetComponent(out DraggableTarget dt) && dt.draggableTargetType == DraggableTargetType.CombatTile)
                {
                    return TilemapUtilityScript.BaseTilemap.WorldToCell(hoveredObject.transform.position);
                }
            }
            return TilemapUtilityScript.InvalidPosition;
        }

        public static Vector3Int GetHoveredTile(Ray ray)
        {
            RaycastHit[] hits = Physics.RaycastAll(ray);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.TryGetComponent<DraggableTarget>(out var dt) &&
                    dt.draggableTargetType == DraggableTargetType.CombatTile)
                {
                    return TilemapUtilityScript.BaseTilemap.WorldToCell(hit.collider.transform.position);
                }
            }
            return TilemapUtilityScript.InvalidPosition;
        }

        public static TargetingModeData GetAffected(CardScript card, Vector3Int aimTile, EntityScript owner, bool usesVision, List<Vector3Int> selectedTiles = null, bool isVetted = false)
        {
            List<EntityScript> entities = new();
            List<Vector3Int> tiles = new();

            TargetingModeData targetingModeData = new TargetingModeData();

            CardData cardData = card.cardData;
            Vector3Int currentCell = owner.GetComponent<EntityOnMap>().currentCell;

            switch (cardData.targetingData.cardTargetingMode)
            {
                case CardTargetingMode.Select:
                    {
                        tiles = selectedTiles;
                    }
                    break;
                case CardTargetingMode.LineFree:
                    {
                        tiles = TilemapUtilityScript.GetTilesInLineFree(selectedTiles, cardData.Range, cardData.Area);
                    }
                    break;
                case CardTargetingMode.Cone:
                    {
                        tiles = TilemapUtilityScript.GetTilesInCone(currentCell, aimTile, cardData.Range, cardData.Area);
                    }
                    break;
                case CardTargetingMode.LineSelf:
                    {
                        tiles = TilemapUtilityScript.GetTilesInLineFromSelf(currentCell, aimTile, cardData.Range);
                    }
                    break;
                case CardTargetingMode.Ring:
                    {
                        tiles = TilemapUtilityScript.GetTilesInRing(aimTile, cardData.Radius, cardData.Area);
                    }
                    break;
                case CardTargetingMode.Radius:
                    {
                        tiles = TilemapUtilityScript.GetTilesInRadius(aimTile, cardData.Radius);
                    }
                    break;
                case CardTargetingMode.Single:
                    {
                        tiles.Add(aimTile);
                    }
                    break;
            }

            if (usesVision)
            {
                tiles = VisionUtility.GetVisibleTiles(aimTile, tiles);
            }

            entities = GetEntitiesFromTiles(tiles);


            targetingModeData.castingPosition = aimTile;

            targetingModeData.targetedTiles = tiles;
            if (isVetted)
            {
                targetingModeData.targetedEntities = GetValidTargets(card.cardData);
            }
            else
            {
                targetingModeData.targetedEntities = entities;
            }

            return targetingModeData;
        }
        #endregion

        #region Path & Reachable Candidates
        /// <summary>
        /// Returns reachable PathData for a card and its potential targets based on available stamina.
        /// </summary>
        public static List<(PathData pathdata,TargetingModeData targetingMode)> GetReachableCandidates(
        CardScript card,
        List<TargetingModeData> templates,
        int stamina,
        Vector3Int virtualPosition)
        {
            var results = new List<(PathData, TargetingModeData)>();

            int stepcost = card.cardData.Owner.entityStats.MovementCostModifier.ApplyFinalValue(5);

            foreach (var template in templates)
            {
                var castTile = template.castingPosition;

                // Quick heuristic check: skip expensive pathfinding when estimated minimum steps
                // are greater than remaining stamina (after card cost). This avoids many full
                // path searches that are guaranteed to be unaffordable.
                int minSteps = MovementUtility.Heuristic(virtualPosition, castTile);
                int remainingStaminaAfterCard = stamina - card.cardData.Cost;
                if (remainingStaminaAfterCard < 0) continue;
                if (minSteps*stepcost > remainingStaminaAfterCard)
                {
                    continue;
                }

                var path = MovementUtility.FindPath(virtualPosition, castTile, movementCostModifier: card.cardData.Owner.entityStats.MovementCostModifier);

                // MovementUtility may return an empty PathData instance with a null Path when no path is found.
                if (path == null || path.Path == null)  
                {
                    continue;
                }

                // If no movement is needed, check if the card can be cast with current stamina
                if (path.Path.Count == 0)
                {
                    if (card.cardData.Cost < stamina)
                    {
                        // Use returned PathData directly to avoid an extra allocation/copy.
                        path.PathCost = 0;
                        results.Add((path, template));
                    }
                    continue;
                }

                if (card.cardData.Owner.entityStats.IsRooted)
                {
                    // If owner is rooted, movement cost is zero (we treat as zero path cost)
                    path.PathCost = 0;
                    results.Add((path, template));
                    continue;
                }

                int totalCost = path.PathCost + card.cardData.Cost;
                if (totalCost > stamina)
                {
                    continue;
                }
                results.Add((path, template));
            }
  
            return results;
        }

        // Coroutine variant that evaluates templates in batches and yields to avoid blocking a frame.
        public static IEnumerator GetReachableCandidatesCoroutine(
            CardScript card,
            List<TargetingModeData> templates,
            int stamina,
            Vector3Int virtualPosition,
            int batchSize,
            System.Action<List<(PathData, TargetingModeData)>> onComplete)
        {
            var results = new List<(PathData, TargetingModeData)>(templates != null ? templates.Count : 0);

            if (templates == null || card == null)
            {
                onComplete?.Invoke(results);
                yield break;
            }

            int cardCost = card.cardData.Cost;
            int remainingStaminaAfterCard = stamina - cardCost;
            if (remainingStaminaAfterCard < 0)
            {
                onComplete?.Invoke(results);
                yield break;
            }

            // Cache movement modifier and compute a conservative per-step cost with a 25% buffer
            var movementMod = card.cardData.Owner.entityStats.MovementCostModifier;
            int baseStepCost = movementMod.ApplyFinalValue(5);

            for (int i = 0; i < templates.Count; i++)
            {
                var template = templates[i];
                var castTile = template.castingPosition;

                // Heuristic
                int minSteps = MovementUtility.Heuristic(virtualPosition, castTile);
                if (minSteps * baseStepCost > remainingStaminaAfterCard)
                {
                    if (batchSize > 0 && (i % batchSize) == batchSize - 1)
                        yield return null;
                    continue;
                }

                // Perform the actual pathfinding
                var path = MovementUtility.FindPath(virtualPosition, castTile, movementCostModifier: movementMod);
                if (path != null && path.Path != null)
                {
                    if (path.Path.Count == 0)
                    {
                        if (cardCost < stamina)
                        {
                            path.PathCost = 0;
                            results.Add((path, template));
                        }
                    }
                        else
                        {
                            if (card.cardData.Owner.entityStats.IsRooted)
                            {
                                path.PathCost = 0;
                                results.Add((path, template));
                            }
                            else
                            {
                                // Use the real computed path cost for final validation (no buffer)
                                int totalCost = path.PathCost + cardCost;
                                if (totalCost <= stamina)
                                {   
                                    results.Add((path, template));
                                }
                            }
                        }
                }

                // Yield after processing each batch to keep the UI responsive
                if (batchSize > 0 && (i % batchSize) == batchSize - 1)
                {
                    yield return null;
                }
            }

            onComplete?.Invoke(results);
            yield break;
        }
        #endregion
    }

    public interface ITargetingMode
    {
        List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner);
    }

    public class TargetingModeData
    {
        public Vector3Int castingPosition { get; set; }
        public List<Vector3Int> targetedTiles { get; set; } = new();
        public List<EntityScript> targetedEntities { get; set; } = new();
    }

    public static class TargetingModeFactory
    {
        public static ITargetingMode Create(CardScript card)
        {
            return card.cardData.targetingData.cardTargetingMode switch
            {
                CardTargetingMode.Radius => new RadiusTargetingMode(),
                CardTargetingMode.Ring => new RingTargetingMode(),
                CardTargetingMode.LineSelf => new LineSelfTargetingMode(),
                CardTargetingMode.LineFree => new LineFreeTargetingMode(),
                CardTargetingMode.Cone => new ConeTargetingMode(),
                CardTargetingMode.Select => new SelectionTargetingMode(),
                CardTargetingMode.All => new AllTilesTargetingMode(),
                CardTargetingMode.Single => new SingleTargetingMode(),
                _ => new SingleTargetingMode(),
            };
        }
    }

    public abstract class BaseTargetingMode : ITargetingMode
    {
        public abstract List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner);

        protected List<EntityScript> GetEntitiesFromTiles(List<Vector3Int> tiles, int maxTargets = 9999)
        {
            return TargetingUtility.GetEntitiesFromTiles(tiles).Take(maxTargets).ToList();
        }
        protected Vector3Int FindCastingPosition(Vector3Int start, Vector3Int key, int range)
        {
            if (range <= 1)
            {
                return key;
            }
            else
            {
                return TilemapUtilityScript.GetReachableTileWithinRangeOfTarget(start, key, range);
            }
        }
    }

    /* -----------------------------------------------------------
     * SINGLE TARGET
     * -----------------------------------------------------------*/
    public class SingleTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData);
            var results = new List<TargetingModeData>();

            foreach (var t in targets)
            {
                var currenttile = t.GetComponent<EntityOnMap>().currentCell;
                var tiles = TilemapUtilityScript.GetTilesInRadius(currenttile, card.cardData.Range);

                foreach (var tile in tiles)
                {
                    results.Add(new TargetingModeData
                    {
                        castingPosition = tile,
                        targetedTiles = new List<Vector3Int>() { currenttile },
                        targetedEntities = new List<EntityScript>() { t },
                    });
                }
            }
            return results;
        }
    }

    /* -----------------------------------------------------------
     * RADIUS
     * -----------------------------------------------------------*/
    public class RadiusTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData);
            var results = new List<TargetingModeData>();
            Dictionary<Vector3Int, int> options = new();

            foreach (var t in targets)
            {
                var center = t.GetComponent<EntityOnMap>().currentCell;
                var preTiles = TilemapUtilityScript.GetTilesInRadius(center, card.cardData.Radius);

                foreach (var tile in preTiles)
                {
                    if (!options.ContainsKey(tile))
                    {
                        options.Add(tile, 0);
                    }
                    else
                    {
                        options[tile]++;
                    }
                }
            }

            foreach (var option in options)
            {
                var tiles = TilemapUtilityScript.GetTilesInRadius(option.Key, card.cardData.Radius);

                results.Add(new TargetingModeData
                {
                    castingPosition = FindCastingPosition(owner.GetComponent<EntityOnMap>().currentCell, option.Key,card.cardData.Range),
                    targetedTiles = tiles,
                    targetedEntities = GetEntitiesFromTiles(tiles)
                });
            }

            return results;
        }
    }

    /* -----------------------------------------------------------
     * RING
     * -----------------------------------------------------------*/
    public class RingTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card,EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData);
            var results = new List<TargetingModeData>();
            Dictionary<Vector3Int, int> options = new();

            foreach (var t in targets)
            {
                var center = t.GetComponent<EntityOnMap>().currentCell;
                var preTiles = TilemapUtilityScript.GetTilesInRing(center, card.cardData.Radius, card.cardData.Area);

                foreach (var tile in preTiles)
                {
                    if (!options.ContainsKey(tile))
                    {
                        options.Add(tile, 0);
                    }
                    else
                    {
                        options[tile]++;
                    }
                }
            }

            foreach (var option in options)
            {
                var tiles = TilemapUtilityScript.GetTilesInRing(option.Key, card.cardData.Radius, card.cardData.Area);

                results.Add(new TargetingModeData
                {
                    castingPosition = FindCastingPosition(owner.GetComponent<EntityOnMap>().currentCell, option.Key, card.cardData.Range),
                    targetedTiles = tiles,
                    targetedEntities = GetEntitiesFromTiles(tiles)
                });
            }

            return results;
        }
    }

    /* -----------------------------------------------------------
     * LINE SELF
     * -----------------------------------------------------------*/
    public class LineSelfTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData);
            var results = new List<TargetingModeData>();

            var candidatePositions = new Dictionary<Vector3Int, int>();

            // Step 1: Generate candidate positions 
            foreach (var t in targets)
            {
                var targetCell = t.GetComponent<EntityOnMap>().currentCell;

                for (int dir = 0; dir < 6; dir++)
                {
                    var directionVector = TilemapUtilityScript.CubeDirs[dir];

                    // Step backward from target up to range
                    for (int step = 1; step <= card.cardData.Range; step++)
                    {
                        Vector3Int candidatePos = targetCell - directionVector * step;

                        if (!candidatePositions.ContainsKey(candidatePos))
                            candidatePositions[candidatePos] = 0;
                    }
                }
            }

            var ownerCell = owner.GetComponent<EntityOnMap>().currentCell;

            // Step 2: Evaluate each candidate
            foreach (var pos in candidatePositions.Keys.ToList())
            {
                for (int dir = 0; dir < 6; dir++)
                {
                    var directionVector = TilemapUtilityScript.CubeDirs[dir];
                    var lineTiles = TilemapUtilityScript.GetTilesInLineFromSelf(pos, pos + directionVector, card.cardData.Range);

                    var hitEntities = GetEntitiesFromTiles(lineTiles);
                    int hitCount = hitEntities.Count;

                    if (hitCount > 0)
                    {
                        results.Add(new TargetingModeData
                        {
                            castingPosition = pos,
                            targetedTiles = lineTiles,
                            targetedEntities = hitEntities
                        });

                        candidatePositions[pos] = Mathf.Max(candidatePositions[pos], hitCount); 
                    }
                }
            }

            // Step 3: Sort by max targets hit descending
            results = results
                .OrderByDescending(r => r.targetedEntities.Count)
                .ThenBy(r => Vector3Int.Distance(ownerCell, r.castingPosition))
                .ToList();

            return results;
        }
    }

    /* -----------------------------------------------------------
     * LINE FREE
     * -----------------------------------------------------------*/
    public class LineFreeTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData);
            var results = new List<TargetingModeData>();

            foreach (var start in targets)
            {
                var startCell = start.GetComponent<EntityOnMap>().currentCell;

                foreach (var end in targets)
                {
                    var endCell = end.GetComponent<EntityOnMap>().currentCell;

                    var lineData = MovementUtility.FindLineFromToWithLength(startCell, endCell, card.cardData.Area);
                    var hitEntities = GetEntitiesFromTiles(lineData.Path);

                    var castPositions = TilemapUtilityScript.GetTilesInRadius(startCell, card.cardData.Range)
                        .Intersect(TilemapUtilityScript.GetTilesInRadius(endCell, card.cardData.Range));

                    foreach (var castPosition in castPositions)
                    {
                        results.Add(new TargetingModeData
                        {
                            castingPosition = castPosition,
                            targetedTiles = lineData.Path,
                            targetedEntities = hitEntities
                        });
                    }
                }
            }
            return results;
        }
    }

    /* -----------------------------------------------------------
     * CONE
     * -----------------------------------------------------------*/
    public class ConeTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData);
            var results = new List<TargetingModeData>();

            foreach (var t in targets)
            {
                var targetCell = t.GetComponent<EntityOnMap>().currentCell;
                var castCandidates = TilemapUtilityScript.GetTilesInStar(targetCell, card.cardData.Range);

                foreach (var cast in castCandidates)
                {
                    var coneTiles = TilemapUtilityScript.GetTilesInCone(cast, targetCell, card.cardData.Range, card.cardData.Area);
                    var hitEntities = GetEntitiesFromTiles(coneTiles);

                    if (hitEntities.Count > 0)
                    {
                        results.Add(new TargetingModeData
                        {
                            castingPosition = cast,
                            targetedTiles = coneTiles,
                            targetedEntities = hitEntities
                        });
                    }
                }
            }
            return results;
        }
    }

    /* -----------------------------------------------------------
     * SELECTION
     * -----------------------------------------------------------*/
    public class SelectionTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData);
            var results = new List<TargetingModeData>();
            Dictionary<Vector3Int, int> options = new();

            foreach (var t in targets)
            {
                var center = t.GetComponent<EntityOnMap>().currentCell;
                var preTiles = TilemapUtilityScript.GetTilesInRadius(center, card.cardData.Range);

                foreach (var tile in preTiles)
                {
                    if (!options.ContainsKey(tile))
                    {
                        options.Add(tile, 0);
                    }
                    else
                    {
                        options[tile]++;
                    }
                }
            }

            foreach (var option in options)
            {
                var tiles = TilemapUtilityScript.GetTilesInRadius(option.Key, card.cardData.Range);
                var entities = GetEntitiesFromTiles(tiles);
                var tilesWithEntities = entities.Select(e => e.GetComponent<EntityOnMap>().currentCell).ToList();


                results.Add(new TargetingModeData
                {
                    castingPosition = option.Key,
                    targetedTiles = tilesWithEntities,
                    targetedEntities = entities
                });
            }

            return results;
        }
    }

    /* -----------------------------------------------------------
     * ALL TILES
     * -----------------------------------------------------------*/
    public class AllTilesTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var tiles = TilemapUtilityScript.GetAllValidTiles();

            TargetingModeData result = new TargetingModeData
            {
                castingPosition = card.cardData.Owner.GetComponent<EntityOnMap>().currentCell,
                targetedTiles = tiles,
                targetedEntities = GetEntitiesFromTiles(tiles,card.cardData.MaxTarget),
            };
            
            return new List<TargetingModeData>() { result };
        }
    }
}