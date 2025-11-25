using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Utility
{
    public static class TilemapUtilityScript
    {
        public static Vector3Int InvalidPosition = new Vector3Int(9999, 9999, 9999);
        public static Tilemap BaseTilemap => Object.FindObjectsByType<Tilemap>(0).FirstOrDefault(tilemap => tilemap.CompareTag("Basemap"));
        public static CostInfoScript CostInfoScript => BaseTilemap?.GetComponent<CostInfoScript>();

        // >>> CONFIG: Point-top offset type (Odd-R by default). Flip if your rows are shifted the other way.
        public static bool UseOddROffset = true; // true = Odd-R, false = Even-R

        /// <summary>Optionally call this once at startup if you need Even-R instead of Odd-R.</summary>
        public static void ConfigurePointTop(bool useOddR) => UseOddROffset = useOddR;

        // Cube directions (point-top agnostic)
        private static readonly Vector3Int[] CubeDirs = new Vector3Int[]
        {
        new Vector3Int(+1, -1, 0),
        new Vector3Int(+1, 0, -1),
        new Vector3Int(0, +1, -1),
        new Vector3Int(-1, +1, 0),
        new Vector3Int(-1, 0, +1),
        new Vector3Int(0, -1, +1)
        };


        // Add this method to TilemapUtilityScript
        public static PathData FindPath(Vector3Int start, Vector3Int goal, bool ignoreCost = false, bool walkClose = false)
        {
            var openSet = new PriorityQueue<Vector3Int>();
            openSet.Enqueue(start, 0);

            var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
            var gScore = new Dictionary<Vector3Int, int> { [start] = 0 };
            PathData pathData = new PathData();

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                if (current == goal)
                {
                    int totalCost;
                    var path = ReconstructPath(cameFrom, start, goal, out totalCost, CostInfoScript, ignoreCost,  walkClose);

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
                    int tentativeGScore = gScore[current] + (ignoreCost ? CostInfoScript.costInfoDict[current].costUnobstructed : CostInfoScript.costInfoDict[current].cost);

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
            var pathData = FindPath(start, goal, ignoreCost, true);

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

        private static int Heuristic(Vector3Int a, Vector3Int b)
        {
            // Use cube distance; convert from offset -> cube based on configured Odd/Even-R
            var ac = OffsetToCube_PointTop(a, UseOddROffset);
            var bc = OffsetToCube_PointTop(b, UseOddROffset);
            return (Mathf.Abs(ac.x - bc.x) + Mathf.Abs(ac.y - bc.y) + Mathf.Abs(ac.z - bc.z)) / 2;
        }
        private static List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom,Vector3Int start,Vector3Int goal,out int totalCost,CostInfoScript costInfoScript,bool ignoreCost,bool walkClose)
        {
            var path = new List<Vector3Int>();
            totalCost = 0;
           
            // Start from the goal and walk backwards
            Vector3Int current = goal;

            if (!walkClose)
            {
            path.Add(current);
            totalCost += ignoreCost ? costInfoScript.costInfoDict[current].costUnobstructed : costInfoScript.costInfoDict[current].cost;
            }

            while (cameFrom.ContainsKey(current))
            {
                var prev = cameFrom[current];

                if (prev == start)
                    break; // Stop before adding the start position


                    totalCost += ignoreCost ? costInfoScript.costInfoDict[prev] .costUnobstructed : costInfoScript.costInfoDict[prev].cost;
                
                current = prev;

                path.Insert(0, current);
            }
            return path;
        }
        private static List<Vector3Int> ReconstructLine(Dictionary<Vector3Int, Vector3Int> cameFrom,Vector3Int start, Vector3Int goal)
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
        
        #region Tile Queries
        public static List<Vector3Int> GetTilesInRadius(Vector3Int center, int radius)
        {
            var results = new List<Vector3Int>();
            var cCenter = OffsetToCube_PointTop(center, UseOddROffset);
            radius -= 1;

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = Mathf.Max(-radius, -dx - radius); dy <= Mathf.Min(radius, -dx + radius); dy++)
                {
                    int dz = -dx - dy;
                    var cube = cCenter + new Vector3Int(dx, dy, dz);
                    var off = CubeToOffset_PointTop(cube, UseOddROffset);
                    results.Add(off);
                }
            }
            return results;
        }
        public static List<Vector3Int> GetTilesInRing(Vector3Int center, int distance, int size)
        {
            var results = new HashSet<Vector3Int>(); // avoid duplicates
            if (distance <= 0 || size <= 0)
                return results.ToList();

            var cCenter = OffsetToCube_PointTop(center, UseOddROffset);

            // For each ring layer from distance → distance + size - 1
            for (int d = distance; d < distance + size; d++)
            {
                // Starting cube coordinate: direction 4, moved d steps
                var cube = cCenter + CubeDirs[4] * d;

                // Walk the 6 sides
                for (int side = 0; side < 6; side++)
                {
                    for (int step = 0; step < d; step++)
                    {
                        var off = CubeToOffset_PointTop(cube, UseOddROffset);
                        results.Add(off);
                        cube += CubeDirs[side];
                    }
                }
            }

            return results.ToList();
        }

        public static List<Vector3Int> GetTilesInLineFromSelf(Vector3Int start, Vector3Int target, int maxLength)
        {
            var tiles = new List<Vector3Int>();

            // Convert start/end to cube coordinates
            Vector3Int startCube = OffsetToCube_PointTop(start, UseOddROffset);
            Vector3Int endCube = OffsetToCube_PointTop(target, UseOddROffset);

            // Determine vector toward end
            Vector3Int delta = endCube - startCube;

            // Find closest cube direction
            Vector3Int bestDir = CubeDirs[0];
            int maxDot = int.MinValue;
            foreach (var dir in CubeDirs)
            {
                int dot = delta.x * dir.x + delta.y * dir.y + delta.z * dir.z;
                if (dot > maxDot)
                {
                    maxDot = dot;
                    bestDir = dir;
                }
            }

            // Extend line along that direction
            Vector3Int currentCube = endCube;

            tiles.Add(target);
            for (int i = 0; i < maxLength-1; i++)
            {
                currentCube += bestDir;
                Vector3Int offset = CubeToOffset_PointTop(currentCube, UseOddROffset);
                tiles.Add(offset);
            }

            return tiles;
        }
        public static List<Vector3Int> GetTilesInLineFree(List<Vector3Int> positions, int maxRange, int maxLength)
        {
            if (positions == null || positions.Count < 2)
            { 
                return new List<Vector3Int>();
            }

            PathData pathData = FindLineFromToWithLength(positions[0], positions[1], maxLength);

            return pathData.Path;
        }
        public static List<Vector3Int> GetTilesInCone(Vector3Int start, Vector3Int direction, int length, int area)
        {
            var results = new HashSet<Vector3Int>();
            var cStart = OffsetToCube_PointTop(start, UseOddROffset);
            var cEnd = OffsetToCube_PointTop(direction, UseOddROffset);
            var cDir = new Vector3Int(
                cEnd.x - cStart.x,
                cEnd.y - cStart.y,
                cEnd.z - cStart.z
            );

            // Normalize cDir to one of the 6 main directions
            int maxAbs = Mathf.Max(Mathf.Abs(cDir.x), Mathf.Abs(cDir.y), Mathf.Abs(cDir.z));
            Vector3Int mainDir = maxAbs == 0 ? Vector3Int.zero : new Vector3Int(
                cDir.x == 0 ? 0 : cDir.x / maxAbs,
                cDir.y == 0 ? 0 : cDir.y / maxAbs,
                cDir.z == 0 ? 0 : cDir.z / maxAbs
            );

            // Find the index of the main direction in CubeDirs
            int mainDirIdx = -1;
            for (int i = 0; i < CubeDirs.Length; i++)
            {
                if (CubeDirs[i] == mainDir)
                {
                    mainDirIdx = i;
                    break;
                }
            }
            if (mainDirIdx == -1)
                return results.ToList(); // Invalid direction

            // For each step forward, fill a wider area, including the start tile
            for (int step = 0; step < length; step++)
            { 
                int areaS = area - step;

                var forward = cStart + mainDir * step;
                for (int w = -areaS; w <= areaS; w++)
                {
                    Vector3Int offset = Vector3Int.zero;
                    if (w < 0)
                    {
                        int leftIdx = (mainDirIdx + 5) % 6;
                        offset = CubeDirs[leftIdx] * -w;
                    }
                    else if (w > 0)
                    {
                        int rightIdx = (mainDirIdx + 1) % 6;
                        offset = CubeDirs[rightIdx] * w;
                    }
                    var coneTile = CubeToOffset_PointTop(forward + offset, UseOddROffset);
                    results.Add(coneTile);
                }
            }
            // Always include the starting tile
            results.Add(start);
            return results.ToList();
        }
        public static List<Vector3Int> GetTilesInStar(Vector3Int start, int length)
        {
            List<Vector3Int> tiles = new List<Vector3Int>();

            // Convert the center to cube space
            Vector3Int centerCube = OffsetToCube_PointTop(start, UseOddROffset);

            // Always include the center tile
            tiles.Add(start);

            // For each of the 6 cube directions
            foreach (var dir in CubeDirs)
            {
                Vector3Int currentCube = centerCube;

                // Step outward maxLength times
                for (int i = 0; i < length; i++)
                {
                    currentCube += dir;
                    Vector3Int offset = CubeToOffset_PointTop(currentCube, UseOddROffset);

                    tiles.Add(offset);
                }
            }

            return tiles;
        }
        public static List<EntityScript> GetEntitiesOnTiles(List<Vector3Int> tiles, List<EntityScript> entities)
        {
            return entities.Where(entitity => tiles.Contains(entitity.GetComponent<EntityOnMap>().currentCell)).ToList();
        }

        #endregion

        public static List<Vector3Int> GetReachableTiles(Vector3Int start, int stamina)
        {
            List<Vector3Int> reachable = new();
            Queue<(Vector3Int cell, int costSoFar)> frontier = new();
            HashSet<Vector3Int> visited = new();

            frontier.Enqueue((start, 0));
            visited.Add(start);

            var costInfoScript = BaseTilemap.GetComponent<CostInfoScript>();

            while (frontier.Count > 0)
            {
                var (current, costSoFar) = frontier.Dequeue();

                // Skip adding the start itself if you only want destinations
                if (current != start)
                    reachable.Add(current);

                // Expand neighbors in cube space
                var currentCube = OffsetToCube_PointTop(current, UseOddROffset);
                foreach (var dir in CubeDirs)
                {
                    var neighborCube = currentCube + dir;
                    var neighbor = CubeToOffset_PointTop(neighborCube, UseOddROffset);

                    if (visited.Contains(neighbor))
                        continue;

                    // default movement cost = 1
                    int tileCost = 1;
                    if (costInfoScript != null &&
                        costInfoScript.costInfoDict.TryGetValue(neighbor, out CostInfo costInfo))
                    {
                        if (costInfo.isOccupied) continue; // can’t move here
                        tileCost = Mathf.Max(1, costInfo.cost);
                    }

                    int newCost = costSoFar + tileCost;
                    if (newCost <= stamina)
                    {
                        frontier.Enqueue((neighbor, newCost));
                        visited.Add(neighbor);
                    }
                }
            }

            return reachable;
        }

        public static List<Vector3Int> GetTilesInRange(Vector3Int targetPos, int range)
        {
            switch (range)
            {
                case <= 0: return new List<Vector3Int> { targetPos };
                default:
                    return TilemapUtilityScript.GetTilesInRadius(targetPos, range);
            }
        }

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

        #region Highlight Helpers
        public static void ResetMaphightlight(Tilemap tilemap)
        {
            if (tilemap == null) return;

            BoundsInt bounds = tilemap.cellBounds;

            foreach (var pos in bounds.allPositionsWithin)
            {
                if (!tilemap.HasTile(pos)) continue;

                GameObject tileObj = tilemap.GetInstantiatedObject(pos);
                if (tileObj == null) continue;

                // Reset UI Image color
                SpriteRenderer sr = tileObj.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.clear;
                    continue;
                }
            }
        }
        public static void ResetMaphightlight(List<Vector3Int> path)
        {
            if (BaseTilemap == null) return;

            foreach (var pos in path)
            {
                if (!BaseTilemap.HasTile(pos)) continue;
                GameObject tileObj = BaseTilemap.GetInstantiatedObject(pos);
                if (tileObj == null) continue;
                // Reset UI Image color
                SpriteRenderer img = tileObj.GetComponentInChildren<SpriteRenderer>();
                if (img != null)
                {
                    img.color = Color.clear;
                    continue;
                }
            }
        }
        public static void SetTilesHighlight(List<Vector3Int> tiles, HighlightType ht)
        {
            Color color = Color.clear;

            switch (ht)
            {
                case HighlightType.Path:
                    color = Color.cyan;
                    break;
                case HighlightType.Target:
                    color = Color.red;
                    break;
                case HighlightType.Selected:
                    color = Color.yellow;
                    break;
                case HighlightType.Range:
                    color = Color.green;
                    break;
                    case HighlightType.Line:
                    color = Color.blueViolet;
                    break;
            }

            if (tiles != null)
            {
                foreach (var pos in tiles)
                {
                    // Get the prefab instance associated with this tile position
                    GameObject tileObj = BaseTilemap.GetInstantiatedObject(pos);
                    if (tileObj == null) continue;

                    // Try Image (UI-based prefab)
                    SpriteRenderer sr = tileObj.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.color = color;
                        continue;
                    }
                }
            }
        }
        internal static List<Vector3Int> GetAllValidTiles()
        {
            var tilemap = BaseTilemap;
            List<Vector3Int> validTiles = new List<Vector3Int>();
            if (tilemap == null) return validTiles;
            BoundsInt bounds = tilemap.cellBounds;
            foreach (var pos in bounds.allPositionsWithin)
            {
                if (tilemap.HasTile(pos))
                {
                    validTiles.Add(pos);
                }
            }
            return validTiles;
        }

        public enum HighlightType
        {
            Path,
            Target,
            Selected,
            Range,
            Line
        }
        #endregion
    }

    public class PathData
    {
        public List<Vector3Int> Path;
        public Vector3Int Start;
        public Vector3Int End;
        public int PathCost;
    }

    #region PriorityQueue Helper
    public class PriorityQueue<T>
    {
        private readonly List<(T item, int priority)> elements = new List<(T, int)>();
        public int Count => elements.Count;

        public void Enqueue(T item, int priority) => elements.Add((item, priority));

        public T Dequeue()
        {
            int best = 0;
            for (int i = 1; i < elements.Count; i++)
                if (elements[i].priority < elements[best].priority)
                    best = i;
            var item = elements[best].item;
            elements.RemoveAt(best);
            return item;
        }
    }
    #endregion
}