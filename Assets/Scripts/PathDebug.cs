using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class PathDebug
{
    public static void DebugPathCost(Tilemap baseMap, CostInfoScript ci, List<Vector3Int> path)
    {
        if (ci == null || path == null) return;
        int sum = 0; StringBuilder sb = new StringBuilder();
        for (int i = 0; i < path.Count; i++)
        {
            Vector3Int c = path[i];
            if (ci.costInfoDict.TryGetValue(c, out var info)) { sum += info.cost; sb.Append(i == 0 ? $"{c}({info.cost})" : $" -> {c}({info.cost})"); }
            else sb.Append(i == 0 ? $"{c}(?)" : $" -> {c}(?)");
        }
        Debug.Log($"[Path] steps={path.Count} costSum={sum} : {sb}");
    }
}
