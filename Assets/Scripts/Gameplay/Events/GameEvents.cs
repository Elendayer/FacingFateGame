using System;
using System.Collections.Generic;
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

    // Dictionary: (gameplayRef, Id) -> event
    private static readonly Dictionary<(gameplayRef, int), Action<TriggerRef>> _refEvents
        = new();

    public static void Subscribe(gameplayRef type, int id, Action<TriggerRef> listener)
    {
        var key = (type, id);

        if (!_refEvents.ContainsKey(key))
            _refEvents[key] = delegate { };

        _refEvents[key] += listener;
    }

    public static void Unsubscribe(gameplayRef type, int id, Action<TriggerRef> listener)
    {
        var key = (type, id);

        if (_refEvents.ContainsKey(key))
            _refEvents[key] -= listener;
    }


    public static void TriggerRefEvent(TriggerRef grs)
    {
        if (grs.References == null || grs.References.Count == 0) return;

        foreach (var reference in grs.References)
        {
            Debug.Log($"[GameEvents] - {reference}, {grs.AffectedEntityId}");

            // Notify for TargetId
            if (_refEvents.TryGetValue((reference, grs.AffectedEntityId), out var targetAction))
                targetAction?.Invoke(grs);
        }
    }
}

public enum gameplayRef
{
    None,
    onBurn,
    onDamage,
    onStunned,
    onBlocking,
    onBuffed,
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

    Spearman,

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
    onDebuffed,
}