using UnityEngine;

namespace facingfate
{
    /// <summary>
    /// Handles audio responses to gameplay events (combat flow, entity actions, card plays).
    /// Works in conjunction with AudioManager (which provides the event registry)
    /// and CardSoundHelper (which handles card-specific audio).
    /// </summary>
    public class CombatAudioController : MonoBehaviour
    {
        private void OnEnable()
        {
            GameEvents.OnCombatStart       += HandleCombatStart;
            GameEvents.OnCombatEnd         += HandleCombatEnd;
            GameEvents.OnTurnStart         += HandleTurnStart;
            GameEvents.OnRoundStart        += HandleRoundStart;
            GameEvents.OnGameplayReference += HandleGameplayReference;
        }

        private void OnDisable()
        {
            GameEvents.OnCombatStart       -= HandleCombatStart;
            GameEvents.OnCombatEnd         -= HandleCombatEnd;
            GameEvents.OnTurnStart         -= HandleTurnStart;
            GameEvents.OnRoundStart        -= HandleRoundStart;
            GameEvents.OnGameplayReference -= HandleGameplayReference;
        }

        private void HandleCombatStart() => PlayGlobal("CombatStartSfx");
        private void HandleTurnStart()   => PlayGlobal("TurnStartSfx");
        private void HandleRoundStart()  => PlayGlobal("RoundStartSfx");

        private void HandleCombatEnd(bool playerWon) =>
            PlayGlobal(playerWon ? "VictorySfx" : "DefeatSfx");

        private void HandleGameplayReference(ToSendTriggerReference refData)
        {
            if (refData.OnTriggerReference == null) return;

            EntityScript affected = FirstAffected(refData);

            foreach (var r in refData.OnTriggerReference)
            {
                switch (r)
                {
                    case GameplayRef.onHitLanded:
                        PlayEntitySfx(refData.UserEntity, "PlayAttackSound");
                        break;
                    case GameplayRef.onDamageRecieved:
                        PlayEntitySfx(affected, "PlayDamageSound");
                        break;
                    case GameplayRef.onBlocking:
                        PlayEntitySfx(affected, "BlockSfx");
                        break;
                    case GameplayRef.onHealRecieved:
                        PlayEntitySfx(affected, "HealSfx");
                        break;
                    case GameplayRef.onDeath:
                        PlayEntitySfx(affected, "DeathSfx");
                        break;
                    case GameplayRef.onModifierApplied:
                        PlayEntitySfx(affected, "StatusAppliedSfx");
                        break;
                    case GameplayRef.onModifierExpired:
                        PlayEntitySfx(affected, "ModifierExpiredSfx");
                        break;
                    case GameplayRef.onCardDrawn:
                        PlayGlobal("PlayCardDrawSound");
                        break;
                    case GameplayRef.onCardDiscarded:
                        PlayGlobal("PlayCardDiscardSound");
                        break;
                    case GameplayRef.onCardPlayed:
                        // Card SFX plays via CardSoundHelper.PlayCardEffect() instead
                        break;
                }
            }
        }

        private void PlayGlobal(string eventName)
        {
            AudioManager.Instance?.PostEvent(eventName, gameObject);
        }

        private static void PlayEntitySfx(EntityScript entity, string eventName)
        {
            if (entity == null) return;
            AudioManager.Instance?.PostEvent(eventName, entity.gameObject);
        }

        private static EntityScript FirstAffected(ToSendTriggerReference refData) =>
            refData.AffectedEntities?.Count > 0 ? refData.AffectedEntities[0] : null;
    }
}
