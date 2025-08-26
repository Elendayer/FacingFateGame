using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CoordinateTextScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 cellPos = GetComponentInParent<Tilemap>().WorldToCell(transform.position);
        GetComponent<TextMeshProUGUI>().text = $"{cellPos.x},{cellPos.y}";
    }
}