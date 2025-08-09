using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TickSlider : MonoBehaviour
{
    public Slider slider;
    public float fillDuration = 5f;  // Time in seconds to fill the slider (5 seconds)

    void Start()
    {
        if (slider == null)
        {
            Debug.LogError("Slider is not assigned!");
            return;
        }

        // Start the filling process on the first frame
        StartCoroutine(FillSlider());
    }

    // Coroutine that will fill the slider over 5 seconds and then reset
    IEnumerator FillSlider()
    {
        while (true)  // Loop to continuously repeat the process
        {
            float timeElapsed = 0f;

            // Gradually fill the slider to the maximum value (1) over the set duration
            while (timeElapsed < fillDuration)
            {
                slider.value = Mathf.Lerp(0, 1, timeElapsed / fillDuration);  // Linearly interpolate from 0 to 1
                timeElapsed += Time.deltaTime;
                yield return null;  // Wait for the next frame
            }

            slider.value = 1;  // Ensure the slider reaches exactly 1 after the loop

            // Trigger the event once the slider is filled
            GameEvents.TriggerTurnStart();
            Debug.Log("Turn Start");

            // Reset the slider to 0 immediately after the event is triggered
            slider.value = 0;

            // Wait a bit before the next filling cycle (optional, can be removed for immediate restart)
            yield return new WaitForSeconds(0.05f);  // Optional delay (e.g., 0.5 seconds)
        }
    }
}
