using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace facingfate
{

    public class EntityScript : MonoBehaviour
    {
        [Header("Main Enity Settings")]
        public EntityAffiliation entityAffiliation = EntityAffiliation.Neutral;

        [Header("Deck Settings")]
        [SerializeField]
        public List<int> deckCardIDs = new List<int>();  // Populate with card IDs

        [Header("Entity Stats")]
        public EntityStats entityStats;

        [Header("Entity Gameplay References")]
        private EntityVisualScript EntityVisual;

        [Header("Entity on Map Reference")]
        private EntityOnMap entityOnMap;

        public virtual void StartUp()
        {


            EntityVisual = GetComponentInChildren<EntityVisualScript>();
            entityOnMap = GetComponentInChildren<EntityOnMap>();

            entityStats = new();
            entityStats.StartUp(this);

            entityOnMap.Startup();

            AddListeners();
        }
        private void AddListeners()
        {
            GameEvents.OnGameplayReference += TriggerAnimation;
        }
        private void TriggerAnimation(ToSendTriggerReference triggerRef)
        {
            var checkTrigger = new RelevantTriggerCheck
            {
                OnTriggerReference = new List<GameplayRef> { GameplayRef.onBurn, GameplayRef.onDamage, GameplayRef.onBleed },
                CheckType = CheckEntityType.User,
                CheckEntity = this
            };

            if (GameEvents.CheckIfRelevantTrigger(triggerRef, checkTrigger))
            {
                Debug.Log("Playing Effect Animation for " + this.name);
                PlayEffectAnimation(triggerRef);
            }
        }
        public void PlayEffectAnimation(ToSendTriggerReference triggerRef)
        {
            foreach (GameplayRef gRef in triggerRef.OnTriggerReference)
            {
                switch (gRef)
                {
                    default: break;
                    case GameplayRef.onBurn:
                        CreateFX("BurnEffect");
                        break;
                    case GameplayRef.onDamage:
                        CreateFX("DamageEffect");
                        break;
                    case GameplayRef.onBleed:
                        CreateFX("BloodEffect");
                        break;
                }
            }
        }

        private void CreateFX(string name)
        {
            GameObject effectObj;

            effectObj = AssetManager.Instance.GetEffectPrefab(name);
            var CreatedObj = Instantiate(effectObj, EntityVisual.transform);
        }

        [Header("Modifier System")]
        [SerializeField]
        private readonly List<IEntityModifier> entityModifiers = new();
        public List<string> modifierNames;

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
                    if (existing is EntityModifier existingMod && modifier is EntityModifier newMod)
                    {
                        existingMod.BaseValue += newMod.BaseValue;
                    }
                    else
                    {
                        entityModifiers.Add(modifier);
                    }
                    break;

                case ModifierMergeStrategy.RefreshDurationAndMerge:
                    if (existing is EntityModifier existingRefresh && modifier is EntityModifier newRefresh)
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
                    if (existing is EntityModifier existingRefreshDuration && modifier is EntityModifier newRefreshDuration)
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

            ToSendTriggerReference OnApplyTrigger = new ToSendTriggerReference
            {
                OnTriggerReference = new() { GameplayRef.onModifierApplied },
                UserEntity = this,
                AffectedEntities = new List<EntityScript> { this },
                CardData = null,
                Throughput = modifier.BaseValue
            };

            modifier.OnApply_ActionTrigger(OnApplyTrigger);
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
            if (entityModifiers == null || entityModifiers.Count == 0)
                return (false, null);

            var modifier = entityModifiers.FirstOrDefault(m =>
                m != null &&
                m.ToTriggerGameplayRefs != null &&
                m.ToTriggerGameplayRefs.Contains(reference) &&
                !m.IsExpired
            );

            return (modifier != null, modifier);
        }

        public bool HasModifier(string name)
            => entityModifiers.Any(m => m.ModifierName == name && !m.IsExpired);

        public bool HasCondition(GameplayCondition condititon)
        {
            bool c = false;

            switch (condititon)
            {
                case GameplayCondition.isDamaged: c = entityStats.CurrentHealth < entityStats.MaxHealth.Value(); break;
            }
            return c;
        }

        public bool ActivateModifierWithReferenceOnce(GameplayRef reference, ToSendTriggerReference triggerRef, bool consumeCharges = false)
        {
            var modifier = entityModifiers.FirstOrDefault(m => m.ToTriggerGameplayRefs.Contains(reference) && !m.IsExpired);
            if (modifier != null)
            {
                modifier.OnManuel_ActionTrigger(triggerRef, consumeCharges);
                return true;
            }
            return false;
        }

        public IEntityModifier GetModifierByName(string name)
            => entityModifiers.FirstOrDefault(m => m.ModifierName == name && !m.IsExpired);

        private void OnValidate()
        {
            modifierNames.Clear();
            modifierNames.AddRange(entityModifiers.Select(c => c.ModifierName));
        }

        public virtual void StartTurn()
        {
            entityStats.CurrentStamina = entityStats.MaxStamina.Value();

            ActionQueueUtility.EnqueueAction(() =>
            {
                entityStats.TickAllStats();
            });
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

    public enum GameplayCondition
    {
        isDamaged,
    }
}