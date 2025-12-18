using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Utility
{
    public static class MovementUtility
    {
        public static CostInfoScript CostInfoScript = TilemapUtilityScript.BaseTilemap?.GetComponent<CostInfoScript>();

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
        public static PathData FindPath(Vector3Int start, Vector3Int goal, bool ignoreCost = false, bool walkClose = false, Stat movementCostModifier = null)
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
                    int totalCost;
                    var path = ReconstructPath(cameFrom, start, goal, out totalCost, CostInfoScript, ignoreCost, walkClose, movementCostModifier);

                    pathData = new()
                    {
                        Start = start,
                        End = goal,
                        Path = path,
                        PathCost = totalCost,
                    };
                    return pathData;
                }

                var currentCube = OffsetToCube_PointTop(current, UseOddROffset);
                for (int dir = 0; dir < CubeDirs.Length; dir++)
                {
                    var neighborCube = currentCube + CubeDirs[dir];
                    var neighbor = CubeToOffset_PointTop(neighborCube, UseOddROffset);

                    // If ignoreCost, all tiles have cost 1
                    if (!CostInfoScript.costInfoDict.ContainsKey(neighbor))
                        continue;
                    int tentativeGScore = gScore[current] + (ignoreCost ? 1 : CostInfoScript.costInfoDict[current].costCheck);

                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        int fScore = tentativeGScore + Heuristic(neighbor, goal);
                        openSet.Enqueue(neighbor, fScore);
                    }
                }
            }
            return new(); // No path found
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
                        int fScore = tentativeGScore + Heuristic(neighbor, goal);
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

        public static int Heuristic(Vector3Int a, Vector3Int b)
        {
            // Use cube distance; convert from offset -> cube based on configured Odd/Even-R
            var ac = OffsetToCube_PointTop(a, UseOddROffset);
            var bc = OffsetToCube_PointTop(b, UseOddROffset);
            return (Mathf.Abs(ac.x - bc.x) + Mathf.Abs(ac.y - bc.y) + Mathf.Abs(ac.z - bc.z)) / 2;
        }
        private static int CostStep(Vector3Int position, Stat moveCostModifier, bool ignoreCost = false)
        {
            if (ignoreCost)
            {
                return 1;
            }
            else if (moveCostModifier != null)
            {
                return moveCostModifier.ApplyFinalValue(CostInfoScript.costInfoDict[position].costCheck);
            }
            else
            {
                return CostInfoScript.costInfoDict[position].costCheck;
            }
        }
        private static List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int start, Vector3Int goal, out int totalCost, CostInfoScript costInfoScript, bool ignoreCost, bool walkClose, Stat moveCostModifier)
        {
            var path = new List<Vector3Int>();
            totalCost = 0;

            // Start from the goal and walk backwards
            Vector3Int current;

            // If walkClose is true, we stop at the tile before the goal
            if (walkClose)
            {
                if (!cameFrom.ContainsKey(goal))
                {
                    current = start; // No path to goal, stay at start
                }
                else
                {
                    current = cameFrom[goal];
                }
                path.Add(current);

                Debug.Log("[MovementUtility] Walking close: stopping before goal.");
                totalCost += CostStep(current, moveCostModifier, ignoreCost);
            }
            else
            {
                current = goal;

                path.Add(current);

                totalCost += CostStep(current, moveCostModifier, ignoreCost);
            }

            while (cameFrom.ContainsKey(current))
            {
                var prev = cameFrom[current];

                if (prev == start)
                    break; // Stop before adding the start position

                totalCost += CostStep(current, moveCostModifier, ignoreCost);

                current = prev;

                path.Insert(0, current);
            }
            return path;
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
        public static void ForcedMove(ForcedMovementType type, EntityScript entity, Vector3Int ReferencePos, int Distance, float speed = 3f)
        {
            EntityOnMap entityOnMap = entity.GetComponent<EntityOnMap>();
            PathData pathData = new();

            switch (type)
            {
                case ForcedMovementType.Random:
                    pathData = GetRandomInRange(entityOnMap.currentCell, Distance);
                    break;
                case ForcedMovementType.Targeted:
                    pathData = FindPathWithMaxLength(entityOnMap.currentCell, ReferencePos, Distance);
                    break;
                case ForcedMovementType.Flee:
                     pathData = GetFleePosition(Distance, entityOnMap, entity);
                    break;
                case ForcedMovementType.Push:
                     pathData = GetFurtherPosition(ReferencePos, Distance, entityOnMap);
                    break;
                case ForcedMovementType.Pull:
                    pathData = GetPathDataToCloserPosition(ReferencePos, Distance, entityOnMap);
                    break;
            }
            entityOnMap.MoveTo(pathData);

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
            List<Vector3Int> possiblePositions = TilemapUtilityScript.GetTilesInRing(pos,  distance, 1 );
            possiblePositions.Where(pos => TilemapUtilityScript.CostInfoScript.costInfoDict[pos].isOccupied == false).First();

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

            PathData path = FindPath(entityOnMap.currentCell, furthest);

            return path;
        }
        private static PathData GetFleePosition(int distance, EntityOnMap entityOnMap, EntityScript entityScript)
        {
            List<EntityScript> allEntities = GameObject.FindObjectsByType<EntityScript>(0).ToList();
            PathData path = GetBestFleePath(entityOnMap.currentCell, allEntities , entityScript);
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
    }
}