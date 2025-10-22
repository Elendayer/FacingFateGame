using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.EventTrigger;

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
                    targets = GetEntitiesFromTiles(new() { pos }, allEntities);
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

            return VetTargetsEntities(card, targets);
        }
        public static List<EntityScript> GetEntitiesFromTiles(List<Vector3Int> tiles, List<EntityScript> allEntities)
        {
            return allEntities.Where(e => tiles.Contains(e.GetComponent<EntityOnMap>().currentCell)).ToList();
        }
        public static List<EntityScript> VetTargetsEntities(CardScript card, List<EntityScript> preTargets)
        {
            var owner = card.cardData.Owner;

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
        public static bool VetTargetEntity(CardScript card, EntityScript target)
        {
            var owner = card.cardData.Owner;

            if (card == null || owner == null || target == null) return false;
            var targeting = card.cardData.targetingData;
            EntityAffiliation ownerAffiliation = owner.entityAffiliation;
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
                    return TilemapUtilityScript.GetTilesInLine(owner.GetComponent<EntityOnMap>().currentCell, centerTile, card.cardData.targetingData.range);
                case CardTargetSelection.Cone:
                    return TilemapUtilityScript.GetTilesInCone(owner.GetComponent<EntityOnMap>().currentCell, centerTile, card.cardData.targetingData.range, card.cardData.targetingData.area);
                case CardTargetSelection.All:
                    return TilemapUtilityScript.GetAllValidTiles();
                default:
                    return new List<Vector3Int> { centerTile };
            }
        }
        public static Vector3Int GetValidTileDrop(PointerEventData eventData, CardScript cardScript)
        {
            List<EntityOnMap> allEntities = Object.FindObjectsByType<EntityOnMap>(0).ToList();
            Vector3Int currentCell = cardScript.cardData.Owner.GetComponent<EntityOnMap>().currentCell;

            foreach (GameObject hoveredObject in eventData.hovered)
            {
                Debug.Log($"[TargetingUtility] Hovered object: {hoveredObject.name}");

                if (!hoveredObject.TryGetComponent(out DraggableTarget dt)) continue;

                Vector3Int cell = TilemapUtilityScript.BaseTilemap.WorldToCell(hoveredObject.transform.position);

                if (TilemapUtilityScript.FindPath(currentCell, cell, ignoreCost: true).Path.Count > cardScript.cardData.targetingData.range)
                {
                    Debug.Log($"[TargetingUtility] Target {cell} is out of range.");
                    return TilemapUtilityScript.InvalidPosition;
                }

                return cell;
            }
            return TilemapUtilityScript.InvalidPosition;
        }
        public static Vector3Int GetValidEntityDrop(PointerEventData eventData, CardScript cardScript)
        {
            List<EntityOnMap> allEntities = Object.FindObjectsByType<EntityOnMap>(0).ToList();
            Vector3Int currentCell = cardScript.cardData.Owner.GetComponent<EntityOnMap>().currentCell;

            foreach (GameObject hoveredObject in eventData.hovered)
            {
                Debug.Log($"[TargetingUtility] Hovered object: {hoveredObject.name}");

                // Only consider tiles with DraggableTarget component
                if (!hoveredObject.TryGetComponent(out DraggableTarget dt))
                    continue;

                // Convert hovered object position to tile cell
                Vector3Int cell = TilemapUtilityScript.BaseTilemap.WorldToCell(hoveredObject.transform.position);

                // Check range
                var path = TilemapUtilityScript.FindPath(currentCell, cell, ignoreCost: true);
                if (path.Path == null || path.Path.Count > cardScript.cardData.targetingData.range)
                {
                    Debug.Log($"[TargetingUtility] Target {cell} is out of range.");
                    continue;
                }

                // Check if there is an entity on this tile
                EntityOnMap entityOnTile = allEntities.FirstOrDefault(e => e.currentCell == cell);
                if (entityOnTile != null)
                {
                    if (TargetingUtility.VetTargetEntity(cardScript, entityOnTile.GetComponent<EntityScript>()))
                    {
                        return cell;
                    }
                }
            }

            // No valid entity found
            return TilemapUtilityScript.InvalidPosition;
        }

        public static Vector3Int GetValidGroundDrop(PointerEventData eventData, CardScript cardScript)
        {
            foreach (GameObject hoveredObject in eventData.hovered)
            {
                Debug.Log($"[TargetingUtility] Hovered object: {hoveredObject.name}");

                // Only consider tiles with DraggableTarget component
                if (!hoveredObject.TryGetComponent(out DraggableTarget dt))
                    continue;

                // Convert hovered object position to tile cell
                Vector3Int cell = TilemapUtilityScript.BaseTilemap.WorldToCell(hoveredObject.transform.position);
                return cell;
            }
            return TilemapUtilityScript.InvalidPosition;
        }
    }
}