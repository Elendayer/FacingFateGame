using System.Collections.Generic;
using UnityEngine;

public class CostInfoScript : MonoBehaviour
{
    public Dictionary<Vector3Int,CostInfo> costInfoDict = new Dictionary<Vector3Int, CostInfo>();
}
public class CostInfo
{
    public int cost = 5;
    public int costCheck => isOccupied ? 999999 : cost;

    [Header("Cost Calc")]
    public bool isUnwalkable = false;
    public bool isOccupied = false;
}