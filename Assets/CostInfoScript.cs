using System.Collections.Generic;
using UnityEngine;

public class CostInfoScript : MonoBehaviour
{
    public Dictionary<Vector3Int,CostInfo> costInfoDict = new Dictionary<Vector3Int, CostInfo>();
}
public class CostInfo
{
    public int cost => 1 +
(isWalkable ? 1 : 9999) +
(isOccupied ? 9999 : 0) +
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