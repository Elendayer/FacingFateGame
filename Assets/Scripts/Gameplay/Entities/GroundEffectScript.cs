using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace facingfate
{
    public class GroundEffectScript : MonoBehaviour
    {
        [Header("References")]
        public Collider groundEffectCollider;

        [Header("Ground Effect (Single)")]
        public GroundEffectDataBase EffectData;

        private readonly HashSet<EntityScript> affectedEntities = new();

        private void Awake()
        {
            if (!groundEffectCollider)
                groundEffectCollider = GetComponent<Collider>();

            GameEvents.OnGameplayReference += OnGameplayRef;
        }

        private void OnDestroy()
        {
            GameEvents.OnGameplayReference -= OnGameplayRef;
        }

        // =============================
        // Trigger-Based Application
        // =============================
        private void OnGameplayRef(TriggerRef trigger)
        {
            if (EffectData == null) return;

            // Tick duration only on creator's turn
            if (trigger.UserEntity == EffectData.CardData.Owner &&
                trigger.OnTriggerReference.Contains(GameplayRef.onTurnStart))
            {
                if (EffectData.Duration < 9999)
                    EffectData.Duration--;

                if (EffectData.Duration <= 0)
                {
                    RemoveEffectFromAll();
                    Destroy(gameObject);
                    return;
                }
            }

            // Apply effect via trigger
            if (GameEvents.CheckIfRelevantTrigger(trigger, EffectData.OnRef_Trigger))
            {
                foreach (var target in trigger.AffectedEntities)
                {
                    if (!TargetingUtility.IsTargetValid(EffectData.CardData, target))
                        continue;

                    ApplyEffect(target);
                }
            }
        }

        // =============================
        // Area-Based Application
        // =============================
        private void OnTriggerEnter(Collider other)
        {
            if (EffectData == null) return;

            EntityScript target = other.GetComponent<EntityScript>();
            if (!target || !TargetingUtility.IsTargetValid(EffectData.CardData, target))
                return;

            ApplyEffect(target);
        }

        private void OnTriggerExit(Collider other)
        {
            if (EffectData == null || !EffectData.RemoveOnExit) return;

            EntityScript target = other.GetComponent<EntityScript>();
            if (!target || !affectedEntities.Contains(target))
                return;

            RemoveEffect(target);
        }

        // =============================
        // Modifier Application
        // =============================
        private void ApplyEffect(EntityScript target)
        {
            if (!affectedEntities.Contains(target))
                affectedEntities.Add(target);

            switch (EffectData)
            {
                case GroundEffectEntityData entityData:
                    entityData.ApplyModifier?.Invoke(entityData.Modifier, target);
                    break;

                case GroundEffectStatData statData:
                    statData.ApplyModifier?.Invoke(statData.Modifier, target);
                    break;
            }
        }

        private void RemoveEffect(EntityScript target)
        {
            switch (EffectData)
            {
                case GroundEffectEntityData entityData:
                    entityData.RemoveModifier?.Invoke(entityData.Modifier, target);
                    break;

                case GroundEffectStatData statData:
                    statData.RemoveModifier?.Invoke(statData.Modifier, target);
                    break;
            }
        }

        private void RemoveEffectFromAll()
        {
            if (!EffectData.RemoveOnEnd) return;

            foreach (var target in affectedEntities)
            {
                RemoveEffect(target);
            }

            affectedEntities.Clear();
        }
    }

    // =============================
    // Base Ground Effect Data
    // =============================
    [System.Serializable]
    public abstract class GroundEffectDataBase
    {
        public CardData CardData;
        public TriggerRef OnRef_Trigger;
        public int Duration;
        public bool RemoveOnExit = true;
        public bool RemoveOnEnd = true;

    }

    [System.Serializable]
    public class GroundEffectEntityData : GroundEffectDataBase
    {
        public EntityModifier Modifier;
        public Action<EntityModifier, EntityScript> ApplyModifier;
        public Action<EntityModifier, EntityScript> RemoveModifier;

        public GroundEffectEntityData(CardData cardData, TriggerRef triggerRef, int duration, bool removeOnExit, bool removeOnEnd, EntityModifier modifier, Action<EntityModifier, EntityScript> applyModifier, Action<EntityModifier, EntityScript> removeModifier)
        {
            CardData = cardData;
            OnRef_Trigger = triggerRef;
            Duration = duration;
            RemoveOnExit = removeOnExit;
            RemoveOnEnd = removeOnEnd;
            Modifier = modifier;
            ApplyModifier = applyModifier;
            RemoveModifier = removeModifier;
        }
    }

    [System.Serializable]
    public class GroundEffectStatData : GroundEffectDataBase
    {
        public StatModifier Modifier;
        public Action<StatModifier, EntityScript> ApplyModifier;
        public Action<StatModifier, EntityScript> RemoveModifier;

        public GroundEffectStatData(CardData cardData, TriggerRef triggerRef, int duration, bool removeOnExit, bool removeOnEnd, StatModifier modifier, Action<StatModifier, EntityScript> applyModifier, Action<StatModifier, EntityScript> removeModifier)
        {
            CardData = cardData;
            OnRef_Trigger = triggerRef;
            Duration = duration;
            RemoveOnExit = removeOnExit;
            RemoveOnEnd = removeOnEnd;
            Modifier = modifier;
            ApplyModifier = applyModifier;
            RemoveModifier = removeModifier;
        }
    }
}
