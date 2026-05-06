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
        [Header("Turn Flow")]
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event combatStartSfx;
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event turnStartSfx;
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event roundStartSfx;

        [Header("Game End")]
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event victorySfx;
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event defeatSfx;

        [Header("Cards")]
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event drawCardSfx;
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event discardCardSfx;

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

        private void HandleCombatStart() => PlayGlobal(combatStartSfx);
        private void HandleTurnStart()   => PlayGlobal(turnStartSfx);
        private void HandleRoundStart()  => PlayGlobal(roundStartSfx);

        private void HandleCombatEnd(bool playerWon) =>
            PlayGlobal(playerWon ? victorySfx : defeatSfx);

        private void HandleGameplayReference(ToSendTriggerReference refData)
        {
            if (refData.OnTriggerReference == null) return;

            EntityScript affected = FirstAffected(refData);

            foreach (var r in refData.OnTriggerReference)
            {
                switch (r)
                {
                    case GameplayRef.onHitLanded:
                        PlayEntitySfx(refData.UserEntity, refData.UserEntity?.attackSfx);
                        break;
                    case GameplayRef.onDamageRecieved:
                        PlayEntitySfx(affected, affected?.damageSfx);
                        break;
                    case GameplayRef.onBlocking:
                        PlayEntitySfx(affected, affected?.blockSfx);
                        break;
                    case GameplayRef.onHealRecieved:
                        PlayEntitySfx(affected, affected?.healSfx);
                        break;
                    case GameplayRef.onDeath:
                        PlayEntitySfx(affected, affected?.deathSfx);
                        break;
                    case GameplayRef.onModifierApplied:
                        PlayEntitySfx(affected, affected?.statusAppliedSfx);
                        break;
                    case GameplayRef.onModifierExpired:
                        PlayEntitySfx(affected, affected?.modifierExpiredSfx);
                        break;
                    case GameplayRef.onCardDrawn:
                        PlayGlobal(drawCardSfx);
                        break;
                    case GameplayRef.onCardDiscarded:
                        PlayGlobal(discardCardSfx);
                        break;
                    case GameplayRef.onCardPlayed:
                        // Card SFX plays via CardSoundHelper.PlayCardEffect() instead
                        break;
                }
            }
        }

        private void PlayGlobal(AK.Wwise.Event sfx)
        {
            if (sfx != null && sfx.IsValid())
            {
                sfx.Post(gameObject);
            }
        }

        private static void PlayEntitySfx(EntityScript entity, AK.Wwise.Event sfx)
        {
            if (entity == null || sfx == null || !sfx.IsValid()) return;
            sfx.Post(entity.gameObject);
        }

        private static EntityScript FirstAffected(ToSendTriggerReference refData) =>
            refData.AffectedEntities?.Count > 0 ? refData.AffectedEntities[0] : null;
    }
}
