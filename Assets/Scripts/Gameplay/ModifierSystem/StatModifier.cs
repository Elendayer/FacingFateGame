using System;
using System.Collections.Generic;
using UnityEngine;

public interface IStatModifier
{
    string ModifierName { get; }
    int BaseValue { get; set; }
    ModifierScaling ModifierScaling { get; }
    int Duration { get; set; }
    bool Condition(EntityScript entityScript = null, CardData cardData = null);
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
    public bool IsExpired => Duration <= 0;
    public List<GameplayRef> To_TriggerGameplayRefs { get; private set; }
    public Stat Stat { get; }

    // --- Condition ---
    private bool? StaticBool;
    private Func<EntityScript, CardData, bool> DynamicConditionFunc;

    public bool Condition(EntityScript entityScript = null, CardData cardData = null) => DynamicConditionFunc != null
        ? DynamicConditionFunc.Invoke(entityScript, cardData)
        : StaticBool ?? false;

    public void OnRemove() => Stat.RemoveModifier(this);
    public int GetRemainingDuration() => Duration;

    public void On_RefEventTriggered(TriggerRef trigger)
    {
        if (Duration < 9999)
            Duration--;

        if (IsExpired)
            OnRemove();
    }



    // -------------------- Constructors --------------------

    // 1) Static int + static bool
    public StatModifier(
        Stat stat,
        int value,
        ModifierScaling scaling,
        bool condition = true,
        List<GameplayRef> to_TriggerRefs = null,
        int duration = 99999,
        string name = null
    )
    {
        ModifierName = name;
        StaticValue = value;
        ModifierScaling = scaling;
        To_TriggerGameplayRefs = to_TriggerRefs;
        Duration = duration;
        StaticBool = condition;
        Stat = stat;
    }

    // 2) Static int + dynamic condition
    public StatModifier(
        Stat stat,
        int value,
        ModifierScaling scaling,
        Func<EntityScript, CardData, bool> conditionFunc,
        List<GameplayRef> to_TriggerRefs = null,
        int duration = 99999,
        string name = null
    )
    {
        ModifierName = name;
        StaticValue = value;
        ModifierScaling = scaling;
        To_TriggerGameplayRefs = to_TriggerRefs;
        Duration = duration;
        DynamicConditionFunc = conditionFunc ?? ((e, c) => true);
        Stat = stat;
    }

    // 3) Dynamic int + static bool
    public StatModifier(
        Stat stat,
        Func<int> value,
        ModifierScaling scaling,
        bool condition = true,
        List<GameplayRef> to_TriggerRefs = null,
        int duration = 99999,
        string name = null
    )
    {
        ModifierName = name;
        DynamicValueFunc = value;
        ModifierScaling = scaling;
        To_TriggerGameplayRefs = to_TriggerRefs;
        Duration = duration;
        StaticBool = condition;
        Stat = stat;
    }

    // 4) Dynamic int + dynamic condition
    public StatModifier(
        Stat stat,
        Func<int> value,
        ModifierScaling scaling,
        Func<EntityScript, CardData, bool> conditionFunc,
        List<GameplayRef> to_TriggerRefs = null,
        int duration = 99999,
        string name = null
    )
    {
        ModifierName = name;
        DynamicValueFunc = value;
        ModifierScaling = scaling;
        To_TriggerGameplayRefs = to_TriggerRefs;
        Duration = duration;
        DynamicConditionFunc = conditionFunc ?? ((e, c) => true);
        Stat = stat;
    }
}