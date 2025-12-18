using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/BasemapHexTile")]
public class BasemapHexTile : HexagonalRuleTile
{
    [Header("Material & Movement")]
    public string materialId = "Grass";       
    [Min(1)] public int baseMoveCost = 1;      
    public bool unwalkable = false;

}