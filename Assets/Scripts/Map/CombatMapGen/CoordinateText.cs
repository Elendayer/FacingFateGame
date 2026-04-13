using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CoordinateText : MonoBehaviour
{
    Vector3Int cellPos = new();
    TileInfoScript costInfoScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Tilemap[] tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        costInfoScript = FindFirstObjectByType<TileInfoScript>(0);

        cellPos = GetComponentInParent<Tilemap>().WorldToCell(transform.position);
        int cost = 0;

        int key = Utility.TilemapUtilityScript.PositionToKey(cellPos);
        costInfoScript.tileInfoDict.TryGetValue(key, out TileInfo costInfo);
        if (costInfo != null)
        {
            cost = costInfo.costCheck;
        }
        GetComponent<TextMeshProUGUI>().text = $"{cellPos.x},{cellPos.y} : {cost}";
    }
}