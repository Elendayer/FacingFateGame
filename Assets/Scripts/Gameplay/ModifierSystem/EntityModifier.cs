using System;
using System.Collections.Generic;
using UnityEngine;

public interface IEntityModifier
{
    string ModifierName { get; }
    int BaseValue { get; set; }
    StatModifier StatModifier { get; set; }
    int Duration { get; set; }
    List<gameplayRef> To_TriggerGameplayRefs { get; }
    bool IsExpired { get; }
    void AddListener();
    void OnRefEventTriggered(TriggerRef reference);
}
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
    public List<gameplayRef> To_TriggerGameplayRefs { get; private set; }
    public bool IsExpired => Duration <= 0;

    private Action<StatModifier, Stat, gameplayRef> OnRefEventAction;
    public TriggerRef TriggerConditionRef { get; private set; } = new TriggerRef();
    public Stat TargetStat { get; private set; }
    public void AddListener()
    {
        if ( TriggerConditionRef.References != null)
        {
            foreach (var Reference in TriggerConditionRef.References)
            {
                GameEvents.Subscribe(Reference, TriggerConditionRef.AffectedEntityId, OnRefEventTriggered);
            }
        }
    }
    public EntityModifier
        (
        string statName,
        int baseValue = 0,
        List<gameplayRef> to_Trigger_refs = null,
        int duration = 0,
        Stat target = null,
        Action<StatModifier, Stat, gameplayRef> onRefEventAction = null,
        //StatModifier statModifier = null,
        TriggerRef triggerConditionRef = new TriggerRef()
        )
    {
        ModifierName = statName;
        BaseValue = baseValue;
        //StatModifier = statModifier;
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
    public void OnRemove()
    {
        foreach (var Reference in TriggerConditionRef.References)
        {
            GameEvents.Unsubscribe(Reference, TriggerConditionRef.AffectedEntityId, OnRefEventTriggered);
        }
        TargetStat.RemoveModifier(StatModifier);
    }
}