using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntentionIcon : MonoBehaviour
{
    public Image IconImage;
    public TextMeshProUGUI IconText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        IconImage = GetComponent<Image>();
        IconText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetIcon(Intention intention, int value)
    {
        IconImage.sprite = AssetManager.Instance.IntentionImage(intention);
        IconText.text = value.ToString();
    }
}