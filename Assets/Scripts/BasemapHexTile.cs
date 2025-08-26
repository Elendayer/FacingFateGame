using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/BasemapHexTile")]
public class BasemapHexTile : HexagonalRuleTile
{
    [SerializeField]   
    public CostInfo costInfo = new CostInfo();

    [Header("Sprites")]
    public bool useHighlightedSprite = false;
    public Sprite DefaultSprite;
    public Sprite HighlightedSprite;
}

[System.Serializable]
public class CostInfo
{
    public int cost => 2 +
  (isWalkable ? 1 : int.MaxValue) +
  (isOccupied ? int.MaxValue : 0) +
  (highAvoidance ? 30 :
   mediumAvoidance ? 20 :
   lowAvoidance ? 10 : 0);

    [Header("Cost Calc")]
    public bool lowAvoidance = false;
    public bool mediumAvoidance = false;
    public bool highAvoidance = false;
    
    public bool isWalkable = true;
    public bool isOccupied = false;
}