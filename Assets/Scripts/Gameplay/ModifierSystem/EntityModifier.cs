using System;
using System.Collections.Generic;
using UnityEngine;

public interface IEntityModifier
{
    string ModifierName { get; }
    int BaseValue { get; set; }
    StatModifier StatModifier { get; set; }
    int Duration { get; set; }
    List<GameplayRef> ToTriggerGameplayRefs { get; }
    TriggerRef OnTriggerConditionRef { get; }

    bool IsExpired { get; }
    void AddListener();
    void OnRefEventTriggered(TriggerRef reference);
    void OnManuelTrigger();
}

[System.Serializable]
public class EntityModifier : IEntityModifier
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
    private int? StaticValue = 0;
    private Func<int> DynamicValueFunc;

    public StatModifier StatModifier { get; set; }
    public int Duration { get; set; }

    public List<GameplayRef> ToTriggerGameplayRefs { get; private set; }
    public TriggerRef OnTriggerConditionRef { get; private set; } = new TriggerRef();

    public bool IsExpired => Duration <= 0;

    private Action<StatModifier, Stat, GameplayRef> OnRefEventAction;
    public Stat TargetStat { get; private set; }
    public void AddListener()
    {
        if ( OnTriggerConditionRef.OnTriggerReference != null)
        {
            foreach (var Reference in OnTriggerConditionRef.OnTriggerReference)
            {
                GameEvents.Subscribe(Reference, OnTriggerConditionRef.AffectedEntityId, OnRefEventTriggered);
            }
        }
    }

    public EntityModifier
        (
        string statName,
        int baseValue = 0,
        List<GameplayRef> toTriggerRefs = null,
        TriggerRef onTriggerConditionRef = new TriggerRef(),
        int duration = 0,
        Stat target = null,
        Action<StatModifier, Stat, GameplayRef> onTriggerEventAction = null
        )
    {
        ModifierName = statName;
        BaseValue = baseValue;
        ToTriggerGameplayRefs = toTriggerRefs;
        OnTriggerConditionRef = onTriggerConditionRef;
        Duration = duration;
        TargetStat = target;
    }

    public int GetRemainingDuration() => Duration;
    public void OnRefEventTriggered(TriggerRef trigger)
    {
        {
            foreach (GameplayRef gRef in ToTriggerGameplayRefs)
            {
                OnRefEventAction?.Invoke(StatModifier, TargetStat, gRef);
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
    public void OnManuelTrigger()
    {
        foreach (GameplayRef gRef in ToTriggerGameplayRefs)
        {
            OnRefEventAction?.Invoke(StatModifier, TargetStat, gRef);
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
    public void OnRemove()
    {
        foreach (var Reference in OnTriggerConditionRef.OnTriggerReference)
        {
            GameEvents.Unsubscribe(Reference, OnTriggerConditionRef.AffectedEntityId, OnRefEventTriggered);
        }
        TargetStat.RemoveModifier(StatModifier);
    }
}