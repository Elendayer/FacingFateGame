using System.Linq;
using UnityEngine;

namespace facingfate
{
    public class EncounterManager : MonoBehaviour
    {
        public static EncounterManager Instance { get; private set; }

        private bool combatEnded = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartUp()
        {
            GameEvents.OnGameplayReference += OnGameplayReference;
            GameEvents.OnCombatStart += OnCombatStart;
        }

        private void OnDestroy()
        {
            GameEvents.OnGameplayReference -= OnGameplayReference;
            GameEvents.OnCombatStart -= OnCombatStart;
        }

        private void OnCombatStart()
        {
            combatEnded = false;
        }

        private void OnGameplayReference(ToSendTriggerReference trigger)
        {
            if (combatEnded) return;
            if (trigger.OnTriggerReference == null) return;
            if (!trigger.OnTriggerReference.Contains(GameplayRef.onDeath)) return;

            Debug.Log($"[EncounterManager] Death event triggered for {trigger.UserEntity?.name}. Checking win/lose conditions...");
            CheckWinLose();
        }

        private void CheckWinLose()
        {
            // Guard against null TurnManager instance
            if (TurnManager.Instance == null)
            {
                Debug.LogWarning("[EncounterManager] TurnManager.Instance is null during win/lose check");
                return;
            }

            var turnOrder = TurnManager.Instance.TurnOrder;

            if (turnOrder == null || turnOrder.Count == 0)
            {
                Debug.LogWarning("[EncounterManager] Turn order is null or empty");
                return;
            }

            // Check for alive entities by verifying they're still enabled and have health > 0
            bool anyEnemyAlive = turnOrder.Any(e => IsEntityAlive(e, EntityAffiliation.Enemy));
            bool anyPlayerAlive = turnOrder.Any(e => IsEntityAlive(e, EntityAffiliation.Player));

            Debug.Log($"[EncounterManager] Win/Lose Check - Enemies Alive: {anyEnemyAlive}, Players Alive: {anyPlayerAlive}, TurnOrder Count: {turnOrder.Count}");

            if (!anyEnemyAlive && anyPlayerAlive)
            {
                EndCombat(playerWon: true);
            }
            else if (!anyPlayerAlive && anyEnemyAlive)
            {
                EndCombat(playerWon: false);
            }
            else if (!anyEnemyAlive && !anyPlayerAlive)
            {
                // Edge case: both sides wiped out - consider it a player loss
                Debug.LogWarning("[EncounterManager] Both player and enemy teams eliminated. Combat declared as player loss.");
                EndCombat(playerWon: false);
            }
        }

        private bool IsEntityAlive(EntityScript entity, EntityAffiliation affiliationToCheck)
        {
            // Validate entity exists and has correct affiliation
            if (entity == null || entity.entityAffiliation != affiliationToCheck)
            {
                return false;
            }

            // Validate entity is still active in the game
            if (!entity.gameObject.activeSelf || !entity.enabled)
            {
                return false;
            }

            // Validate entity has valid stats and positive health
            if (entity.entityStats == null || entity.entityStats.CurrentHealth <= 0)
            {
                return false;
            }

            return true;
        }

        public void ResetCombatForNewWave()
        {
            combatEnded = false;
        }

        private void EndCombat(bool playerWon)
        {
            combatEnded = true;
            Debug.Log($"[EncounterManager] Combat ended. Player won: {playerWon}");
            GameEvents.TriggerCombatEnd(playerWon);
        }
    }
}
