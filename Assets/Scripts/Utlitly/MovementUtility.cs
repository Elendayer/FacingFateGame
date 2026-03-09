using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using facingfate;

namespace Utility
{
    public static class MovementUtility
    {
        public static TileInfoScript CostInfoScript = TilemapUtilityScript.BaseTilemap?.GetComponent<TileInfoScript>();

        // >>> CONFIG: Point-top offset type (Odd-R by default). Flip if your rows are shifted the other way.
        public static bool UseOddROffset = true; // true = Odd-R, false = Even-R

        /// <summary>Optionally call this once at startup if you need Even-R instead of Odd-R.</summary>
        public static void ConfigurePointTop(bool useOddR) => UseOddROffset = useOddR;

        // Cube directions (point-top agnostic)
        public static readonly Vector3Int[] CubeDirs = new Vector3Int[]
        {
        new Vector3Int(+1, -1, 0),
        new Vector3Int(+1, 0, -1),
        new Vector3Int(0, +1, -1),
        new Vector3Int(-1, +1, 0),
        new Vector3Int(-1, 0, +1),
        new Vector3Int(0, -1, +1)
        };

        #region Offset <-> Cube (Point-Top)
        // Odd-R and Even-R conversions per redblobgames
        private static Vector3Int OffsetToCube_PointTop(Vector3Int off, bool oddR)
        {
            int col = off.x;
            int row = off.y;
            int x, z = row, y;
            if (oddR)
            {
                x = col - (row - (row & 1)) / 2; // Odd-R
            }
            else
            {
                x = col - (row + (row & 1)) / 2; // Even-R
            }
            y = -x - z;
            return new Vector3Int(x, y, z);
        }

        private static Vector3Int CubeToOffset_PointTop(Vector3Int cube, bool oddR)
        {
            int x = cube.x;
            int z = cube.z;
            int col, row = z;
            if (oddR)
            {
                col = x + (z - (z & 1)) / 2; // Odd-R
            }
            else
            {
                col = x + (z + (z & 1)) / 2; // Even-R
            }
            return new Vector3Int(col, row, 0);
        }
        #endregion

        // Add this method to TilemapUtilityScript
        public static PathData FindPath(Vector3Int startPos, Vector3Int goalPos, bool ignoreCost = false, bool walkClose = false, Stat movementCostModifier = null)
        {
            var costDict = CostInfoScript?.tileInfoDict;

            if (costDict == null)
                return new PathData();

            // convert positions to tile index
            if (!TryGetTileIndex(startPos, out int start))
                return new PathData();

            if (!TryGetTileIndex(goalPos, out int goal))
                return new PathData();

            var openSet = new PriorityQueue<int>();
            openSet.Enqueue(start, 0);

            var cameFrom = new Dictionary<int, int>();
            var gScore = new Dictionary<int, int> { [start] = 0 };

            while (openSet.Count > 0)
            {
                int current = openSet.Dequeue();

                if (!gScore.TryGetValue(current, out int currentG))
                    continue;

                if (current == goal)
                {
                    return BuildPath(cameFrom, start, goal, ignoreCost, movementCostModifier);
                }

                TileInfo currentTile = costDict[current];

                // use cached cube coordinate from TileInfo to avoid repeated conversions
                Vector3Int cube = currentTile.cube;

                for (int dir = 0; dir < CubeDirs.Length; dir++)
                {
                    Vector3Int neighborCube = cube + CubeDirs[dir];
                    Vector3Int neighborOffset = CubeToOffset_PointTop(neighborCube, UseOddROffset);

                    if (!TryGetTileIndex(neighborOffset, out int neighborIndex))
                        continue;

                    TileInfo neighborTile = costDict[neighborIndex];

                    int stepCost;

                    if (ignoreCost)
                        stepCost = 1;
                    else if (movementCostModifier != null)
                        stepCost = movementCostModifier.ApplyFinalValue(neighborTile.costCheck);
                    else
                        stepCost = neighborTile.costCheck;

                    int tentativeG = currentG + stepCost;

                    if (!gScore.TryGetValue(neighborIndex, out int neighborG) || tentativeG < neighborG)
                    {
                        cameFrom[neighborIndex] = current;
                        gScore[neighborIndex] = tentativeG;

                        int fScore = tentativeG + Heuristic(currentTile.cube, neighborTile.cube);

                        openSet.Enqueue(neighborIndex, fScore);
                    }
                }
            }

            return new PathData();
        }

        public static PathData FindPathWithMaxLength(Vector3Int start, Vector3Int goal, int maxLength, bool ignoreCost = false, bool walkClose = false)
        {
            var pathData = FindPath(start, goal, ignoreCost, walkClose);

            if (pathData == null || pathData.Path == null || pathData.Path.Count == 0)
            {
                Debug.LogWarning($"[TilemapUtilityScript] No valid path found from {start} to {goal}");
                return new PathData(); // Return empty path data if no path
            }

            Debug.Log($"[TilemapUtilityScript] Found path from {start} to {goal} with length {pathData.Path.Count} (max allowed: {maxLength})");

            // Truncate the path if it's longer than maxLength
            if (pathData.Path.Count > maxLength)
            {
                pathData.Path = pathData.Path.Take(maxLength).ToList();
            }

            return pathData;
        }

        public static PathData FindLine(Vector3Int start, Vector3Int goal)
        {
            var openSet = new PriorityQueue<Vector3Int>();
            openSet.Enqueue(start, 0);

            var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
            var gScore = new Dictionary<Vector3Int, int> { [start] = 0 };
            PathData pathData = new PathData();

            if (start == goal)
            {
                pathData = new()
                {
                    Start = start,
                    End = goal,
                    Path = new List<Vector3Int> { },
                    PathCost = 0,
                };
                return pathData;
            }

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                if (current == goal)
                {
                    var path = ReconstructLine(cameFrom, start, goal);

                    pathData = new()
                    {
                        Start = start,
                        End = goal,
                        Path = path,
                        PathCost = 0,
                    };
                    return pathData;
                }

                var currentCube = OffsetToCube_PointTop(current, UseOddROffset);
                var goalCube = OffsetToCube_PointTop(goal, UseOddROffset);
                for (int dir = 0; dir < CubeDirs.Length; dir++)
                {
                    var neighborCube = currentCube + CubeDirs[dir];
                    var neighbor = CubeToOffset_PointTop(neighborCube, UseOddROffset);

                    // If ignoreCost, all tiles have cost 1
                    int tentativeGScore = gScore[current] + 1;

                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        int fScore = tentativeGScore + Heuristic(neighborCube, goalCube);
                        openSet.Enqueue(neighbor, fScore);
                    }
                }
            }
            return new(); // No Line found
        }

        public static PathData FindLineFromToWithLength(Vector3Int start, Vector3Int goal, int maxLength)
        {
            var pathData = FindLine(start, goal);

            if (pathData == null || pathData.Path == null || pathData.Path.Count == 0)
            {
                Debug.LogWarning($"[TilemapUtilityScript] No valid Line found from {start} to {goal}");
                return new PathData(); // Return empty path data if no path
            }
            // Truncate the path if it's longer than maxLength
            if (pathData.Path.Count > maxLength)
            {
                pathData.Path = pathData.Path.Take(maxLength).ToList();
            }

            return pathData;
        }

        public static int Heuristic(Vector3Int ac, Vector3Int bc)
        {
            return (Mathf.Abs(ac.x - bc.x) + Mathf.Abs(ac.y - bc.y) + Mathf.Abs(ac.z - bc.z)) / 2;
        }

        private static PathData BuildPath(
            Dictionary<int, int> cameFrom,
            int start,
            int goal,
            bool ignoreCost,
            Stat movementCostModifier)
        {
            var path = new List<Vector3Int>();
            int totalCost = 0;

            int current = goal;

            while (cameFrom.ContainsKey(current))
            {
                TileInfo tile = CostInfoScript.tileInfoDict[current];

                path.Insert(0, tile.position);

                totalCost += tile.costCheck;

                current = cameFrom[current];

                if (current == start)
                    break;
            }

            return new PathData
            {
                Start = CostInfoScript.tileInfoDict[start].position,
                End = CostInfoScript.tileInfoDict[goal].position,
                Path = path,
                PathCost = totalCost
            };
        }
        private static bool TryGetTileIndex(Vector3Int pos, out int index)
        {
            if (CostInfoScript == null)
            {
                index = -1;
                return false;
            }

            int key = TilemapUtilityScript.PositionToKey(pos);
            if (CostInfoScript.tileInfoDict.ContainsKey(key))
            {
                index = key;
                return true;
            }

            index = -1;
            return false;
        }
        private static List<Vector3Int> ReconstructLine(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int start, Vector3Int goal)
        {
            var path = new List<Vector3Int>();

            // Start from the goal and walk backwards
            Vector3Int current = goal;

            path.Add(current);
            while (cameFrom.ContainsKey(current))
            {
                var prev = cameFrom[current];
                current = prev;

                path.Insert(0, current);
            }
            return path;
        }
        public static void ForcedMove(ForcedMovementType type, EntityScript entity, Vector3Int ReferencePos, int Distance = 99, float speed = 3f)
        {
            EntityOnMap entityOnMap = entity.GetComponent<EntityOnMap>();
            PathData pathData = new();

            Vector3Int targetPosition = FindPositionToBeMoveTo(entityOnMap, ReferencePos);

            switch (type)
            {
                case ForcedMovementType.Random:
                    pathData = GetRandomInRange(entityOnMap.currentCell, Distance);
                    ActionQueueUtility.EnqueueMovement(entityOnMap, pathData);
                    break;
                case ForcedMovementType.Targeted:
                    pathData = FindPathWithMaxLength(entityOnMap.currentCell, targetPosition, Distance);
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
        public static Vector3Int FindPositionToBeMoveTo(EntityOnMap eom, Vector3Int targetPosition)
        {
            // If target tile is valid, use it immediately
            if (IsTileUsable(targetPosition))
            {
                Debug.Log($"[MovementUtility] Target tile {targetPosition} is valid for teleportation.");
                return targetPosition;
            }

            // Search outward (ring radius 1 -> N)
            const int maxRadius = 2;

            for (int radius = 1; radius <= maxRadius; radius++)
            {
                List<Vector3Int> tiles = TilemapUtilityScript.GetTilesInRing(targetPosition, radius, radius);

                // Sort by distance to ensure closest-first selection
                tiles.Sort((a, b) =>
                    Vector3Int.Distance(a, targetPosition)
                        .CompareTo(Vector3Int.Distance(b, targetPosition)));

                foreach (Vector3Int tile in tiles)
                {
                    if (IsTileUsable(tile))
                    {
                        Debug.Log($"[MovementUtility] Found valid tile {tile} at radius {radius} for teleportation.");
                        return tile;
                    }
                }
            }

            // Optional: no valid tile found
            Debug.LogWarning($"Teleport failed: no valid tile near {targetPosition}");
            return TilemapUtilityScript.InvalidPosition;
        }

        private static bool IsTileUsable(Vector3Int position)
        {
            if (CostInfoScript == null) return false;

            int key = TilemapUtilityScript.PositionToKey(position);
            if (!CostInfoScript.tileInfoDict.TryGetValue(key, out var costInfo))
                return false;

            if (costInfo.isOccupied) return false;
            if (costInfo.isUnwalkable) return false;

            return true;
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
        public static PathData GetRandomInRange(Vector3Int pos, int distance)
        {
            List<Vector3Int> possiblePositions = TilemapUtilityScript.GetTilesInRing(pos, distance, 1);

            // Filter to valid, unoccupied tiles
            possiblePositions = possiblePositions
                .Where(p => TilemapUtilityScript.CostInfoScript != null
                         && TilemapUtilityScript.CostInfoScript.tileInfoDict.ContainsKey(TilemapUtilityScript.PositionToKey(p))
                         && !TilemapUtilityScript.CostInfoScript.tileInfoDict[TilemapUtilityScript.PositionToKey(p)].isOccupied)
                .ToList();

            if (possiblePositions.Count == 0)
                return new PathData();

            PathData pathData = FindPath(pos, possiblePositions[new System.Random().Next(0, possiblePositions.Count)]);

            return pathData;
        }
        public static PathData GetPathDataToCloserPosition(Vector3Int to, int distance, EntityOnMap entityOnMap)
        {
            PathData path = FindPathWithMaxLength(entityOnMap.currentCell, to, distance, walkClose: true);
            return path;
        }
        public static PathData GetFurtherPosition(Vector3Int from, int distance, EntityOnMap entityOnMap)
        {
            List<Vector3Int> targets = TilemapUtilityScript.GetTilesInRing(entityOnMap.currentCell, distance, 1);

            // Filter out occupied tiles
            targets = targets
                .Where(p => TilemapUtilityScript.CostInfoScript != null
                         && TilemapUtilityScript.CostInfoScript.tileInfoDict.ContainsKey(TilemapUtilityScript.PositionToKey(p))
                         && !TilemapUtilityScript.CostInfoScript.tileInfoDict[TilemapUtilityScript.PositionToKey(p)].isOccupied)
                .ToList();

            if (targets.Count == 0)
                return null; // No valid target found

            // Find the tile furthest from 'from'
            Vector3Int furthest = targets
                .OrderByDescending(pos => Vector3Int.Distance(pos, from))
                .First();

            PathData path = FindPath(entityOnMap.currentCell, furthest);

            return path;
        }
        private static PathData GetFleePosition(int distance, EntityOnMap entityOnMap, EntityScript entityScript)
        {
            List<EntityScript> allEntities = GameObject.FindObjectsByType<EntityScript>(0).ToList();
            PathData path = GetBestFleePath(entityOnMap.currentCell, allEntities, entityScript);
            return path;
        }
        public static PathData GetBestFleePath(Vector3Int virtualPosition, List<EntityScript> allEntities, EntityScript entity)
        {
            // Count hostiles
            int hostileCount = allEntities.Count(e => e.entityAffiliation != entity.entityAffiliation);
            if (hostileCount == 0)
            {
                // No enemies — stay in place
                return new();
            }

            int maxFleeDistance = Mathf.Min(entity.entityStats.CurrentStamina, hostileCount);

            PathData bestPath = null;
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
                    var pathData = FindPath(virtualPosition, candidate);
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
                        bestPath = pathData;
                    }
                }
            }

            if (bestPath != null)
            {
                return bestPath;
            }

            // Fallback: stay in place
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
}