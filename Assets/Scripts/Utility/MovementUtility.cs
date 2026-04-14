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

    /// <summary>
    /// Finds a path from start to goal position with optional movement cost modifiers.
    /// </summary>
    public static PathData FindPath(Vector3Int startPos, Vector3Int goalPos, bool ignoreCost = false, bool walkClose = false, Stat movementCostModifier = null)
    {
        return FindPath((Vector3)startPos, (Vector3)goalPos, ignoreCost, walkClose, movementCostModifier);
    }

    /// <summary>
    /// Finds a path from start to goal position with optional movement cost modifiers.
    /// Returns PathData with path corners and cost information.
    /// Cost is calculated as 4 stamina per meter of path length.
    /// </summary>
    public static PathData FindPath(Vector3 startPos, Vector3 goalPos, bool ignoreCost = false, bool walkClose = false, Stat movementCostModifier = null)
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

        // Calculate path cost as 4 stamina per meter of actual path distance
        float totalDistance = 0f;
        Vector3 previous = startPos;
        foreach (var corner in path.corners)
        {
            totalDistance += Vector3.Distance(previous, corner);
            previous = corner;
        }
        int pathCost = Mathf.Max(1, Mathf.RoundToInt(totalDistance * 4f));

        return new PathData
        {
            Start = startPos,
            End = goalPos,
            Path = pathPositions,
            PathCost = pathCost
        };
    }

    /// <summary>
    /// Finds a path limited by maximum length.
    /// </summary>
    public static PathData FindPathWithMaxLength(Vector3Int start, Vector3Int goal, int maxLength, bool ignoreCost = false, bool walkClose = false)
    {
        return FindPathWithMaxLength((Vector3)start, (Vector3)goal, maxLength, ignoreCost, walkClose);
    }

    /// <summary>
    /// Finds a path limited by maximum length. Truncates path if it exceeds maxLength.
    /// Cost is calculated as 4 stamina per meter of actual path length.
    /// </summary>
    public static PathData FindPathWithMaxLength(Vector3 start, Vector3 goal, int maxLength, bool ignoreCost = false, bool walkClose = false)
    {
        var pathData = FindPath(start, goal, ignoreCost, walkClose);

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
        }

        return pathData;
    }

    /// <summary>
    /// Finds a line-of-sight path between two positions.
    /// </summary>
    public static PathData FindLine(Vector3Int start, Vector3Int goal)
    {
        return FindLine((Vector3)start, (Vector3)goal);
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

    /// <summary>
    /// Checks if a position is reachable from another position on the NavMesh.
    /// </summary>
    public static bool IsReachable(Vector3 from, Vector3 to, int navMeshAreaMask = -1)
    {
        NavMeshPath path = new NavMeshPath();

        if (NavMesh.CalculatePath(from, to, navMeshAreaMask, path))
        {
            return path.status == NavMeshPathStatus.PathComplete;
        }

        return false;
    }

    /// <summary>
    /// Checks if an entity can reach all positions in a list from its current position.
    /// </summary>
    public static bool CanReachAllPositions(Vector3 from, Vector3[] destinations, int navMeshAreaMask = -1)
    {
        foreach (var dest in destinations)
        {
            if (!IsReachable(from, dest, navMeshAreaMask))
                return false;
        }

        return true;
    }

    #endregion

    #region Position Sampling

    /// <summary>
    /// Samples a position on the NavMesh near the given position.
    /// Useful for converting world positions to valid NavMesh positions.
    /// </summary>
    public static bool TryGetNavMeshPosition(Vector3 position, out Vector3 navMeshPosition, float maxDistance = 5f)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, maxDistance, NavMesh.AllAreas))
        {
            navMeshPosition = hit.position;
            return true;
        }

        navMeshPosition = position;
        return false;
    }

    /// <summary>
    /// Gets the closest reachable position to a target from a starting position.
    /// </summary>
    public static Vector3 GetClosestReachablePosition(Vector3 from, Vector3 target, int navMeshAreaMask = -1, float searchRadius = 20f)
    {
        // Check if target is already reachable
        if (IsReachable(from, target, navMeshAreaMask))
        {
            return target;
        }

        // Search for nearby reachable positions
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, searchRadius, navMeshAreaMask))
        {
            if (IsReachable(from, hit.position, navMeshAreaMask))
            {
                return hit.position;
            }
        }

        return from; // Return starting position if nothing else is reachable
    }

    #endregion

    #region Forced Movement & Utility Methods

    /// <summary>
    /// Performs forced movement on an entity based on the specified movement type.
    /// </summary>
    public static void ForcedMove(ForcedMovementType type, EntityScript entity, Vector3 ReferencePos, int Distance = 99, float speed = 3f)
    {
        EntityOnMap entityOnMap = entity.GetComponent<EntityOnMap>();
        PathData pathData = new();

        Vector3 targetPosition = FindPositionToBeMoveTo(entityOnMap, ReferencePos);

        switch (type)
        {
            case ForcedMovementType.Random:
                pathData = GetRandomInRange(entityOnMap.transform.position, Distance);
                ActionQueueUtility.EnqueueMovement(entityOnMap, pathData);
                break;
            case ForcedMovementType.Targeted:
                pathData = FindPathWithMaxLength(entityOnMap.transform.position, targetPosition, Distance);
                ActionQueueUtility.EnqueueMovement(entityOnMap, pathData);
                break;
            case ForcedMovementType.Flee:
                pathData = GetFleePosition(Distance, entityOnMap, entity);
                ActionQueueUtility.EnqueueMovement(entityOnMap, pathData);
                break;
            case ForcedMovementType.Push:
                pathData = GetFurtherPosition(ReferencePos, Distance, entityOnMap);
                ActionQueueUtility.EnqueueMovement(entityOnMap, pathData);
                break;
            case ForcedMovementType.Pull:
                pathData = GetPathDataToCloserPosition(ReferencePos, Distance, entityOnMap);
                ActionQueueUtility.EnqueueMovement(entityOnMap, pathData);
                break;
            case ForcedMovementType.Jump:
                ActionQueueUtility.EnqueueActionRoutine(entityOnMap, () => entityOnMap.StartJumpRoutine(targetPosition));
                break;
            case ForcedMovementType.Teleport:
                ActionQueueUtility.EnqueueAction(() => { entityOnMap.TeleportTo(targetPosition); });
                break;
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
    public static PathData GetRandomInRange(Vector3Int pos, int distance)
    {
        return GetRandomInRange((Vector3)pos, distance);
    }

    /// <summary>
    /// Gets a random reachable position within the specified distance.
    /// </summary>
    public static PathData GetRandomInRange(Vector3 pos, int distance)
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
        PathData pathData = FindPath(pos, randomTarget);

        return pathData;
    }

    /// <summary>
    /// Gets a path to a closer position toward a target.
    /// </summary>
    public static PathData GetPathDataToCloserPosition(Vector3Int to, int distance, EntityOnMap entityOnMap)
    {
        return GetPathDataToCloserPosition((Vector3)to, distance, entityOnMap);
    }

    /// <summary>
    /// Gets a path to a closer position toward a target.
    /// </summary>
    public static PathData GetPathDataToCloserPosition(Vector3 to, int distance, EntityOnMap entityOnMap)
    {
        PathData path = FindPathWithMaxLength(entityOnMap.transform.position, to, distance, walkClose: true);
        return path;
    }

    /// <summary>
    /// Gets the furthest reachable position from a reference point.
    /// </summary>
    public static PathData GetFurtherPosition(Vector3Int from, int distance, EntityOnMap entityOnMap)
    {
        return GetFurtherPosition((Vector3)from, distance, entityOnMap);
    }

    /// <summary>
    /// Gets the furthest reachable position from a reference point.
    /// </summary>
    public static PathData GetFurtherPosition(Vector3 from, int distance, EntityOnMap entityOnMap)
    {
        List<Vector3> targets = new List<Vector3>();

        for (float x = entityOnMap.transform.position.x - distance; x <= entityOnMap.transform.position.x + distance; x++)
        {
            for (float z = entityOnMap.transform.position.z - distance; z <= entityOnMap.transform.position.z + distance; z++)
            {
                Vector3 candidate = new Vector3(x, entityOnMap.transform.position.y, z);
                if (Vector3.Distance(candidate, entityOnMap.transform.position) <= distance && IsPositionOnNavMesh(candidate))
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

        PathData path = FindPath(entityOnMap.transform.position, furthest);

        return path;
    }

    /// <summary>
    /// Gets the best flee position away from all hostile entities.
    /// </summary>
    private static PathData GetFleePosition(int distance, EntityOnMap entityOnMap, EntityScript entityScript)
    {
        List<EntityScript> allEntities = UnityEngine.Object.FindObjectsByType<EntityScript>(0).ToList();
        PathData path = GetBestFleePath(entityOnMap.transform.position, allEntities, entityScript);
        return path;
    }

    /// <summary>
    /// Gets the best flee path using integer grid positions.
    /// </summary>
    public static PathData GetBestFleePath(Vector3Int virtualPosition, List<EntityScript> allEntities, EntityScript entity)
    {
        return GetBestFleePath((Vector3)virtualPosition, allEntities, entity);
    }

    /// <summary>
    /// Gets the best flee path away from hostile entities.
    /// Scores positions based on distance from enemies and movement cost.
    /// </summary>
    public static PathData GetBestFleePath(Vector3 virtualPosition, List<EntityScript> allEntities, EntityScript entity)
    {
        int hostileCount = allEntities.Count(e => e.entityAffiliation != entity.entityAffiliation);
        if (hostileCount == 0)
        {
            return new();
        }

        int maxFleeDistance = Mathf.Min(Mathf.RoundToInt(entity.entityStats.CurrentStamina), hostileCount);

        PathData bestPath = null;
        float bestScore = float.MinValue;

        for (float dx = -maxFleeDistance; dx <= maxFleeDistance; dx++)
        {
            for (float dz = -maxFleeDistance; dz <= maxFleeDistance; dz++)
            {
                Vector3 candidate = new Vector3(virtualPosition.x + dx, virtualPosition.y, virtualPosition.z + dz);

                if (candidate == virtualPosition || !IsPositionOnNavMesh(candidate))
                    continue;

                var pathData = FindPath(virtualPosition, candidate);
                if (pathData == null || pathData.Path == null || pathData.Path.Count == 0)
                    continue;

                int moveCost = pathData.PathCost;
                if (moveCost == 0 || moveCost > entity.entityStats.CurrentStamina)
                    continue;

                float minEnemyDist = float.MaxValue;
                foreach (var e in allEntities)
                {
                    if (e.entityAffiliation == entity.entityAffiliation)
                        continue;

                    float distToEnemy = Vector3.Distance(e.GetComponent<EntityOnMap>().transform.position, candidate);
                    if (distToEnemy < minEnemyDist)
                        minEnemyDist = distToEnemy;
                }

                float score = (minEnemyDist * 2f) - moveCost;
                if (minEnemyDist < 2f)
                    score -= 100f;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestPath = pathData;
                }
            }
        }

        if (bestPath != null)
        {
            return bestPath;
        }

        return new();
    }

    #endregion
}