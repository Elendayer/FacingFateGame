using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
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

                        // Linie vom gewählten Tile "pos" in diese Richtung aufspannen
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
            int effRange = GetEffectiveRange(card, card.cardData.Owner);
            List < Vector3Int > candidateTiles = effRange <= 0
                 ? new List<Vector3Int> { targetCell }
                 : TilemapUtilityScript.GetTilesInRadius(targetCell, effRange);

            return candidateTiles;
        }
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
        public static Vector3Int GetValidTileDrop(PointerEventData eventData, CardScript cardScript )
        {
            List<EntityOnMap> allEntities = UnityEngine.Object.FindObjectsByType<EntityOnMap>(0).ToList();
            Vector3Int currentCell = cardScript.cardData.Owner.GetComponent<EntityOnMap>().currentCell;

            int effRange = GetEffectiveRange(cardScript, cardScript.cardData.Owner);
            Debug.Log($"[TargetingUtility] base={cardScript.cardData.targetingData.range} eff={effRange}");

            foreach (GameObject hoveredObject in eventData.hovered)
            {
                Debug.Log($"[TargetingUtility] Hovered object: {hoveredObject.name}");

                if (!hoveredObject.TryGetComponent(out DraggableTarget dt)) continue;

                Vector3Int cell = TilemapUtilityScript.BaseTilemap.WorldToCell(hoveredObject.transform.position);

                var path = TilemapUtilityScript.FindPath(currentCell, cell, ignoreCost: true);
                int pathLen = (path.Path == null) ? int.MaxValue : path.Path.Count;

                if (pathLen > effRange)
                {
                    Debug.Log($"[TargetingUtility] Target {cell} is out of range. (need <= {effRange}, got {pathLen})");
                    return TilemapUtilityScript.InvalidPosition;
                }

                return cell;
            }
            return TilemapUtilityScript.InvalidPosition;
        }
        public static Vector3Int GetValidEntityDrop(PointerEventData eventData, CardScript cardScript)
        {
            List<EntityOnMap> allEntities = UnityEngine.Object.FindObjectsByType<EntityOnMap>(0).ToList();
            Vector3Int currentCell = cardScript.cardData.Owner.GetComponent<EntityOnMap>().currentCell;

            int effRange = GetEffectiveRange(cardScript, cardScript.cardData.Owner);
            Debug.Log($"[TargetingUtility] base={cardScript.cardData.targetingData.range} eff={effRange}");

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
                int pathLen = (path.Path == null) ? int.MaxValue : path.Path.Count;

                if (pathLen > effRange)
                {
                    Debug.Log($"[TargetingUtility] Target {cell} is out of range. (need <= {effRange}, got {pathLen})");
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

        // Berechnet die effektive Reichweite einer Karte.
        // - Addiert Range-Buffs aus Class/Type/Identity
        // - Gilt NICHT für Self-Karten (Affiliation = Self)
        // --- typsichere, defensive Wrapper ---
        private static int SafeGetStat(EntityStats stats, CardClass key, StatAspect aspect)
        {
            try { return stats.GetStatValue(key, aspect); }
            catch (System.Exception e) { Debug.LogWarning($"[TargetingUtility] No class stat for '{key}' ({aspect}). 0 used. {e.Message}"); return 0; }
        }
        private static int SafeGetStat(EntityStats stats, CardType key, StatAspect aspect)
        {
            try { return stats.GetStatValue(key, aspect); }
            catch (System.Exception e) { Debug.LogWarning($"[TargetingUtility] No type stat for '{key}' ({aspect}). 0 used. {e.Message}"); return 0; }
        }
        private static int SafeGetStat(EntityStats stats, CardIdentity key, StatAspect aspect)
        {
            try { return stats.GetStatValue(key, aspect); }
            catch (System.Exception e) { Debug.LogWarning($"[TargetingUtility] No id stat for '{key}' ({aspect}). 0 used. {e.Message}"); return 0; }
        }

        // Nimmt eine Tile-Liste in Reihenfolge (Linie) und sammelt bis zu maxHits valide Entities
        private static List<EntityScript> CollectLinearTargets(
            List<Vector3Int> lineTiles,
            List<EntityScript> allEntities,
            CardScript card,
            int maxHits)
        {
            var result = new List<EntityScript>();
            if (lineTiles == null || allEntities == null || card == null) return result;

            foreach (var tile in lineTiles)
            {
                var hit = allEntities.FirstOrDefault(e =>
                    e != null &&
                    e.TryGetComponent<EntityOnMap>(out var eom) &&
                    eom.currentCell == tile);

                if (hit == null) continue;
                if (!VetTargetEntity(card, hit)) continue;

                result.Add(hit);
                if (result.Count >= maxHits) break; // nur 1 oder mehrere, je nach area
            }

            return result;
        }

        // Ein Schrittvektor (-1/0/1 pro Achse) von 'from' Richtung 'to'
        private static Vector3Int DirectionStep(Vector3Int from, Vector3Int to)
        {
            int dx = Mathf.Clamp(to.x - from.x, -1, 1);
            int dy = Mathf.Clamp(to.y - from.y, -1, 1);
            return new Vector3Int(dx, dy, 0);
        }

        // Effektive Reichweite unter Einbezug deiner Stat-Aspekte (Self-Karten bekommen keinen Bonus)
        private static int GetEffectiveRange(CardScript card, EntityScript owner)
        {
            int baseRange = card?.cardData?.targetingData?.range ?? 0;
            if (card?.cardData?.targetingData?.CardTargetAffiliation == CardTargetAffiliation.Self) return baseRange;
            if (owner == null || owner.entityStats == null) return baseRange;

            int bonus = 0;
            bonus += owner.entityStats.GetStatValue(card.cardData.cardClass, StatAspect.Range);
            bonus += owner.entityStats.GetStatValue(card.cardData.cardType, StatAspect.Range);
            foreach (var id in card.cardData.cardIdentities)
                bonus += owner.entityStats.GetStatValue(id, StatAspect.Range);

            return Mathf.Max(0, baseRange + bonus);
        }

    }
}