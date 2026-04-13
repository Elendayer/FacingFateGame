using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Utility
{
    public static class TilemapUtilityScript
    {
        public static Vector3Int InvalidPosition = new Vector3Int(9999, 9999, 9999);
        public static Tilemap BaseTilemap => UnityEngine.Object.FindObjectsByType<Tilemap>(0).FirstOrDefault(tilemap => tilemap.CompareTag("Basemap"));
        public static TileInfoScript CostInfoScript => BaseTilemap?.GetComponent<TileInfoScript>();

        private static Color baseTileColor = new Color(0, 0, 0, 0.25f);
        private static Color pathTileColor = new Color(0, 0, 1, 0.25f);
        private static Color targetTileColor = new Color(1, 0, 0, 0.25f);
        private static Color selecetedTileColor = new Color(1, 1, 0, 0.25f);
        private static Color rangeTileColor = new Color(0, 1, 0, 0.25f);
        private static Color lineTileColor = new Color(0.5f, 0.5f, 0, 0.25f);
        private static Color blockedTileColor = new Color(1, 1, 1, 0.25f);
       

        // Map indexing configuration - must match CombatMapMaster CollectMap
        public const int MapWidth = 50;
        public const int MapOffset = 25; // maps -25..24 to 0..49

        public static int PositionToKey(Vector3Int pos)
        {
            return (pos.x + MapOffset) * MapWidth + (pos.y + MapOffset);
        }   

        public static Vector3Int KeyToPosition(int key)
        {
            int x = (key / MapWidth) - MapOffset;
            int y = (key % MapWidth) - MapOffset;
            return new Vector3Int(x, y, 0);
        }

        public static Sprite HexThick;
        public static Sprite HexThin;


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
        public static Vector3Int OffsetToCube_PointTop(Vector3Int off, bool oddR)
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

        public static Vector3Int CubeToOffset_PointTop(Vector3Int cube, bool oddR)
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
            Vector3Int targetCube = OffsetToCube_PointTop(target, UseOddROffset);

            // Determine direction vector
            Vector3Int delta = targetCube - startCube;

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

            // Start one tile in front of self
            Vector3Int currentCube = startCube + bestDir;

            // Collect tiles forward
            for (int i = 0; i < maxLength; i++)
            {
                Vector3Int offset = CubeToOffset_PointTop(currentCube, UseOddROffset);
                tiles.Add(offset);

                // Step forward in the direction
                currentCube += bestDir;
            }

            return tiles;
        }

        public static List<Vector3Int> GetTilesInLineFree(List<Vector3Int> positions, int maxRange, int maxLength)
        {
            if (positions == null || positions.Count < 2)
            { 
                return new List<Vector3Int>();
            }

            PathData pathData = MovementUtility.FindLineFromToWithLength(positions[0], positions[1], maxLength);

            return pathData.Path;
        }
        public static List<Vector3Int> GetTilesInCone(Vector3Int start, Vector3Int direction, int length, int area)
        {
            var results = new HashSet<Vector3Int>();

            var cStart = OffsetToCube_PointTop(start, UseOddROffset);
            var cEnd = OffsetToCube_PointTop(direction, UseOddROffset);

            // Determine main direction
            Vector3Int cDir = new Vector3Int(cEnd.x - cStart.x, cEnd.y - cStart.y, cEnd.z - cStart.z);
            int maxAbs = Mathf.Max(Mathf.Abs(cDir.x), Mathf.Abs(cDir.y), Mathf.Abs(cDir.z));
            if (maxAbs == 0) return results.ToList();

            Vector3Int mainDir = new Vector3Int(cDir.x / maxAbs, cDir.y / maxAbs, cDir.z / maxAbs);

            // Find index of mainDir in CubeDirs
            int mainDirIdx = Array.IndexOf(CubeDirs, mainDir);
            if (mainDirIdx < 0) return results.ToList();

            Vector3Int leftDir = CubeDirs[(mainDirIdx + 5) % 6];
            Vector3Int rightDir = CubeDirs[(mainDirIdx + 1) % 6];

            // Iterate over each step forward (range)
            for (int step = 1; step <= length; step++) // start at 1 to skip caster
            {
                // Width at this step (you can adjust formula: e.g., area - step + 1 or constant width)
                int rowWidth = area + step - 1;

                var forward = cStart + mainDir * step;

                for (int w = -rowWidth; w <= rowWidth; w++)
                {
                    Vector3Int offset = w < 0 ? leftDir * -w : rightDir * w;
                    var tile = CubeToOffset_PointTop(forward + offset, UseOddROffset);
                    results.Add(tile);
                }
            }

            return results.ToList();
        }
        public static List<Vector3Int> GetTilesInStar(Vector3Int start, int length)
        {
            List<Vector3Int> tiles = new List<Vector3Int>();

            //Get Tiles in all 6 directions excluding the start tile itself with specified length
            var cStart = OffsetToCube_PointTop(start, UseOddROffset);
            for (int dir = 0; dir < CubeDirs.Length; dir++)
            {
                for (int step = 1; step <= length; step++)
                {
                    var cube = cStart + CubeDirs[dir] * step;
                    var off = CubeToOffset_PointTop(cube, UseOddROffset);
                    tiles.Add(off);
                }
            }

            return tiles;
        }
        #endregion

        // Return the closest reachable tile within movement range of the start that minimizes distance to the target
        // Flood-fill (BFS) from the start up to `range` steps (movement range), collect reachable tiles,
        // then pick the reachable tile closest to the target (hex distance). Tie-breaker: fewer steps.
        public static Vector3Int GetReachableTileWithinRangeOfTarget(Vector3Int startPos, Vector3Int targetPos, int range)
        {
            var costInfo = CostInfoScript;
            if (costInfo == null) return InvalidPosition;

            int startKey = PositionToKey(startPos);
            if (!costInfo.tileInfoDict.ContainsKey(startKey)) return InvalidPosition;

            var reachable = new Dictionary<Vector3Int, int>(); // pos -> steps from start
            var q = new Queue<Vector3Int>();

            reachable[startPos] = 0;
            q.Enqueue(startPos);

            while (q.Count > 0)
            {
                var cur = q.Dequeue();
                int curDist = reachable[cur];

                // do not expand beyond movement range
                if (curDist >= range)
                    continue;

                // use cached cube when available
                int curKey = PositionToKey(cur);
                Vector3Int curCube = costInfo.tileInfoDict.TryGetValue(curKey, out var curInfo) && curInfo != null
                    ? curInfo.cube
                    : OffsetToCube_PointTop(cur, UseOddROffset);

                for (int d = 0; d < CubeDirs.Length; d++)
                {
                    var neighborCube = curCube + CubeDirs[d];
                    var neighborOffset = CubeToOffset_PointTop(neighborCube, UseOddROffset);
                    int nKey = PositionToKey(neighborOffset);

                    if (!costInfo.tileInfoDict.TryGetValue(nKey, out var nInfo))
                        continue;

                    if (nInfo.isUnwalkable || nInfo.isOccupied)
                        continue;

                    if (reachable.ContainsKey(neighborOffset))
                        continue;

                    reachable[neighborOffset] = curDist + 1;
                    q.Enqueue(neighborOffset);
                }
            }

            // Pick reachable tile closest to the target (hex distance). Tie-breaker: fewer steps.
            Vector3Int best = InvalidPosition;
            int bestHeu = int.MaxValue;
            int bestSteps = int.MaxValue;

            foreach (var kv in reachable)
            {
                // use cached cube coord for reachable tile
                var pos = kv.Key;
                int posKey = PositionToKey(pos);
                Vector3Int posCube = costInfo.tileInfoDict.TryGetValue(posKey, out var posInfo) && posInfo != null
                    ? posInfo.cube
                    : OffsetToCube_PointTop(pos, UseOddROffset);

                // target cube: prefer cached if present
                int targetKey = PositionToKey(targetPos);
                Vector3Int targetCube = costInfo.tileInfoDict.TryGetValue(targetKey, out var tInfo) && tInfo != null
                    ? tInfo.cube
                    : OffsetToCube_PointTop(targetPos, UseOddROffset);

                int heu = MovementUtility.Heuristic(posCube, targetCube);
                int steps = kv.Value;

                if (heu < bestHeu || (heu == bestHeu && steps < bestSteps))
                {
                    best = kv.Key;
                    bestHeu = heu;
                    bestSteps = steps;
                }
            }

            return best;
        }

        // Coroutine version: runs the BFS flood-fill over multiple frames to avoid frame spikes.
        // onComplete will be invoked with the selected best tile when finished.
        public static IEnumerator GetReachableTileWithinRangeOfTargetCoroutine(
            Vector3Int startPos,
            Vector3Int targetPos,
            int range,
            Action<Vector3Int> onComplete,
            int batchSize = 200)
        {
            var costInfo = CostInfoScript;
            if (costInfo == null)
            {
                onComplete?.Invoke(InvalidPosition);
                yield break;
            }

            int startKey = PositionToKey(startPos);
            if (!costInfo.tileInfoDict.ContainsKey(startKey))
            {
                onComplete?.Invoke(InvalidPosition);
                yield break;
            }

            var reachable = new Dictionary<Vector3Int, int>(); // pos -> steps from start
            var q = new Queue<Vector3Int>();

            reachable[startPos] = 0;
            q.Enqueue(startPos);

            int processed = 0;

            while (q.Count > 0)
            {
                var cur = q.Dequeue();
                int curDist = reachable[cur];

                // do not expand beyond movement range
                if (curDist < range)
                {
                    // use cached cube when available
                    int curKey = PositionToKey(cur);
                    Vector3Int curCube = costInfo.tileInfoDict.TryGetValue(curKey, out var curInfo) && curInfo != null
                        ? curInfo.cube
                        : OffsetToCube_PointTop(cur, UseOddROffset);

                    for (int d = 0; d < CubeDirs.Length; d++)
                    {
                        var neighborCube = curCube + CubeDirs[d];
                        var neighborOffset = CubeToOffset_PointTop(neighborCube, UseOddROffset);
                        int nKey = PositionToKey(neighborOffset);

                        if (!costInfo.tileInfoDict.TryGetValue(nKey, out var nInfo))
                            continue;

                        if (nInfo.isUnwalkable || nInfo.isOccupied)
                            continue;

                        if (reachable.ContainsKey(neighborOffset))
                            continue;

                        reachable[neighborOffset] = curDist + 1;
                        q.Enqueue(neighborOffset);
                    }
                }

                processed++;
                if (processed >= batchSize)
                {
                    processed = 0;
                    // yield control to avoid frame spike
                    yield return null;
                }
            }

            // Pick reachable tile closest to the target (hex distance). Tie-breaker: fewer steps.
            Vector3Int best = InvalidPosition;
            int bestHeu = int.MaxValue;
            int bestSteps = int.MaxValue;

            foreach (var kv in reachable)
            {
                int heu = MovementUtility.Heuristic(kv.Key, targetPos);
                int steps = kv.Value;

                if (heu < bestHeu || (heu == bestHeu && steps < bestSteps))
                {
                    best = kv.Key;
                    bestHeu = heu;
                    bestSteps = steps;
                }
            }

            onComplete?.Invoke(best);
        }

        // Helper to start the coroutine via a runner MonoBehaviour. Creates a persistent runner if none exists.
        public static Coroutine StartGetReachableTileWithinRangeOfTarget(Vector3Int startPos, Vector3Int targetPos, int range, Action<Vector3Int> onComplete, int batchSize = 200)
        {
            if (CoroutineRunner.Instance == null)
            {
                var go = new GameObject("Utility.CoroutineRunner");
                UnityEngine.Object.DontDestroyOnLoad(go);
                go.AddComponent<CoroutineRunner>();
            }

            return CoroutineRunner.Instance.StartCoroutineManaged(GetReachableTileWithinRangeOfTargetCoroutine(startPos, targetPos, range, onComplete, batchSize));
        }


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
                    sr.sprite = HexThin;
                    sr.color = baseTileColor;
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
                SpriteRenderer sr = tileObj.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = HexThin;
                    sr.color = baseTileColor;
                    continue;
                }
            }
        }
        public static void SetTilesHighlight(List<Vector3Int> tiles, HighlightType ht)
        {
            Color color = baseTileColor;
            Sprite texture = null;

            switch (ht)
            {
                case HighlightType.Path:
                    color = pathTileColor;
                    texture = HexThin;
                    break;

                case HighlightType.Target:
                    color = targetTileColor;
                    texture = HexThick;
                    break;

                case HighlightType.Selected:
                    color = selecetedTileColor;
                    texture = HexThick;
                    break;

                case HighlightType.Range:
                    color = rangeTileColor;
                    texture = HexThin;
                    break;

                case HighlightType.Line:
                    color = lineTileColor;
                    texture = HexThin;
                    break;

                case HighlightType.Blocked:
                    color = blockedTileColor;  
                    texture = HexThick;
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
                        sr.sprite = texture;
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
            Line,
            Blocked
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
        private List<(T item, int priority)> heap = new();

        public int Count => heap.Count;

        public void Enqueue(T item, int priority)
        {
            heap.Add((item, priority));
            HeapifyUp(heap.Count - 1);
        }

        public T Dequeue()
        {
            var root = heap[0].item;
            heap[0] = heap[^1];
            heap.RemoveAt(heap.Count - 1);
            HeapifyDown(0);
            return root;
        }

        void HeapifyUp(int i)
        {
            while (i > 0)
            {
                int parent = (i - 1) / 2;

                if (heap[parent].priority <= heap[i].priority)
                    break;

                (heap[parent], heap[i]) = (heap[i], heap[parent]);

                i = parent;
            }
        }

        void HeapifyDown(int i)
        {
            while (true)
            {
                int left = i * 2 + 1;
                int right = i * 2 + 2;

                int smallest = i;

                if (left < heap.Count && heap[left].priority < heap[smallest].priority)
                    smallest = left;

                if (right < heap.Count && heap[right].priority < heap[smallest].priority)
                    smallest = right;

                if (smallest == i)
                    break;

                (heap[i], heap[smallest]) = (heap[smallest], heap[i]);

                i = smallest;
            }
        }
    }
    #endregion
}