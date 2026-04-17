using System.Collections.Generic;
using UnityEngine;

namespace facingfate
{
    /// <summary>
    /// Sets up a controlled training encounter.
    /// Spawns 1–2 simple enemies with a small, fixed neutral deck.
    /// Player entities are placed manually in the scene as PlayerScript prefabs.
    ///
    /// Unity Setup (Training Scene):
    ///   1. Place 1–4 PlayerScript prefabs in the scene. Set their deckCardIDs in the Inspector.
    ///   2. Add this component to a GameObject in the scene.
    ///   3. Assign EnemySpawnPoints (empty GameObjects as position markers).
    ///   4. Optionally override EnemyCardIDs in the Inspector for a custom training enemy deck.
    /// </summary>
    public class TrainingEncounterManager : MonoBehaviour
    {
        public static TrainingEncounterManager Instance { get; private set; }

        [Header("Enemy Config")]
        [Tooltip("Number of training enemies to spawn")]
        [Range(1, 4)]
        [SerializeField] private int enemyCount = 1;

        [Tooltip("Spawn positions for training enemies")]
        [SerializeField] private List<Transform> enemySpawnPoints = new();

        [Header("Enemy Deck")]
        [Tooltip("Fixed card IDs for the training enemy deck. Keep simple for learning.")]
        [SerializeField] private List<string> enemyCardIDs = new()
        {
            "Neutral_Tech_Strike",
            "Neutral_Tech_Strike",
            "Neutral_Tech_Strike",
            "Neutral_Abil_Recover",
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Called by StartupManager before the entity StartUp loop.
        /// </summary>
        public void SpawnEntities()
        {
            if (enemySpawnPoints.Count < enemyCount)
            {
                Debug.LogWarning($"[TrainingEncounterManager] Only {enemySpawnPoints.Count} spawn points for {enemyCount} enemies. Clamping.");
                enemyCount = enemySpawnPoints.Count;
            }

            for (int i = 0; i < enemyCount; i++)
            {
                SpawnTrainingEnemy(enemySpawnPoints[i].position, i);
            }
        }

        private void SpawnTrainingEnemy(Vector3 position, int index)
        {
            GameObject obj = Instantiate(AssetManager.Instance.entityPrefab);
            obj.transform.position = position;
            obj.name = $"TrainingEnemy_{index + 1}";

            NonPlayerScript entity = obj.GetComponent<NonPlayerScript>();

            if (entity == null)
            {
                Debug.LogError("[TrainingEncounterManager] entityPrefab has no NonPlayerScript.");
                Destroy(obj);
                return;
            }

            entity.usePresetConfig = true;
            entity.entityAffiliation = EntityAffiliation.Enemy;
            entity.deckCardIDs = new List<string>(enemyCardIDs);
            entity.npcData = new NpcData
            {
                id = $"TrainingEnemy_{index}",
                name = "Training Dummy",
                aiBias = AiBiasDatabase.GetBiasById("StupidFuck"),
                cardIds = new List<string>(enemyCardIDs)
            };
        }
    }
}
