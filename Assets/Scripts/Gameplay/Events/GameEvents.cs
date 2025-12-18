using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GameEvents
{
    public static event Action OnTurnStart;
    public static event Action OnTurnEnd;
    public static event Action OnRoundEnd;
    public static event Action OnRoundStart;
    public static event Action OnCombatStart;
    public static event Action OnCombatEnd;

    public static event Action<TriggerRef> OnGameplayReference;

    public static void TriggerTurnStart() => OnTurnStart?.Invoke();
    public static void TriggerTurnEnd() => OnTurnEnd?.Invoke();

    public static void TriggerRoundEnd() => OnRoundEnd?.Invoke();
    public static void TriggerRoundStart() => OnRoundStart?.Invoke();

    public static void TriggerCombatStart() => OnCombatStart?.Invoke();
    public static void TriggerCombatEnd() => OnCombatEnd?.Invoke();

    public static void GameplayReferenceCall() => OnGameplayReference?.Invoke(new());

    public static void TriggerRefEvent(TriggerRef grs)
    {
        TimelineManager.AddToTimeline(grs);
        OnGameplayReference?.Invoke(grs);
    }
    public static bool CheckIfRelevantTrigger(TriggerRef sendReference, TriggerRef checkReference)
    {
        if (sendReference.AffectedEntities != null && checkReference.AffectedEntities != null)
        {
            if (checkReference.AffectedEntities.Any(entity => sendReference.AffectedEntities.Contains(entity)))
            {
                if (sendReference.OnTriggerReference == null || sendReference.OnTriggerReference.Count == 0)
                {
                    return false;
                }
                return sendReference.OnTriggerReference.Any(tr => checkReference.OnTriggerReference.Contains(tr));
            }
        }
        return false;
    }
}

// -------------------- Referenece Struct --------------------
public struct TriggerRef
{
    public List<GameplayRef> OnTriggerReference;
    public EntityScript UserEntity;
    public List<EntityScript> AffectedEntities;
    public CardData CardData;
    public int Throughput;

    public TriggerRef(
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
    onCardDrawn,
    onCardDiscarded,
    onStatChanged,
    onModifierApplied,
    onModifierExpired,
    onHitLanded,

    //Misc
    onMove,

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