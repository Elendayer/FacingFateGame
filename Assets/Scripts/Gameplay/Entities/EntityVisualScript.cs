using TMPro;
using UnityEngine;

public class EntityVisualScript : MonoBehaviour
{
    public TextMeshPro textMeshProUGUI;
    public GameObject UIAnchor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {
        textMeshProUGUI.text = transform.parent.name;

        if (UIAnchor != null)
        {
            UIAnchor.transform.position = transform.position;
            UIAnchor.transform.rotation = Quaternion.LookRotation(Vector3.forward);
        }
    }
}
