using TMPro;
using UnityEngine;

public class EntityVisualScript : MonoBehaviour
{
    public TextMeshPro textMeshProUGUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {
        textMeshProUGUI.text = transform.parent.name;
    }
}
