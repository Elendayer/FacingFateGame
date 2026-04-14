using System;
using System.Collections.Generic;
using UnityEngine;

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

            switch (EffectData)
            {
                case GroundEffect_Ref_EntityData: GameEvents.OnGameplayReference += OnGameplayRef; break;
                case GroundEffect_Ref_StatData: GameEvents.OnGameplayReference += OnGameplayRef; break;
                case GroundEffect_Ref_Effect: GameEvents.OnGameplayReference += OnGameplayRef; break;
            }
        }

        private void OnDestroy()
        {
            GameEvents.OnGameplayReference -= OnGameplayRef;
        }

        // =============================
        // Trigger-Based Application
        // =============================
        private void OnGameplayRef(ToSendTriggerReference trigger)
        {
            if (EffectData == null) return;

            // Tick duration only on creator's turn
            if (trigger.UserEntity == EffectData.CardData.Owner &&
                trigger.OnTriggerReference.Contains(GameplayRef.onTurnStart))
            {
                if (EffectData.Duration < 9999)
                    EffectData.Duration--;

                if (EffectData.Duration == 0)
                {
                    if (EffectData.RemoveOnEnd)
                    {
                        RemoveEffectFromAll();
                        return;
                    }
                    Destroy(gameObject);
                    return;
                }           
            }

            // Apply effect via trigger
            if (GameEvents.CheckIfRelevantTrigger(trigger, EffectData.RelevantTrigger))
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
            {
                affectedEntities.Add(target);
            }
            switch (EffectData)
            {
                case GroundEffect_Enter_EntityData entityData:
                    {
                        entityData.OnEnter?.Invoke(entityData.Modifier, target);
                    }
                    break;

                case GroundEffect_Enter_StatData statData:
                    {
                        statData.OnEnter?.Invoke(statData.Modifier, target);
                    }
                    break;
                case GroundEffect_Enter_Effect effectData:
                    {
                        effectData.OnEnter?.Invoke(target);
                    }
                    break;
            }
        }

        private void RemoveEffect(EntityScript target)
        {
            switch (EffectData)
            {
                case GroundEffect_Enter_EntityData entityData:
                    {
                        entityData.OnExit?.Invoke(entityData.Modifier, target);
                        if (EffectData.RemoveOnExit)
                        {
                            RemoveEffect(target);
                        }
                    }
                    break;

                case GroundEffect_Enter_StatData statData:
                    {
                        statData.OnExit?.Invoke(statData.Modifier, target);
                        if (EffectData.RemoveOnExit)
                        {
                            RemoveEffect(target);
                        }
                    }
                    break;
                case GroundEffect_Enter_Effect effectData:
                    {
                        effectData.OnExit?.Invoke(target);
                    }
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
        public RelevantTriggerCheck RelevantTrigger;
        public int Duration;
        public bool RemoveOnExit = true;
        public bool RemoveOnEnd = true;

    }


    // =============================
    // GroundEffects Enter / Exit Data
    // =============================
    [System.Serializable]
    public class GroundEffect_Enter_EntityData : GroundEffectDataBase
    {
        public EntityModifier Modifier;
        public Action<EntityModifier, EntityScript> OnEnter;
        public Action<EntityModifier, EntityScript> OnExit;

        public GroundEffect_Enter_EntityData(CardData cardData, RelevantTriggerCheck relevantTrigger, int duration, bool removeOnExit, bool removeOnEnd, EntityModifier modifier, Action<EntityModifier, EntityScript> onEnter, Action<EntityModifier, EntityScript> onExit)
        {
            CardData = cardData;
            RelevantTrigger = relevantTrigger;
            Duration = duration;
            RemoveOnExit = removeOnExit;
            RemoveOnEnd = removeOnEnd;
            Modifier = modifier;
            OnEnter = onEnter;
            OnExit = onExit;
        }
    }

    [System.Serializable]
    public class GroundEffect_Enter_StatData : GroundEffectDataBase
    {
        public StatModifier Modifier;
        public Action<StatModifier, EntityScript> OnEnter;
        public Action<StatModifier, EntityScript> OnExit;

        public GroundEffect_Enter_StatData(CardData cardData, RelevantTriggerCheck relevantTrigger, int duration, bool removeOnExit, bool removeOnEnd, StatModifier modifier, Action<StatModifier, EntityScript> onEnter, Action<StatModifier, EntityScript> onExit)
        {
            CardData = cardData;
            RelevantTrigger = relevantTrigger;
            Duration = duration;
            RemoveOnExit = removeOnExit;
            RemoveOnEnd = removeOnEnd;
            Modifier = modifier;
            OnEnter = onEnter;
            OnExit = onExit;
        }
    }

    public class GroundEffect_Enter_Effect : GroundEffectDataBase
    {
        public StatModifier Modifier;
        public Action< EntityScript> OnEnter;
        public Action< EntityScript> OnExit;
        public GroundEffect_Enter_Effect(CardData cardData, RelevantTriggerCheck relevantTrigger, int duration, bool removeOnExit, bool removeOnEnd, Action<EntityScript> onEnter, Action<EntityScript> onExit)
        {
            CardData = cardData;
            RelevantTrigger = relevantTrigger;
            Duration = duration;
            RemoveOnExit = removeOnExit;
            RemoveOnEnd = removeOnEnd;
            OnEnter = onEnter;
            OnExit = onExit;
        }
    }
    // =============================
    // GroundEffects Reference Triggered
    // =============================
    [System.Serializable]
    public class GroundEffect_Ref_EntityData : GroundEffectDataBase
    {
        public EntityModifier Modifier;
        public Action<EntityModifier, EntityScript> OnEnter;
        public Action<EntityModifier, EntityScript> OnExit;
        public Action<StatModifier, EntityScript> OnRef;

        public GroundEffect_Ref_EntityData(CardData cardData, RelevantTriggerCheck relevantTrigger, int duration, EntityModifier modifier, Action<StatModifier, EntityScript> onRef, bool removeOnExit = false, bool removeOnEnd = false)
        {
            CardData = cardData;
            RelevantTrigger = relevantTrigger;
            Duration = duration;
            RemoveOnExit = removeOnExit;
            RemoveOnEnd = removeOnEnd;
            Modifier = modifier;
            OnRef = onRef;
        }
    }

    [System.Serializable]
    public class GroundEffect_Ref_StatData : GroundEffectDataBase
    {
        public StatModifier Modifier;
        public Action<StatModifier, EntityScript> OnEnter;
        public Action<StatModifier, EntityScript> OnExit;
        public Action<StatModifier, EntityScript> OnRef;

        public GroundEffect_Ref_StatData(CardData cardData, RelevantTriggerCheck relevantTrigger, int duration, StatModifier modifier, Action<StatModifier, EntityScript> onRef, bool removeOnExit =false, bool removeOnEnd = false )
        {
            CardData = cardData;
            RelevantTrigger = relevantTrigger;
            Duration = duration;
            RemoveOnExit = removeOnExit;
            RemoveOnEnd = removeOnEnd;
            Modifier = modifier;
            OnRef = onRef;
        }
    }

    public class GroundEffect_Ref_Effect : GroundEffectDataBase
    {
        public StatModifier Modifier;
        public Action<EntityScript> OnRef;
        public GroundEffect_Ref_Effect(CardData cardData, RelevantTriggerCheck relevantTrigger, int duration, Action<EntityScript> onRef, bool removeOnExit = false, bool removeOnEnd = false)
        {
            CardData = cardData;
            RelevantTrigger = relevantTrigger;
            Duration = duration;
            RemoveOnExit = removeOnExit;
            RemoveOnEnd = removeOnEnd;
            OnRef = onRef;
        }
    }
}
