using facingfate;
using UnityEngine;
using UnityEngine.UI;

public class StartEncounterBinder : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            GameEvents.TriggerEncounterStart();
            gameObject.SetActive(false);
        });
    }
}
