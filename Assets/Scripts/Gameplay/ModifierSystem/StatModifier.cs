using System;
using System.Collections.Generic;
using UnityEngine;
using static TimelineManager;

namespace facingfate
{
    string ModifierName { get; }
    int BaseValue { get; set; }
    ModifierScaling ModifierScaling { get; }
    int Duration { get; set; }
    int Charges { get; set; }
    bool Condition(EntityScript entityScript = null, CardData cardData = null);
    List<GameplayRef> To_TriggerGameplayRefs { get; }
    bool IsExpired { get; }
    bool IsSpend { get; }

    RelevantTriggerCheck On_RefTrigger { get; set; }
    void RefAction(ToSendTriggerReference reference);

    Stat Stat { get; }

    public void Init();
    void Tick();
    void UpdateStatModifier();
    void OnRemove();
}

    public class StatModifier : IStatModifier
    {
        public string ModifierName { get; private set; }

        public int BaseValue
        {
            get => DynamicValueFunc != null ? DynamicValueFunc.Invoke() : StaticValue ?? 0;
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
    public int Charges { get; set; }
    public bool IsExpired => Duration <= 0;
    public bool IsSpend => Charges <= 0;
    public List<GameplayRef> To_TriggerGameplayRefs { get; private set; }
    public Stat Stat { get; }

    public RelevantTriggerCheck On_RefTrigger { get; set; }
    public Action<StatModifier, EntityScript, CardData, int> On_RefAction { get; set; }

    // --- Condition ---
    private bool? StaticBool;
    private Func<EntityScript, CardData, bool> DynamicConditionFunc;

        public bool Condition(EntityScript entityScript = null, CardData cardData = null) => DynamicConditionFunc != null
            ? DynamicConditionFunc.Invoke(entityScript, cardData)
            : StaticBool ?? false;

    public int GetRemainingDuration() => Duration;

    public void Init()
    {
        if (On_RefTrigger.OnTriggerReference == null) return;
        if (On_RefTrigger.OnTriggerReference.Count == 0) return;

        GameEvents.OnGameplayReference += (trigger) => RefAction(trigger);
    }
    public void OnRemove()
    {
        GameEvents.OnGameplayReference -= (trigger) => RefAction(trigger);
        Stat.statModifiers.Remove(this);
    }
    public void Tick()
    {
        Duration--;

        if (IsExpired)
        {
            GlobalActionQueue.Enqueue(() =>
            {
                OnRemove();
            });
        }
    }
    public void UpdateStatModifier()
    {
        if (IsSpend)
        {
            GlobalActionQueue.Enqueue(() =>
            {
                OnRemove();
            });
        }
    }

    public void RefAction(ToSendTriggerReference trigger)
    {
        // Check if relevant
        if (GameEvents.CheckIfRelevantTrigger(trigger, On_RefTrigger))
        {
            Debug.Log($"StatModifier '{ModifierName}' RefAction triggered. {Charges}");
            
            Charges--;
            
            // Execute action
            if (On_RefAction != null)
            {
                On_RefAction(this, trigger.UserEntity, trigger.CardData, trigger.Throughput);
            }
        }
    }
    // -------------------- Constructors --------------------

    // 1) Static int + static bool
    public StatModifier(
        string name,
        Stat stat,
        int value,
        ModifierScaling scaling,
        bool condition = true,
        List<GameplayRef> to_TriggerRefs = null,
        int duration = 99999,
        int charges = 99999,
        RelevantTriggerCheck on_RefTrigger = new(),
        Action<StatModifier, EntityScript, CardData, int> on_RefAction = null
    )
    {
        ModifierName = name;
        Stat = stat;
        StaticValue = value;
        ModifierScaling = scaling;
        StaticBool = condition;
        To_TriggerGameplayRefs = to_TriggerRefs;
        Duration = duration;
        Charges = charges;
        On_RefTrigger = on_RefTrigger;
        On_RefAction = on_RefAction;
    }

    // 2) Static int + dynamic condition
    public StatModifier(
        string name,
        Stat stat,
        int value,
        ModifierScaling scaling,
        Func<EntityScript, CardData, bool> condition,
        List<GameplayRef> to_TriggerRefs = null,
        int duration = 99999,
        int charges = 99999,
        RelevantTriggerCheck on_RefTrigger = new(),
        Action<StatModifier, EntityScript, CardData, int> on_RefAction = null
        )
    {
        ModifierName = name;
        Stat = stat;
        StaticValue = value;
        ModifierScaling = scaling;
        DynamicConditionFunc = condition;
        To_TriggerGameplayRefs = to_TriggerRefs;
        Duration = duration;
        Charges = charges;
        On_RefTrigger = on_RefTrigger;
        On_RefAction = on_RefAction;
    }

    // 3) Dynamic int + static bool
    public StatModifier(
        string name,
        Stat stat,
        Func<int> value,
        ModifierScaling scaling,
        bool condition = true,
        List<GameplayRef> to_TriggerRefs = null,
        int duration = 99999,
        int charges = 99999,
        RelevantTriggerCheck on_RefTrigger = new(),
        Action<StatModifier, EntityScript, CardData, int> on_RefAction = null
    )
    {
        ModifierName = name;
        DynamicValueFunc = value;
        ModifierScaling = scaling;
        To_TriggerGameplayRefs = to_TriggerRefs;
        Duration = duration;
        Charges = charges;
        StaticBool = condition;
        Stat = stat;
        On_RefTrigger = on_RefTrigger;
        On_RefAction = on_RefAction;
    }

    // 4) Dynamic int + dynamic condition
    public StatModifier(
        string name,
        Stat stat,
        Func<int> value,
        ModifierScaling scaling,
        Func<EntityScript, CardData, bool> condition,
        List<GameplayRef> to_TriggerRefs = null,
        int duration = 99999,
        int charges = 99999,   
        RelevantTriggerCheck on_RefTrigger = new(),
        Action<StatModifier, EntityScript, CardData, int> on_RefAction = null
    )
    {
        ModifierName = name;
        Stat = stat;
        DynamicValueFunc = value;
        ModifierScaling = scaling;
        DynamicConditionFunc = condition;
        To_TriggerGameplayRefs = to_TriggerRefs;
        Duration = duration;
        Charges = charges;
        On_RefTrigger = on_RefTrigger;
        On_RefAction = on_RefAction;
    }
}