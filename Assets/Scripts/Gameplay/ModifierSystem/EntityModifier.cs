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
    TriggerRef OnRef_Trigger { get; }

    void AddListener();
    void OnRef_ActionCall(TriggerRef reference);
    void OnApply_ActionTrigger();
    void onRemove_ActionTrigger();
    void OnManuel_ActionTrigger(TriggerRef trigger, bool consumeCharges = false);
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
    public TriggerRef OnRef_Trigger { get; private set; } = new TriggerRef();
    public TriggerRef OnRemove_Trigger { get; private set; } = new TriggerRef();
    public TriggerRef OnApply_Trigger { get; private set; } = new TriggerRef();

    // Action to perform on trigger
    public Action<TriggerActionData, EntityScript> OnRef_Action;
    public Action<TriggerActionData, EntityScript> OnApply_Action;
    public Action<TriggerActionData, EntityScript> OnRemove_Action;

    // Constructor
    public EntityModifier
    (
    string modifierName,
    int baseValue = 0,
    List<GameplayRef> toTriggerRefs = null,
    TriggerRef onRef_Trigger = new TriggerRef(),
    TriggerRef onApply_Trigger = new TriggerRef(),
    TriggerRef onRemove_Trigger = new TriggerRef(),
    int duration = 99999,
    int charges = 99999,
    Stat targetStat = null,
    Action<TriggerActionData, EntityScript> onRef_Action = null,
    Action<TriggerActionData, EntityScript> onApply_Action = null,
    Action<TriggerActionData, EntityScript> onRemove_Action = null
    )
    {
        ModifierName = modifierName;
        BaseValue = baseValue;
        ToTriggerGameplayRefs = toTriggerRefs;
        OnRef_Trigger = onRef_Trigger;
        OnApply_Trigger = onApply_Trigger;
        OnRemove_Trigger = onRemove_Trigger;
        Duration = duration;
        Charges = charges;
        OnRef_Action = onRef_Action;
        OnApply_Action = onApply_Action;
        OnRemove_Action = onRemove_Action;
    }

    // Methods
    public void AddListener()
    {
        GameEvents.OnGameplayReference += OnRef_ActionCall;
    }
    public int GetRemainingDuration() => Duration;

    public void OnRef_ActionCall(TriggerRef trigger)
    {

        if (GameEvents.CheckIfRelevantTrigger(trigger, OnRef_Trigger))
        {
            OnRef_Action?.Invoke(new TriggerActionData(trigger, StatModifier, BaseValue), trigger.AffectedEntities[0]);

            if (ToTriggerGameplayRefs != null && ToTriggerGameplayRefs.Count > 0)
            {
                GameEvents.TriggerRefEvent(new TriggerRef(ToTriggerGameplayRefs, trigger.UserEntity, trigger.AffectedEntities, null, BaseValue));
            }

            // Consume Charge if it has Charges
            if (Charges < 9999)
            {
                Charges--;
            }

            // Tick Duration if Effect Triggers on Turnstart
            if(trigger.OnTriggerReference.Contains(GameplayRef.onTurnStart))
            {
                if (Duration < 9999)
                {
                    Duration--;
                }
            }
            if (IsExpired || IsSpend)
            {
                OnRemove();
            }
        }

        // Tick Duration if its not triggered at TurnStart and has Duration
        if(!OnRef_Trigger.OnTriggerReference.Contains(GameplayRef.onTurnStart))
        {
            TriggerRef DurationTrigger = new TriggerRef(
            references: new() { GameplayRef.onTurnEnd },
            userEntity: OnRef_Trigger.UserEntity,
            affectedEntities: new() { OnRef_Trigger.AffectedEntities[0]
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
    }
    public void OnApply_ActionTrigger()
    {
        OnApply_Action?.Invoke(new TriggerActionData(OnApply_Trigger, StatModifier, BaseValue), OnApply_Trigger.AffectedEntities[0]);

        //GameEvents.TriggerRefEvent(new TriggerRef(ToTriggerGameplayRefs, OnApply_Trigger.UserEntity, OnApply_Trigger.AffectedEntities, OnApply_Trigger.CardData, OnApply_Trigger.Throughput));
    }
    public void onRemove_ActionTrigger()
    {
        OnRemove_Action?.Invoke(new TriggerActionData(OnRemove_Trigger, StatModifier, BaseValue), OnRemove_Trigger.AffectedEntities[0]);

        //GameEvents.TriggerRefEvent(new TriggerRef(ToTriggerGameplayRefs, OnRemove_Trigger.UserEntity, OnRemove_Trigger.AffectedEntities, OnRemove_Trigger.CardData, OnRemove_Trigger.Throughput));
    }
    public void OnManuel_ActionTrigger(TriggerRef trigger, bool consumeCharges = false)
    {
        Debug.Log($"EntityModifier {ModifierName} manually triggered action. By {trigger.UserEntity} at {trigger.AffectedEntities[0]}");
        OnRef_Action?.Invoke(new TriggerActionData(trigger, StatModifier, BaseValue), trigger.AffectedEntities[0]);

        GameEvents.TriggerRefEvent(new TriggerRef(ToTriggerGameplayRefs, trigger.UserEntity, trigger.AffectedEntities, trigger.CardData, trigger.Throughput));

        if (Charges < 9999)
        {
            Charges--;
        }
        
        if (consumeCharges)
        {
            if (Charges < 99999)
            {
                Duration--;
                Debug.Log($"FunctionModifier duration: {Duration}");
            }
        }
        if (IsExpired || IsSpend)
        {
            OnRemove();
        }
    }
    public void OnRemove()
    {
        onRemove_ActionTrigger();

        foreach (var Reference in OnRef_Trigger.OnTriggerReference)
        {
            GameEvents.OnGameplayReference -= OnRef_ActionCall;
        }
        OnRef_Trigger.AffectedEntities[0].RemoveModifier(this);
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