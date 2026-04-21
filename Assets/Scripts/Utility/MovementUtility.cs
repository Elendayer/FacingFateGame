using UnityEngine;
using facingfate;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

#region PathData Definition

public class PathData
{
    public Vector3 Start { get; set; }
    public Vector3 End { get; set; }
    public List<Vector3> Path { get; set; }
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

    public static PathData FindPath(Vector3 startPos, Vector3 goalPos, EntityStats entityStats, bool ignoreCost = false, bool walkClose = false)
    {
        startPos = GetNavMeshPosition(startPos);
        goalPos = GetNavMeshPosition(goalPos);

        if (!IsPositionOnNavMesh(startPos) || !IsPositionOnNavMesh(goalPos))
            return new PathData();

        var path = new NavMeshPath();
        if (!NavMesh.CalculatePath(startPos, goalPos, NavMesh.AllAreas, path))
            return new PathData();

        if (path.status == NavMeshPathStatus.PathPartial && !walkClose)
            return new PathData();

        var cornersList = new List<Vector3>(path.corners);
        var pathPositions = cornersList.ConvertAll(v => v);

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
            float multiplierProduct = CalculateMultiplierProduct(entityStats.MovementCostModifier_Multiplier);
            movementCostModifier = flat * (1f + (increase / 100f)) * multiplierProduct;
        }

        float costRate = 4f + movementCostModifier;
        int pathCost = Mathf.Max(1, Mathf.RoundToInt(totalDistance * costRate));

        return new PathData
        {
            Start = startPos,
            End = goalPos,
            Path = pathPositions,
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

    public static PathData FindPathWithMaxLength(Vector3 start, Vector3 goal, float maxLength, EntityStats entityStats, bool ignoreCost = false, bool walkClose = false)
    {
        var pathData = FindPath(start, goal, entityStats, ignoreCost, walkClose);

        if (pathData == null || pathData.Path == null || pathData.Path.Count == 0)
        {
            return new PathData();
        }

        // Truncate path based on maximum distance
        float accumulatedDistance = 0f;
        Vector3 previous = start;
        List<Vector3> truncatedPath = new List<Vector3>();
        bool pathTruncated = false;

        foreach (var corner in pathData.Path)
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
            pathData.Path = truncatedPath;
            // Recalculate cost based on truncated path
            float truncatedDistance = 0f;
            previous = start;
            foreach (var corner in pathData.Path)
            {
                truncatedDistance += Vector3.Distance(previous, corner);
                previous = corner;
            }
            pathData.PathCost = Mathf.Max(1, Mathf.RoundToInt(truncatedDistance * 4f));

            // Clear cached path since truncation invalidates it
            pathData.CachedNavMeshPath = null;
        }

        return pathData;
    }

    /// <summary>
    /// Finds a line-of-sight path between two positions using NavMesh raycasts.
    /// </summary>
    public static PathData FindLine(Vector3 start, Vector3 goal)
    {
        start = GetNavMeshPosition(start);
        goal = GetNavMeshPosition(goal);

        if (start == goal)
        {
            return new PathData
            {
                Start = start,
                End = goal,
                Path = new List<Vector3> { },
                PathCost = 0,
            };
        }

        if (!NavMesh.Raycast(start, goal, out NavMeshHit hit, NavMesh.AllAreas))
        {
            var path = new NavMeshPath();
            if (NavMesh.CalculatePath(start, goal, NavMesh.AllAreas, path))
            {
                var corners = new List<Vector3>(path.corners);
                var pathPositions = corners.ConvertAll(v => v);

                return new PathData
                {
                    Start = start,
                    End = goal,
                    Path = pathPositions,
                    PathCost = 0,
                    CachedNavMeshPath = path
                };
            }
        }

        return new PathData();
    }

    /// <summary>
    /// Finds a line path limited by maximum length.
    /// </summary>
    public static PathData FindLineFromToWithLength(Vector3Int start, Vector3Int goal, int maxLength)
    {
        return FindLineFromToWithLength((Vector3)start, (Vector3)goal, maxLength);
    }

    /// <summary>
    /// Finds a line path limited by maximum length. Truncates if it exceeds maxLength.
    /// Cost is calculated as 4 stamina per meter of actual path length.
    /// Note: CachedNavMeshPath is cleared when path is truncated since it no longer matches waypoints.
    /// </summary>
    public static PathData FindLineFromToWithLength(Vector3 start, Vector3 goal, int maxLength)
    {
        var pathData = FindLine(start, goal);

        if (pathData == null || pathData.Path == null || pathData.Path.Count == 0)
        {
            return new PathData();
        }

        if (pathData.Path.Count > maxLength)
        {
            pathData.Path = pathData.Path.Take(maxLength).ToList();
            // Recalculate cost based on truncated path
            float truncatedDistance = 0f;
            Vector3 previous = start;
            foreach (var corner in pathData.Path)
            {
                truncatedDistance += Vector3.Distance(previous, corner);
                previous = corner;
            }
            pathData.PathCost = Mathf.Max(1, Mathf.RoundToInt(truncatedDistance * 4f));

            // Clear cached path since truncation invalidates it
            pathData.CachedNavMeshPath = null;
        }

        return pathData;
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

        PathData pathData = new PathData();
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
        if (pathData?.Path != null && pathData.Path.Count > 0)
        {
            ActionQueueUtility.EnqueueMovement(entityOnMap, pathData);
        }
    }

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
    public static PathData GetRandomInRange(Vector3 pos, float distance, EntityStats entityStats)
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
            return new PathData();

        Vector3 randomTarget = possiblePositions[new System.Random().Next(0, possiblePositions.Count)];
        PathData pathData = FindPath(pos, randomTarget, entityStats);

        return pathData;
    }


    /// <summary>
    /// Gets a path to a closer position toward a target.
    /// </summary>
    public static PathData GetPathDataToCloserPosition(Vector3 to, float distance, EntityScript entity)
    {
        PathData path = FindPathWithMaxLength(entity.transform.position, to, distance,entity.entityStats, walkClose: true);
        return path;
    }


    /// <summary>
    /// Gets the furthest reachable position from a reference point.
    /// </summary>
    public static PathData GetFurtherPosition(Vector3 from, float distance, EntityScript entity)
    {
        List<Vector3> targets = new List<Vector3>();

        for (float x = entity.transform.position.x - distance; x <= entity.transform.position.x + distance; x++)
        {
            for (float z = entity.transform.position.z - distance; z <= entity.transform.position.z + distance; z++)
            {
                Vector3 candidate = new Vector3(x, entity.transform.position.y, z);
                if (Vector3.Distance(candidate, entity.transform.position) <= distance && IsPositionOnNavMesh(candidate))
                {
                    targets.Add(GetNavMeshPosition(candidate));
                }
            }
        }

        if (targets.Count == 0)
            return new PathData();

        Vector3 furthest = targets
            .OrderByDescending(pos => Vector3.Distance(pos, from))
            .First();

        PathData path = FindPath(entity.transform.position, furthest, entity.entityStats);

        return path;
    }

    /// <summary>
    /// Gets the best flee position away from all hostile entities.
    /// </summary>
    private static PathData GetFleePosition(float distance, EntityOnMap entityOnMap, EntityScript entityScript)
    {
        List<EntityScript> allEntities = UnityEngine.Object.FindObjectsByType<EntityScript>(0).ToList();
        PathData path = GetBestFleePath(entityOnMap.transform.position, allEntities, entityScript);
        return path;
    }

    /// <summary>
    /// Gets the best flee path away from hostile entities.
    /// Scores positions based on distance from enemies and movement cost.
    /// </summary>
    public static PathData GetBestFleePath(Vector3 virtualPosition, List<EntityScript> allEntities, EntityScript entity)
    {
        if (entity == null || entity.entityStats == null)
            return new PathData();

        int hostileCount = allEntities.Count(e => e != null && e.entityAffiliation != entity.entityAffiliation);
        if (hostileCount == 0)
        {
            return new PathData();
        }

        int maxFleeDistance = Mathf.Min(Mathf.RoundToInt(entity.entityStats.CurrentStamina), hostileCount);
        if (maxFleeDistance <= 0)
            return new PathData();

        PathData bestPath = null;
        float bestScore = float.MinValue;

        for (float dx = -maxFleeDistance; dx <= maxFleeDistance; dx++)
        {
            for (float dz = -maxFleeDistance; dz <= maxFleeDistance; dz++)
            {
                Vector3 candidate = new Vector3(virtualPosition.x + dx, virtualPosition.y, virtualPosition.z + dz);

                if (candidate == virtualPosition || !IsPositionOnNavMesh(candidate))
                    continue;

                var pathData = FindPath(virtualPosition, candidate, entity.entityStats);
                if (pathData?.Path == null || pathData.Path.Count == 0)
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

        return bestPath ?? new PathData();
    }

    #endregion
}