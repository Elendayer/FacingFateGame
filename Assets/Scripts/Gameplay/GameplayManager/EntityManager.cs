using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    [Header("Base Stats")]
    public Stat MaxHealth = new() { Value = 100 };
    public Stat CurrentHealth = new() { Value = 100 };
    public Stat MaxStamina = new() { Value = 10 };
    public Stat CurrentStamina = new() { Value = 10 };

    public Stat Block = new() { Value = 0 };
    public Stat Armour = new() { Value = 0 };

    public Dictionary<EntityAttributeEnum, Stat> EntityAttributes = new();

    public Dictionary<(CardType, StatAspect), Stat> CardTypeStats = new();
    public Dictionary<(CardElement, StatAspect), Stat> CardElementStats = new();
    public Dictionary<(CardClass, StatAspect), Stat> CardClassStats = new();

    private void Awake()
    {
        // Fill EntityAttributes
        foreach (EntityAttributeEnum attr in Enum.GetValues(typeof(EntityAttributeEnum)))
        {
            EntityAttributes.Add(attr, new Stat() { Value = 2});
        }

        // Fill CardTypeStats
        foreach (CardType type in Enum.GetValues(typeof(CardType)))
        {
            foreach (StatAspect aspect in Enum.GetValues(typeof(StatAspect)))
            {
                CardTypeStats.Add((type, aspect), new Stat());
            }
        }

        // Fill CardElementStats
        foreach (CardElement element in Enum.GetValues(typeof(CardElement)))
        {
            foreach (StatAspect aspect in Enum.GetValues(typeof(StatAspect)))
            {
                CardElementStats.Add((element, aspect), new Stat());
            }
        }

        // Fill CardClassStats
        foreach (CardClass cls in Enum.GetValues(typeof(CardClass)))
        {
            foreach (StatAspect aspect in Enum.GetValues(typeof(StatAspect)))
            {
                CardClassStats.Add((cls, aspect), new Stat());
            }
        }
    }

    public int GetStatValue(EntityAttributeEnum attr)
    {
        if (EntityAttributes.TryGetValue(attr, out var stat))
        {
            return stat.GetFinalValue();
        }

        Debug.LogWarning($"{this.name} Stat not found for ({attr})");
        return 0;
    }

    public int GetStatValue(CardType type, StatAspect aspect)
    {
        if (CardTypeStats.TryGetValue((type, aspect), out var stat))
        {
            return stat.GetFinalValue();
        }

        Debug.LogWarning($"{this.name} Stat not found for ({type}, {aspect})");
        return 0;
    }
    public int GetStatValue(CardClass cls, StatAspect aspect)
    {
        if (CardClassStats.TryGetValue((cls, aspect), out var stat))
        {
            return stat.GetFinalValue();
        }

        Debug.LogWarning($"{this.name} Stat not found for ({cls}, {aspect})");
        return 0;
    }

    public int GetStatValue(CardElement element, StatAspect aspect)
    {
        if (CardElementStats.TryGetValue((element, aspect), out var stat))
        {
            return stat.GetFinalValue();
        }

        Debug.LogWarning($"{this.name} Stat not found for ({element}, {aspect})");
        return 0;
    }
    private void OnEnable()
    {
        // Subscribe to the event when the slider value changes and triggers the event
        GameEvents.OnTurnStart += ProcessTurnStart;
    }

    private void OnDisable()
    {
        // Unsubscribe from the event when this object is disabled or destroyed to avoid memory leaks
        GameEvents.OnTurnStart -= ProcessTurnStart;
    }

    public void ProcessTurnStart()
    {
        MaxHealth.TickModifiers();
        CurrentHealth.TickModifiers();
        MaxStamina.TickModifiers();
        CurrentStamina.TickModifiers();

        Block.TickModifiers();
        Armour.TickModifiers();

        foreach (KeyValuePair<EntityAttributeEnum, Stat> i in EntityAttributes)
        {
            i.Value.TickModifiers(); // Decrements durations and removes expired
        }
        foreach (KeyValuePair<(CardType, StatAspect), Stat> i in CardTypeStats)
        {
            i.Value.TickModifiers(); // Decrements durations and removes expired
        }
        foreach (KeyValuePair<(CardClass, StatAspect), Stat> i in CardClassStats)
        {
            i.Value.TickModifiers(); // Decrements durations and removes expired
        }
        foreach (KeyValuePair<(CardElement, StatAspect), Stat> i in CardElementStats)
        {
            i.Value.TickModifiers(); // Decrements durations and removes expired
        }
    }
}

public enum EntityAttributeEnum
{
    Strength,
    Dexterity,
    Wisdom,
    Foresight,
    Endurance,
    Tenacity
}