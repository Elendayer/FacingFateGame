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

            CheckWinLose();
        }

        private void CheckWinLose()
        {
            var turnOrder = TurnManager.Instance.TurnOrder;

            bool anyEnemyAlive = turnOrder.Any(e => e.entityAffiliation == EntityAffiliation.Enemy);
            bool anyPlayerAlive = turnOrder.Any(e => e.entityAffiliation == EntityAffiliation.Player);

            if (!anyEnemyAlive)
            {
                EndCombat(playerWon: true);
            }
            else if (!anyPlayerAlive)
            {
                EndCombat(playerWon: false);
            }
        }

        private void EndCombat(bool playerWon)
        {
            combatEnded = true;
            Debug.Log($"[EncounterManager] Combat ended. Player won: {playerWon}");
            GameEvents.TriggerCombatResult(playerWon);
        }
    }
}
