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
        var existing = modifiers.FirstOrDefault(m => m.ModifierName == modifier.ModifierName);

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
                switch (statMod.ModifierScaling)
                {
                    case ModifierScaling.Flat:
                        baseValue += statMod.BaseValue;
                        break;
                    case ModifierScaling.Percent:
                        percent += statMod.BaseValue;
                        break;
                    case ModifierScaling.Multiplier:
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

    public List<int> GetAllValues(ModifierScaling? filterType = null)
    {
        return modifiers
            .Where(m => !m.IsExpired && m is StatModifier sm && (!filterType.HasValue || sm.ModifierScaling == filterType.Value))
            .Cast<StatModifier>()
            .Select(m => m.BaseValue)
            .ToList();
    }

    public bool HasReference(gameplayRef reference)
        => modifiers.Any(m => m.To_TriggerGameplayRefs.Contains(reference) && !m.IsExpired);

    public IStatModifier GetModifierByName(string name)
        => modifiers.FirstOrDefault(m => m.ModifierName == name && !m.IsExpired);

    public void AddOrReplaceModifier(IStatModifier modifier)
    {
        var existing = modifiers.FirstOrDefault(m => m.ModifierName == modifier.ModifierName);
        if (existing != null) modifiers.Remove(existing);
        modifiers.Add(modifier);
    }
}

// ----------------- Interfaces and Implementations --------------------

public interface IStatModifier
{
    string ModifierName { get; }
    int BaseValue { get; set; }
    ModifierScaling ModifierScaling { get; }
    int Duration { get; set; }
    List< gameplayRef> To_TriggerGameplayRefs { get; }
    bool IsExpired { get; }
    void AddListener();
    void OnRefEventTriggered(TriggerRef reference);
}

public class StatModifier : IStatModifier
{
    public string ModifierName { get; private set; }

    public int BaseValue
    {
        get
        {
            if (DynamicValueFunc != null)
                return DynamicValueFunc.Invoke();
            return StaticValue ?? 0;
        }
        set
        {
            if (DynamicValueFunc != null)
                throw new InvalidOperationException("Cannot set BaseValue when using dynamicValueFunc.");
            StaticValue = value;
        }
    }

    private int? StaticValue;
    private Func<int> DynamicValueFunc;

    public ModifierScaling ModifierScaling { get; private set; }
    public int Duration { get; set; }
    public List<gameplayRef> To_TriggerGameplayRefs { get; private set; }
    public bool IsExpired => Duration <= 0;
    public TriggerRef TriggerConditionRef { get; private set; }
    public Stat TargetStat;
    public int GetRemainingDuration() => Duration;

    public void AddListener() 
    {
        foreach (var Reference in TriggerConditionRef.References)
        {
            GameEvents.Subscribe(Reference, TriggerConditionRef.AffectedEntityId, OnRefEventTriggered);
        }
    }
    public void OnRemove()
    {
        foreach (var Reference in TriggerConditionRef.References)
        {
            GameEvents.Unsubscribe(Reference, TriggerConditionRef.AffectedEntityId, OnRefEventTriggered);
        }
        TargetStat.RemoveModifier(this);
    }
    public void OnRefEventTriggered(TriggerRef trigger)
    {
        if (Duration < 9999)
        {
            Duration--;
            Debug.Log($"StatModifier duration: {Duration}");
        }

        if (IsExpired)
        {
            OnRemove();
        }
    }

    public StatModifier(
        int value,
        ModifierScaling scaling,
        List<gameplayRef> gReferences = null,
        int duration = 99999,
        Stat target = null,
        string name = null)
    {
        ModifierName = name;
        StaticValue = value;
        ModifierScaling = scaling;
        To_TriggerGameplayRefs = gReferences;
        Duration = duration;
        TargetStat = target;
    }
}

public class FunctionModifier : IStatModifier
{
    public string ModifierName { get; private set; }

    public int BaseValue
    {
        get
        {
            if (DynamicValueFunc != null)
                return DynamicValueFunc.Invoke();
            return StaticValue ?? 0;
        }
        set
        {
            if (DynamicValueFunc != null)
                throw new InvalidOperationException("Cannot set BaseValue when using dynamicValueFunc.");
            StaticValue = value;
        }
    }
    private int? StaticValue;
    private Func<int> DynamicValueFunc;

    public ModifierScaling ModifierScaling { get; private set; }
    public int Duration { get; set; }
    public List<gameplayRef> To_TriggerGameplayRefs { get; private set; }
    public bool IsExpired => Duration <= 0;

    private Action<FunctionModifier, Stat, gameplayRef> OnRefEventAction;
    public TriggerRef TriggerConditionRef { get; private set; }
    public Stat TargetStat { get; private set; }
    public void AddListener()
    {
        foreach (var Reference in TriggerConditionRef.References)
        {
            GameEvents.Subscribe(Reference, TriggerConditionRef.AffectedEntityId, OnRefEventTriggered);
        }
    }
    public FunctionModifier
        (
        string statName,
        int baseValue,
        ModifierScaling statScaling,
        List<gameplayRef> to_Trigger_refs = null,
        int duration = 0,
        Stat target = null,
        Action<FunctionModifier, Stat, gameplayRef> onRefEventAction = null,
        TriggerRef triggerConditionRef = new TriggerRef()
        )
    {
        ModifierName = statName;
        BaseValue = baseValue;
        ModifierScaling = statScaling;
        To_TriggerGameplayRefs = to_Trigger_refs;
        Duration = duration;
        TargetStat = target;
        this.OnRefEventAction = onRefEventAction;
        TriggerConditionRef = triggerConditionRef;
    }

    public int GetRemainingDuration() => Duration;

    public void OnRefEventTriggered(TriggerRef trigger)
    {
        {
            foreach (gameplayRef gRef in To_TriggerGameplayRefs)
            {
                OnRefEventAction?.Invoke(this, TargetStat, gRef);
            }

            if (Duration < 9999)
            {
                Duration--;
                Debug.Log($"FunctionModifier duration: {Duration}");
            }

            if (IsExpired)
            {
                OnRemove();
            }
        }
    }
    public void OnRemove()
    {
        foreach (var Reference in TriggerConditionRef.References)
        {
            GameEvents.Unsubscribe(Reference, TriggerConditionRef.AffectedEntityId, OnRefEventTriggered);
        }
        TargetStat.RemoveModifier(this);
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

    public enum ModifierScaling
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
    public List<gameplayRef> References;
    public int UserId;
    public int AffectedEntityId;
    public TriggerRef(List<gameplayRef> references = null, int userId = 0, int targetId = 0)
    {
        References = references;
        UserId = userId;
        AffectedEntityId = targetId;
    }
}