using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility;

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

            private static bool IsTargetValid(CardScript card, EntityScript owner, EntityScript target)
            {
                var targeting = card.cardData.targetingData;
                var targetAff = target.entityAffiliation;
                var ownerAff = owner.entityAffiliation;

                if (target.HasReference(GameplayRef.untargetableByAll).found)
                    return false;

                bool baseValid = targeting.CardTargetAffiliation switch
                {
                    CardTargetAffiliation.Ally => targetAff == ownerAff && targetAff != EntityAffiliation.Neutral,
                    CardTargetAffiliation.Enemy => targetAff != ownerAff && targetAff != EntityAffiliation.Neutral,
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
                Debug.Log(hoveredObject.name);
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
                Debug.Log($"[TargetingUtility] Raycast hit: {hit.collider.name}"); // Check if the hit object is a draggable target of type CombatTile
                if (hit.collider.TryGetComponent<DraggableTarget>(out var dt) && dt.draggableTargetType == DraggableTargetType.CombatTile)
                { // Convert world position to tilemap cell
                    return TilemapUtilityScript.BaseTilemap.WorldToCell(hit.collider.transform.position);
                }
            }
            return TilemapUtilityScript.InvalidPosition;
        }

            public static List<Vector3Int> GetEffectAreaTiles(CardScript card, Vector3Int centerTile, EntityScript owner, List<Vector3Int> selectedTiles = null)
            {
                switch (card.cardData.targetingData.cardSelectionType)
                {
                    case CardTargetSelection.Radius:
                        return TilemapUtilityScript.GetTilesInRadius(centerTile, card.cardData.Area);
                    case CardTargetSelection.Ring:
                        return TilemapUtilityScript.GetTilesInRingFromSelf(centerTile, card.cardData.Range, card.cardData.Area);
                    case CardTargetSelection.LineFree:
                        return selectedTiles != null ? TilemapUtilityScript.GetTilesInLineFree(selectedTiles, card.cardData.Range, card.cardData.Area) : new List<Vector3Int>();
                    case CardTargetSelection.LineSelf:
                        return TilemapUtilityScript.GetTilesInLineFromSelf(owner.GetComponent<EntityOnMap>().currentCell, centerTile, card.cardData.Area);
                    case CardTargetSelection.Cone:
                        return TilemapUtilityScript.GetTilesInCone(owner.GetComponent<EntityOnMap>().currentCell, centerTile, card.cardData.Range, card.cardData.Area);
                    case CardTargetSelection.All:
                        return TilemapUtilityScript.GetAllValidTiles();
                    case CardTargetSelection.Select:
                        return TilemapUtilityScript.GetTilesInRange(centerTile, card.cardData.Range);
                    default:
                        return new List<Vector3Int> { centerTile };
                }
            }
            #endregion

            #region Path & Reachable Candidates
            /// <summary>
            /// Returns reachable PathData for a card and its potential targets based on available stamina.
            /// </summary>
            public static List<PathData> GetReachablePathCandidates(CardScript card, List<EntityScript> targets, int stamina, Vector3Int currentCell)
            {
                if (card == null || targets == null || targets.Count == 0) return new List<PathData>();

                var targetingMode = TargetingModeFactory.Create(card);
                var modeDataList = targetingMode.GetTargetingData(card, targets, card.cardData.Owner);

                var tileFrequency = new Dictionary<Vector3Int, int>();
                var hitEntitiesSet = new HashSet<EntityScript>();

            foreach (var md in modeDataList)
            {

                if (!tileFrequency.ContainsKey(md.CastingPosition))
                    tileFrequency[md.CastingPosition] = 0;
                tileFrequency[md.CastingPosition]++;


                if (md.TargetedEntities != null)
                    foreach (var e in md.TargetedEntities)
                        hitEntitiesSet.Add(e);
            }

                return RankPathRecords(BuildPathRecords(tileFrequency, hitEntitiesSet.ToList(), stamina, card.cardData.Cost, currentCell));
            }

            private static List<PathRecord> BuildPathRecords(Dictionary<Vector3Int, int> tileFreq, List<EntityScript> hitEntities, int stamina, int cardCost, Vector3Int currentCell)
            {
                var records = new List<PathRecord>();

                foreach (var kvp in tileFreq)
                {
                    Vector3Int tile = kvp.Key;
                    int freq = kvp.Value;

                    var path = TilemapUtilityScript.FindPath(currentCell, tile, walkClose: true);
                    if (path == null || path.Path == null || path.Path.Count == 0) continue;

                    int totalCost = path.PathCost + cardCost;
                    if (totalCost > stamina) continue;
                    if (hitEntities == null || hitEntities.Count == 0) continue;

                    records.Add(new PathRecord
                    {
                        Tile = tile,
                        Frequency = freq,
                        Distance = Vector3Int.Distance(currentCell, tile),
                        PathData = new PathData
                        {
                            Start = currentCell,
                            End = tile,
                            Path = path.Path,
                            PathCost = path.PathCost
                        }
                    });
                }

                return records;
            }

            private static List<PathData> RankPathRecords(List<PathRecord> records)
            {
                if (records == null || records.Count == 0) return new List<PathData>();

                int maxFreq = records.Max(r => r.Frequency);

                return records
                    .Where(r => r.Frequency >= maxFreq - 1)
                    .OrderByDescending(r => r.Frequency)
                    .ThenBy(r => r.PathData.PathCost)
                    .ThenBy(r => r.Distance)
                    .Select(r => r.PathData)
                    .ToList();
            }
            #endregion
        }
    }

    public interface ITargetingMode
{
    List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner);
}
public class PathRecord
{
    public Vector3Int Tile;       // The tile being considered
    public int Frequency;         // How often this tile is "targeted" by candidate positions
    public float Distance;        // Distance from NPC's current position
    public PathData PathData;     // Path information to reach this tile
}

public static class TargetingModeFactory
{
    public static ITargetingMode Create(CardScript card)
    {
        return card.cardData.targetingData.cardSelectionType switch
        {
            CardTargetSelection.Radius => new RadiusTargetingMode(),
            CardTargetSelection.Ring => new RingTargetingMode(),
            CardTargetSelection.LineSelf => new LineSelfTargetingMode(),
            CardTargetSelection.LineFree => new LineFreeTargetingMode(),
            CardTargetSelection.Cone => new ConeTargetingMode(),
            CardTargetSelection.Select => new SelectionTargetingMode(),
            CardTargetSelection.All => new AllTilesTargetingMode(),
            CardTargetSelection.Single => new SingleTargetingMode(),
            _ => new SingleTargetingMode(),
        };
    }
}
public class TargetingModeData
{
    // Tile the agent should move to in order to use this targeting option (null if none)
    public Vector3Int CastingPosition { get; set; }

    // Tiles that will be aimed/affected when using the card from WhereToGo
    public List<Vector3Int> TargetedTiles { get; set; } = new();

    // Entities that will be hit when using the card from WhereToGo and aiming at WhereToAim
    public List<EntityScript> TargetedEntities { get; set; } = new();
}
public abstract class BaseTargetingMode : ITargetingMode
{
    public abstract List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner);

    protected List<EntityScript> GetValidEntities(CardScript card, List<EntityScript> entities, EntityScript owner)
    {
        return TargetingUtility.GetValidTargets(card, owner, entities);
    }

    protected void AddTiles(Dictionary<Vector3Int, int> dict, List<Vector3Int> tiles)
    {
        foreach (var tile in tiles)
        {
            if (!dict.ContainsKey(tile)) dict[tile] = 0;
            dict[tile]++;
        }
    }

    protected List<EntityScript> GetEntitiesFromTiles(List<Vector3Int> tiles, List<EntityScript> allEntities)
    {
        return TargetingUtility.GetEntitiesFromTiles(tiles, allEntities);
    }
}

public class SingleTargetingMode : BaseTargetingMode
{
    public override List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner)
    {
        var results = new List<TargetingModeData>();
        targets = GetValidEntities(card, targets, owner);

        foreach (var t in targets)
        {
            var cell = t.GetComponent<EntityOnMap>().currentCell;

            var aimTiles = TargetingUtility.GetEffectAreaTiles(card, cell, owner, null) ?? new List<Vector3Int> { cell };
            var hitEntities = GetEntitiesFromTiles(aimTiles, targets);

            results.Add(new TargetingModeData
            {
                CastingPosition = cell,
                TargetedTiles = aimTiles,
                TargetedEntities = hitEntities
            });
        }

        return results;
    }
}

public class RadiusTargetingMode : BaseTargetingMode
{
    public override List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner)
    {
        var results = new List<TargetingModeData>();

        targets = GetValidEntities(card, targets, owner);

        foreach (var t in targets)
        {
            var center = t.GetComponent<EntityOnMap>().currentCell;
            var tiles = TilemapUtilityScript.GetTilesInRadius(center, card.cardData.Area);

            foreach (var tile in tiles)
            {
                var aimTiles = TargetingUtility.GetEffectAreaTiles(card, tile, owner, null) ?? new List<Vector3Int> { tile };
                var hitEntities = GetEntitiesFromTiles(aimTiles, targets);

                results.Add(new TargetingModeData
                {
                    CastingPosition = tile,
                    TargetedTiles = aimTiles,
                    TargetedEntities = hitEntities
                });
            }
        }

        return results;
    }
}

public class RingTargetingMode : BaseTargetingMode
{
    public override List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner)
    {
        var results = new List<TargetingModeData>();
        targets = GetValidEntities(card, targets, owner);

        foreach (var t in targets)
        {
            var center = t.GetComponent<EntityOnMap>().currentCell;
            var tiles = TilemapUtilityScript.GetTilesInRingFromSelf(center, card.cardData.Range, card.cardData.Area);

            foreach (var tile in tiles)
            {
                var aimTiles = TargetingUtility.GetEffectAreaTiles(card, tile, owner, null) ?? new List<Vector3Int> { tile };
                var hitEntities = GetEntitiesFromTiles(aimTiles, targets);

                results.Add(new TargetingModeData
                {
                    CastingPosition = tile,
                    TargetedTiles = aimTiles,
                    TargetedEntities = hitEntities
                });
            }
        }

        return results;
    }
}

public class LineSelfTargetingMode : BaseTargetingMode
{
    public override List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner)
    {
        var results = new List<TargetingModeData>();
        targets = GetValidEntities(card, targets, owner);

        var ownerCell = owner.GetComponent<EntityOnMap>().currentCell;

        foreach (var t in targets)
        {
            var targetCell = t.GetComponent<EntityOnMap>().currentCell;
            var tiles = TilemapUtilityScript.GetTilesInLineFromSelf(ownerCell, targetCell, card.cardData.Area);

            foreach (var tile in tiles)
            {
                var aimTiles = TargetingUtility.GetEffectAreaTiles(card, tile, owner, null) ?? new List<Vector3Int> { tile };
                var hitEntities = GetEntitiesFromTiles(aimTiles, targets);

                results.Add(new TargetingModeData
                {
                    CastingPosition = tile,
                    TargetedTiles = aimTiles,
                    TargetedEntities = hitEntities
                });
            }
        }

        return results;
    }
}

public class LineFreeTargetingMode : BaseTargetingMode
{
    public override List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner)
    {
        var results = new List<TargetingModeData>();
        var allHitsByCandidate = new Dictionary<Vector3Int, HashSet<EntityScript>>();

        // Collect all entity positions up front
        List<Vector3Int> cells = GetValidEntities(card, targets, owner)
            .Select(t => t.GetComponent<EntityOnMap>().currentCell)
            .ToList();


        foreach (var t in targets)
        {
            var startTargetCell = t.GetComponent<EntityOnMap>().currentCell;

            foreach (var t2 in targets)
            {
                var endTargetCell = t2.GetComponent<EntityOnMap>().currentCell;
                var linePathData = TilemapUtilityScript.FindLineFromToWithLength(startTargetCell, endTargetCell, card.cardData.Area);
                var hitEntities = GetEntitiesFromTiles(linePathData.Path, targets);

                var tiles = TilemapUtilityScript.GetTilesInRadius(startTargetCell, card.cardData.Range).Intersect(TilemapUtilityScript.GetTilesInRadius(endTargetCell, card.cardData.Range));
                foreach (var tile in tiles)
                {
                    results.Add(new TargetingModeData
                    {
                        CastingPosition = tile,
                        TargetedTiles = linePathData.Path,
                        TargetedEntities = hitEntities
                    });
                }
            }
        }
        return results;
    }
}


public class ConeTargetingMode : BaseTargetingMode
{
    public override List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner)
    {
        var results = new List<TargetingModeData>();
        targets = GetValidEntities(card, targets, owner);

        foreach (var t in targets)
        {
            var targetCell = t.GetComponent<EntityOnMap>().currentCell;
            var candidateTiles = TilemapUtilityScript.GetTilesInStar(targetCell, card.cardData.Range);

            foreach (var ct in candidateTiles)
            {
                var coneTiles = TilemapUtilityScript.GetTilesInCone(ct, targetCell, card.cardData.Range, card.cardData.Area);
                var hitEntities = GetEntitiesFromTiles(coneTiles, targets);

                if (hitEntities.Count > 0)
                {
                    results.Add(new TargetingModeData
                    {
                        CastingPosition = ct,
                        TargetedTiles = coneTiles,
                        TargetedEntities = hitEntities
                    });
                }
            }
        }
        return results;
    }
}

public class SelectionTargetingMode : BaseTargetingMode
{
    public override List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner)
    {
        var results = new List<TargetingModeData>();
        targets = GetValidEntities(card, targets, owner);

        foreach (var t in targets)
        {
            var center = t.GetComponent<EntityOnMap>().currentCell;
            var tiles = TilemapUtilityScript.GetTilesInRadius(center, card.cardData.Area);

            foreach (var tile in tiles)
            {
                var aimTiles = TargetingUtility.GetEffectAreaTiles(card, tile, owner, null) ?? new List<Vector3Int> { tile };
                var hitEntities = GetEntitiesFromTiles(aimTiles, targets);

                results.Add(new TargetingModeData
                {
                    CastingPosition = tile,
                    TargetedTiles = aimTiles,
                    TargetedEntities = hitEntities
                });
            }
        }

        return results;
    }
}

public class AllTilesTargetingMode : BaseTargetingMode
{
    public override List<TargetingModeData> GetTargetingData(CardScript card, List<EntityScript> targets, EntityScript owner)
    {
        var results = new List<TargetingModeData>();
        targets = GetValidEntities(card,targets, owner);

        foreach (var t in targets)
        {
            var center = t.GetComponent<EntityOnMap>().currentCell;
            var tiles = TilemapUtilityScript.GetTilesInRadius(center, card.cardData.Area);

            foreach (var tile in tiles)
            {
                var aimTiles = TargetingUtility.GetEffectAreaTiles(card, tile, owner, null) ?? new List<Vector3Int> { tile };
                var hitEntities = GetEntitiesFromTiles(aimTiles, targets);

                results.Add(new TargetingModeData
                {
                    CastingPosition = tile,
                    TargetedTiles = aimTiles,
                    TargetedEntities = hitEntities
                });
            }
        }

        return results;
    }
}