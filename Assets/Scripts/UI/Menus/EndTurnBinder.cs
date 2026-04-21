using facingfate;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnBinder : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            GameEvents.TriggerTurnEnd();
        });
    }
}
