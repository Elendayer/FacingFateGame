using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action OnTurnStart;
    public static event Action OnTurnEnd;
    public static event Action OnRoundEnd;
    public static event Action OnRoundStart;


    public static void TriggerTurnStart() => OnTurnStart?.Invoke();
    public static void TriggerTurnEnd() => OnTurnEnd?.Invoke();
    public static void TriggerRoundEnd() => OnRoundEnd?.Invoke();
    public static void TriggerRoundStart() => OnRoundStart?.Invoke();


    public static event Action<TriggerRef> OnRefEvent;
    public static void TriggerRefEvent(TriggerRef grs) => OnRefEvent?.Invoke(grs);
}

public enum gameplayRef
{
    None,
    onBurningRef,
    onDamageRef,
    onStunnedRef,
    onBlockingRef,
    onBuffedRef,
    onAttack,
    onHeal,
    onDeath,
    onSummon,

    onTurnStart,
    onTurnEnd,
    onRoundEnd,
    onCardPlayed,
    onCardDrawn,
    onStatChanged,
    onModifierApplied,
    onModifierExpired,

    Skill,
    Item,
    Ability,
    Technique,
    Spell,
    Blessing,
    Curse,

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