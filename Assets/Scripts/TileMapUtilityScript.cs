using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TileMapUtilityScript
{
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

    #region Pathfinding (A*)
    public static List<Vector3Int> FindPath(Vector3Int start, Vector3Int goal, Tilemap tilemap)
    {

        if (tilemap.GetTile<BasemapHexTile>(start) == null || tilemap.GetTile<BasemapHexTile>(goal) == null)
            return null;

        CostInfoScript costInfoScript = tilemap.GetComponent<CostInfoScript>();

        var openSet = new PriorityQueue<Vector3Int>();
        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        var gScore = new Dictionary<Vector3Int, int> { [start] = 0 };
        var fScore = new Dictionary<Vector3Int, int> { [start] = Heuristic(start, goal) };
        var closed = new HashSet<Vector3Int>();

        openSet.Enqueue(start, fScore[start]);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();
            if (closed.Contains(current)) continue;

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            closed.Add(current);

            // Generate neighbors by cube -> offset conversion (handles odd/even automatically via conversion)
            var currentCube = OffsetToCube_PointTop(current, UseOddROffset);
            for (int i = 0; i < 6; i++)
            {
                var neighborCube = currentCube + CubeDirs[i];
                var neighbor = CubeToOffset_PointTop(neighborCube, UseOddROffset);

                var tile = tilemap.GetTile<BasemapHexTile>(neighbor);
                if (tile == null) continue;

                int tileCost = costInfoScript.costInfoDict.TryGetValue(neighbor, out var costInfo) ? costInfo.cost : 1;
                if (tileCost >= 10000) continue; // blocked

                int tentative = gScore[current] + tileCost;
                if (!gScore.ContainsKey(neighbor) || tentative < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentative;
                    fScore[neighbor] = tentative + Heuristic(neighbor, goal);
                    openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }

        return null;
    }

    private static int Heuristic(Vector3Int a, Vector3Int b)
    {
        // Use cube distance; convert from offset -> cube based on configured Odd/Even-R
        var ac = OffsetToCube_PointTop(a, UseOddROffset);
        var bc = OffsetToCube_PointTop(b, UseOddROffset);
        return (Mathf.Abs(ac.x - bc.x) + Mathf.Abs(ac.y - bc.y) + Mathf.Abs(ac.z - bc.z)) / 2;
    }

    private static List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
    {
        var path = new List<Vector3Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }
    #endregion

    #region Tile Queries
    public static List<Vector3Int> GetTilesInRadius(Vector3Int center, int radius, Tilemap tilemap)
    {
        var results = new List<Vector3Int>();
        var cCenter = OffsetToCube_PointTop(center, UseOddROffset);

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = Mathf.Max(-radius, -dx - radius); dy <= Mathf.Min(radius, -dx + radius); dy++)
            {
                int dz = -dx - dy;
                var cube = cCenter + new Vector3Int(dx, dy, dz);
                var off = CubeToOffset_PointTop(cube, UseOddROffset);
                if (tilemap.GetTile<BasemapHexTile>(off) != null)
                    results.Add(off);
            }
        }
        return results;
    }

    public static List<Vector3Int> GetTilesInRing(Vector3Int center, int distance, Tilemap tilemap)
    {
        var results = new List<Vector3Int>();
        if (distance <= 0) return results;

        var cCenter = OffsetToCube_PointTop(center, UseOddROffset);
        // start at one cube direction * distance
        var cube = cCenter + CubeDirs[4] * distance; // arbitrary starting edge
        for (int side = 0; side < 6; side++)
        {
            for (int step = 0; step < distance; step++)
            {
                var off = CubeToOffset_PointTop(cube, UseOddROffset);
                if (tilemap.GetTile<BasemapHexTile>(off) != null)
                    results.Add(off);
                cube += CubeDirs[side];
            }
        }
        return results;
    }

    public static List<Vector3Int> GetTilesInLine(Vector3Int start, int length, int directionIndex, Tilemap tilemap)
    {
        var results = new List<Vector3Int>();
        directionIndex = ((directionIndex % 6) + 6) % 6;

        var cube = OffsetToCube_PointTop(start, UseOddROffset);
        for (int i = 0; i < length; i++)
        {
            cube += CubeDirs[directionIndex];
            var off = CubeToOffset_PointTop(cube, UseOddROffset);
            if (tilemap.GetTile<BasemapHexTile>(off) != null)
                results.Add(off);
        }
        return results;
    }

    public static Dictionary<int, List<Vector3Int>> GetTilesInAllDirections(Vector3Int start, int length, Tilemap tilemap)
    {
        var dict = new Dictionary<int, List<Vector3Int>>();
        for (int d = 0; d < 6; d++)
            dict[d] = GetTilesInLine(start, length, d, tilemap);
        return dict;
    }
    #endregion

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
    public static void ResetMaphightlight(Tilemap tilemap, BasemapHexTile defaultTile)
    {
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            var tile = tilemap.GetTile<BasemapHexTile>(pos);
            if (tile != null)
                tilemap.SetTile(pos, defaultTile);
        }
    }

    public static void SetPathHighlight(List<Vector3Int> path, Tilemap tilemap, BasemapHexTile highlightedTile)
    {
        if (path == null) return;
        foreach (var pos in path)
        {
            var tile = tilemap.GetTile<BasemapHexTile>(pos);
            if (tile != null)
                tilemap.SetTile(pos, highlightedTile);
        }
    }
    #endregion
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
