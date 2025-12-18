using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenMaster : MonoBehaviour
{
    public CostInfoScript costInfoScript;

    public Tilemap BaseMap;
    public Tilemap OverlayMap;

    public HexagonalRuleTile defaultTile;
    public HexagonalRuleTile unwalkableTile;
    public HexagonalRuleTile lowAvoidanceTile;

    public HexagonalRuleTile overlayTile;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetUp()
    {
        BaseMap.CompressBounds();

        OverlayGen();
        CollectMap();
    }

    private void CollectMap()
    {
        costInfoScript.costInfoDict.Clear();

        for (int i = -25; i < 25; i++)
        {
            for (int j = -25; j < 25; j++)
            {
                Vector3Int vector3Int = new Vector3Int(i,j,0);

                TileBase tile = BaseMap.GetTile(vector3Int);

                CostInfo costInfo = new CostInfo();

                switch (tile)
                {
                    case var t when t == defaultTile:
                        costInfo.cost = 5;
                        costInfo.isUnwalkable = false;
                        break;

                    case var t when t == unwalkableTile:
                        costInfo.cost = 999999;
                        costInfo.isUnwalkable = true;
                        costInfo.isOccupied = true;
                        break;

                    case var t when t == lowAvoidanceTile:
                        costInfo.cost = 7;
                        costInfo.isUnwalkable = false;
                        break;
                    default:
                        costInfo.cost = 999999;
                        costInfo.isUnwalkable = true;
                        costInfo.isOccupied = true;
                        break;
                }

                Debug.Log($"{vector3Int}");
                costInfoScript.costInfoDict.Add(vector3Int, costInfo);
            }
        }
    }
    private void OverlayGen()
    {        
        BoundsInt bounds = BaseMap.cellBounds;

        foreach (Vector3Int cellPos in bounds.allPositionsWithin)
        {
            OverlayMap.SetTile(cellPos, overlayTile);
        }
    }
}