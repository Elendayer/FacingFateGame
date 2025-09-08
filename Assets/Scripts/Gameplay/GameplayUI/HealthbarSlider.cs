using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarSlider : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI textMeshPro;
    public EntityScript eM;

    int max => eM.MaxHealth.GetFinalValue();
    int current => eM.CurrentHealth.GetFinalValue();

    private void Start()
    {
        eM = GetComponentInParent<EntityScript>();
    }
    // Update is called once per frame
    void Update()
    {
        slider.maxValue = max;

        slider.value = current;
        textMeshPro.text = $"{current} / {max}";
    }
}