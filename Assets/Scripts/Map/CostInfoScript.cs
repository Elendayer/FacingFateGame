using System.Collections.Generic;
using UnityEngine;

public class CostInfoScript : MonoBehaviour
{
    public Dictionary<Vector3Int,CostInfo> costInfoDict = new Dictionary<Vector3Int, CostInfo>();
}
public class CostInfo
{
    public string materialId = "Grass";
    [Min(1)] public int costUnobstructed = 1;  
    public bool isUnwalkable = false;


    public bool lowAvoidance = false;
    public bool mediumAvoidance = false;
    public bool highAvoidance = false;
    public bool isOccupied = false;    


    public int cost
    {
        get
        {
            int avoidance = highAvoidance ? 10 : (mediumAvoidance ? 5 : (lowAvoidance ? 2 : 0));
            int hard = (isUnwalkable ? 9999 : 0) + (isOccupied ? 9999 : 0);
            return costUnobstructed + avoidance + hard;
        }
    }
}