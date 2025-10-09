using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityStats
{
    public EntityScript Owner;
   
    [Header("Base Stats")]
    [SerializeField]
    public Stat MaxHealth = new();
    public Stat CurrentHealth = new();
    public Stat MaxStamina = new();
    public Stat CurrentStamina = new();

    public Stat Block = new();
    public Stat Armour = new();

    [Header("Attributes")]
    public Dictionary<EntityAttributeEnum, Stat> EntityAttributes = new();

    [Header("Card Related Stats")]
    public Dictionary<(CardType, StatAspect), Stat> CardTypeStats = new();
    public Dictionary<(CardIdentity, StatAspect), Stat> CardIdentityStats = new();
    public Dictionary<(CardClass, StatAspect), Stat> CardClassStats = new();

    [Header ("Combat Modifiers")]
    public Stat DamageReduction = new();

    public Stat DamageIncrease = new();
    public Stat HealingIncrease = new();

    public Stat IgnoreArmour = new();
    public Stat IgnoreBlock = new();

    public Stat Lifesteal = new();

    public void StartUp(EntityScript entityScript)
    {
        Owner = entityScript;
        GameEvents.OnCombatStart += PostStartUp;

        // Fill EntityAttributes
        foreach (EntityAttributeEnum attr in Enum.GetValues(typeof(EntityAttributeEnum)))
        {
            EntityAttributes.Add(attr, new Stat());
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
        foreach (CardIdentity element in Enum.GetValues(typeof(CardIdentity)))
        {
            foreach (StatAspect aspect in Enum.GetValues(typeof(StatAspect)))
            {
                CardIdentityStats.Add((element, aspect), new Stat());
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

        //PostStartUp();
    }

    public void PostStartUp()
    {
        foreach (var attr in EntityAttributes.Values)
        {
            attr.AddModifier(new StatModifier(10, ModifierScaling.Flat, name: "BaseValue"));
        }

        Debug.Log($"[EntityStats] PostStartUp called for {Owner.name}");
        MaxHealth.AddModifier(new StatModifier(value: () => GetStatValue(EntityAttributeEnum.Tenacity) * 50, ModifierScaling.Flat, name: "BaseValue"));
        MaxStamina.AddModifier(new StatModifier(value: () => GetStatValue(EntityAttributeEnum.Endurance) * 5, ModifierScaling.Flat, name: "BaseValue"));

        CurrentHealth.AddModifier(new StatModifier(MaxHealth.Value, ModifierScaling.Flat, name: "BaseValue"));
        CurrentStamina.AddModifier(new StatModifier(MaxStamina.Value, ModifierScaling.Flat, name: "BaseValue"));
    }

    public int GetStatValue(EntityAttributeEnum attr)
    {
        if (EntityAttributes.TryGetValue(attr, out var stat))
        {
            return stat.Value;
        }

        Debug.LogWarning($"{this} Stat not found for ({attr})");
        return 0;
    }
    public int GetStatValue(CardType type, StatAspect aspect)
    {
        if (CardTypeStats.TryGetValue((type, aspect), out var stat))
        {
            return stat.Value;
        }

        Debug.LogWarning($"{this} Stat not found for ({type}, {aspect})");
        return 0;
    }
    public int GetStatValue(CardClass cls, StatAspect aspect)
    {
        if (CardClassStats.TryGetValue((cls, aspect), out var stat))
        {
            return stat.Value;
        }

        Debug.LogWarning($"{this} Stat not found for ({cls}, {aspect})");
        return 0;
    }
    public int GetStatValue(CardIdentity element, StatAspect aspect)
    {
        if (CardIdentityStats.TryGetValue((element, aspect), out var stat))
        {
            return stat.Value;
        }

        Debug.LogWarning($"{this} Stat not found for ({element}, {aspect})");
        return 0;
    }
}
