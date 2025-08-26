using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerationScript : MonoBehaviour
{
    public HexagonalRuleTile hexagonalTile;

    public Tilemap tilemap;
    public int SizeX;
    public int SizeY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < SizeX; i++)
        {
            for (int j = 0; j < SizeY; j++)
            { 
                tilemap.SetTile( new Vector3Int( i-(SizeX/2), j - (SizeY / 2), 0), hexagonalTile);
            }
        }
    }
}
