using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityScript : MonoBehaviour
{
    [Header("Main Enity Settings")]
    public EntityAffiliation entityAffiliation = EntityAffiliation.Neutral;

    [Header("Deck Settings")]
    public List<int> deckCardIDs = new List<int>();  // Populate with card IDs

    [Header("Entity Stats")]
    public EntityStats entityStats;

    [Header("Entity Gameplay References")]
    private EntityVisualScript EntityVisual;

    public virtual void StartUp()
    {
        EntityVisual = GetComponentInChildren<EntityVisualScript>();
        
        entityStats = new();
        entityStats.StartUp(this);
        //entityStats.PostStartUp();

        AddListeners();
    }

    private void AddListeners()
    {
        GameEvents.Subscribe(GameplayRef.onTurnStart, GetInstanceID(), GameEvents_OnTurnStart);

        GameEvents.Subscribe(GameplayRef.onBurn, GetInstanceID(), TriggerAnimation);
        GameEvents.Subscribe(GameplayRef.onDamage, GetInstanceID(), TriggerAnimation);
    }
    private void TriggerAnimation(TriggerRef triggerRef)
    {
        GameObject effectObj;
        foreach (GameplayRef gRef in triggerRef.References)
        {
            switch (gRef)
            {
                default: break;
                case GameplayRef.onBurn:
                    effectObj = AssetManager.Instance.GetEffectPrefab("BurnEffect");
                    Debug.Log("Tried to Add Burn Effect");
                    Instantiate(effectObj, EntityVisual.transform);
                    break;

                case GameplayRef.onDamage:
                    effectObj = AssetManager.Instance.GetEffectPrefab("DamageEffect");
                    Debug.Log("Tried to Add Damage Effect");
                    Instantiate(effectObj, EntityVisual.transform);
                    break;
            }
        }
    }

    private readonly List<IEntityModifier> entityModifiers = new();

    public void AddModifier(IEntityModifier modifier, ModifierMergeStrategy strategy = ModifierMergeStrategy.Override)
    {
        var existing = entityModifiers.FirstOrDefault(m => m.ModifierName == modifier.ModifierName);

        switch (strategy)
        {
            case ModifierMergeStrategy.AddUnique:
                entityModifiers.Add(modifier);
                break;

            case ModifierMergeStrategy.Override:
                if (existing != null) entityModifiers.Remove(existing);
                entityModifiers.Add(modifier);
                break;

            case ModifierMergeStrategy.Merge:
                if (existing is StatModifier existingMod && modifier is StatModifier newMod)
                {
                    existingMod.BaseValue += newMod.BaseValue;
                }
                else
                {
                    entityModifiers.Add(modifier);
                }
                break;
            case ModifierMergeStrategy.RefreshDurationAndMerge:
                if (existing is StatModifier existingRefresh && modifier is StatModifier newRefresh)
                {
                    existingRefresh.BaseValue += newRefresh.BaseValue;
                    existingRefresh.Duration = Math.Max(existingRefresh.GetRemainingDuration(), newRefresh.GetRemainingDuration());
                }
                else
                {
                    entityModifiers.Add(modifier);
                }
                break;

            case ModifierMergeStrategy.RefreshDurationAndOverride:
                if (existing is StatModifier existingRefreshDuration && modifier is StatModifier newRefreshDuration)
                {
                    existingRefreshDuration.BaseValue = Mathf.Max(existingRefreshDuration.BaseValue, newRefreshDuration.BaseValue);
                    existingRefreshDuration.Duration = Math.Max(existingRefreshDuration.GetRemainingDuration(), newRefreshDuration.GetRemainingDuration());
                }
                else
                {
                    entityModifiers.Add(modifier);
                }
                break;
        }
        modifier.AddListener();
    }
   
    private void GameEvents_OnTurnStart(TriggerRef trigger)
    {
        if (trigger.UserId == this.GetInstanceID())
        {
            entityStats.CurrentStamina.AddModifier(new StatModifier(entityStats.MaxStamina.Value, ModifierScaling.Flat, name: "BaseValue"), ModifierMergeStrategy.Override);
        }
    }

    public void RemoveModifier(IEntityModifier modifier) => entityModifiers.Remove(modifier);
    public void AddOrReplaceModifier(IEntityModifier modifier)
    {
        var existing = entityModifiers.FirstOrDefault(m => m.ModifierName == modifier.ModifierName);
        if (existing != null) entityModifiers.Remove(existing);
        entityModifiers.Add(modifier);
    }
    public (bool found, IEntityModifier modifier) HasReference(GameplayRef reference)
    {
        var modifier = entityModifiers
            .FirstOrDefault(m => m.To_TriggerGameplayRefs.Contains(reference) && !m.IsExpired);

        return (modifier != null, modifier);
    }

    public bool HasModifier(string name)
        => entityModifiers.Any(m => m.ModifierName == name && !m.IsExpired);

    public bool ActivateModifierWithReferenceOnce(GameplayRef reference, TriggerRef triggerRef)
    {
        var modifier = entityModifiers.FirstOrDefault(m => m.To_TriggerGameplayRefs.Contains(reference) && !m.IsExpired);
        if (modifier != null)
        {
            modifier.OnRefEventTriggered(triggerRef);
            return true;
        }
        return false;
    }

    public IEntityModifier GetModifierByName(string name)
        => entityModifiers.FirstOrDefault(m => m.ModifierName == name && !m.IsExpired);
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