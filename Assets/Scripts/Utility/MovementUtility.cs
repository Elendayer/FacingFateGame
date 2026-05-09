using UnityEngine;
using facingfate;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

namespace facingfate
{

    #region PathData Definition

    public class NavMeshPathData
    {
        public Vector3 Start { get; set; }
        public Vector3 End { get; set; }
        public int PathCost { get; set; }
        public NavMeshPath CachedNavMeshPath { get; set; }
    }

    #endregion

    /// <summary>
    /// Physics-based movement utilities for NavMesh integration.
    /// Provides helper methods for calculating paths and distances using physics colliders and NavMesh.
    /// Combines pathfinding, distance calculation, and position validation utilities.
    /// </summary>
    public static class MovementUtility
    {
        #region NavMesh Position Utilities

        /// <summary>Get the closest valid position on NavMesh near the target position.</summary>
        private static Vector3 GetNavMeshPosition(Vector3 position, float searchRadius = 5f)
        {
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
            {
                return hit.position;
            }
            return position;
        }

        /// <summary>Check if a position is walkable on the NavMesh.</summary>
        private static bool IsPositionOnNavMesh(Vector3 position, float searchRadius = 5f)
        {
            return NavMesh.SamplePosition(position, out NavMeshHit _, searchRadius, NavMesh.AllAreas);
        }

        #endregion

        #region Pathfinding

        public static NavMeshPathData FindPath(Vector3 startPos, Vector3 goalPos, EntityStats entityStats, bool ignoreCost = false, bool walkClose = false)
        {
            startPos = GetNavMeshPosition(startPos);
            goalPos = GetNavMeshPosition(goalPos);

            if (!IsPositionOnNavMesh(startPos) || !IsPositionOnNavMesh(goalPos))
                return new NavMeshPathData();

            var path = new NavMeshPath();
            if (!NavMesh.CalculatePath(startPos, goalPos, NavMesh.AllAreas, path))
                return new NavMeshPathData();

            if (path.status == NavMeshPathStatus.PathPartial && !walkClose)
                return new NavMeshPathData();

            // Calculate path cost as (4 + modifier) stamina per meter of actual path distance
            float totalDistance = 0f;
            Vector3 previous = startPos;
            foreach (var corner in path.corners)
            {
                totalDistance += Vector3.Distance(previous, corner);
                previous = corner;
            }

            // Calculate movement cost modifier from EntityStats if provided
            float movementCostModifier = 0f;
            if (entityStats != null)
            {
                float flat = entityStats.MovementCostModifier_Flat.Value();
                float increase = entityStats.MovementCostModifier_Increase.Value();
                movementCostModifier = flat * (1f + (increase / 100f));
            }

            float costRate = (4f + movementCostModifier) * CalculateMultiplierProduct(entityStats?.MovementCostModifier_Multiplier);
            int pathCost = Mathf.Max(1, Mathf.RoundToInt(totalDistance * costRate));

            return new NavMeshPathData
            {
                Start = startPos,
                End = goalPos,
                PathCost = pathCost,
                CachedNavMeshPath = path
            };
        }

        /// <summary>
        /// Helper to calculate multiplier product from a Stat.
        /// </summary>
        private static float CalculateMultiplierProduct(Stat multiplierStat)
        {
            if (multiplierStat == null)
                return 1f;

            float product = 1f;
            var multipliers = multiplierStat.GetAllMultiplierValues();
            foreach (var mult in multipliers)
            {
                product *= (mult / 100f);
            }
            return product;
        }

        public static NavMeshPathData FindPathWithMaxLength(Vector3 start, Vector3 goal, float maxLength, EntityStats entityStats, bool ignoreCost = false, bool walkClose = false)
        {
            var pathData = FindPath(start, goal, entityStats, ignoreCost, walkClose);

            if (pathData?.CachedNavMeshPath == null || pathData.CachedNavMeshPath.corners.Length == 0)
            {
                return new NavMeshPathData();
            }

            // Truncate path based on maximum distance
            float accumulatedDistance = 0f;
            Vector3 previous = start;
            List<Vector3> truncatedPath = new List<Vector3>();
            bool pathTruncated = false;

            foreach (var corner in pathData.CachedNavMeshPath.corners)
            {
                float distanceToCorner = Vector3.Distance(previous, corner);

                if (accumulatedDistance + distanceToCorner > maxLength)
                {
                    // This corner exceeds the max length, don't add it
                    pathTruncated = true;
                    break;
                }

                truncatedPath.Add(corner);
                accumulatedDistance += distanceToCorner;
                previous = corner;
            }

            if (pathTruncated)
            {
                var truncatedEnd = truncatedPath.Count > 0 ? truncatedPath[truncatedPath.Count - 1] : start;

                // Recalculate cost based on truncated path
                float truncatedDistance = CalculateDistanceAlongPath(start, truncatedPath);
                int truncatedCost = Mathf.Max(1, Mathf.RoundToInt(truncatedDistance * 4f));

                // Return truncated path with cleared cache (since it no longer matches the original NavMeshPath)
                return new NavMeshPathData
                {
                    Start = start,
                    End = truncatedEnd,
                    PathCost = truncatedCost,
                    CachedNavMeshPath = null
                };
            }

            return pathData;
        }

        /// <summary>
        /// Calculates the distance along a path of waypoints.
        /// </summary>
        private static float CalculateDistanceAlongPath(Vector3 start, IEnumerable<Vector3> waypoints)
        {
            float totalDistance = 0f;
            Vector3 previous = start;

            foreach (var waypoint in waypoints)
            {
                totalDistance += Vector3.Distance(previous, waypoint);
                previous = waypoint;
            }

            return totalDistance;
        }

        /// <summary>
        /// Finds a line-of-sight path between two positions using NavMesh raycasts.
        /// </summary>
        public static NavMeshPathData FindLine(Vector3 start, Vector3 goal)
        {
            start = GetNavMeshPosition(start);
            goal = GetNavMeshPosition(goal);

            if (start == goal)
            {
                return new NavMeshPathData
                {
                    Start = start,
                    End = goal,
                    PathCost = 0,
                };
            }

            if (!NavMesh.Raycast(start, goal, out NavMeshHit hit, NavMesh.AllAreas))
            {
                var path = new NavMeshPath();
                if (NavMesh.CalculatePath(start, goal, NavMesh.AllAreas, path))
                {
                    return new NavMeshPathData
                    {
                        Start = start,
                        End = goal,
                        PathCost = 0,
                        CachedNavMeshPath = path
                    };
                }
            }

            return new NavMeshPathData();
        }


        /// <summary>
        /// Finds a line path limited by maximum length. Truncates if it exceeds maxLength.
        /// Cost is calculated as 4 stamina per meter of actual path length.
        /// Note: CachedNavMeshPath is cleared when path is truncated since it no longer matches waypoints.
        /// </summary>
        public static NavMeshPathData FindLineFromToWithLength(Vector3 start, Vector3 goal, int maxLength)
        {
            var pathData = FindLine(start, goal);

            if (pathData?.CachedNavMeshPath == null || pathData.CachedNavMeshPath.corners.Length == 0)
            {
                return new NavMeshPathData();
            }

            // If path fits within max length, no truncation needed
            if (pathData.CachedNavMeshPath.corners.Length <= maxLength)
            {
                return pathData;
            }

            // Truncate path to maxLength waypoints
            var truncatedWaypoints = pathData.CachedNavMeshPath.corners.Take(maxLength).ToArray();
            var truncatedEnd = truncatedWaypoints.Length > 0 ? truncatedWaypoints[truncatedWaypoints.Length - 1] : start;

            // Recalculate cost based on truncated path
            float truncatedDistance = CalculateDistanceAlongPath(start, truncatedWaypoints);
            int truncatedCost = Mathf.Max(1, Mathf.RoundToInt(truncatedDistance * 4f));

            // Return truncated path with cleared cache (since it no longer matches the original NavMeshPath)
            return new NavMeshPathData
            {
                Start = start,
                End = truncatedEnd,
                PathCost = truncatedCost,
                CachedNavMeshPath = null
            };
        }

        #endregion

        #region Distance & Reachability

        /// <summary>
        /// Calculates the distance along a NavMesh path between two positions.
        /// Returns -1 if no path exists.
        /// </summary>
        public static float GetPathDistance(Vector3 from, Vector3 to, int navMeshAreaMask = -1)
        {
            NavMeshPath path = new NavMeshPath();

            if (NavMesh.CalculatePath(from, to, navMeshAreaMask, path))
            {
                if (path.status == NavMeshPathStatus.PathComplete || path.status == NavMeshPathStatus.PathPartial)
                {
                    float distance = 0f;
                    Vector3 previous = from;

                    foreach (var corner in path.corners)
                    {
                        distance += Vector3.Distance(previous, corner);
                        previous = corner;
                    }

                    return distance;
                }
            }

            return -1f;
        }

        #endregion

        #region Forced Movement & Utility Methods

        /// <summary>
        /// Performs forced movement on an entity based on the specified movement type.
        /// </summary>
        public static void ForcedMove(ForcedMovementType type, EntityScript entity, Vector3 ReferencePos, float Distance = 99, float speed = 3f)
        {
            if (entity == null)
                return;

            EntityOnMap entityOnMap = entity.EntityOnMap;
            if (entityOnMap == null)
                return;

            NavMeshPathData pathData = new NavMeshPathData();
            Vector3 targetPosition = FindPositionToBeMoveTo(entityOnMap, ReferencePos);

            switch (type)
            {
                case ForcedMovementType.Random:
                    pathData = GetRandomInRange(entityOnMap.transform.position, Distance, entity.entityStats);
                    break;
                case ForcedMovementType.Targeted:
                    pathData = FindPathWithMaxLength(entityOnMap.transform.position, targetPosition, Distance, entity.entityStats);
                    break;
                case ForcedMovementType.Flee:
                    pathData = GetFleePosition(Distance, entityOnMap, entity);
                    break;
                case ForcedMovementType.Push:
                    pathData = GetFurtherPosition(ReferencePos, Distance, entity);
                    break;
                case ForcedMovementType.Pull:
                    pathData = GetPathDataToCloserPosition(ReferencePos, Distance, entity);
                    break;
                case ForcedMovementType.Jump:
                    ActionQueueUtility.EnqueueActionRoutine(entityOnMap, () => entityOnMap.StartJumpRoutine(targetPosition));
                    return;
                case ForcedMovementType.Teleport:
                    ActionQueueUtility.EnqueueAction(() => entityOnMap.TeleportTo(targetPosition));
                    return;
            }

            // Only enqueue movement if a valid path was found
            if (HasValidPath(pathData))
            {
                ActionQueueUtility.EnqueueMovement(entityOnMap, pathData);
            }
        }

        /// <summary>
        /// Helper to check if a NavMeshPathData has a valid path.
        /// </summary>
        private static bool HasValidPath(NavMeshPathData pathData)
        {
            if (pathData == null)
                return false;

            // A valid path must have a cached NavMeshPath with corners
            return pathData.CachedNavMeshPath != null && pathData.CachedNavMeshPath.corners.Length > 0;
        }

        #endregion

        #region Distance & Reachability

        /// <summary>
        /// Finds a valid position for movement/teleportation.
        /// </summary>
        public static Vector3 FindPositionToBeMoveTo(EntityOnMap eom, Vector3 targetPosition)
        {
            if (IsPositionOnNavMesh(targetPosition))
            {
                return GetNavMeshPosition(targetPosition);
            }

            return GetNavMeshPosition(targetPosition);
        }

        /// <summary>
        /// Swaps the locations of two entities.
        /// </summary>
        public static void SwapLocations(EntityScript entityA, EntityScript entityB)
        {
            EntityOnMap entityOnMapA = entityA.GetComponent<EntityOnMap>();
            EntityOnMap entityOnMapB = entityB.GetComponent<EntityOnMap>();
            Vector3 posA = entityOnMapA.transform.position;
            Vector3 posB = entityOnMapB.transform.position;
            entityOnMapA.TeleportTo(posB);
            entityOnMapB.TeleportTo(posA);
        }

        #endregion

        #region Force Movement Candidate Generation


        /// <summary>
        /// Gets a random reachable position within the specified distance.
        /// </summary>
        public static NavMeshPathData GetRandomInRange(Vector3 pos, float distance, EntityStats entityStats)
        {
            List<Vector3> possiblePositions = new List<Vector3>();

            for (float x = pos.x - distance; x <= pos.x + distance; x++)
            {
                for (float z = pos.z - distance; z <= pos.z + distance; z++)
                {
                    Vector3 candidate = new Vector3(x, pos.y, z);
                    if (Vector3.Distance(candidate, pos) <= distance && IsPositionOnNavMesh(candidate))
                    {
                        possiblePositions.Add(GetNavMeshPosition(candidate));
                    }
                }
            }

            if (possiblePositions.Count == 0)
                return new NavMeshPathData();

            Vector3 randomTarget = possiblePositions[new System.Random().Next(0, possiblePositions.Count)];
            NavMeshPathData pathData = FindPath(pos, randomTarget, entityStats);

            return pathData;
        }


        /// <summary>
        /// Gets a path to a closer position toward a target.
        /// </summary>
        public static NavMeshPathData GetPathDataToCloserPosition(Vector3 to, float distance, EntityScript entity)
        {
            NavMeshPathData path = FindPathWithMaxLength(entity.transform.position, to, distance, entity.entityStats, walkClose: true);
            return path;
        }


        /// <summary>
        /// Gets the furthest reachable position from a reference point.
        /// Prioritizes positions in the direction away from the reference point.
        /// </summary>
        public static NavMeshPathData GetFurtherPosition(Vector3 referencePos, float distance, EntityScript entity)
        {
            List<Vector3> targets = new List<Vector3>();

            var entityOriginalPos = entity.transform.position;
            var directionAwayFromReference = (entityOriginalPos - referencePos).normalized;

            for (float x = entityOriginalPos.x - distance; x <= entityOriginalPos.x + distance; x++)
            {
                for (float z = entityOriginalPos.z - distance; z <= entityOriginalPos.z + distance; z++)
                {
                    Vector3 candidate = new Vector3(x, entityOriginalPos.y, z);
                    if (Vector3.Distance(candidate, entity.transform.position) <= distance && IsPositionOnNavMesh(candidate))
                    {
                        targets.Add(GetNavMeshPosition(candidate));
                    }
                }
            }

            if (targets.Count == 0)
                return new NavMeshPathData();

            Vector3 furthest = targets
                .OrderByDescending(pos =>
                {
                    float distFromReference = Vector3.Distance(pos, referencePos);
                    float directionAlignment = Vector3.Dot((pos - entityOriginalPos).normalized, directionAwayFromReference);
                    return (distFromReference * 2f) + (directionAlignment * distance);
                })
                .First();

            NavMeshPathData path = FindPath(entity.transform.position, furthest, entity.entityStats);

            return path;
        }

        /// <summary>
        /// Gets the best flee position away from all hostile entities.
        /// </summary>
        private static NavMeshPathData GetFleePosition(float distance, EntityOnMap entityOnMap, EntityScript entityScript)
        {
            List<EntityScript> allEntities = UnityEngine.Object.FindObjectsByType<EntityScript>(0).ToList();
            NavMeshPathData path = GetBestFleePath(entityOnMap.transform.position, allEntities, entityScript);
            return path;
        }

        /// <summary>
        /// Gets the best flee path away from hostile entities.
        /// Scores positions based on distance from enemies and movement cost.
        /// </summary>
        public static NavMeshPathData GetBestFleePath(Vector3 virtualPosition, List<EntityScript> allEntities, EntityScript entity)
        {
            if (entity == null || entity.entityStats == null)
                return new NavMeshPathData();

            int hostileCount = allEntities.Count(e => e != null && e.entityAffiliation != entity.entityAffiliation);
            if (hostileCount == 0)
            {
                return new NavMeshPathData();
            }

            int maxFleeDistance = Mathf.Min(Mathf.RoundToInt(entity.entityStats.CurrentStamina), hostileCount);
            if (maxFleeDistance <= 0)
                return new NavMeshPathData();

            NavMeshPathData bestPath = null;
            float bestScore = float.MinValue;

            for (float dx = -maxFleeDistance; dx <= maxFleeDistance; dx++)
            {
                for (float dz = -maxFleeDistance; dz <= maxFleeDistance; dz++)
                {
                    Vector3 candidate = new Vector3(virtualPosition.x + dx, virtualPosition.y, virtualPosition.z + dz);

                    if (candidate == virtualPosition || !IsPositionOnNavMesh(candidate))
                        continue;

                    var pathData = FindPath(virtualPosition, candidate, entity.entityStats);
                    if (!HasValidPath(pathData))
                        continue;

                    int moveCost = pathData.PathCost;
                    if (moveCost == 0 || moveCost > entity.entityStats.CurrentStamina)
                        continue;

                    // Calculate minimum distance to any hostile
                    float minEnemyDist = float.MaxValue;
                    foreach (var e in allEntities)
                    {
                        if (e == null || e.entityAffiliation == entity.entityAffiliation)
                            continue;

                        EntityOnMap enemyEntityOnMap = e.EntityOnMap;
                        if (enemyEntityOnMap == null)
                            continue;

                        float distToEnemy = Vector3.Distance(enemyEntityOnMap.transform.position, candidate);
                        if (distToEnemy < minEnemyDist)
                            minEnemyDist = distToEnemy;
                    }

                    // Score based on distance from enemies (higher is better) minus movement cost
                    float score = (minEnemyDist * 2f) - moveCost;

                    // Heavy penalty for positions too close to enemies
                    if (minEnemyDist < 2f)
                        score -= 100f;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestPath = pathData;
                    }
                }
            }

            return bestPath ?? new NavMeshPathData();
        }

        #endregion
    }

    public enum ForcedMovementType
    {
        Random,
        Targeted,
        Flee,
        Push,
        Pull,
        Jump,
        Teleport
    }
}