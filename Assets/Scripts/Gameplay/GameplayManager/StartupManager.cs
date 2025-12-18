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

        new WaitForSeconds(1f);

        Invoke(nameof(HandleMapStartup), 1f);
        Invoke(nameof(HandleStartup), 1f);
        Invoke(nameof(HandlePostStartup), 1f);
    }
    private void HandleMapStartup()
    {
        MapGenMaster mapGenMaster = FindAnyObjectByType<MapGenMaster>();

        mapGenMaster.SetUp();
    }
    private void HandleStartup()
    {
        CardDatabase.RegisterAll();
        AiBiasDatabase.RegisterAll();
        NpcDatabase.RegisterAll();

        DeckManager.Instance.StartUp();
        TurnManager.Instance.StartUp();

        foreach (var entity in GameObject.FindObjectsByType<EntityScript>(0))
        {
            entity.StartUp();
        }
    }

    private void HandlePostStartup()
    {
        GameEvents.TriggerCombatStart();
    }
}
