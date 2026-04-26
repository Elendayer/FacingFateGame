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
    ///
    /// Unity Setup:
    ///   1. Copy Gameplay_Combat_Map.unity → RandomEncounter_Map.unity
    ///   2. Remove TrainingEncounterManager from that scene, add this component instead.
    ///   3. Assign at least 6 EnemySpawnPoints (empty GameObjects as markers).
    ///   4. Fill npcPool with NPC IDs from NpcDatabase + their power values.
    ///   5. Fill cardPool with allowed card string IDs for enemy decks.
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

        [Header("Enemy Deck")]
        [Tooltip("Card IDs the random deck is drawn from (with replacement).")]
        [SerializeField] private List<string> cardPool = new();
        [SerializeField] private int minDeckSize = 8;
        [SerializeField] private int maxDeckSize = 12;

        [Header("Spawn Points")]
        [Tooltip("At least 6 empty GameObjects marking spawn positions.")]
        [SerializeField] private List<Transform> enemySpawnPoints = new();

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
            if (npcPool == null || npcPool.Count == 0)
            {
                Debug.LogError("[RandomEncounterManager] npcPool is empty — assign NPC entries in Inspector.");
                return;
            }
            if (cardPool == null || cardPool.Count == 0)
            {
                Debug.LogError("[RandomEncounterManager] cardPool is empty — assign card IDs in Inspector.");
                return;
            }
            if (enemySpawnPoints == null || enemySpawnPoints.Count == 0)
            {
                Debug.LogError("[RandomEncounterManager] No enemy spawn points assigned.");
                return;
            }

            SpawnEnemies();
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
                NpcPoolEntry candidate = npcPool[Random.Range(0, npcPool.Count)];

                if (candidate.powerValue > budget - spent)
                    break;

                int pick = Random.Range(0, availableSpawnIndices.Count);
                int spawnIndex = availableSpawnIndices[pick];
                availableSpawnIndices.RemoveAt(pick);

                SpawnEnemy(candidate.npcId, enemySpawnPoints[spawnIndex].position);
                spent += candidate.powerValue;
            }

            Debug.Log($"[RandomEncounterManager] Done. Budget: {budget}, Spent: {spent}");
        }

        private void SpawnEnemy(string npcId, Vector3 position)
        {
            GameObject obj = Instantiate(AssetManager.Instance.entityPrefab);
            obj.transform.position = position;
            obj.name = npcId;

            NonPlayerScript entity = obj.GetComponent<NonPlayerScript>();
            if (entity == null)
            {
                Debug.LogError("[RandomEncounterManager] entityPrefab missing NonPlayerScript.");
                Destroy(obj);
                return;
            }

            NpcData data = NpcDatabase.GetNpcById(npcId, entity);
            if (data == null)
            {
                Debug.LogError($"[RandomEncounterManager] NPC not found in NpcDatabase: '{npcId}'");
                Destroy(obj);
                return;
            }

            List<string> deck = BuildRandomDeck();
            data.cardIds = deck;

            entity.usePresetConfig = true;
            entity.entityAffiliation = EntityAffiliation.Enemy;
            entity.npcData = data;
            entity.deckCardIDs = deck;
        }

        private List<string> BuildRandomDeck()
        {
            int size = Random.Range(minDeckSize, maxDeckSize + 1);
            List<string> deck = new List<string>(size);
            for (int i = 0; i < size; i++)
                deck.Add(cardPool[Random.Range(0, cardPool.Count)]);
            return deck;
        }
    }
}
