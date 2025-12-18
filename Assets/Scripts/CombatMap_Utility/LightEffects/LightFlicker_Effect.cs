using UnityEngine;

public class LightPulse : MonoBehaviour
{
    private Light light;

    [Header("Brightness Range")]
    public float minIntensity = 1f;
    public float maxIntensity = 2f;

    [Header("Pulse Speed")]
    public float speed = 1f;   // lower = slower breathing

    [Header("Color Pulse")]
    public bool enableColorPulse = false;
    [Range(0f, 1f)]
    public float colorShiftAmount = 0.1f; // 0.1 = ±10% shift
    private Color defaultColor;

    void Start()
    {
        light = GetComponent<Light>();
        defaultColor = light.color;
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;

        // Smooth brightness
        light.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        // Smooth color shift
        if (enableColorPulse)
        {
            // Use 3 offset sine waves for smooth RGB variation
            float r = defaultColor.r * (1f + Mathf.Sin(Time.time * speed + 0f) * colorShiftAmount);
            float g = defaultColor.g * (1f + Mathf.Sin(Time.time * speed + 2f) * colorShiftAmount);
            float b = defaultColor.b * (1f + Mathf.Sin(Time.time * speed + 4f) * colorShiftAmount);

            light.color = new Color(r, g, b);
        }
    }
}
