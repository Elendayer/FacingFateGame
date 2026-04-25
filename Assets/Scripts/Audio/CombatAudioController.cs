using UnityEngine;

namespace facingfate
{
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

        private void HandleCombatStart() => WwiseAudioHelper.PlayGlobal(combatStartSfx, gameObject);
        private void HandleTurnStart()   => WwiseAudioHelper.PlayGlobal(turnStartSfx, gameObject);
        private void HandleRoundStart()  => WwiseAudioHelper.PlayGlobal(roundStartSfx, gameObject);

        private void HandleCombatEnd(bool playerWon) =>
            WwiseAudioHelper.PlayGlobal(playerWon ? victorySfx : defeatSfx, gameObject);

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
                        WwiseAudioHelper.PlayGlobal(drawCardSfx, gameObject);
                        break;
                    case GameplayRef.onCardDiscarded:
                        WwiseAudioHelper.PlayGlobal(discardCardSfx, gameObject);
                        break;
                    case GameplayRef.onCardPlayed:
                        if (!string.IsNullOrEmpty(refData.CardData?.playSfxEvent))
                            AkUnitySoundEngine.PostEvent(refData.CardData.playSfxEvent, gameObject);
                        break;
                }
            }
        }

        private static void PlayEntitySfx(EntityScript entity, AK.Wwise.Event sfx)
        {
            if (entity == null) return;
            WwiseAudioHelper.Play(sfx, entity.gameObject);
        }

        private static EntityScript FirstAffected(ToSendTriggerReference refData) =>
            refData.AffectedEntities?.Count > 0 ? refData.AffectedEntities[0] : null;
    }
}
