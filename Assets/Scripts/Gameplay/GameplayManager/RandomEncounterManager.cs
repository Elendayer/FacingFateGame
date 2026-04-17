using System.Collections.Generic;
using UnityEngine;

namespace facingfate
{
    /// <summary>
    /// Spawns a random encounter in the Demo scene.
    /// Always 4 player entities, 3–7 enemy entities.
    /// Each entity gets a randomly generated deck based on the assigned DeckBuildConfigs.
    /// Call SpawnEntities() from StartupManager BEFORE the entity StartUp loop.
    ///
    /// Unity Setup:
    ///   - Add this component to a GameObject in the Demo scene.
    ///   - Assign 4 PlayerSpawnPoints and 7 EnemySpawnPoints (empty GameObjects as position markers).
    ///   - Create DeckBuildConfig assets via Assets > Create > FacingFate > Deck Build Config.
    ///   - Assign PlayerDeckConfig and EnemyDeckConfig in the Inspector.
    /// </summary>
    public class RandomEncounterManager : MonoBehaviour
    {
        public static RandomEncounterManager Instance { get; private set; }

        [Header("Encounter Config")]
        [SerializeField] private int minEnemies = 3;
        [SerializeField] private int maxEnemies = 7;

        [Header("Available Classes")]
        [SerializeField] private List<CardClass> availablePlayerClasses = new()
        {
            CardClass.Spearman, CardClass.Assassin, CardClass.Mystic, CardClass.Physician
        };
        [SerializeField] private List<CardClass> availableEnemyClasses = new()
        {
            CardClass.Spearman, CardClass.Assassin, CardClass.Mystic, CardClass.Physician
        };

        [Header("Deck Configs")]
        [SerializeField] private DeckBuildConfig playerDeckConfig;
        [SerializeField] private DeckBuildConfig enemyDeckConfig;

        [Header("Spawn Points")]
        [Tooltip("Exactly 4 spawn points for player entities")]
        [SerializeField] private List<Transform> playerSpawnPoints = new();

        [Tooltip("At least 7 spawn points for enemy entities")]
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
        /// Instantiates and pre-configures all entities.
        /// Called by StartupManager before the entity StartUp loop.
        /// </summary>
        public void SpawnEntities()
        {
            SpawnPlayers();
            SpawnEnemies();
        }

        private void SpawnPlayers()
        {
            if (playerSpawnPoints.Count < 4)
            {
                Debug.LogError("[RandomEncounterManager] Need at least 4 PlayerSpawnPoints.");
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                CardClass cls = GetRandomClass(availablePlayerClasses);
                List<string> cardIds = RandomDeckBuilder.Build(cls, playerDeckConfig);
                SpawnEntity(playerSpawnPoints[i].position, EntityAffiliation.Player, cls, cardIds, $"Player_{cls}");
            }
        }

        private void SpawnEnemies()
        {
            int count = Random.Range(minEnemies, maxEnemies + 1);

            if (enemySpawnPoints.Count < count)
            {
                Debug.LogWarning($"[RandomEncounterManager] Only {enemySpawnPoints.Count} enemy spawn points, clamping to that.");
                count = enemySpawnPoints.Count;
            }

            for (int i = 0; i < count; i++)
            {
                CardClass cls = GetRandomClass(availableEnemyClasses);
                List<string> cardIds = RandomDeckBuilder.Build(cls, enemyDeckConfig);
                SpawnEntity(enemySpawnPoints[i].position, EntityAffiliation.Enemy, cls, cardIds, $"Enemy_{cls}");
            }
        }

        private void SpawnEntity(Vector3 position, EntityAffiliation affiliation, CardClass cardClass, List<string> cardIds, string displayName)
        {
            GameObject obj = Instantiate(AssetManager.Instance.entityPrefab);
            obj.transform.position = position;

            NonPlayerScript entity = obj.GetComponent<NonPlayerScript>();

            if (entity == null)
            {
                Debug.LogError("[RandomEncounterManager] entityPrefab has no NonPlayerScript component.");
                Destroy(obj);
                return;
            }

            // Mark as pre-configured so NonPlayerScript.StartUp skips NpcDatabase lookup
            entity.usePresetConfig = true;
            entity.entityAffiliation = affiliation;
            entity.deckCardIDs = cardIds;
            entity.npcData = new NpcData
            {
                id = displayName,
                name = $"{cardClass}",
                aiBias = AiBiasDatabase.GetBiasById("StupidFuck"),
                cardIds = cardIds
            };

            obj.name = displayName;
        }

        private CardClass GetRandomClass(List<CardClass> pool)
        {
            if (pool == null || pool.Count == 0)
            {
                Debug.LogWarning("[RandomEncounterManager] Class pool empty, defaulting to Spearman.");
                return CardClass.Spearman;
            }
            return pool[Random.Range(0, pool.Count)];
        }
    }
}
