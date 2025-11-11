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
    void On_RefEventTriggered(TriggerRef reference);
    Stat Stat { get; }

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
    public bool Condition { get; }
    public List<GameplayRef> To_TriggerGameplayRefs { get; private set; }
    public bool IsExpired => Duration <= 0;
    public int GetRemainingDuration() => Duration;
    public Stat Stat { get; }

    public void OnRemove()
    {
        Stat.RemoveModifier(this);
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
        Stat stat,
        int value,
        ModifierScaling scaling,
        List<GameplayRef> to_triggerReferences = null,
        int duration = 99999,
        TriggerRef on_triggerConditionRef = new(),
        string name = null,
        bool condition = true
        )
    {
        ModifierName = name;
        StaticValue = value;
        ModifierScaling = scaling;
        To_TriggerGameplayRefs = to_triggerReferences;
        Duration = duration;
        Condition = condition;
        Stat = stat;
    }
    public StatModifier
        (
        Stat stat,
        Func<int> value,
        ModifierScaling scaling,
        List<GameplayRef> gReferences = null,
        int duration = 99999,
        TriggerRef triggerConditionRef = new(),
        Stat target = null,
        string name = null,
        bool condition = true
        )
    {
        ModifierName = name;
        DynamicValueFunc = value;
        ModifierScaling = scaling;
        To_TriggerGameplayRefs = gReferences;
        Duration = duration;
        Condition = condition;
        Stat = stat;
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
    public int GetRemainingDuration() => Duration;
    public Stat Stat { get; }

    public void OnRemove()
    {
        Stat.RemoveModifier(this);
    }

    public void AddListener() { }

    public void On_RefEventTriggered(TriggerRef trigger)
    {
        if( GameEvents.CheckIfRelevantTrigger(trigger, new TriggerRef(new List<GameplayRef> { GameplayRef.onTurnStart }, Stat.owner)))
        {       
            if (Duration < 9999)
            {
                Duration--;
                Debug.Log($"StatModifier duration: {Duration}");
            }
        }

        if (IsExpired)
        {
            OnRemove();
        }
    }

    public ConditionalStatModifier
        (
        Stat stat,
        int value,
        bool condition,
        ModifierScaling scaling,
        int duration = 99999,
        List<GameplayRef> gReferences = null,
        TriggerRef on_triggerConditionRef = new(),
        string name = null
        )
    {
        ModifierName = name;
        StaticValue = value;
        ModifierScaling = scaling;
        To_TriggerGameplayRefs = gReferences;
        Duration = duration;
        Stat = stat;
    }
    public ConditionalStatModifier
        (
        Stat stat,
        Func<int> value,
         bool condition,
        ModifierScaling scaling,
        int duration = 99999,
        List<GameplayRef> gReferences = null,
        TriggerRef triggerConditionRef = new(),
        string name = null
        )
    {
        ModifierName = name;
        DynamicValueFunc = value;
        ModifierScaling = scaling;
        To_TriggerGameplayRefs = gReferences;
        Duration = duration;
        Stat = stat;
    }
    public ConditionalStatModifier
        (
        Stat target,
        int value,
        Func<bool> condition,
        ModifierScaling scaling,
        int duration = 99999,
        List<GameplayRef> gReferences = null,
        TriggerRef on_triggerConditionRef = new(),
        string name = null
        )
    {
        ModifierName = name;
        StaticValue = value;
        ModifierScaling = scaling;
        To_TriggerGameplayRefs = gReferences;
        Duration = duration;
        Stat = target;
    }
    public ConditionalStatModifier
        (
        Stat stat,
        Func<int> value,
        Func<bool> condition,
        ModifierScaling scaling,
        int duration = 99999,
        List<GameplayRef> gReferences = null,
        TriggerRef triggerConditionRef = new(),
        string name = null
        )
    {
        ModifierName = name;
        DynamicValueFunc = value;
        ModifierScaling = scaling;
        To_TriggerGameplayRefs = gReferences;
        Duration = duration;
        Stat = stat;
    }
}