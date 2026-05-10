using System.Collections.Generic;
using UnityEngine;

namespace facingfate
{
    [System.Serializable]
    public class NpcPoolEntry
    {
        public string npcId;
        public int powerValue;
    }

    /// <summary>
    /// Spawns a random encounter using a power-budget system.
    /// Players are pre-placed in the scene as PlayerScript prefabs — this manager
    /// only spawns enemies. Draws NPCs from npcPool until the random budget is spent.
    /// Each NPC uses its predefined deck from NpcDatabase.
    ///
    /// Unity Setup:
    ///   1. Copy Gameplay_Combat_Map.unity → RandomEncounter_Map.unity
    ///   2. Remove TrainingEncounterManager from that scene, add this component instead.
    ///   3. Assign at least 6 EnemySpawnPoints (empty GameObjects as markers).
    ///   4. Fill npcPool with NPC IDs from NpcDatabase + their power values.
    /// </summary>
    public class RandomEncounterManager : MonoBehaviour
    {
        public static RandomEncounterManager Instance { get; private set; }

        [Header("Power Budget")]
        [SerializeField] private int targetPowerMin = 10;
        [SerializeField] private int targetPowerMax = 18;

        [Header("NPC Pool")]
        [Tooltip("NPCs available to spawn. Each entry: NPC ID from NpcDatabase + power cost.")]
        [SerializeField] private List<NpcPoolEntry> npcPool = new();

        [Header("Spawn Points")]
        [Tooltip("At least 6 empty GameObjects marking spawn positions.")]
        [SerializeField] private List<Transform> enemySpawnPoints = new();

        // Tracks enemies spawned this encounter for post-init pass (material assignment etc.)
        private readonly List<NonPlayerScript> spawnedEnemies = new();

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
        /// Called by StartupManager BEFORE the entity StartUp loop.
        /// Only instantiates and configures entities — does NOT call StartUp or AddTurn.
        /// StartUp is deferred to the entity init loop so all managers are ready.
        /// </summary>
        public void SpawnEntities()
        {
            spawnedEnemies.Clear();

            if (npcPool.Count == 0)
            {
                Debug.LogError("[RandomEncounterManager] npcPool is empty — assign NPC entries in Inspector.");
                return;
            }
            if (enemySpawnPoints.Count == 0)
            {
                Debug.LogError("[RandomEncounterManager] No enemy spawn points assigned.");
                return;
            }

            SpawnEnemies();
        }

        /// <summary>
        /// Called by StartupManager AFTER the entity init loop.
        /// Applies anything that requires EntityVisual (set in StartUp) to be ready.
        /// </summary>
        public void PostInit()
        {
            foreach (var npc in spawnedEnemies)
            {
                if (npc != null && npc.EntityVisual != null)
                    npc.EntityVisual.meshRenderer.material = AssetManager.Instance.EnemyMaterial;
            }
            spawnedEnemies.Clear();
        }

        private void SpawnEnemies()
        {
            int budget = Random.Range(targetPowerMin, targetPowerMax + 1);
            int spent = 0;

            List<int> availableSpawnIndices = new List<int>();
            for (int i = 0; i < enemySpawnPoints.Count; i++)
                availableSpawnIndices.Add(i);

            while (spent < budget && availableSpawnIndices.Count > 0)
            {
                List<NpcPoolEntry> affordable = npcPool.FindAll(e => e != null && !string.IsNullOrEmpty(e.npcId) && e.powerValue <= budget - spent);
                if (affordable.Count == 0)
                    break;

                NpcPoolEntry candidate = affordable[Random.Range(0, affordable.Count)];

                int pick = Random.Range(0, availableSpawnIndices.Count);
                int spawnIndex = availableSpawnIndices[pick];
                availableSpawnIndices.RemoveAt(pick);

                SpawnEnemy(candidate.npcId, enemySpawnPoints[spawnIndex].position);
                spent += candidate.powerValue;
            }

            if (spent == 0)
                Debug.LogWarning("[RandomEncounterManager] No enemies spawned — check pool power values vs. budget range.");

            Debug.Log($"[RandomEncounterManager] Done. Budget: {budget}, Spent: {spent}");
        }

        private void SpawnEnemy(string npcId, Vector3 position)
        {
            // Fetch NPC template BEFORE instantiation so we can use usePresetConfig.
            // This is the same pattern as TrainingEncounterManager — defers entity.StartUp()
            // to the entity init loop in StartupManager, ensuring all managers are ready.
            NpcData npcData = NpcDatabase.GetNpcById(npcId, null);
            if (npcData == null)
            {
                Debug.LogError($"[RandomEncounterManager] NpcDatabase has no entry for '{npcId}'. Check npcPool IDs.");
                return;
            }

            GameObject obj = Instantiate(AssetManager.Instance.entityPrefab, position, Quaternion.identity);
            obj.GetComponent<EntityOnMap>()?.TeleportTo(position);
            obj.name = $"Enemy_{npcData.name}";

            NonPlayerScript npc = obj.GetComponent<NonPlayerScript>();
            npc.entityAffiliation = EntityAffiliation.Enemy;
            npc.usePresetConfig    = true;
            npc.npcData            = npcData;
            npc.deckCardIDs        = new List<string>(npcData.cardIds);

            spawnedEnemies.Add(npc);
        }
    }
}
