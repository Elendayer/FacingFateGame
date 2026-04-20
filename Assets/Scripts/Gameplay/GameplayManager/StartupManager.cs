using UnityEngine;

namespace facingfate
{

    public class StartupManager : MonoBehaviour
    {
        public static StartupManager Instance { get; private set; }
        public void Awake()
        {
            // Singleton enforcement
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Ensure only one instance exists
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: persist between scenes

            Invoke(nameof(HandleMapStartup), 0.1f);
            Invoke(nameof(HandleStartup), 0.2f);
            Invoke(nameof(HandlePostStartup), 2f);
        }
        private void HandleMapStartup()
        {
            //CombatMapMaster mapGenMaster = FindAnyObjectByType<CombatMapMaster>();

            //mapGenMaster.SetUp();
        }
        private void HandleStartup()
        {
            CardDatabase.RegisterAll();
            AiBiasDatabase.RegisterAll();
            NpcDatabase.RegisterAll();

            DeckManager.Instance.StartUp();
            TurnManager.Instance.StartUp();
            EncounterManager.Instance.StartUp();

            if (RandomEncounterManager.Instance != null)
                RandomEncounterManager.Instance.SpawnEntities();
            if (TrainingEncounterManager.Instance != null)
                TrainingEncounterManager.Instance.SpawnEntities();

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
}
