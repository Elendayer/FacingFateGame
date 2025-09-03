using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    protected void Awake()
    {
        // Singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Ensure only one instance exists
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: persist between scenes
    }
    public void Endturn()
    {
        GameEvents.TriggerTurnEnd();
        Invoke(nameof(Startturn), 0.1f);
    }
    public void Startturn()
    {
        GameEvents.TriggerTurnStart();
    }
}