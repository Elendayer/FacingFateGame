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
        public List<string> deckCardIDs = new List<string>();  // Populate with card IDs

        [Header("Entity Stats")]
        public EntityStats entityStats;

        [Header("Entity Gameplay References")]
        public EntityVisualScript EntityVisual;
        public MeshFilter EntityModel;
        public EntityOnMap EntityOnMap;

        [Header("Audio")]
        [Tooltip("Optional, empty = silent")]
        public EntityAudioProfile audioProfile;



        public List<IEntityModifier> GetActiveModifiers()
        {
            return entityModifiers.Where(m => m != null && !m.IsExpired).ToList();
        }

        public virtual void StartUp()
        {
            EntityVisual = GetComponentInChildren<EntityVisualScript>();
            EntityVisual.EntityScript = this;

            EntityOnMap = GetComponentInChildren<EntityOnMap>();

            entityStats = new();
            entityStats.StartUp(this);

            EntityOnMap.Startup();
        }
        #region Events

        #endregion

        #region Modifier System

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

        public void RemoveAllModifiers()
        {
            entityModifiers.Clear();
        }
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
                case GameplayCondition.isDamaged: c = entityStats.CurrentHealth < entityStats.MaxHealth; break;
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
        #endregion

        public virtual void StartTurn()
        {
            EntityVisual.HighlightTurn();

            ActionQueueUtility.EnqueueAction(() =>
            {
                entityStats.TickAllStats();
            });
        }
        public virtual void EndTurn()
        {
            EntityVisual.ClearHighlight ();
        }

        public virtual void DrawCards(int toDraw) { }

        public virtual void DiscardCards(int toDiscard) { }
    }
    public enum EntityAttributeEnum
    {
        Strength,
        Dexterity,
        Wisdom,
        Intelligence,
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