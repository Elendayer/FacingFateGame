using System.Collections.Generic;
using UnityEngine;

public class TileInfoScript : MonoBehaviour
{
    public Dictionary<int,TileInfo> tileInfoDict = new Dictionary<int, TileInfo>();
}

public class TileInfo
{
    public int index;
    public Vector3Int position;
    public Vector3Int cube; // cached cube coordinate (point-top)
    public List<int> neighbors = new List<int>();

    public int cost = 5;
    public int costCheck => isOccupied ? 999999 : cost;

    public bool isUnwalkable;
    public bool isOccupied;
}