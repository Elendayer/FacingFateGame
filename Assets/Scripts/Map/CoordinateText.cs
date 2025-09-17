using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CoordinateText : MonoBehaviour
{
    Vector3Int cellPos = new();
    CostInfoScript costInfoScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Tilemap[] tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        costInfoScript = FindFirstObjectByType<CostInfoScript>(0);

        cellPos = GetComponentInParent<Tilemap>().WorldToCell(transform.position);
        int cost = 0;

        costInfoScript.costInfoDict.TryGetValue(cellPos, out CostInfo costInfo);
        cost = costInfo.cost;

        GetComponent<TextMeshProUGUI>().text = $"{cellPos.x},{cellPos.y} : {cost}";
    }
    private void Update()
    {
        GetComponent<TextMeshProUGUI>().text = $"{cellPos.x},{cellPos.y} : {costInfoScript.costInfoDict[cellPos].cost}";
    }
}