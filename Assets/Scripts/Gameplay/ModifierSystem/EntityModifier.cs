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
    int Charges { get; set; }


    // Triggering
    List<GameplayRef> ToTriggerGameplayRefs { get; }
    TriggerRef OnTriggerConditionRef { get; }


    void AddListener();
    void OnRefEventTriggered(TriggerRef reference);
    void OnManuelTrigger(TriggerRef trigger, bool consumeCharges = false);
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
    private Func<int> DynamicValueFunc = null;

    // Duration in number of triggers
    public int Duration { get; set; }
    public bool IsExpired => Duration <= 0;

    public int Charges { get; set; } = 0;
    public bool IsSpend => Charges <= 0;

    // Associated StatModifier
    public StatModifier StatModifier { get; set; }

    // Triggering
    public List<GameplayRef> ToTriggerGameplayRefs { get; private set; }
    public TriggerRef OnTriggerConditionRef { get; private set; } = new TriggerRef();

    // Action to perform on trigger
    public Action<TriggerActionData, EntityScript> onTriggerEventAction;

    // Constructor
    public EntityModifier
    (
    string modifierName,
    int baseValue = 0,
    List<GameplayRef> toTriggerRefs = null,
    TriggerRef onTriggerConditionRef = new TriggerRef(),
    int duration = 99999,
    int charges = 99999,
    Stat targetStat = null,
    Action<TriggerActionData, EntityScript> onTriggerEventAction = null
    )
    {
        ModifierName = modifierName;
        BaseValue = baseValue;
        ToTriggerGameplayRefs = toTriggerRefs;
        OnTriggerConditionRef = onTriggerConditionRef;
        Duration = duration;
        Charges = charges;
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
            onTriggerEventAction?.Invoke(new TriggerActionData(trigger, StatModifier, BaseValue), trigger.AffectedEntities[0]);

            if (ToTriggerGameplayRefs != null && ToTriggerGameplayRefs.Count > 0)
            {
                GameEvents.TriggerRefEvent(new TriggerRef(ToTriggerGameplayRefs, trigger.UserEntity, trigger.AffectedEntities, null, BaseValue));
            }

            if (Charges < 9999)
            {
                Charges--;
            }
        }

        TriggerRef DurationTrigger = new TriggerRef
            ( 
            references: new() { GameplayRef.onTurnStart },
            userEntity: OnTriggerConditionRef.UserEntity,
            affectedEntities: new() { OnTriggerConditionRef.UserEntity 
            });

        if (GameEvents.CheckIfRelevantTrigger(trigger, DurationTrigger))
        {
            if (Duration < 99999)
            {
                Duration--;
            }

        }
        if (IsExpired || IsSpend)
        {
            OnRemove();
        }
    }
    public void OnManuelTrigger(TriggerRef trigger, bool consumeCharges = false)
    {
        Debug.Log($"EntityModifier {ModifierName} manually triggered action. By {trigger.UserEntity} at {trigger.AffectedEntities[0]}");
        onTriggerEventAction?.Invoke(new TriggerActionData(trigger, StatModifier, BaseValue), trigger.AffectedEntities[0]);
        GameEvents.TriggerRefEvent(new TriggerRef(ToTriggerGameplayRefs, trigger.UserEntity, trigger.AffectedEntities, trigger.CardData, trigger.Throughput));
        
        if (consumeCharges)
        {
            if (Charges < 99999)
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
    public void OnRemove()
    {
        foreach (var Reference in OnTriggerConditionRef.OnTriggerReference)
        {
            GameEvents.OnGameplayReference -= OnRefEventTriggered;
        }
        OnTriggerConditionRef.AffectedEntities[0].RemoveModifier(this);
    }
}

public struct TriggerActionData
{
    public TriggerRef TriggerReference;
    public StatModifier StatModifier;

    public int Value;

    public TriggerActionData(TriggerRef triggerReference, StatModifier statModifier,  int value)
    {
        TriggerReference = triggerReference;
        StatModifier = statModifier;
        Value = value;
    }
}