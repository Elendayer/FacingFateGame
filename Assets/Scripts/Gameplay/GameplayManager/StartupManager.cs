using UnityEngine;

public class StartupManager : MonoBehaviour
{
    public static StartupManager Instance { get; private set; }
    public void Awake()
    {
        // Singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Ensure only one instance exists
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: persist between scenes

        Invoke(nameof(HandleStartup), 0.1f);
    }
    private void HandleStartup()
    {
        DeckManager.Instance.Startup();
        TurnManager.Instance.Startup();

        foreach (var entity in GameObject.FindObjectsByType<EntityScript>(0))
        {
            entity.Startup();
        }
    }
}
