using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utility
{
    public static class MovementUtility
    {
        public static void ForcedMove(ForcedMovementType type, EntityScript entity, Vector3Int ReferencePos, int Distance = 0, float speed = 3f)
        {
            EntityOnMap entityOnMap = entity.GetComponent<EntityOnMap>();

            switch (type)
            {
                case ForcedMovementType.Random:
                    entityOnMap.MoveTo(GetRandomInRange(entityOnMap.currentCell, Distance));
                    break;
                case ForcedMovementType.Targeted:
                    entityOnMap.MoveTo(ReferencePos, 20f);
                    break;
                case ForcedMovementType.Flee:
                    List< Vector3Int> vector3Ints = GetFleePosition( Distance, entityOnMap, entity);
                    entityOnMap.MoveToViaPath(vector3Ints);
                    break;
                case ForcedMovementType.Push:
                    entityOnMap.MoveToViaPath(GetFurtherPosition(ReferencePos, Distance, entityOnMap));
                    break;
                case ForcedMovementType.Pull:
                    entityOnMap.MoveToViaPath(GetCloserPosition(ReferencePos, Distance, entityOnMap));
                    break;
            }
        }

        public static void SwapLocations(EntityScript entityA, EntityScript entityB)
        {
            EntityOnMap entityOnMapA = entityA.GetComponent<EntityOnMap>();
            EntityOnMap entityOnMapB = entityB.GetComponent<EntityOnMap>();
            Vector3Int posA = entityOnMapA.currentCell;
            Vector3Int posB = entityOnMapB.currentCell;
            entityOnMapA.TeleportTo(posB);
            entityOnMapB.TeleportTo(posA);
        }

        #region Force Movement Types 
        public static Vector3Int GetRandomInRange(Vector3Int pos, int distance)
        {
            List<Vector3Int> possiblePositions = TilemapUtilityScript.GetTilesInRing(pos,  distance, 1 );

            return possiblePositions.Where(pos => TilemapUtilityScript.CostInfoScript.costInfoDict[pos].isOccupied == false).First();
        }
        public static List<Vector3Int> GetCloserPosition(Vector3Int to, int distance, EntityOnMap entityOnMap)
        {
            List<Vector3Int> path = TilemapUtilityScript.FindPathWithMaxLength(entityOnMap.currentCell, to, distance)?.Path;
            return path;
        }
        public static List<Vector3Int> GetFurtherPosition(Vector3Int from, int distance, EntityOnMap entityOnMap)
        {
            List<Vector3Int> targets = TilemapUtilityScript.GetTilesInRing(entityOnMap.currentCell,  distance , 1);
            
            // Filter out occupied tiles
            targets = targets
                .Where(pos => TilemapUtilityScript.CostInfoScript.costInfoDict.ContainsKey(pos)
                           && !TilemapUtilityScript.CostInfoScript.costInfoDict[pos].isOccupied)
                .ToList();

            if (targets.Count == 0)
                return null; // No valid target found

            // Find the tile furthest from 'from'
            Vector3Int furthest = targets
                .OrderByDescending(pos => Vector3Int.Distance(pos, from))
                .First();

            List<Vector3Int> path = TilemapUtilityScript.FindPath(entityOnMap.currentCell, furthest)?.Path;

            return path;
        }
        private static List<Vector3Int> GetFleePosition(int distance, EntityOnMap entityOnMap, EntityScript entityScript)
        {
            List<EntityScript> allEntities = GameObject.FindObjectsByType<EntityScript>(0).ToList();
            List<Vector3Int> path = GetBestFleePath(entityOnMap.currentCell, allEntities , entityScript);
            return path;
        }
        public static List<Vector3Int> GetBestFleePath(Vector3Int virtualPosition, List<EntityScript> allEntities, EntityScript entity)
        {
            // Count hostiles
            int hostileCount = allEntities.Count(e => e.entityAffiliation != entity.entityAffiliation);
            if (hostileCount == 0)
            {
                // No enemies — stay in place
                return new List<Vector3Int> { virtualPosition };
            }

            int maxFleeDistance = Mathf.Min(entity.entityStats.CurrentStamina, hostileCount);

            List<Vector3Int> bestPath = null;
            float bestScore = float.MinValue;

            for (int dx = -maxFleeDistance; dx <= maxFleeDistance; dx++)
            {
                for (int dy = -maxFleeDistance; dy <= maxFleeDistance; dy++)
                {
                    Vector3Int candidate = new Vector3Int(virtualPosition.x + dx, virtualPosition.y + dy, virtualPosition.z);

                    // Skip current position and invalid tiles
                    if (candidate == virtualPosition || !TilemapUtilityScript.BaseTilemap.HasTile(candidate))
                        continue;

                    // Pathfinding
                    var pathData = TilemapUtilityScript.FindPath(virtualPosition, candidate);
                    if (pathData == null || pathData.Path == null || pathData.Path.Count == 0)
                        continue;

                    int moveCost = pathData.PathCost;
                    if (moveCost == 0 || moveCost > entity.entityStats.CurrentStamina)
                        continue;

                    // Minimum distance to any hostile
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
                    if (minEnemyDist < 2f)
                        score -= 100f;

                    // Track best path
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestPath = pathData.Path;
                    }
                }
            }

            // Fallback: stay in place
            return bestPath ?? new List<Vector3Int> { virtualPosition };
        }
        #endregion


    }

    public enum ForcedMovementType
    {
        Random,
        Targeted,
        Flee,
        Pull,
        Push,
    }
}