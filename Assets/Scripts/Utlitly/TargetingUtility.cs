using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using facingfate;

namespace Utility
{
    public static class TargetingUtility
    {
        #region Entity Validation
        public static List<EntityScript> GetValidTargets(CardScript card, EntityScript owner, List<EntityScript> candidates)
        {
            if (card == null || owner == null || candidates == null) return new List<EntityScript>();

            return candidates.Where(target => target != null && IsTargetValid(card, owner, target)).ToList();
        }
        public static bool IsTargetValid(CardScript card, EntityScript owner, EntityScript target)
        {
            if(target == null) return false;

            var targeting = card.cardData.targetingData;
            var targetAff = target.entityAffiliation;
            var ownerAff = owner.entityAffiliation;

            if (target.HasReference(GameplayRef.untargetableByAll).found)
                return false;

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

            if (isEnemyTarget && target.HasReference(GameplayRef.untargetableByEnemies).found)
                return false;

            if (isAllyTarget && target.HasReference(GameplayRef.untargetableByAllies).found)
                return false;

            return true;
        }
        #endregion

        #region Entity / Tile Conversion
        public static List<EntityScript> GetEntitiesFromTiles(List<Vector3Int> tiles, List<EntityScript> allEntities)
        {
            if (tiles == null || allEntities == null) return new List<EntityScript>();

            return allEntities.Where(e => e != null && e.GetComponent<EntityOnMap>() != null &&
                                          tiles.Contains(e.GetComponent<EntityOnMap>().currentCell)).ToList();
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
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent<DraggableTarget>(out var dt) && dt.draggableTargetType == DraggableTargetType.CombatTile)
                { // Convert world position to tilemap cell
                    return TilemapUtilityScript.BaseTilemap.WorldToCell(hit.collider.transform.position);
                }
            }
            return TilemapUtilityScript.InvalidPosition;
        }
        public static TargetingModeData GetAffected(CardScript card, Vector3Int aimTile, EntityScript owner, bool usesVision, List<Vector3Int> selectedTiles = null, bool isVetted = false)
        {
            List<EntityScript> allEntities = Object.FindObjectsByType<EntityScript>(0).ToList();
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

            entities = GetEntitiesFromTiles(tiles, allEntities);


            targetingModeData.castingPosition = aimTile;

            targetingModeData.targetedTiles = tiles;
            if (isVetted)
            {
                targetingModeData.targetedEntities = GetValidTargets(card, owner, entities);
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

            foreach (var template in templates)
            {
                var castTile = template.castingPosition;

                var path = MovementUtility.FindPath(virtualPosition, castTile, movementCostModifier: card.cardData.Owner.entityStats.MovementCostModifier);

                if (path == null)
                {
                    continue;
                }

                // If no movement is needed, check if the card can be cast with current stamina
                if (path.Path.Count == 0)
                {
                    if (card.cardData.Cost < stamina)
                    {
                        results.Add((new PathData { Start = path.Start, End = path.End, Path = path.Path, PathCost = 0 }, template));
                    }
                    continue;
                }

                if (card.cardData.Owner.entityStats.IsRooted) 
                {
                    results.Add((new PathData { Start = path.Start, End = path.End, Path = path.Path, PathCost = 0 }, template));
                    continue; 
                }

                int totalCost = path.PathCost + card.cardData.Cost;
                if (totalCost > stamina)
                {
                    continue;
                }
                results.Add((new PathData { Start = path.Start, End = path.End, Path = path.Path, PathCost = path.PathCost }, template));
            }
  
            return results;
        }
        #endregion
    }

    public interface ITargetingMode
    {
        List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner);
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
        public abstract List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner);

        protected List<EntityScript> GetValidEntities(CardScript card, List<EntityScript> entities, EntityScript owner)
        {
            return TargetingUtility.GetValidTargets(card, owner, entities);
        }

        protected List<EntityScript> GetEntitiesFromTiles(List<Vector3Int> tiles, List<EntityScript> allEntities, int maxTargets = 9999)
        {
            return TargetingUtility.GetEntitiesFromTiles(tiles, allEntities).Take(maxTargets).ToList();
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
        public override List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner)
        {
            var results = new List<TargetingModeData>();
            targets = GetValidEntities(card, targets, owner);

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
        public override List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner)
        {
            var results = new List<TargetingModeData>();
            Dictionary<Vector3Int, int> options = new();
            targets = GetValidEntities(card, targets, owner);

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
                    targetedEntities = GetEntitiesFromTiles(tiles, targets)
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
        public override List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner)
        {
            var results = new List<TargetingModeData>();
            Dictionary<Vector3Int, int> options = new();
            targets = GetValidEntities(card, targets, owner);

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
                    targetedEntities = GetEntitiesFromTiles(tiles, targets)
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
        public override List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner)
        {
            var results = new List<TargetingModeData>();
            targets = GetValidEntities(card, targets, owner);

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

                    var hitEntities = GetEntitiesFromTiles(lineTiles, targets);
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
        public override List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner)
        {
            var results = new List<TargetingModeData>();

            targets = GetValidEntities(card, targets, owner);

            foreach (var start in targets)
            {
                var startCell = start.GetComponent<EntityOnMap>().currentCell;

                foreach (var end in targets)
                {
                    var endCell = end.GetComponent<EntityOnMap>().currentCell;

                    var lineData = MovementUtility.FindLineFromToWithLength(startCell, endCell, card.cardData.Area);
                    var hitEntities = GetEntitiesFromTiles(lineData.Path, targets);

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
        public override List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner)
        {
            var results = new List<TargetingModeData>();
            targets = GetValidEntities(card, targets, owner);

            foreach (var t in targets)
            {
                var targetCell = t.GetComponent<EntityOnMap>().currentCell;
                var castCandidates = TilemapUtilityScript.GetTilesInStar(targetCell, card.cardData.Range);

                foreach (var cast in castCandidates)
                {
                    var coneTiles = TilemapUtilityScript.GetTilesInCone(cast, targetCell, card.cardData.Range, card.cardData.Area);
                    var hitEntities = GetEntitiesFromTiles(coneTiles, targets);

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
        public override List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner)
        {
            var results = new List<TargetingModeData>();
            Dictionary<Vector3Int, int> options = new();
            targets = GetValidEntities(card, targets, owner);

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
                var entities = GetEntitiesFromTiles(tiles, targets);
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
        public override List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner)
        {
            targets = GetValidEntities(card, targets, owner);
            var tiles = TilemapUtilityScript.GetTilesInRadius(new(), 999);

            TargetingModeData result = new TargetingModeData
            {
                castingPosition = card.cardData.Owner.GetComponent<EntityOnMap>().currentCell,
                targetedTiles = tiles,
                targetedEntities = GetEntitiesFromTiles(tiles,targets,card.cardData.MaxTarget),
            };
            
            return new List<TargetingModeData>() { result };
        }
    }
}