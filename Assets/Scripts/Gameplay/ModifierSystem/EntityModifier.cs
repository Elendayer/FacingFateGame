using System;
using System.Collections.Generic;
using UnityEngine;

public interface IEntityModifier
{
    string ModifierName { get; }
    int BaseValue { get; set; }
    StatModifier StatModifier { get; set; }
    int Duration { get; set; }
    bool IsExpired { get; }

    // Triggering
    List<GameplayRef> ToTriggerGameplayRefs { get; }
    TriggerRef OnTriggerConditionRef { get; }


    void AddListener();
    void OnRefEventTriggered(TriggerRef reference);
    void OnManuelTrigger(TriggerRef trigger);
}

[System.Serializable]
public class EntityModifier : IEntityModifier
{
    //Main
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
    private int? StaticValue = 0;
    private Func<int> DynamicValueFunc;

    // Duration in number of triggers
    public int Duration { get; set; }
    public bool IsExpired => Duration <= 0;

    // Associated StatModifier
    public StatModifier StatModifier { get; set; }
    public Stat TargetStat { get; private set; }

    // Triggering
    public List<GameplayRef> ToTriggerGameplayRefs { get; private set; }
    public TriggerRef OnTriggerConditionRef { get; private set; } = new TriggerRef();

    // Action to perform on trigger
    public Action<TriggerActionData> onTriggerEventAction;

    // Constructor
    public EntityModifier
    (
    string modifierName,
    int baseValue = 0,
    List<GameplayRef> toTriggerRefs = null,
    TriggerRef onTriggerConditionRef = new TriggerRef(),
    int duration = 0,
    Stat target = null,
    Action<TriggerActionData> onTriggerEventAction = null
    )
    {
        ModifierName = modifierName;
        BaseValue = baseValue;
        ToTriggerGameplayRefs = toTriggerRefs;
        OnTriggerConditionRef = onTriggerConditionRef;
        Duration = duration;
        TargetStat = target;
        this.onTriggerEventAction = onTriggerEventAction;
    }

    // Methods
    public void AddListener()
    {
        GameEvents.OnGameplayReference += OnRefEventTriggered;
    }
    public int GetRemainingDuration() => Duration;
    public void OnRefEventTriggered(TriggerRef trigger)
    {
        if (GameEvents.CheckIfRelevantTrigger(trigger, OnTriggerConditionRef))
        {

            onTriggerEventAction?.Invoke(new TriggerActionData(trigger,StatModifier,TargetStat,BaseValue));

            GameEvents.TriggerRefEvent(new TriggerRef(ToTriggerGameplayRefs, OnTriggerConditionRef.UserEntity, OnTriggerConditionRef.AffectedEntity));

            if (Duration < 9999)
            {
                Duration--;
                Debug.Log($"FunctionModifier duration: {Duration}");
            }
        }
        if (IsExpired)
        {
            OnRemove();
        }
    }
    public void OnManuelTrigger(TriggerRef trigger)
    {
        onTriggerEventAction?.Invoke(new TriggerActionData(trigger, StatModifier, TargetStat, BaseValue));
        GameEvents.TriggerRefEvent(new TriggerRef(ToTriggerGameplayRefs, trigger.UserEntity, trigger.AffectedEntity));

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
    public void OnRemove()
    {
        foreach (var Reference in OnTriggerConditionRef.OnTriggerReference)
        {
            GameEvents.OnGameplayReference -= OnRefEventTriggered;
        }
        OnTriggerConditionRef.AffectedEntity.RemoveModifier(this);
    }
}

public struct TriggerActionData
{
    public TriggerRef TriggerReference;
    public StatModifier StatModifier;
    public Stat TargetStat;

    public int Value;

    public TriggerActionData(TriggerRef triggerReference, StatModifier statModifier, Stat targetStat, int value)
    {
        TriggerReference = triggerReference;
        StatModifier = statModifier;
        TargetStat = targetStat;
        Value = value;
    }
}