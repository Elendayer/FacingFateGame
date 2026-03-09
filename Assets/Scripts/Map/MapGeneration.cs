using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGeneration : MonoBehaviour
{
    public HexagonalRuleTile hexagonalTile;
    public TileInfoScript costInfoScript = null;

    public Tilemap tilemap;
    public int SizeX;
    public int SizeY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Gen()
    {
        for (int i = 0; i < SizeX; i++)
        {
            for (int j = 0; j < SizeY; j++)
            {
                Vector3Int vector3Int = new Vector3Int(i - (SizeX / 2), j - (SizeY / 2), 0);

                tilemap.SetTile(vector3Int, hexagonalTile);
            }
        }
    }
}
