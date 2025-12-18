using System.Collections.Generic;
using UnityEngine;
using Utility;

public static class VisionUtility
{
    /// <summary>
    /// Returns all tiles from tilesToCheck that have line-of-sight from origin
    /// using CostInfoScript to check blockers (isOccupied or isUnwalkable).
    /// </summary>
    public static List<Vector3Int> GetVisibleTiles(
        Vector3Int originOffset,
        List<Vector3Int> tilesToCheckOffset
    )
    {
        CostInfoScript costInfoScript = Object.FindAnyObjectByType<CostInfoScript>();
        List<Vector3Int> result = new();

        foreach (var targetOffset in tilesToCheckOffset)
        {
            if (HasLineOfSight(originOffset, targetOffset, costInfoScript))
                result.Add(targetOffset);
        }

        return result;
    }


    /// <summary>
    /// Performs a hex line-of-sight check using cube interpolation.
    /// Works with Point-Top Odd-R/Even-R (auto-configured).
    /// </summary>
    private static bool HasLineOfSight(
       Vector3Int originOffset,
       Vector3Int targetOffset,
       CostInfoScript costInfo
   )
    {
        if (originOffset == targetOffset)
            return true;

        // Convert to cube coordinates
        var originCube = TilemapUtilityScript.OffsetToCube_PointTop(
            originOffset,
            TilemapUtilityScript.UseOddROffset
        );
        var targetCube = TilemapUtilityScript.OffsetToCube_PointTop(
            targetOffset,
            TilemapUtilityScript.UseOddROffset
        );

        var line = HexLine(originCube, targetCube);

        // Step along the line
        for (int i = 1; i < line.Count; i++)
        {
            var off = TilemapUtilityScript.CubeToOffset_PointTop(
                line[i],
                TilemapUtilityScript.UseOddROffset
            );

            if (!costInfo.costInfoDict.TryGetValue(off, out var tileInfo))
                continue;

            // If this tile blocks vision
            if (tileInfo.isUnwalkable || tileInfo.isOccupied)
            {
                // If this is the target tile, it's still visible
                if (i == line.Count - 1)
                    return true;

                // Otherwise, vision is blocked
                return false;
            }
        }

        // No blockers found, fully visible
        return true;
    }

    // ------------------------------------------------------------------
    // HEX LINE MATH (cube space)
    // ------------------------------------------------------------------

    private static List<Vector3Int> HexLine(Vector3Int a, Vector3Int b)
    {
        int N = HexDistance(a, b);
        List<Vector3Int> results = new();

        for (int i = 0; i <= N; i++)
        {
            float t = (float)i / N;
            results.Add(HexRound(HexLerp(a, b, t)));
        }

        return results;
    }

    private static int HexDistance(Vector3Int a, Vector3Int b)
    {
        return (Mathf.Abs(a.x - b.x)
              + Mathf.Abs(a.y - b.y)
              + Mathf.Abs(a.z - b.z)) / 2;
    }

    private static Vector3 HexLerp(Vector3Int a, Vector3Int b, float t)
    {
        return new Vector3(
            Mathf.Lerp(a.x, b.x, t),
            Mathf.Lerp(a.y, b.y, t),
            Mathf.Lerp(a.z, b.z, t)
        );
    }

    private static Vector3Int HexRound(Vector3 h)
    {
        float rx = Mathf.Round(h.x);
        float ry = Mathf.Round(h.y);
        float rz = Mathf.Round(h.z);

        float dx = Mathf.Abs(rx - h.x);
        float dy = Mathf.Abs(ry - h.y);
        float dz = Mathf.Abs(rz - h.z);

        if (dx > dy && dx > dz)
            rx = -ry - rz;
        else if (dy > dz)
            ry = -rx - rz;
        else
            rz = -rx - ry;

        return new Vector3Int((int)rx, (int)ry, (int)rz);
    }
}
