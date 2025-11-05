using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    public static event Action OnTurnStart;
    public static event Action OnTurnEnd;
    public static event Action OnRoundEnd;
    public static event Action OnRoundStart;
    public static event Action OnCombatStart;
    public static event Action OnCombatEnd;

    public static void TriggerTurnStart() => OnTurnStart?.Invoke();
    public static void TriggerTurnEnd() => OnTurnEnd?.Invoke();

    public static void TriggerRoundEnd() => OnRoundEnd?.Invoke();
    public static void TriggerRoundStart() => OnRoundStart?.Invoke();

    public static void TriggerCombatStart() => OnCombatStart?.Invoke();
    public static void TriggerCombatEnd() => OnCombatEnd?.Invoke();

    // Dictionary: (gameplayRef, Id) -> event
    private static readonly Dictionary<(GameplayRef, int), Action<TriggerRef>> _refEvents
        = new();

    public static void Subscribe(GameplayRef type, int id, Action<TriggerRef> listener)
    {
        var key = (type, id);

        if (!_refEvents.ContainsKey(key))
            _refEvents[key] = delegate { };

        _refEvents[key] += listener;
    }

    public static void Unsubscribe(GameplayRef type, int id, Action<TriggerRef> listener)
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
            Debug.Log($"[GameEvents] - {reference}, {grs.UserId} ,  {grs.AffectedEntityId}");
            TimelineManager.AddToTimeline(grs);

            // Notify for TargetId
            if (_refEvents.TryGetValue((reference, grs.AffectedEntityId), out var targetAction))
            {
                targetAction?.Invoke(grs);
            }
        }
    }
}

// -------------------- Referenece Struct --------------------
public struct TriggerRef
{
    public List<GameplayRef> References;
    public int UserId;
    public int AffectedEntityId;
    public CardData CardData;

    public TriggerRef(List<GameplayRef> references = null, int userId = 0, int targetId = 0, CardData cardData = null)
    {
        References = references;
        UserId = userId;
        AffectedEntityId = targetId;
        CardData = cardData;
    }
}
public enum GameplayRef
{
    None,

    //Status Effects
    onBurn,
    onBleed,
    onPoison,
    onDebuffed,

    //Targeting
    untargetableByAll,
    untargetableByEnemies,
    untargetableByAllies,

    taunt,

    //Combat Events
    onDamage,
    onStunned,
    onBlocking,
    onBuffed,
    onAttack,
    onHeal,
    onDeath,
    onSummon,
    onLifesteal,

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