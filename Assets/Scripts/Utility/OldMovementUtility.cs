using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using facingfate;


    public static class OldMovementUtility
    {
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

        public static PathData FindPath(Vector3Int startPos, Vector3Int goalPos, bool ignoreCost = false, bool walkClose = false, Stat movementCostModifier = null)
        {
            return FindPath((Vector3)startPos, (Vector3)goalPos, ignoreCost, walkClose, movementCostModifier);
        }

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

            return new PathData
            {
                Start = startPos,
                End = goalPos,
                Path = pathPositions,
                PathCost = pathPositions.Count
            };
        }

        public static PathData FindPathWithMaxLength(Vector3Int start, Vector3Int goal, int maxLength, bool ignoreCost = false, bool walkClose = false)
        {
            return FindPathWithMaxLength((Vector3)start, (Vector3)goal, maxLength, ignoreCost, walkClose);
        }

        public static PathData FindPathWithMaxLength(Vector3 start, Vector3 goal, int maxLength, bool ignoreCost = false, bool walkClose = false)
        {
            var pathData = FindPath(start, goal, ignoreCost, walkClose);

            if (pathData == null || pathData.Path == null || pathData.Path.Count == 0)
            {
                Debug.LogWarning($"[MovementUtility] No valid path found from {start} to {goal}");
                return new PathData();
            }

            Debug.Log($"[MovementUtility] Found path from {start} to {goal} with length {pathData.Path.Count} (max allowed: {maxLength})");

            if (pathData.Path.Count > maxLength)
            {
                pathData.Path = pathData.Path.Take(maxLength).ToList();
            }

            return pathData;
        }

        public static PathData FindLine(Vector3Int start, Vector3Int goal)
        {
            return FindLine((Vector3)start, (Vector3)goal);
        }

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

        public static PathData FindLineFromToWithLength(Vector3Int start, Vector3Int goal, int maxLength)
        {
            return FindLineFromToWithLength((Vector3)start, (Vector3)goal, maxLength);
        }

        public static PathData FindLineFromToWithLength(Vector3 start, Vector3 goal, int maxLength)
        {
            var pathData = FindLine(start, goal);

            if (pathData == null || pathData.Path == null || pathData.Path.Count == 0)
            {
                Debug.LogWarning($"[MovementUtility] No valid Line found from {start} to {goal}");
                return new PathData();
            }

            if (pathData.Path.Count > maxLength)
            {
                pathData.Path = pathData.Path.Take(maxLength).ToList();
            }

            return pathData;
        }

        public static void ForcedMove(ForcedMovementType type, EntityScript entity, Vector3Int ReferencePos, int Distance = 99, float speed = 3f)
        {
            ForcedMove(type, entity, (Vector3)ReferencePos, Distance, speed);
        }

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

        public static Vector3 FindPositionToBeMoveTo(EntityOnMap eom, Vector3 targetPosition)
        {
            if (IsPositionOnNavMesh(targetPosition))
            {
                Debug.Log($"[MovementUtility] Target position {targetPosition} is valid for teleportation.");
                return GetNavMeshPosition(targetPosition);
            }

            return GetNavMeshPosition(targetPosition);
        }
        public static void SwapLocations(EntityScript entityA, EntityScript entityB)
        {
            EntityOnMap entityOnMapA = entityA.GetComponent<EntityOnMap>();
            EntityOnMap entityOnMapB = entityB.GetComponent<EntityOnMap>();
            Vector3 posA = entityOnMapA.transform.position;
            Vector3 posB = entityOnMapB.transform.position;
            entityOnMapA.TeleportTo(posB);
            entityOnMapB.TeleportTo(posA);
        }

        #region Force Movement Types 
        public static PathData GetRandomInRange(Vector3Int pos, int distance)
        {
            return GetRandomInRange((Vector3)pos, distance);
        }

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

        public static PathData GetPathDataToCloserPosition(Vector3Int to, int distance, EntityOnMap entityOnMap)
        {
            return GetPathDataToCloserPosition((Vector3)to, distance, entityOnMap);
        }

        public static PathData GetPathDataToCloserPosition(Vector3 to, int distance, EntityOnMap entityOnMap)
        {
            PathData path = FindPathWithMaxLength(entityOnMap.transform.position, to, distance, walkClose: true);
            return path;
        }

        public static PathData GetFurtherPosition(Vector3Int from, int distance, EntityOnMap entityOnMap)
        {
            return GetFurtherPosition((Vector3)from, distance, entityOnMap);
        }

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

        private static PathData GetFleePosition(int distance, EntityOnMap entityOnMap, EntityScript entityScript)
        {
            List<EntityScript> allEntities = GameObject.FindObjectsByType<EntityScript>(0).ToList();
            PathData path = GetBestFleePath(entityOnMap.transform.position, allEntities, entityScript);
            return path;
        }

        public static PathData GetBestFleePath(Vector3Int virtualPosition, List<EntityScript> allEntities, EntityScript entity)
        {
            return GetBestFleePath((Vector3)virtualPosition, allEntities, entity);
        }

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

    public enum ForcedMovementType
    {
        Random,
        Targeted,
        Flee,
        Pull,
        Push,
        Jump,
        Teleport
    }
