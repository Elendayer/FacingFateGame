using System.Collections.Generic;
using UnityEngine;

namespace facingfate
{
    /// <summary>
    /// Sets up the fixed Tutorial encounter: one weak dummy enemy.
    /// Player deck is configured directly on the PlayerScript prefab in the scene (deckCardIDs).
    /// Called by StartupManager.HandleStartup() — same pattern as TrainingEncounterManager.
    /// </summary>
    public class TutorialEncounterSetup : MonoBehaviour
    {
        public static TutorialEncounterSetup Instance { get; private set; }

        [Header("Enemy Spawn")]
        [SerializeField] private Transform enemySpawnPoint;

        [Header("Dummy Enemy Deck")]
        [Tooltip("Card IDs for the tutorial dummy. Use a near-zero-damage card.")]
        [SerializeField] private List<string> dummyCardIds = new()
        {
            "Neutral_Tech_Strike",
        };

        [Header("AI Bias ID")]
        [Tooltip("Weakest AI bias available. Check AiBiasDatabase for valid IDs.")]
        [SerializeField] private string dummyAiBiasId = "StupidFuck";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>Called by StartupManager before entity initialization loop.</summary>
        public void SpawnEntities()
        {
            if (enemySpawnPoint == null)
            {
                Debug.LogError("[TutorialEncounterSetup] enemySpawnPoint not assigned.");
                return;
            }

            GameObject obj = Instantiate(AssetManager.Instance.entityPrefab);
            obj.transform.position = enemySpawnPoint.position;
            obj.name = "TutorialDummy";

            NonPlayerScript entity = obj.GetComponent<NonPlayerScript>();
            if (entity == null)
            {
                Debug.LogError("[TutorialEncounterSetup] entityPrefab missing NonPlayerScript.");
                Destroy(obj);
                return;
            }

            entity.usePresetConfig = true;
            entity.entityAffiliation = EntityAffiliation.Enemy;
            entity.deckCardIDs = new List<string>(dummyCardIds);
            entity.npcData = new NpcData
            {
                id     = "TutorialDummy",
                name   = "Training Dummy",
                aiBias = AiBiasDatabase.GetBiasById(dummyAiBiasId),
                cardIds = new List<string>(dummyCardIds)
            };
        }
    }
}
