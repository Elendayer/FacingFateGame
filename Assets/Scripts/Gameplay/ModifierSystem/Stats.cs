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
        modifier.AddListener();
    }

    public void RemoveModifier(IStatModifier modifier) => modifiers.Remove(modifier);

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

    public bool HasReference(gameplayRef reference)
        => modifiers.Any(m => m.To_TriggerGameplayRef == reference && !m.IsExpired);

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
    gameplayRef To_TriggerGameplayRef { get; }
    bool IsExpired { get; }

    void AddListener();
    void OnRefEventTriggered(TriggerRef reference);
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
    public gameplayRef To_TriggerGameplayRef { get; private set; }
    public bool IsExpired => Duration <= 0;

    private int? staticValue;
    private Func<int> dynamicValueFunc;

    public Stat targetStat;
    public int GetRemainingDuration() => Duration;

    public void AddListener() 
    {
        GameEvents.OnRefEvent += OnRefEventTriggered;
    }
    public void OnRefEventTriggered(TriggerRef trigger)
    {
        if (trigger.Reference == To_TriggerGameplayRef)
        {
            Debug.Log($"[StatModifier: {StatName}] Triggered by reference: {trigger.Reference}");
            if (Duration < 9999)
            {
                Duration--;
                Debug.Log($"StatModifier duration: {Duration}");
            }
        }
    }

    public StatModifier(
        int value,
        StatScaling scaling,
        gameplayRef reference = gameplayRef.None,
        int duration = 99999,
        Stat target = null,
        string name = null)
    {
        StatName = name;
        staticValue = value;
        StatScaling = scaling;
        To_TriggerGameplayRef = reference;
        Duration = duration;
        targetStat = target;
    }
}

public class FunctionModifier : IStatModifier
{
    public string StatName { get; private set; }
    public int BaseValue { get; set; }
    public StatScaling StatScaling { get; private set; }
    public int Duration { get; set; }
    public gameplayRef To_TriggerGameplayRef { get; private set; }
    public bool IsExpired => Duration <= 0;

    private Action<FunctionModifier, Stat, gameplayRef> OnRefEventAction;
    public TriggerRef TriggerConditionRef { get; private set; }
    public Stat TargetStat { get; private set; }
    public void AddListener()
    {
        GameEvents.OnRefEvent += OnRefEventTriggered;
    }
    public FunctionModifier
        (
        string statName,
        int baseValue,
        StatScaling statScaling,
        gameplayRef toTrigger_ref = gameplayRef.None,
        int duration = 0,
        Stat target = null,
        Action<FunctionModifier, Stat, gameplayRef> onRefEventAction = null,
        TriggerRef triggerConditionRef = new TriggerRef()
        )
    {
        StatName = statName;
        BaseValue = baseValue;
        StatScaling = statScaling;
        To_TriggerGameplayRef = toTrigger_ref;
        Duration = duration;
        TargetStat = target;
        this.OnRefEventAction = onRefEventAction;
        TriggerConditionRef = triggerConditionRef;
    }

    public int GetRemainingDuration() => Duration;

    public void OnRefEventTriggered(TriggerRef trigger)
    {
        if (CheckTrigger(trigger))
        {
            Debug.Log($"[StatModifier: {StatName}] Triggered by reference: {trigger.Reference}");
            OnRefEventAction?.Invoke(this, TargetStat, To_TriggerGameplayRef);

            if (Duration < 9999)
            {
                Duration--;
                Debug.Log($"FunctionModifier duration: {Duration}");
            }
        }
    }

    bool CheckTrigger(TriggerRef trigger)
    {
        Debug.Log($"Checking trigger: {trigger.Reference}, {trigger.UserId}, {trigger.TargetId}");

        // Must always match reference
        if (trigger.Reference != TriggerConditionRef.Reference)
            return false;

        // If TargetId is specified, it must match
        if (TriggerConditionRef.TargetId == 0) { } 
        else if (trigger.TargetId != TriggerConditionRef.TargetId)
            return false;

        // If UserId is specified, it must match
        if (TriggerConditionRef.UserId == 0) { }
        else if (trigger.UserId != TriggerConditionRef.UserId)
            return false;

        return true;
    }
}

// -------------------- Enums --------------------

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
// -------------------- Referenece Struct --------------------
public struct TriggerRef
{
    public gameplayRef Reference;
    public int UserId;
    public int TargetId;
    public TriggerRef(gameplayRef reference = gameplayRef.None, int userId = 0, int targetId = 0)
    {
        Reference = reference;
        UserId = userId;
        TargetId = targetId;
    }
}