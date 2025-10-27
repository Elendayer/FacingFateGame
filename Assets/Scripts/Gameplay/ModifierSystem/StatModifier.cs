using System;
using System.Collections.Generic;
using UnityEngine;

public interface IStatModifier
{
    string ModifierName { get; }
    int BaseValue { get; set; }
    ModifierScaling ModifierScaling { get; }
    int Duration { get; set; }
    List<GameplayRef> To_TriggerGameplayRefs { get; }
    bool IsExpired { get; }
    void AddListener();
    void On_RefEventTriggered(TriggerRef reference);
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
    public List<GameplayRef> To_TriggerGameplayRefs { get; private set; }
    public TriggerRef On_TriggerConditionRef { get; private set; }

    public bool IsExpired => Duration <= 0;
    public Stat TargetStat;
    public int GetRemainingDuration() => Duration;

    public void AddListener()
    {
        if (On_TriggerConditionRef.References == null)
        {
            return;
        }

        foreach (var Reference in On_TriggerConditionRef.References)
        {
            GameEvents.Subscribe(Reference, On_TriggerConditionRef.AffectedEntityId, On_RefEventTriggered);
        }
    }
    public void OnRemove()
    {
        foreach (var Reference in On_TriggerConditionRef.References)
        {
            GameEvents.Unsubscribe(Reference, On_TriggerConditionRef.AffectedEntityId, On_RefEventTriggered);
        }
        TargetStat.RemoveModifier(this);
    }
    public void On_RefEventTriggered(TriggerRef trigger)
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

    public StatModifier
        (    
        int value,
        ModifierScaling scaling,
        List<GameplayRef> to_triggerReferences = null,
        int duration = 99999,
        TriggerRef on_triggerConditionRef = new(),
        Stat target = null,
        string name = null
        )
    {
        ModifierName = name;
        StaticValue = value;
        ModifierScaling = scaling;
        To_TriggerGameplayRefs = to_triggerReferences;
        On_TriggerConditionRef = on_triggerConditionRef;
        Duration = duration;
        TargetStat = target;
    }
    public StatModifier
        (
        Func<int> value,
        ModifierScaling scaling,
        List<GameplayRef> gReferences = null,
        int duration = 99999,
        TriggerRef triggerConditionRef = new(),
        Stat target = null,
        string name = null
        )
    {
        ModifierName = name;
        DynamicValueFunc = value;
        ModifierScaling = scaling;
        To_TriggerGameplayRefs = gReferences;
        On_TriggerConditionRef = triggerConditionRef;
        Duration = duration;
        TargetStat = target;
    }
}


public class ConditionalStatModifier : IStatModifier
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

    public bool Condition
    {
        get
        {
            if (DynamicBoolFunc != null)
                return DynamicBoolFunc.Invoke();
            return StaticBool ?? false;
        }
        set
        {
            if (DynamicBoolFunc != null)
                throw new InvalidOperationException("Cannot set BaseValue when using dynamicValueFunc.");
            StaticBool = value;
        }
    }
    private bool? StaticBool;
    private Func<bool> DynamicBoolFunc;

    public ModifierScaling ModifierScaling { get; private set; }
    public int Duration { get; set; }
    public int Uses { get; set; } = 1;
    public List<GameplayRef> To_TriggerGameplayRefs { get; private set; }
    public bool IsExpired => Duration <= 0;
    public Stat TargetStat;
    public int GetRemainingDuration() => Duration;

    public void OnRemove()
    {
        TargetStat.RemoveModifier(this);
    }
    public void AddListener() { }
   
    public void On_RefEventTriggered(TriggerRef trigger)
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
    public void On_RefEventTriggered()
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

    public ConditionalStatModifier
        (
        int value,
       bool condition,
        ModifierScaling scaling,
        int duration = 99999,
        List<GameplayRef> gReferences = null,
        TriggerRef on_triggerConditionRef = new(),
        Stat target = null,
        string name = null
        )
    {
        ModifierName = name;
        StaticValue = value;
        ModifierScaling = scaling;
        To_TriggerGameplayRefs = gReferences;
        Duration = duration;
        TargetStat = target;
    }
    public ConditionalStatModifier
        (
        Func<int> value,
         bool condition,
        ModifierScaling scaling,
        int duration = 99999,
        List<GameplayRef> gReferences = null,
        TriggerRef triggerConditionRef = new(),
        Stat target = null,
        string name = null
        )
    {
        ModifierName = name;
        DynamicValueFunc = value;
        ModifierScaling = scaling;
        To_TriggerGameplayRefs = gReferences;
        Duration = duration;
        TargetStat = target;
    }
    public ConditionalStatModifier
    (
    int value,
    Func<bool> condition,
    ModifierScaling scaling,
    int duration = 99999,
    List<GameplayRef> gReferences = null,
    TriggerRef on_triggerConditionRef = new(),
    Stat target = null,
    string name = null
    )
    {
        ModifierName = name;
        StaticValue = value;
        ModifierScaling = scaling;
        To_TriggerGameplayRefs = gReferences;
        Duration = duration;
        TargetStat = target;
    }
    public ConditionalStatModifier
        (
        Func<int> value,
        Func<bool> condition,
        ModifierScaling scaling,
        int duration = 99999,
        List<GameplayRef> gReferences = null,
        TriggerRef triggerConditionRef = new(),
        Stat target = null,
        string name = null
        )
    {
        ModifierName = name;
        DynamicValueFunc = value;
        ModifierScaling = scaling;
        To_TriggerGameplayRefs = gReferences;
        Duration = duration;
        TargetStat = target;
    }
}