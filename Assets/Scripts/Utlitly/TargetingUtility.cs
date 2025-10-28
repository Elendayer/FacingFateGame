using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
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
                    var radiusTiles = TilemapUtilityScript.GetTilesInRadius(pos, GetEffectiveRange(card, owner));
                    targets = GetEntitiesFromTiles(radiusTiles, allEntities);
                    break;
                case CardTargetSelection.Ring:
                    var ringTiles = TilemapUtilityScript.GetTilesInRing(pos, GetEffectiveRange(card, owner));
                    targets = GetEntitiesFromTiles(ringTiles, allEntities);
                    break;
                case CardTargetSelection.LineFree:
                    {
                        int effRange = GetEffectiveRange(card, owner);
                        var ownerCell = owner.GetComponent<EntityOnMap>().currentCell;

                        // Richtung aus Besitzer -> Maus/pos ableiten; fallback = (1,0,0)
                        Vector3Int dir = DirectionStep(ownerCell, pos);
                        if (dir == Vector3Int.zero) dir = Vector3Int.right;

                        // Linie vom gew�hlten Tile "pos" in diese Richtung aufspannen
                        var end = new Vector3Int(pos.x + dir.x * effRange, pos.y + dir.y * effRange, pos.z);
                        var lineTiles = TilemapUtilityScript.GetTilesInLine(pos, end, effRange);

                        int maxHits = Mathf.Max(1, card.cardData.targetingData.area);
                        targets = CollectLinearTargets(lineTiles, allEntities, card, maxHits);
                        break;
                    }
                case CardTargetSelection.LineSelf:
                    {
                        int effRange = GetEffectiveRange(card, owner);
                        var ownerCell = owner.GetComponent<EntityOnMap>().currentCell;

                        // Linie vom Besitzer in Richtung "pos"
                        var lineTiles = TilemapUtilityScript.GetTilesInLine(ownerCell, pos, effRange);

                        int maxHits = Mathf.Max(1, card.cardData.targetingData.area);
                        targets = CollectLinearTargets(lineTiles, allEntities, card, maxHits);
                        break;
                    }

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
                    return TilemapUtilityScript.GetTilesInRadius(centerTile, GetEffectiveRange(card, owner));
                case CardTargetSelection.Ring:
                    return TilemapUtilityScript.GetTilesInRing(centerTile, GetEffectiveRange(card, owner));
                case CardTargetSelection.LineFree:
                    return TilemapUtilityScript.GetTilesInLine(centerTile, centerTile, GetEffectiveRange(card, owner));
                case CardTargetSelection.LineSelf:
                    return TilemapUtilityScript.GetTilesInLine(owner.GetComponent<EntityOnMap>().currentCell, centerTile, GetEffectiveRange(card, owner));
                case CardTargetSelection.Cone:
                    return TilemapUtilityScript.GetTilesInCone(owner.GetComponent<EntityOnMap>().currentCell, centerTile, GetEffectiveRange(card, owner), card.cardData.targetingData.area);
                case CardTargetSelection.All:
                    return TilemapUtilityScript.GetAllValidTiles();
                default:
                    return new List<Vector3Int> { centerTile };
            }
        }

        #region Drop Validation
        public static Vector3Int GetValidTileDrop(PointerEventData eventData, CardScript cardScript)
        {
            var ownerEom = cardScript?.cardData?.Owner?.GetComponent<EntityOnMap>();
            if (ownerEom == null) return TilemapUtilityScript.InvalidPosition;


            foreach (GameObject hoveredObject in eventData.hovered)
            {
                Debug.Log($"[TargetingUtility] Hovered object: {hoveredObject.name}");
                if (!hoveredObject.TryGetComponent(out DraggableTarget _)) continue;

                var cell = TilemapUtilityScript.BaseTilemap.WorldToCell(hoveredObject.transform.position);

                Vector3Int cell = TilemapUtilityScript.BaseTilemap.WorldToCell(hoveredObject.transform.position);

                if (TilemapUtilityScript.FindPath(currentCell, cell, ignoreCost: true).Path.Count > cardScript.cardData.targetingData.range)
                {
                    Debug.Log($"[TargetingUtility] Target {cell} is out of range. (need <= {effRange}, got {path.Path?.Count ?? -1})");
                    continue;
                }

                return cell;
            }

            return TilemapUtilityScript.InvalidPosition;
        }

        public static Vector3Int GetValidEntityDrop(PointerEventData eventData, CardScript cardScript)
        {
            var ownerEom = cardScript?.cardData?.Owner?.GetComponent<EntityOnMap>();
            if (ownerEom == null) return TilemapUtilityScript.InvalidPosition;

            // SELF: keine Hover-Objekte, kein Range-Check � immer eigene Zelle
            if (IsSelf(cardScript))
                return ownerEom.currentCell;

            // Bisherige Logik f�r Nicht-Self
            var allEntities = UnityEngine.Object
                .FindObjectsByType<EntityOnMap>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .ToList();


            var currentCell = ownerEom.currentCell;
            int effRange = GetEffectiveRange(cardScript, cardScript.cardData.Owner);

            foreach (var hoveredObject in eventData.hovered)
            {
                Debug.Log($"[TargetingUtility] Hovered object: {hoveredObject.name}");
                if (!hoveredObject.TryGetComponent(out DraggableTarget _)) continue;

                var cell = TilemapUtilityScript.BaseTilemap.WorldToCell(hoveredObject.transform.position);

                var path = TilemapUtilityScript.FindPath(currentCell, cell, ignoreCost: true);
                if (path.Path == null || path.Path.Count > effRange)
                {
                    Debug.Log($"[TargetingUtility] Target {cell} is out of range. (need <= {effRange}, got {path.Path?.Count ?? -1})");
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