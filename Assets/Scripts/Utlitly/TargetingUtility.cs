using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Utility
{

public static class TargetingUtility
{
    public static List<EntityScript> GetTargetsFromPosition(CardScript card, Vector3Int pos, List<EntityScript> allEntities, EntityScript owner)
    {
        List<EntityScript> targets = new();

        switch (card.cardData.targetingData.SelectionType)
        {
            case CardTargetSelection.Single:
                var SingleTarget = TilemapUtilityScript.GetTilesInRadius(pos, card.cardData.targetingData.range);
                List< Vector3Int> v3 = new List< Vector3Int>();
                v3.Add(SingleTarget[0]);  

                targets = GetEntitiesFromTiles(v3, allEntities);
                break;
            case CardTargetSelection.All:
                targets = allEntities;
                break;
            case CardTargetSelection.Radius:
                var radiusTiles = TilemapUtilityScript.GetTilesInRadius(pos, card.cardData.targetingData.range);
                targets = GetEntitiesFromTiles(radiusTiles, allEntities);
                break;
            case CardTargetSelection.Ring:
                var ringTiles = TilemapUtilityScript.GetTilesInRing(pos, card.cardData.targetingData.range);
                targets = GetEntitiesFromTiles(ringTiles, allEntities);
                break;
            case CardTargetSelection.LineFree:
                var lineFreeTiles = TilemapUtilityScript.GetTilesInLine(pos, pos, card.cardData.targetingData.range);
                targets = GetEntitiesFromTiles(lineFreeTiles, allEntities);
                break;
            case CardTargetSelection.LineSelf:
                var lineSelfTiles = TilemapUtilityScript.GetTilesInLine(owner.GetComponent<EntityOnMap>().currentCell, pos, card.cardData.targetingData.range);
                targets = GetEntitiesFromTiles(lineSelfTiles, allEntities);
                break;
                    case CardTargetSelection.Cone:
                    break;
                    case CardTargetSelection.Select:
                    break;
            }

        return VetTargetsEntities(card, owner, targets);
    }
    public static List<EntityScript> GetEntitiesFromTiles(IEnumerable<Vector3Int> tiles, List<EntityScript> allEntities)
    {
        return allEntities.Where(e => tiles.Contains(e.GetComponent<EntityOnMap>().currentCell)).ToList();
    }
    public static List<EntityScript> VetTargetsEntities(CardScript card, EntityScript owner, List<EntityScript> preTargets)
    {
        if (card == null || owner == null || preTargets == null) return new List<EntityScript>();

        var targeting = card.cardData.targetingData;
        EntityAffiliation ownerAffiliation = owner.entityAffiliation;

        return preTargets.Where(target =>
        {
            EntityAffiliation targetAff = target.entityAffiliation;

            return targeting.CardTargetAffiliation switch
            {
                CardTargetAffiliation.Ally => targetAff == ownerAffiliation && targetAff != EntityAffiliation.Neutral,
                CardTargetAffiliation.Enemy => targetAff != ownerAffiliation && targetAff != EntityAffiliation.Neutral,
                CardTargetAffiliation.Self => target == owner,
                CardTargetAffiliation.All => true,
                CardTargetAffiliation.AllyNeutral => targetAff == ownerAffiliation || targetAff == EntityAffiliation.Neutral,
                CardTargetAffiliation.EnemyNeutral => targetAff != ownerAffiliation || targetAff == EntityAffiliation.Neutral,
                CardTargetAffiliation.AllyEnemy => targetAff != EntityAffiliation.Neutral,
                _ => false
            };
        }).ToList();
    }
        public static List<EntityScript> VetTargetsEntities(EntityScript owner, CardTargetAffiliation targetAffiliation, List<EntityScript> preTargets)
        {
            EntityAffiliation ownerAffiliation = owner.entityAffiliation;

            return preTargets.Where(target =>
            {
                EntityAffiliation targetAff = target.entityAffiliation;

                return targetAffiliation switch
                {
                    CardTargetAffiliation.Ally => targetAff == ownerAffiliation && targetAff != EntityAffiliation.Neutral,
                    CardTargetAffiliation.Enemy => targetAff != ownerAffiliation && targetAff != EntityAffiliation.Neutral,
                    CardTargetAffiliation.Self => target == owner,
                    CardTargetAffiliation.All => true,
                    CardTargetAffiliation.AllyNeutral => targetAff == ownerAffiliation || targetAff == EntityAffiliation.Neutral,
                    CardTargetAffiliation.EnemyNeutral => targetAff != ownerAffiliation || targetAff == EntityAffiliation.Neutral,
                    CardTargetAffiliation.AllyEnemy => targetAff != EntityAffiliation.Neutral,
                    _ => false
                };
            }).ToList();
        }

        public static bool IsValidTarget(CardScript card, EntityScript owner, EntityScript target)
    {
        var aff = target.entityAffiliation;
        var ownerAff = owner.entityAffiliation;
        switch (card.cardData.targetingData.CardTargetAffiliation)
        {
            case CardTargetAffiliation.Ally: return aff == ownerAff;
            case CardTargetAffiliation.Enemy: return aff != ownerAff;
            case CardTargetAffiliation.Self: return target == owner;
            case CardTargetAffiliation.All: return true;
            case CardTargetAffiliation.AllyNeutral: return (aff == ownerAff || aff == EntityAffiliation.Neutral);
            case CardTargetAffiliation.EnemyNeutral: return (aff != ownerAff || aff == EntityAffiliation.Neutral);
            case CardTargetAffiliation.AllyEnemy: return (aff != EntityAffiliation.Neutral);
            default: return false;
        }
    }
    public static List<EntityScript> GetValidTargets(CardScript card, EntityScript owner, List<EntityScript> allEntities)
    {
        if (card == null || owner == null || allEntities == null) return new List<EntityScript>();

        var targeting = card.cardData.targetingData;

        // Filter out self if needed
        return allEntities.Where(target =>
        {
            if (target == null) return false;

            var aff = target.entityAffiliation;
            var ownerAff = owner.entityAffiliation;

            bool valid = targeting.CardTargetAffiliation switch
            {
                CardTargetAffiliation.Ally => aff == ownerAff && aff != EntityAffiliation.Neutral,
                CardTargetAffiliation.Enemy => aff != ownerAff && aff != EntityAffiliation.Neutral,
                CardTargetAffiliation.Self => target == owner,
                CardTargetAffiliation.All => true,
                CardTargetAffiliation.AllyNeutral => aff == ownerAff || aff == EntityAffiliation.Neutral,
                CardTargetAffiliation.EnemyNeutral => aff != ownerAff || aff == EntityAffiliation.Neutral,
                CardTargetAffiliation.AllyEnemy => aff != EntityAffiliation.Neutral,
                _ => false
            };

            return valid;
        }).ToList();
    }
    public static List<Vector3Int> GetCandidateTilesForTarget(CardScript card, Vector3Int targetCell)
    {
        List<Vector3Int> candidateTiles = card.cardData.targetingData.range <= 0
            ? new List<Vector3Int> { targetCell }
            : TilemapUtilityScript.GetTilesInRadius(targetCell, card.cardData.targetingData.range);

        // Remove occupied tiles
        if (TilemapUtilityScript.BaseTilemap != null)
        {
            var costInfoScript = TilemapUtilityScript.BaseTilemap.GetComponent<CostInfoScript>();
            if (costInfoScript != null)
            {
                candidateTiles = candidateTiles
                    .Where(tile =>
                    {
                        costInfoScript.costInfoDict.TryGetValue(tile, out var costInfo);
                        return costInfo == null || !costInfo.isOccupied;
                    })
                    .ToList();
            }
        }

        return candidateTiles;
    }
    public static List<Vector3Int> GetEffectAreaTiles(CardScript card, Vector3Int centerTile, EntityScript owner)
    {
        switch (card.cardData.targetingData.SelectionType)
        {
            case CardTargetSelection.Radius:
                return TilemapUtilityScript.GetTilesInRadius(centerTile, card.cardData.targetingData.area);
            case CardTargetSelection.Ring:
                return TilemapUtilityScript.GetTilesInRing(centerTile, card.cardData.targetingData.area);
            case CardTargetSelection.LineFree:
                return TilemapUtilityScript.GetTilesInLine(centerTile, centerTile, card.cardData.targetingData.area);
            case CardTargetSelection.LineSelf:
                return TilemapUtilityScript.GetTilesInLine(owner.GetComponent<EntityOnMap>().currentCell, centerTile, card.cardData.targetingData.area);
            default:
                return new List<Vector3Int> { centerTile };
        }
    }
    public static Vector3Int GetValidDropTarget(PointerEventData eventData, CardScript cardScript)
    {
        foreach (GameObject hoveredObject in eventData.hovered)
        {
            if (!hoveredObject.TryGetComponent(out DraggableTarget dt)) continue;

            switch (cardScript.cardData.targetingData.CardTargetType)
            {
                case CardTargetType.Entity:
                    if (dt.draggableTargetType == DraggableTargetType.CombatCharacter &&
                        hoveredObject.TryGetComponent(out EntityScript entity))
                    {
                        return entity.GetComponent<EntityOnMap>().currentCell;
                    }
                    break;

                case CardTargetType.CombatTile:
                    if (dt.draggableTargetType == DraggableTargetType.CombatTile)
                        return TilemapUtilityScript.BaseTilemap.WorldToCell(hoveredObject.transform.position);
                    break;
            }
        }

        return TilemapUtilityScript.InvalidPosition;
    }
}

}