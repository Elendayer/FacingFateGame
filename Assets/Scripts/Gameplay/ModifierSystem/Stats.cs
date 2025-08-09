using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Stat
{
    public int Value;

    private readonly List<IStatModifier> modifiers = new();

    public void AddModifier(IStatModifier modifier, ModifierMergeStrategy strategy = ModifierMergeStrategy.Replace)
    {
        var existing = modifiers.FirstOrDefault(m => m.StatName == modifier.StatName);

        switch (strategy)
        {
            case ModifierMergeStrategy.Add:
                modifiers.Add(modifier);
                break;

            case ModifierMergeStrategy.Replace:
                if (existing != null) modifiers.Remove(existing);
                modifiers.Add(modifier);
                break;

            case ModifierMergeStrategy.Increase:
                if (existing is StatModifier existingMod && modifier is StatModifier newMod)
                {
                    existingMod.BaseValue += newMod.BaseValue;
                }
                else
                {
                    modifiers.Add(modifier);
                }
                break;

            case ModifierMergeStrategy.RefreshIncrease:
                if (existing is StatModifier existingRefresh && modifier is StatModifier newRefresh)
                {
                    existingRefresh.BaseValue += newRefresh.BaseValue;
                    existingRefresh.Duration = Math.Max(existingRefresh.GetRemainingDuration(), newRefresh.GetRemainingDuration());
                }
                else
                {
                    modifiers.Add(modifier);
                }
                break;

            case ModifierMergeStrategy.RefreshDuration:
                if (existing is StatModifier existingRefreshDuration && modifier is StatModifier newRefreshDuration)
                {
                    existingRefreshDuration.BaseValue = Mathf.Max(existingRefreshDuration.BaseValue, newRefreshDuration.BaseValue);
                    existingRefreshDuration.Duration = Math.Max(existingRefreshDuration.GetRemainingDuration(), newRefreshDuration.GetRemainingDuration());
                }
                else
                {
                    modifiers.Add(modifier);
                }
                break;
        }
    }

    public void RemoveModifier(IStatModifier modifier) => modifiers.Remove(modifier);

    public void TickModifiers()
    {
        foreach (var mod in modifiers)
            mod.OnTurnStart();

        modifiers.RemoveAll(m => m.IsExpired);
    }

    public void TriggerReferenceEvent(gameplayReference reference)
    {
        foreach (var mod in modifiers.Where(m => !m.IsExpired && m.Reference == reference))
            mod.OnReferenceEventTriggered(reference);
    }

    public int GetFinalValue()
    {
        int baseValue = Value;
        int percent = 0;
        List<int> multipliers = new();

        foreach (var mod in modifiers.Where(m => !m.IsExpired))
        {
            if (mod is StatModifier statMod)
            {
                switch (statMod.StatScaling)
                {
                    case StatScaling.Flat:
                        baseValue += statMod.BaseValue;
                        break;
                    case StatScaling.Percent:
                        percent += statMod.BaseValue;
                        break;
                    case StatScaling.Multiplier:
                        multipliers.Add(statMod.BaseValue);
                        break;
                }
            }
        }

        baseValue = (baseValue * (100 + percent)) / 100;

        foreach (var mult in multipliers)
            baseValue = (baseValue * mult) / 100;

        return baseValue;
    }

    public List<int> GetAllValues(StatScaling? filterType = null)
    {
        return modifiers
            .Where(m => !m.IsExpired && m is StatModifier sm && (!filterType.HasValue || sm.StatScaling == filterType.Value))
            .Cast<StatModifier>()
            .Select(m => m.BaseValue)
            .ToList();
    }

    public bool HasReference(gameplayReference reference)
        => modifiers.Any(m => m.Reference == reference && !m.IsExpired);

    public IStatModifier GetModifierByName(string name)
        => modifiers.FirstOrDefault(m => m.StatName == name && !m.IsExpired);

    public void AddOrReplaceModifier(IStatModifier modifier)
    {
        var existing = modifiers.FirstOrDefault(m => m.StatName == modifier.StatName);
        if (existing != null) modifiers.Remove(existing);
        modifiers.Add(modifier);
    }
}

// ----------------- Interfaces and Implementations --------------------

public interface IStatModifier
{
    string StatName { get; }
    int BaseValue { get; set; }
    StatScaling StatScaling { get; }
    int Duration { get; set; }
    gameplayReference Reference { get; }
    bool IsExpired { get; }
    void OnTurnStart();
    void OnReferenceEventTriggered(gameplayReference reference);
}

public class StatModifier : IStatModifier
{
    public string StatName { get; private set; }

    public int BaseValue
    {
        get
        {
            if (dynamicValueFunc != null)
                return dynamicValueFunc.Invoke();
            return staticValue ?? 0;
        }
        set
        {
            if (dynamicValueFunc != null)
                throw new InvalidOperationException("Cannot set BaseValue when using dynamicValueFunc.");
            staticValue = value;
        }
    }

    public StatScaling StatScaling { get; private set; }
    public int Duration { get; set; }
    public gameplayReference Reference { get; private set; }
    public bool IsExpired => Duration <= 0;

    private int? staticValue;
    private Func<int> dynamicValueFunc;

    public Stat targetStat;
    public int GetRemainingDuration() => Duration;

    public StatModifier(
        int value,
        StatScaling scaling,
        gameplayReference reference = gameplayReference.noRef,
        int duration = 99999,
        Stat target = null,
        string name = null)
    {
        StatName = name;
        staticValue = value;
        StatScaling = scaling;
        Reference = reference;
        Duration = duration;
        targetStat = target;
    }

    public void OnTurnStart()
    {
        Debug.Log($"[StatModifier: {StatName}] OnTurnStart");

        if (Duration < 9999)
        {
            Duration--;
            Debug.Log($"StatModifier duration: {Duration}");
        }
    }

    public void OnReferenceEventTriggered(gameplayReference reference)
    {
        if (reference == Reference)
            Debug.Log($"[StatModifier: {StatName}] Triggered by reference: {reference}");
    }
}

public class FunctionModifier : IStatModifier
{
    public string StatName { get; private set; }
    public int BaseValue { get; set; }
    public StatScaling StatScaling { get; private set; }
    public int Duration { get; set; }
    public gameplayReference Reference { get; private set; }
    public bool IsExpired => Duration <= 0;

    private Action<FunctionModifier, Stat> onTurnStartAction;
    private Action<FunctionModifier, Stat, gameplayReference> onReferenceEventAction;

    public Stat targetStat;

    public FunctionModifier(
        string statName,
        int baseValue,
        StatScaling statScaling,
        gameplayReference reference = gameplayReference.noRef,
        int duration = 0,
        Stat target = null,
        Action<FunctionModifier, Stat> onTurnStart = null,
        Action<FunctionModifier, Stat, gameplayReference> onReferenceEvent = null)
    {
        StatName = statName;
        BaseValue = baseValue;
        StatScaling = statScaling;
        Reference = reference;
        Duration = duration;
        targetStat = target;
        onTurnStartAction = onTurnStart;
        onReferenceEventAction = onReferenceEvent;
    }

    public int GetRemainingDuration() => Duration;

    public void OnTurnStart()
    {
        Debug.Log($"[FunctionModifier: {StatName}] OnTurnStart");
        onTurnStartAction?.Invoke(this, targetStat);

        if (Duration < 9999)
        {
            Duration--;
            Debug.Log($"FunctionModifier duration: {Duration}");
        }
    }

    public void OnReferenceEventTriggered(gameplayReference reference)
    {
        if (reference == Reference)
        {
            Debug.Log($"[FunctionModifier: {StatName}] Reacted to event: {reference}");
            onReferenceEventAction?.Invoke(this, targetStat, reference);
        }
    }
}

// -------------------- Enums --------------------

public enum gameplayReference
{
    noRef,
    burningRef,
    damagedRef,
    stunnedRef,
    blockingRef,
    buffedRef,
}

public enum ModifierMergeStrategy
{
    Add,
    Replace,
    Increase,
    RefreshIncrease,
    RefreshDuration
}

public enum StatScaling
{
    Flat,
    Percent,
    Multiplier
}

public enum StatAspect
{
    Cost,
    Power,
    Duration,
    Repeats
}
