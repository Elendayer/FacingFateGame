using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityScript : MonoBehaviour
{
    [Header("Main Enity Settings")]
    public EntityAffiliation entityAffiliation = EntityAffiliation.Neutral;

    [Header("Deck Settings")]
    public List<int> deckCardIDs = new List<int>();  // Populate with card IDs

    [Header("Base Stats")]
    public Stat MaxHealth = new() { Value = 100 };
    public Stat CurrentHealth = new() { Value = 100 };
    public Stat MaxStamina = new() { Value = 10 };
    public Stat CurrentStamina = new() { Value = 10 };

    public Stat Block = new() { Value = 0 };
    public Stat Armour = new() { Value = 0 };

    public Dictionary<EntityAttributeEnum, Stat> EntityAttributes = new();

    public Dictionary<(CardType, StatAspect), Stat> CardTypeStats = new();
    public Dictionary<(CardIdentity, StatAspect), Stat> CardElementStats = new();
    public Dictionary<(CardClass, StatAspect), Stat> CardClassStats = new();

    private EntityVisualScript EntityVisual;

    public virtual void StartUp()
    {
        EntityVisual = GetComponentInChildren<EntityVisualScript>();

        // Fill EntityAttributes
        foreach (EntityAttributeEnum attr in Enum.GetValues(typeof(EntityAttributeEnum)))
        {
            EntityAttributes.Add(attr, new Stat() { Value = 2 });
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

        AddListeners();
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
    public int GetStatValue(CardIdentity element, StatAspect aspect)
    {
        if (CardElementStats.TryGetValue((element, aspect), out var stat))
        {
            return stat.GetFinalValue();
        }

        Debug.LogWarning($"{this.name} Stat not found for ({element}, {aspect})");
        return 0;
    }

    private void AddListeners()
    {
        GameEvents.Subscribe(gameplayRef.onBurn, GetInstanceID(), TriggerAnimation);
        GameEvents.Subscribe(gameplayRef.onDamage, GetInstanceID(), TriggerAnimation);
    }
    private void TriggerAnimation(TriggerRef triggerRef)
    {
        GameObject effectObj;
        foreach (gameplayRef gRef in triggerRef.References)
        {
            switch (gRef)
            {
                default: break;
                case gameplayRef.onBurn:
                    effectObj = AssetManager.Instance.GetEffectPrefab("BurnEffect");
                    Debug.Log("Tried to Add Burn Effect");
                    Instantiate(effectObj, EntityVisual.transform);
                    break;

                case gameplayRef.onDamage:
                    effectObj = AssetManager.Instance.GetEffectPrefab("DamageEffect");
                    Debug.Log("Tried to Add Damage Effect");
                    Instantiate(effectObj, EntityVisual.transform);
                    break;
            }
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

public enum EntityAffiliation
{
    Neutral,
    Player,
    Enemy
}