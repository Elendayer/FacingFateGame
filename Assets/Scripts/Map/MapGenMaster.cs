using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;
using System.Linq;

public class MapGenMaster : MonoBehaviour
{
    public BasemapHexTile MapTile;
    public HexagonalRuleTile OverlayTile;
    public Tilemap BaseMap;
    public Tilemap OverlayMap;

    public enum HexLayout { PointyOddR, PointyEvenR, FlatOddQ, FlatEvenQ }
    [Header("Hex Layout")]
    public HexLayout hexLayout = HexLayout.PointyOddR; // falls deine Grid 'Point Top' ist, meist OddR oder EvenR

    public CostInfoScript costInfoScript;

    [Header("Map Size")]
    public int SizeX = 32;
    public int SizeY = 32;

    public enum TerrainMode { Perlin, Voronoi }

    [Header("Generator Mode")]
    public TerrainMode terrainMode = TerrainMode.Perlin;

    [Header("Voronoi Settings")]
    public int voronoiSites = 16;               // 12–24 ist meistens gut
    [Range(0f, 0.5f)]
    public float siteJitter = 0.25f;            // 0.15–0.35 = etwas unregelmäßig
    private List<Vector2> vSites = new List<Vector2>();
    private List<float> vValues = new List<float>(); // 0..1 je Site


    [Header("Noise Settings")]
    public float noiseScale = 0.08f;
    public int seed = 1337;

    [Serializable]
    public struct TerrainBand
    {
        public BasemapHexTile tile;        
        [Range(0f, 1f)] public float threshold;
    }

    [Header("Terrain Bands (ascending thresholds)")]
    public TerrainBand[] terrainBands;

    [Header("Connectivity")]
    public bool ensureConnectivity = true;
    public BasemapHexTile connectorTile;
    public string[] bridgeableMaterials = new[] { "Water" }; 
    public int waterCrossPenalty = 20;                       


    public enum ShorelineMode
    {
        None,          
        PaintTile,     
        CliffUnwalkable 
    }

    [Header("Shoreline")]
    public bool enableShoreline = true;                 // Master-Toggle
    public ShorelineMode shorelineMode = ShorelineMode.PaintTile;
    public BasemapHexTile shorelineTile;                
    [Range(1, 3)] public int shorelineWidth = 1;        

    public string[] shorelineFromMaterialIds = new[] { "Water" };
    public string[] shorelineOnLandMaterialIds = new[] { "Grass", "Base", "BaseHexTile" };


    void Awake()
    {
        StartCoroutine(GenerateMapCoroutine());
    }

    private System.Collections.IEnumerator GenerateMapCoroutine()
    {
        MapGen();
        yield return null;   

        if (enableShoreline && shorelineMode != ShorelineMode.None)
        {
            ApplyShoreline();
            yield return null;
        }

        if (ensureConnectivity)
        {
            EnsureConnectivity();
            yield return null;
        }

        OverlayGen();
    }



    private void MapGen()
    {
        if (BaseMap == null) { Debug.LogError("MapGenMaster: BaseMap is not assigned."); return; }
        if (costInfoScript == null) { Debug.LogError("MapGenMaster: costInfoScript is not assigned."); return; }

        costInfoScript.costInfoDict.Clear();

        if (terrainMode == TerrainMode.Voronoi)
            BuildVoronoiSites(); // NEU

        int halfX = SizeX / 2;
        int halfY = SizeY / 2;

        for (int i = 0; i < SizeX; i++)
            for (int j = 0; j < SizeY; j++)
            {
                var cell = new Vector3Int(i - halfX, j - halfY, 0);

                // Nur diese Zeile ist neu/anders:
                float n = (terrainMode == TerrainMode.Perlin)
                    ? Mathf.PerlinNoise((i + seed) * noiseScale, (j + seed) * noiseScale)
                    : VoronoiValue(i, j);

                var chosen = PickTile(n);

                BaseMap.SetTile(cell, chosen);

                var info = new CostInfo();
                var tile = BaseMap.GetTile(cell) as BasemapHexTile;
                if (tile != null)
                {
                    info.materialId = string.IsNullOrEmpty(tile.materialId) ? tile.name : tile.materialId;
                    info.costUnobstructed = Mathf.Max(1, tile.baseMoveCost);
                    info.isUnwalkable = tile.unwalkable;
                }
                else
                {
                    info.materialId = "Default";
                    info.costUnobstructed = 1;
                    info.isUnwalkable = false;
                }
                costInfoScript.costInfoDict[cell] = info;
            }

        LogTerrainDistribution(); // falls du die Verteilungs-Logs nutzt
    }


    private BasemapHexTile PickTile(float noiseValue)
    {
        // terrainBands: nach threshold aufsteigend (0..1) sortiert
        if (terrainBands != null && terrainBands.Length > 0)
        {
            for (int k = 0; k < terrainBands.Length; k++)
            {
                var b = terrainBands[k];
                if (noiseValue < Mathf.Clamp01(b.threshold))
                    return b.tile != null ? b.tile : MapTile;
            }
            // Wenn kein threshold getroffen: letztes Band nehmen
            var last = terrainBands[terrainBands.Length - 1];
            return last.tile != null ? last.tile : MapTile;
        }
        // Wenn keine Bänder definiert sind: Fallback
        return MapTile;
    }

    private void OnValidate()
    {
        // thresholds clampen & sortieren, damit sie aufsteigend sind
        if (terrainBands == null || terrainBands.Length == 0) return;

        for (int i = 0; i < terrainBands.Length; i++)
            terrainBands[i].threshold = Mathf.Clamp01(terrainBands[i].threshold);

        // Aufsteigend nach threshold sortieren
        Array.Sort(terrainBands, (a, b) => a.threshold.CompareTo(b.threshold));

        /* Optional: sicherstellen, dass das letzte Band wirklich 1.0 abdeckt
        var last = terrainBands[terrainBands.Length - 1];
        if (last.threshold < 0.9999f)
        {
            last.threshold = 1f;
            terrainBands[terrainBands.Length - 1] = last;
        }
        */
    }

    private void OverlayGen()
    {
        if (OverlayMap == null || OverlayTile == null) return;

        int halfX = SizeX / 2;
        int halfY = SizeY / 2;
        for (int i = 0; i < SizeX; i++)
            for (int j = 0; j < SizeY; j++)
            {
                Vector3Int cell = new Vector3Int(i - halfX, j - halfY, 0);
                OverlayMap.SetTile(cell, OverlayTile);
            }
    }

    private void LogTerrainDistribution()
    {
        var dict = costInfoScript?.costInfoDict;
        if (dict == null || dict.Count == 0)
        {
            Debug.Log("[MapGen] No costInfoDict (empty or missing)");
            return;
        }

        int total = dict.Count;
        var groups = dict.Values
            .GroupBy(v => (v.isUnwalkable ? "X:" : "") + (string.IsNullOrEmpty(v.materialId) ? "Default" : v.materialId))
            .OrderByDescending(g => g.Count());

        foreach (var g in groups)
        {
            float pct = (100f * g.Count()) / total;
            Debug.Log($"[MapGen] {g.Key}: {g.Count()} ({pct:F1}%)");
        }
    }

    private void EnsureConnectivity()
    {
        if (BaseMap == null || costInfoScript == null || costInfoScript.costInfoDict == null || costInfoScript.costInfoDict.Count == 0)
        {
            Debug.LogWarning("[MapGen] Connectivity: BaseMap or costInfo missing, skipping.");
            return;
        }

        // 1) Walkable Felder
        var walkable = new HashSet<Vector3Int>();
        foreach (var kv in costInfoScript.costInfoDict)
            if (!kv.Value.isUnwalkable) walkable.Add(kv.Key);
        if (walkable.Count == 0) { Debug.Log("[MapGen] Connectivity: No walkable tiles."); return; }

        // 2) Komponenten
        var comps = LabelComponents(walkable);
        if (comps.Count <= 1) { Debug.Log("[MapGen] Connectivity: Already single component."); return; }

        // 3) Größte Komponente verbinden
        var main = comps.OrderByDescending(c => c.Count).First();
        foreach (var comp in comps.Where(c => !ReferenceEquals(c, main)))
        {
            var result = FindCheapestBridge(main, comp); // a,b + beste Linie
            if (result.path == null || result.path.Count == 0)
            {
                Debug.LogWarning("[MapGen] Connectivity: no valid bridge path (blocked by non-bridgeables?)");
                continue;
            }
            Debug.Log($"[MapGen] Connectivity: bridge {result.a} -> {result.b} len={result.path.Count}");
            CarvePath(result.path);
            foreach (var c in comp) main.Add(c);
        }

        Debug.Log("[MapGen] Connectivity: All components connected.");
    }

    private HashSet<Vector3Int> GetBorder(HashSet<Vector3Int> comp)
    {
        var border = new HashSet<Vector3Int>();
        foreach (var c in comp)
        {
            foreach (var n in GetNeighbors(c))
            {
                if (!costInfoScript.costInfoDict.TryGetValue(n, out var ni)) continue;
                if (ni.isUnwalkable) { border.Add(c); break; }
            }
        }
        return border;
    }

    private bool IsBridgeable(Vector3Int cell)
    {
        if (!costInfoScript.costInfoDict.TryGetValue(cell, out var info)) return false;
        // Land ist immer okay (muss nicht „überbrückt“ werden)
        if (!info.isUnwalkable) return true;

        // Für unwalkable Felder: nur bestimmte MaterialIds erlauben (z. B. Water)
        var t = BaseMap.GetTile(cell) as BasemapHexTile;
        string mid = t != null ? (string.IsNullOrEmpty(t.materialId) ? t.name : t.materialId) : (info.materialId ?? "");
        if (bridgeableMaterials == null || bridgeableMaterials.Length == 0) return false;
        for (int i = 0; i < bridgeableMaterials.Length; i++)
            if (mid == bridgeableMaterials[i]) return true;
        return false;
    }

    private (Vector3Int a, Vector3Int b, List<Vector3Int> path) FindCheapestBridge(HashSet<Vector3Int> main, HashSet<Vector3Int> comp)
    {
        var mainBorder = GetBorder(main);
        var compBorder = GetBorder(comp);

        int bestCost = int.MaxValue; Vector3Int bestA = default, bestB = default; List<Vector3Int> bestPath = null;

        foreach (var a in mainBorder)
        {
            foreach (var b in compBorder)
            {
                var line = HexLine(a, b);
                int cost = 0; bool invalid = false;

                for (int k = 0; k < line.Count; k++)
                {
                    var c = line[k];
                    if (!costInfoScript.costInfoDict.TryGetValue(c, out var info)) { invalid = true; break; }
                    if (!info.isUnwalkable) { cost += 1; continue; }        // Land: gering
                    if (!IsBridgeable(c)) { invalid = true; break; }        // z. B. Berge -> verbieten
                    cost += waterCrossPenalty;                               // Wasser: teuer -> kürzeste Querung gewinnt
                }

                if (!invalid && cost < bestCost)
                {
                    bestCost = cost; bestA = a; bestB = b; bestPath = line;
                }
            }
        }
        return (bestA, bestB, bestPath);
    }


    private List<HashSet<Vector3Int>> LabelComponents(HashSet<Vector3Int> walkable)
    {
        var result = new List<HashSet<Vector3Int>>();
        var visited = new HashSet<Vector3Int>();

        foreach (var start in walkable)
        {
            if (visited.Contains(start)) continue;
            var comp = new HashSet<Vector3Int>();
            var q = new Queue<Vector3Int>();
            q.Enqueue(start); visited.Add(start); comp.Add(start);

            while (q.Count > 0)
            {
                var c = q.Dequeue();
                foreach (var n in GetNeighbors(c))
                {
                    if (!walkable.Contains(n) || visited.Contains(n)) continue;
                    visited.Add(n); comp.Add(n); q.Enqueue(n);
                }
            }
            result.Add(comp);
        }
        return result;
    }

    static readonly Vector2Int[] AxialDirs = new[]
    {
    new Vector2Int(+1, 0),
    new Vector2Int(+1, -1),
    new Vector2Int(0, -1),
    new Vector2Int(-1, 0),
    new Vector2Int(-1, +1),
    new Vector2Int(0, +1),
    };


    private IEnumerable<Vector3Int> GetNeighbors(Vector3Int cell)
    {
        var a = OffsetToAxial(cell);
        for (int i = 0; i < AxialDirs.Length; i++)
        {
            var na = new Vector2Int(a.x + AxialDirs[i].x, a.y + AxialDirs[i].y);
            var no = AxialToOffset(na);
            if (costInfoScript.costInfoDict.ContainsKey(no)) yield return no;
        }
    }


    private (Vector3Int a, Vector3Int b) FindClosestPair(HashSet<Vector3Int> main, HashSet<Vector3Int> comp)
    {
        // Simple & robust: vollvergleich; für 32x32 völlig ok
        int best = int.MaxValue; Vector3Int bestA = default, bestB = default;
        foreach (var a in main)
        {
            foreach (var b in comp)
            {
                int d = HexDistance(a, b);
                if (d < best) { best = d; bestA = a; bestB = b; }
            }
        }
        return (bestA, bestB);
    }

    // Axial -> Cube

    private int HexDistance(Vector3Int A, Vector3Int B)
    {
        var a = OffsetToAxial(A);
        var b = OffsetToAxial(B);
        int dq = a.x - b.x, dr = a.y - b.y, ds = -(dq + dr);
        return (Mathf.Abs(dq) + Mathf.Abs(dr) + Mathf.Abs(ds)) / 2;
    }


    // Hex-Linie zwischen a und b (inklusive beider Endpunkte)
    private List<Vector3Int> HexLine(Vector3Int A, Vector3Int B)
    {
        var a = OffsetToAxial(A);
        var b = OffsetToAxial(B);
        int N = HexDistance(A, B);
        var ac = AxialToCube(a);
        var bc = AxialToCube(b);
        var result = new List<Vector3Int>(N + 1);
        for (int i = 0; i <= N; i++)
        {
            float t = (N == 0) ? 0f : (float)i / N;
            var pCube = CubeRound(Vector3.Lerp(ac, bc, t));
            var pAx = CubeToAxial(pCube);
            result.Add(AxialToOffset(pAx));
        }
        return result;
    }

    private void CarvePath(IEnumerable<Vector3Int> path)
    {
        foreach (var c in path)
        {
            if (!costInfoScript.costInfoDict.TryGetValue(c, out var info)) continue;

            // Nur unwalkable & bridgeable ändern (Land-Endpunkte unberührt lassen)
            if (!info.isUnwalkable) continue;
            if (!IsBridgeable(c)) continue;

            // Optisch: Connector-Tile (z. B. Steg/Pass) setzen, wenn vorhanden
            if (connectorTile != null)
            {
                BaseMap.SetTile(c, connectorTile);
                info.materialId = string.IsNullOrEmpty(connectorTile.materialId) ? connectorTile.name : connectorTile.materialId;
                info.costUnobstructed = Mathf.Max(1, connectorTile.baseMoveCost);
            }
            else
            {
                // Ohne spezielles Tile: einfach begehbar machen; Kosten moderat halten
                info.costUnobstructed = Mathf.Max(1, info.costUnobstructed);
            }

            info.isUnwalkable = false; // jetzt passierbar
            costInfoScript.costInfoDict[c] = info;
        }
    }

    // Offset(x,y) -> Axial(q,r)
    private Vector2Int OffsetToAxial(Vector3Int cell)
    {
        int x = cell.x, y = cell.y;
        switch (hexLayout)
        {
            case HexLayout.PointyOddR: return new Vector2Int(x - ((y - (y & 1)) / 2), y);
            case HexLayout.PointyEvenR: return new Vector2Int(x - ((y + (y & 1)) / 2), y);
            case HexLayout.FlatOddQ: return new Vector2Int(x, y - ((x - (x & 1)) / 2));
            case HexLayout.FlatEvenQ: return new Vector2Int(x, y - ((x + (x & 1)) / 2));
            default: return new Vector2Int(x, y);
        }
    }

    // Axial(q,r) -> Offset(x,y)
    private Vector3Int AxialToOffset(Vector2Int axial)
    {
        int q = axial.x, r = axial.y;
        switch (hexLayout)
        {
            case HexLayout.PointyOddR: return new Vector3Int(q + ((r - (r & 1)) / 2), r, 0);
            case HexLayout.PointyEvenR: return new Vector3Int(q + ((r + (r & 1)) / 2), r, 0);
            case HexLayout.FlatOddQ: return new Vector3Int(q, r + ((q - (q & 1)) / 2), 0);
            case HexLayout.FlatEvenQ: return new Vector3Int(q, r + ((q + (q & 1)) / 2), 0);
            default: return new Vector3Int(q, r, 0);
        }
    }

    // Axial <-> Cube (für Distanz/Linie)
    private static Vector3 AxialToCube(Vector2Int a) { float x = a.x, z = a.y, y = -x - z; return new Vector3(x, y, z); }
    private static Vector2Int CubeToAxial(Vector3 c) { int q = Mathf.RoundToInt(c.x), r = Mathf.RoundToInt(c.z); return new Vector2Int(q, r); }
    private static Vector3 CubeRound(Vector3 c)
    {
        float rx = Mathf.Round(c.x), ry = Mathf.Round(c.y), rz = Mathf.Round(c.z);
        float xdiff = Mathf.Abs(rx - c.x), ydiff = Mathf.Abs(ry - c.y), zdiff = Mathf.Abs(rz - c.z);
        if (xdiff > ydiff && xdiff > zdiff) rx = -ry - rz;
        else if (ydiff > zdiff) ry = -rx - rz;
        else rz = -rx - ry;
        return new Vector3(rx, ry, rz);
    }


    private void BuildVoronoiSites()
    {
        vSites.Clear(); vValues.Clear();
        var rnd = new System.Random(seed);
        for (int s = 0; s < Mathf.Max(1, voronoiSites); s++)
        {
            float x = (float)rnd.NextDouble() * SizeX;
            float y = (float)rnd.NextDouble() * SizeY;
            x += ((float)rnd.NextDouble() * 2f - 1f) * siteJitter;
            y += ((float)rnd.NextDouble() * 2f - 1f) * siteJitter;
            vSites.Add(new Vector2(x, y));
            vValues.Add((float)rnd.NextDouble()); // 0..1 → wird gegen thresholds gemappt
        }
    }

    private float VoronoiValue(int i, int j)
    {
        if (vSites.Count == 0) return 0.5f;
        int best = 0; float bestD = float.MaxValue;
        float px = i + 0.5f, py = j + 0.5f;
        for (int s = 0; s < vSites.Count; s++)
        {
            float dx = px - vSites[s].x;
            float dy = py - vSites[s].y;
            float d2 = dx * dx + dy * dy;
            if (d2 < bestD) { bestD = d2; best = s; }
        }
        return vValues[best]; // 0..1, geht direkt in PickTile(n)
    }


    private void ApplyShoreline()
    {
        var dict = costInfoScript?.costInfoDict;
        if (dict == null || dict.Count == 0) return;

        if (shorelineMode == ShorelineMode.PaintTile && shorelineTile == null)
        {
            Debug.LogWarning("[MapGen] Shoreline: PaintTile gewählt, aber shorelineTile ist nicht gesetzt.");
            return;
        }

        // 1) Erstes Ring-Set: Landzellen, die an 'from'-Materialien grenzen
        var firstRing = new HashSet<Vector3Int>();
        foreach (var kv in dict)
        {
            var cell = kv.Key;
            var info = kv.Value;

            // Nur erlaubte Land-Materialien als Kandidaten
            if (info.isUnwalkable) continue;
            string landId = string.IsNullOrEmpty(info.materialId) ? "Default" : info.materialId;
            if (!StringInArray(landId, shorelineOnLandMaterialIds)) continue;

            bool touchesFrom = false;
            foreach (var n in GetNeighbors(cell))
            {
                if (!dict.TryGetValue(n, out var ni)) continue;

                // vom Nachbar die MaterialId bestimmen (wenn Tile vorhanden, das; sonst Info)
                string nid = null;
                var nTile = BaseMap.GetTile(n) as BasemapHexTile;
                if (nTile != null) nid = string.IsNullOrEmpty(nTile.materialId) ? nTile.name : nTile.materialId;
                else nid = string.IsNullOrEmpty(ni.materialId) ? "Default" : ni.materialId;

                if (StringInArray(nid, shorelineFromMaterialIds))
                {
                    touchesFrom = true;
                    break;
                }
            }
            if (touchesFrom) firstRing.Add(cell);
        }

        // 2) Breite >1: in erlaubtes Land hinein erweitern
        var targetCells = new HashSet<Vector3Int>(firstRing);
        for (int w = 1; w < Mathf.Max(1, shorelineWidth); w++)
        {
            var expand = new HashSet<Vector3Int>();
            foreach (var c in targetCells)
            {
                foreach (var n in GetNeighbors(c))
                {
                    if (targetCells.Contains(n) || firstRing.Contains(n)) continue;
                    if (!dict.TryGetValue(n, out var ni)) continue;
                    if (ni.isUnwalkable) continue;
                    string nid = string.IsNullOrEmpty(ni.materialId) ? "Default" : ni.materialId;
                    if (StringInArray(nid, shorelineOnLandMaterialIds)) expand.Add(n);
                }
            }
            foreach (var e in expand) targetCells.Add(e);
        }

        // 3) Anwenden nach Modus
        int changed = 0;
        foreach (var c in targetCells)
        {
            var info = dict[c];

            if (shorelineMode == ShorelineMode.PaintTile)
            {
                BaseMap.SetTile(c, shorelineTile);

                info.materialId = string.IsNullOrEmpty(shorelineTile.materialId) ? shorelineTile.name : shorelineTile.materialId;
                info.costUnobstructed = Mathf.Max(1, shorelineTile.baseMoveCost);
                info.isUnwalkable = shorelineTile.unwalkable;
                dict[c] = info;
                changed++;
            }
            else if (shorelineMode == ShorelineMode.CliffUnwalkable)
            {
                // Optik optional: wenn ein Tile gesetzt ist, male es; sonst Originalsprite behalten
                if (shorelineTile != null)
                {
                    BaseMap.SetTile(c, shorelineTile);
                    info.materialId = string.IsNullOrEmpty(shorelineTile.materialId) ? shorelineTile.name : shorelineTile.materialId;
                    info.costUnobstructed = Mathf.Max(1, shorelineTile.baseMoveCost);
                }
                // Wichtig: Klippe ist unwalkable
                info.isUnwalkable = true;
                dict[c] = info;
                changed++;
            }
        }

        Debug.Log($"[MapGen] Shoreline applied: {changed} cells (mode={shorelineMode}, width={shorelineWidth}).");
    }

    private static bool StringInArray(string s, string[] arr)
    {
        if (arr == null || arr.Length == 0) return false;
        for (int i = 0; i < arr.Length; i++) if (s == arr[i]) return true;
        return false;
    }

}