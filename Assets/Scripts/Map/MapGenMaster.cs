using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class MapGenMaster : MonoBehaviour
{
    public BasemapHexTile MapTile;
    public HexagonalRuleTile OverlayTile;

    public CostInfoScript costInfoScript;

    public Tilemap BaseMap;
    public Tilemap OverlayMap;

    public int SizeX = 32;
    public int SizeY = 32;


    [Header("Noise Settings")]
    public float noiseScale = 0.08f;
    public int seed = 1337;

    [Serializable]
    public struct TerrainBand
    {
        public BasemapHexTile tile;        
        [Range(0f, 1f)] public float threshold; // n < threshold -> wähle dieses Tile
    }

    [Header("Terrain Bands (ascending thresholds)")]
    public TerrainBand[] terrainBands;

    void Awake()
    {
        MapGen();
        OverlayGen();
    }

    private void MapGen()
    {
        if (BaseMap == null) { Debug.LogError("MapGenMaster: BaseMap is not assigned."); return; }
        if (costInfoScript == null) { Debug.LogError("MapGenMaster: costInfoScript is not assigned."); return; }

        costInfoScript.costInfoDict.Clear();

        int halfX = SizeX / 2;
        int halfY = SizeY / 2;

        // Hinweis: PerlinNoise braucht keinen Random.InitState; wir benutzen seed als Offset
        for (int i = 0; i < SizeX; i++)
        {
            for (int j = 0; j < SizeY; j++)
            {
                var cell = new Vector3Int(i - halfX, j - halfY, 0);

                // 1) Noise-Wert pro Zelle
                float n = Mathf.PerlinNoise((i + seed) * noiseScale, (j + seed) * noiseScale);

                // 2) Passendes Tile nach Band wählen
                var chosen = PickTile(n);

                // 3) Auf Tilemap setzen (+ optional: Zellen-Tint)
                BaseMap.SetTile(cell, chosen);

                // 4) CostInfo aus dem tatsächlich gesetzten Tile ableiten
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
                    // Fallback (z. B. wenn Bands leer sind)
                    info.materialId = "Default";
                    info.costUnobstructed = 1;
                    info.isUnwalkable = false;
                }

                costInfoScript.costInfoDict[cell] = info;
            }
        }
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

        // Optional: sicherstellen, dass das letzte Band wirklich 1.0 abdeckt
        var last = terrainBands[terrainBands.Length - 1];
        if (last.threshold < 0.9999f)
        {
            last.threshold = 1f;
            terrainBands[terrainBands.Length - 1] = last;
        }
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
}