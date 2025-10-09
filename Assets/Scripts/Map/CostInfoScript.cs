using System.Collections.Generic;
using UnityEngine;

public class CostInfoScript : MonoBehaviour
{
    public Dictionary<Vector3Int,CostInfo> costInfoDict = new Dictionary<Vector3Int, CostInfo>();
}
public class CostInfo
{
    public int cost => 1 +
(isUnwalkable ?  9999: 0) +
(isOccupied ? 9999 : 0) +
(highAvoidance ? 10 :
mediumAvoidance ? 5 :
lowAvoidance ? 2 : 0);

    public int costUnobstructed = 1;

    [Header("Cost Calc")]
    public bool lowAvoidance = false;
    public bool mediumAvoidance = false;
    public bool highAvoidance = false;

    public bool isUnwalkable = false;
    public bool isOccupied = false;
}