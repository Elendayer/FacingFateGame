using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

namespace Utility
{

    public static class TargetingUtility
    {
        #region Get Entities
        public static List<EntityScript> GetEntitiesFromPosition(CardScript card, Vector3Int pos, List<EntityScript> allEntities, EntityScript owner)
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

            return GetValidTargets(card, owner, targets);
        }
        public static List<EntityScript> GetEntitiesFromTiles(List<Vector3Int> tiles, List<EntityScript> allEntities)
        {
            return allEntities.Where(e => tiles.Contains(e.GetComponent<EntityOnMap>().currentCell)).ToList();
        }
        #endregion

        public static bool CheckForValidTarget(CardScript card, EntityScript target)
        {
            var owner = card.cardData.Owner;

            if (card == null || owner == null || target == null) return false;
            var targeting = card.cardData.targetingData;

            // Check for untargetable modifier
            var hasRef = target.HasReference(GameplayRef.untargetableByAll);
            if (hasRef.found)
            {
                return false;
            }

            // Check affiliation
            var aff = target.entityAffiliation;
            var ownerAff = owner.entityAffiliation;

            return IsValidAffiliation(targeting.CardTargetAffiliation, ownerAff, aff, owner, target);
        }

        public static List<EntityScript> GetValidTargets(CardScript card, EntityScript owner, List<EntityScript> allEntities)
        {
            if (card == null || owner == null || allEntities == null) return new List<EntityScript>();

            var targeting = card.cardData.targetingData;

            return allEntities.Where(target =>
            {
                if (target == null) return false;

                // Check for untargetable modifier
                var hasRef = target.HasReference(GameplayRef.untargetableByAll);
                if (hasRef.found) 
                { 
                    return false; 
                }

                // Check affiliation
                var aff = target.entityAffiliation;
                var ownerAff = owner.entityAffiliation;

                // Determine if target is valid based on affiliation
                bool valid = IsValidAffiliation(targeting.CardTargetAffiliation, ownerAff, aff, owner, target);
                return valid;
            }).ToList();
        }
        private static bool IsValidAffiliation(
            CardTargetAffiliation targetingAffiliation,
            EntityAffiliation ownerAffiliation,
            EntityAffiliation targetAffiliation,
            EntityScript owner,
            EntityScript target)
        {
            // Check for untargetable modifiers on the target
            var (hasUntargetableEnemies, _) = target.HasReference(GameplayRef.untargetableByEnemies);
            var (hasUntargetableAllies, _) = target.HasReference(GameplayRef.untargetableByAllies);

            // Base affiliation logic
            bool baseValid = targetingAffiliation switch
            {
                CardTargetAffiliation.Ally => targetAffiliation == ownerAffiliation && targetAffiliation != EntityAffiliation.Neutral,
                CardTargetAffiliation.Enemy => targetAffiliation != ownerAffiliation && targetAffiliation != EntityAffiliation.Neutral,
                CardTargetAffiliation.Self => target == owner,
                CardTargetAffiliation.All => true,
                CardTargetAffiliation.AllyNeutral => targetAffiliation == ownerAffiliation || targetAffiliation == EntityAffiliation.Neutral,
                CardTargetAffiliation.EnemyNeutral => targetAffiliation != ownerAffiliation || targetAffiliation == EntityAffiliation.Neutral,
                CardTargetAffiliation.AllyEnemy => targetAffiliation != EntityAffiliation.Neutral,
                _ => false
            };

            if (!baseValid)
                return false;

            // Apply untargetable logic
            bool isEnemyTarget = targetAffiliation != ownerAffiliation && targetAffiliation != EntityAffiliation.Neutral;
            bool isAllyTarget = targetAffiliation == ownerAffiliation && targetAffiliation != EntityAffiliation.Neutral;

            if (isEnemyTarget && hasUntargetableEnemies)
                return false;

            if (isAllyTarget && hasUntargetableAllies)
                return false;

            return true;
        }


        // Get affected tiles based on card targeting data
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

        #region Drop Validation
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
                    if (TargetingUtility.CheckForValidTarget(cardScript, entityOnTile.GetComponent<EntityScript>()))
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

        public static Vector3Int? GetHoveredTile(PointerEventData eventData)
        {
            foreach (GameObject hoveredObject in eventData.hovered)
            {
                if (hoveredObject.TryGetComponent(out DraggableTarget dt) &&
                    dt.draggableTargetType == DraggableTargetType.CombatTile)
                {
                    return TilemapUtilityScript.BaseTilemap.WorldToCell(hoveredObject.transform.position);
                }
            }
            return TilemapUtilityScript.InvalidPosition;
        }
        public static Vector3Int GetHoveredTile(Ray ray )
        {
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log($"[TargetingUtility] Raycast hit: {hit.collider.name}");
                // Check if the hit object is a draggable target of type CombatTile
                if (hit.collider.TryGetComponent<DraggableTarget>(out var dt) &&
                    dt.draggableTargetType == DraggableTargetType.CombatTile)
                {
                    // Convert world position to tilemap cell
                    return TilemapUtilityScript.BaseTilemap.WorldToCell(hit.collider.transform.position);
                }
            }

            return TilemapUtilityScript.InvalidPosition;
        }
    }
}