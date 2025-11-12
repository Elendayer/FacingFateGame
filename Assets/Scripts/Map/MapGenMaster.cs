using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenMaster : MonoBehaviour
{
    public BasemapHexTile MapTile;
    public HexagonalRuleTile OverlayTile;

    public CostInfoScript costInfoScript;

    public Tilemap BaseMap;
    public Tilemap OverlayMap;

    public int SizeX = 32;
    public int SizeY = 32;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

        for (int i = 0; i < SizeX; i++)
        {
            for (int j = 0; j < SizeY; j++)
            {
                Vector3Int cell = new Vector3Int(i - halfX, j - halfY, 0);

                BaseMap.SetTile(cell, MapTile);

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