using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace facingfate
{
    public static class GameEvents
    {
        public static event Action OnEncounterStart;
        public static event Action OnTurnStart;
        public static event Action OnTurnEnd;
        public static event Action OnRoundEnd;
        public static event Action OnRoundStart;
        public static event Action OnCombatStart;
        public static event Action<bool> OnCombatEnd;
        public static event Action<EntityScript> OnActivePlayerChanged;
        public static event Action<EntityScript> OnTurnEntityChanged;

        public static event Action<ToSendTriggerReference> OnGameplayReference;

        public static void TriggerEncounterStart() => OnEncounterStart?.Invoke();
        public static void TriggerTurnStart() => OnTurnStart?.Invoke();
        public static void TriggerTurnEnd() => OnTurnEnd?.Invoke();

        public static void TriggerRoundEnd() => OnRoundEnd?.Invoke();
        public static void TriggerRoundStart() => OnRoundStart?.Invoke();

        public static void TriggerCombatStart() => OnCombatStart?.Invoke();
        public static void TriggerCombatEnd(bool playerWon) => OnCombatEnd?.Invoke(playerWon);


        public static void TriggerActivePlayerChanged(EntityScript entity)=> OnActivePlayerChanged?.Invoke(entity);
        public static void TriggerTurnEntityChanged(EntityScript entity)=> OnTurnEntityChanged?.Invoke(entity);


        public static void GameplayReferenceCall() => OnGameplayReference?.Invoke(new());

        public static ToSendTriggerReference LastGameplayTrigger;

        public static void TriggerRefEvent(ToSendTriggerReference grs)
        {
            LastGameplayTrigger = grs;
            TimelineManager.AddToTimeline(grs);
            OnGameplayReference?.Invoke(grs);
        }

        public static bool CheckIfRelevantTrigger(ToSendTriggerReference sendReference, RelevantTriggerCheck checkReference)
        {
            // Null checks � positive condition: both references must be non-null
            if (sendReference.OnTriggerReference != null && checkReference.OnTriggerReference != null)
            {
                // Check for overlap in OnTriggerReference � positive condition: there is at least one overlapping trigger
                if (sendReference.OnTriggerReference.Intersect(checkReference.OnTriggerReference).Any())
                {
                    // Check entity relevance based on type
                    switch (checkReference.CheckType)
                    {
                        case CheckEntityType.User:
                            if (sendReference.UserEntity == checkReference.CheckEntity)
                                return true;
                            break;

                        case CheckEntityType.Target:
                            if (sendReference.AffectedEntities != null && sendReference.AffectedEntities.Contains(checkReference.CheckEntity))
                                return true;
                            break;
                    }
                }
            }

            // If none of the positive checks succeed, return false
            return false;
        }
    }

    // -------------------- Referenece Struct --------------------
    public struct ToSendTriggerReference
    {
        public List<GameplayRef> OnTriggerReference;
        public EntityScript UserEntity;
        public List<EntityScript> AffectedEntities;
        public CardData CardData;
        public int Throughput;

        public ToSendTriggerReference(
            List<GameplayRef> references,
            EntityScript userEntity,
            List<EntityScript> affectedEntities,
            CardData cardData = null,
            int throughput = 0
            )
        {
            OnTriggerReference = references;
            UserEntity = userEntity;
            AffectedEntities = affectedEntities;
            CardData = cardData;
            Throughput = throughput;
        }
    }

    public struct RelevantTriggerCheck
    {
        public List<GameplayRef> OnTriggerReference;
        public CheckEntityType CheckType;
        public EntityScript CheckEntity;
        public CardData CardData;

        public RelevantTriggerCheck(
            List<GameplayRef> references,
            CheckEntityType checkType,
            EntityScript checkEntity,
            CardData cardData = null
            )
        {
            OnTriggerReference = references;
            CheckType = checkType;
            CheckEntity = checkEntity;
            CardData = cardData;
        }
    }

    public enum CheckEntityType
    {
        User,
        Target
    }
    // -------------------- Gameplay References Enum --------------------
    public enum GameplayRef
    {
        None,

        //Status Effects
        onBurn,
        onBleed,
        onPoison,
        onThorns,
        onDebuffed,
        onSlowed,
        onStunned,
        onRooted,
        onSilenced,
        onWeakened,
        onEmpowered,
        onShielded,
        onHasted,
        onFreeze,
        onTaunted,
        onInvisible,
        onCharmed,
        onCleansed,
        onRevived,
        onEnraged,

        //Targeting
        untargetableByAll,
        untargetableByEnemies,
        untargetableByAllies,

        taunt,

        //Combat Events
        onDamage,
        onBlocking,
        onBuffed,
        onAttack,
        onHeal,
        onDeath,
        onSummon,
        onLifesteal,
        onCounterRecieved,
        onDamageRecieved,
        onHealRecieved,
        onBuffRecieved,
        onDebuffRecieved,

        //Game Flow
        onTurnStart,
        onTurnEnd,
        onRoundStart,
        onRoundEnd,
        onCardPlayed,
        onCardEffectEnd,
        onCardDrawn,
        onCardDiscarded,
        onStatChanged,
        onModifierApplied,
        onModifierExpired,
        onHitLanded,

        //Misc
        onMove,
        onChase,
        onFlee,
        onReposition,

        //Card Types
        Skill,
        Item,
        Ability,
        Technique,
        Spell,
        Blessing,
        Curse,

        //Identites
        Non,
        Physical,
        Fire,
        Ice,
        Air,
        Earth,
        Shadow,
        Poison,
        Light,
        Blood,
        Arcane,
        Soul,
        Divine,
        Occult,
        Melee,
        Ranged,

        //Alchemy
        Venom,
        Alchemical,

        //Classes
        Spearman,
        Assassin,
        Mystic,
        Physician,
        Neutral,


        //Classes Old
        Knight,
        Rogue,
        Wizard,
        Cleric,
        Paladin,
        Warlock,
        Ranger,
        Druid,
        Barbarian,
        Alchemist,
        Monster,

    }
}