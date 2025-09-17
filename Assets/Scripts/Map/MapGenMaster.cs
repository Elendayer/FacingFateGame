using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenMaster : MonoBehaviour
{
    public HexagonalRuleTile MapTile;
    public HexagonalRuleTile OverlayTile;

    public CostInfoScript costInfoScript;

    public Tilemap BaseMap;
    public Tilemap OverlayMap;

    public int SizeX;
    public int SizeY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        MapGen();
        OverlayGen();
    }

    private void MapGen()
    {
        for (int i = 0; i < SizeX; i++)
        {
            for (int j = 0; j < SizeY; j++)
            {
                Vector3Int vector3Int = new Vector3Int(i - (SizeX / 2), j - (SizeY / 2), 0);

                BaseMap.SetTile(vector3Int, MapTile);
                costInfoScript.costInfoDict.Add(vector3Int, new CostInfo() { isWalkable = true } );
            }
        }
    }

    private void OverlayGen()
    {
        for (int i = 0; i < SizeX; i++)
        {
            for (int j = 0; j < SizeY; j++)
            {
                Vector3Int vector3Int = new Vector3Int(i - (SizeX / 2), j - (SizeY / 2), 0);

                OverlayMap.SetTile(vector3Int, OverlayTile);
            }
        }
    }
}