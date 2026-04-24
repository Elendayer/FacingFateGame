using System;
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

            foreach (var r in refData.OnTriggerReference)
            {
                switch (r)
                {
                    case GameplayRef.onHitLanded:
                        PlayEntitySfx(refData.UserEntity, p => p.attackSfx);
                        break;
                    case GameplayRef.onDamageRecieved:
                        PlayEntitySfx(FirstAffected(refData), p => p.damageSfx);
                        break;
                    case GameplayRef.onBlocking:
                        PlayEntitySfx(FirstAffected(refData), p => p.blockSfx);
                        break;
                    case GameplayRef.onHealRecieved:
                        PlayEntitySfx(FirstAffected(refData), p => p.healSfx);
                        break;
                    case GameplayRef.onDeath:
                        PlayEntitySfx(FirstAffected(refData), p => p.deathSfx);
                        break;
                    case GameplayRef.onModifierApplied:
                        PlayEntitySfx(FirstAffected(refData), p => p.statusAppliedSfx);
                        break;
                    case GameplayRef.onModifierExpired:
                        PlayEntitySfx(FirstAffected(refData), p => p.modifierExpiredSfx);
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

        private void PlayEntitySfx(EntityScript entity, Func<EntityAudioProfile, AK.Wwise.Event> selector)
        {
            if (entity == null || entity.audioProfile == null) return;
            WwiseAudioHelper.Play(selector(entity.audioProfile), entity.gameObject);
        }

        private static EntityScript FirstAffected(ToSendTriggerReference refData) =>
            refData.AffectedEntities?.Count > 0 ? refData.AffectedEntities[0] : null;
    }
}
