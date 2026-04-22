using System.Collections;
using UnityEngine;

namespace facingfate
{

    public class StartupManager : MonoBehaviour
    {
        public static StartupManager Instance { get; private set; }
        public void Start()
        {
            // Singleton enforcement
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Ensure only one instance exists
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: persist between scenes

            AddListener();
        }

        private void AddListener()
        {
            GameEvents.OnEncounterStart += OnEncounterStart;
        }
        private void OnDestroy()
        {
            GameEvents.OnEncounterStart -= OnEncounterStart;
        }

        private void OnEncounterStart()
        {
            // Start the initialization sequence when the encounter begins
            StartCoroutine(InitialStartup());
        }

        private IEnumerator InitialStartup()
        {
            yield return new WaitForSeconds(0.1f);

            Debug.Log("Starting encounter initialization sequence...");
            // Run the full startup sequence BEFORE triggering combat
            // yield return StartCoroutine(HandleMapStartup());
            yield return StartCoroutine(HandleStartup());

            // Only trigger combat start after everything is initialized
            GameEvents.TriggerCombatStart();
        }

        private void HandleMapStartup()
        {

        }
        private IEnumerator HandleStartup()
        {
            // Register all databases
            CardDatabase.RegisterAll();
            yield return null;

            AiBiasDatabase.RegisterAll();
            yield return null;

            NpcDatabase.RegisterAll();
            yield return null;

            // Initialize managers
            DeckManager.Instance.StartUp();
            yield return null;

            TurnManager.Instance.StartUp();
            yield return null;

            EncounterManager.Instance.StartUp();
            yield return null;

            // Spawn entities
            if (RandomEncounterManager.Instance != null)
            {
                RandomEncounterManager.Instance.SpawnEntities();
                yield return null;
            }

            if (TrainingEncounterManager.Instance != null)
            {
                TrainingEncounterManager.Instance.SpawnEntities();
                yield return null;
            }

            if (TutorialEncounterSetup.Instance != null)
            {
                TutorialEncounterSetup.Instance.SpawnEntities();
                yield return null;
            }

            // Initialize all entities
            foreach (var entity in GameObject.FindObjectsByType<EntityScript>(0))
            {
                entity.StartUp();
                yield return null;
            }
        }
    }
}
