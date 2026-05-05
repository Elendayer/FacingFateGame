using System;
using System.Collections.Generic;
using UnityEngine;

namespace facingfate
{
    public interface IEntityModifier
    {
        string ModifierName { get; }
        string Description { get; set; }
        Sprite Icon { get; set; }
        EntityScript Owner { get; set; }

        int BaseValue { get; set; }
        int Duration { get; set; }
        bool IsExpired { get; }
        int Charges { get; set; }

        ModifierMergeStrategy ModifierMergeStrategy { get; set; }

        // Triggering
        List<GameplayRef> ToTriggerGameplayRefs { get; }
        RelevantTriggerCheck OnRef_Trigger { get; }

        void AddListener();
        void OnRef_ActionCall(ToSendTriggerReference triggerReference);
        void OnApply_ActionTrigger(ToSendTriggerReference triggerReference);
        void onRemove_ActionTrigger(ToSendTriggerReference triggerReference);
        void OnManuel_ActionTrigger(ToSendTriggerReference triggerReference, bool consumeCharges = false);
    }

    [System.Serializable]
    public class EntityModifier : IEntityModifier
    {
        //Main
        public string ModifierName { get; set; }
        public string Description { get; set; }
        public Sprite Icon { get; set; }

        public EntityScript Owner { get; set; }
        public int BaseValue
        {
            get
            {
                if (DynamicValueFunc != null)
                    return DynamicValueFunc.Invoke();
                return StaticValue ?? 0;
            }
            set
            {
                if (DynamicValueFunc != null)
                    throw new InvalidOperationException("Cannot set BaseValue when using dynamicValueFunc.");
                StaticValue = value;
            }
        }
        private int? StaticValue = 0;
        private Func<int> DynamicValueFunc = null;

        // Duration in number of triggers
        public int Duration { get; set; }
        public bool IsExpired => Duration <= 0;

        public int Charges { get; set; } = 0;
        public bool IsSpend => Charges <= 0;

        public ModifierMergeStrategy ModifierMergeStrategy { get; set; } = ModifierMergeStrategy.RefreshDurationAndMerge;

        // Triggering
        public List<GameplayRef> ToTriggerGameplayRefs { get; set; }

        public RelevantTriggerCheck OnRef_Trigger { get; set; }
        public RelevantTriggerCheck OnRemove_Trigger { get; set; }
        public RelevantTriggerCheck OnApply_Trigger { get; set; }

        // Action Target Type
        public ActionTargetType TargetType { get; set; } = ActionTargetType.User;
        // Action to perform on trigger
        public Action<EntityScript, CardData, int> OnRef_Action;
        public Action<EntityScript, CardData, int> OnApply_Action;
        public Action<EntityScript, CardData, int> OnRemove_Action;

        // Constructor
        public EntityModifier
        (
        string modifierName,
        EntityScript owner,
        string description = null,
        Sprite icon = null,
        int baseValue = 0,
        ModifierMergeStrategy modifierMergeStrategy = ModifierMergeStrategy.RefreshDurationAndMerge,
        List<GameplayRef> toTriggerRefs = null,
        RelevantTriggerCheck onRef_Trigger = new RelevantTriggerCheck(),
        RelevantTriggerCheck onApply_Trigger = new RelevantTriggerCheck(),
        RelevantTriggerCheck onRemove_Trigger = new RelevantTriggerCheck(),
        int duration = 99999,
        int charges = 99999,
        ActionTargetType actionTargetType = ActionTargetType.User,
        Action<EntityScript, CardData, int> onRef_Action = null,
        Action<EntityScript, CardData, int> onApply_Action = null,
        Action<EntityScript, CardData, int> onRemove_Action = null
        )
        {
            ModifierName = modifierName;
            Description = description ?? "";
            Icon = icon;
            Owner = owner;
            BaseValue = baseValue;
            ModifierMergeStrategy = modifierMergeStrategy;
            ToTriggerGameplayRefs = toTriggerRefs;
            OnRef_Trigger = onRef_Trigger;
            OnApply_Trigger = onApply_Trigger;
            OnRemove_Trigger = onRemove_Trigger;
            Duration = duration;
            Charges = charges;
            TargetType = actionTargetType;
            OnRef_Action = onRef_Action;
            OnApply_Action = onApply_Action;
            OnRemove_Action = onRemove_Action;
        }

        // Methods
        public void AddListener()
        {
            GameEvents.OnGameplayReference += OnRef_ActionCall;
        }
        public int GetRemainingDuration() => Duration;


        public void OnRef_ActionCall(ToSendTriggerReference trigger)
        {
            // Check if Trigger is Relevant
            if (GameEvents.CheckIfRelevantTrigger(trigger, OnRef_Trigger))
            {
                //Activate Effect
                switch (TargetType)
                {
                    case ActionTargetType.User:
                        {
                            ActionQueueUtility.EnqueueAction(() =>
                            {
                                //OnRef_Action?.Invoke(OnRef_Trigger.CheckEntity, trigger.CardData, trigger.Throughput);
                                OnRef_Action?.Invoke(OnRef_Trigger.CheckEntity, trigger.CardData, BaseValue);

                                // Trigger further GameplayRefs
                                if (ToTriggerGameplayRefs != null && ToTriggerGameplayRefs.Count > 0)
                                {
                                    GameEvents.TriggerRefEvent(new ToSendTriggerReference(ToTriggerGameplayRefs, trigger.UserEntity, trigger.AffectedEntities, null, BaseValue));
                                }
                            });
                        }
                        break;

                    case ActionTargetType.Affected:
                        {
                            foreach (EntityScript entity in trigger.AffectedEntities)
                            {
                                ActionQueueUtility.EnqueueAction(() =>
                                {
                                    //OnRef_Action?.Invoke(entity, trigger.CardData, trigger.Throughput);
                                    OnRef_Action?.Invoke(entity, trigger.CardData, BaseValue);

                                    // Trigger further GameplayRefs
                                    if (ToTriggerGameplayRefs != null && ToTriggerGameplayRefs.Count > 0)
                                    {
                                        GameEvents.TriggerRefEvent(new ToSendTriggerReference(ToTriggerGameplayRefs, trigger.UserEntity, trigger.AffectedEntities, null, BaseValue));
                                    }
                                });
                            }
                        }
                        break;
                }

                // Consume Charge if it has Charges
                if (Charges < 9999)
                {
                    Charges--;
                }

                if (Duration < 9999)
                {
                    Duration--;
                }

                if (IsExpired || IsSpend)
                {
                    OnRemove();
                    return;
                }
            }

            // If the trigger is not relevant for the modifier, we still want to tick duration if its a turn start or end trigger, as well as check for expiration
            if (OnRef_Trigger.OnTriggerReference == null || OnRef_Trigger.OnTriggerReference.Count == 0)
            {
                return;
            }
            
            // Tick Duration if its not triggered at TurnStart and has Duration
            if (!OnRef_Trigger.OnTriggerReference.Contains(GameplayRef.onTurnStart))
            {
                RelevantTriggerCheck FallBackEndTurnTrigger = new RelevantTriggerCheck()
                {
                    OnTriggerReference = new List<GameplayRef>() { GameplayRef.onTurnEnd },
                    CheckType = CheckEntityType.User,
                    CheckEntity = Owner,
                };

                if (GameEvents.CheckIfRelevantTrigger(trigger, FallBackEndTurnTrigger))
                {
                    if (Duration < 9999)
                    {
                        Duration--;
                    }
                }

                if (IsExpired || IsSpend)
                {
                    OnRemove();
                }
            }
        }

        public void OnApply_ActionTrigger(ToSendTriggerReference trigger)
        {
            ActionQueueUtility.EnqueueAction(() =>
            {
                OnApply_Action?.Invoke(trigger.UserEntity, trigger.CardData, trigger.Throughput);
            });
        }
        public void onRemove_ActionTrigger(ToSendTriggerReference trigger)
        {
            ActionQueueUtility.EnqueueAction(() =>
            {
                OnRemove_Action?.Invoke(trigger.UserEntity, trigger.CardData, trigger.Throughput);
            });
        }
        public void OnManuel_ActionTrigger(ToSendTriggerReference trigger, bool consumeCharges = false)
        {
            ActionQueueUtility.EnqueueAction(() =>
            {
                Debug.Log($"EntityModifier {ModifierName} manually triggered action. By {trigger.UserEntity} at {trigger.AffectedEntities[0]}");
                OnRef_Action?.Invoke(trigger.UserEntity, trigger.CardData, trigger.Throughput);

                GameEvents.TriggerRefEvent(new ToSendTriggerReference(ToTriggerGameplayRefs, trigger.UserEntity, trigger.AffectedEntities, trigger.CardData, trigger.Throughput));
            });


            if (consumeCharges)
            {
                if (Charges < 99999)
                {
                    Duration--;
                    Debug.Log($"FunctionModifier duration: {Duration}");
                }
            }
            if (IsExpired || IsSpend)
            {

                    OnRemove();
            }
        }
        public void OnRemove()
        {
            ToSendTriggerReference RemovalTrigger = new ToSendTriggerReference(OnRemove_Trigger.OnTriggerReference, Owner, new List<EntityScript>() { Owner }, null, 0);

            onRemove_ActionTrigger(RemovalTrigger);

            foreach (var Reference in OnRef_Trigger.OnTriggerReference)
            {
                GameEvents.OnGameplayReference -= OnRef_ActionCall;
            }
            Debug.Log($"Modifier {ModifierName} removed from {Owner}. Duration: {Duration}, Charges: {Charges}");
            Owner.RemoveModifier(this);
        }

        public EntityModifier CloneOverrideFromData(
            CardData cd,
            ThroughputSource source,
            EntityScript user)
        {
            int baseValue = source switch
            {
                ThroughputSource.Damage => cd.Damage,
                ThroughputSource.Heal => cd.Healing,
                ThroughputSource.Power => cd.Power,
                _ => 0
            };
            return new EntityModifier
            (
                modifierName: ModifierName,
                description: Description,
                icon: Icon,
                owner: cd.Owner,
                baseValue: baseValue,
                toTriggerRefs: ToTriggerGameplayRefs,
                onRef_Trigger: OnRef_Trigger,
                onApply_Trigger: OnApply_Trigger,
                onRemove_Trigger: OnRemove_Trigger,
                duration: cd.Duration,
                charges: cd.Charges,
                actionTargetType: TargetType,
                modifierMergeStrategy: ModifierMergeStrategy,
                onRef_Action: OnRef_Action,
                onApply_Action: OnApply_Action,
                onRemove_Action: OnRemove_Action
            );
        }
        public EntityModifier CloneDefaults(
          CardData cd,
          ThroughputSource source,
          EntityScript user)
        {
            int baseValue = source switch
            {
                ThroughputSource.Damage => cd.Damage,
                ThroughputSource.Heal => cd.Healing,
                ThroughputSource.Power => cd.Power,
                _ => 0
            };
            return new EntityModifier
            (
                modifierName: ModifierName,
                description: Description,
                icon: Icon,
                owner: user,
                baseValue: baseValue,
                toTriggerRefs: ToTriggerGameplayRefs,
                onRef_Trigger: OnRef_Trigger,
                onApply_Trigger: OnApply_Trigger,
                onRemove_Trigger: OnRemove_Trigger,
                duration: Duration,
                charges: Charges,
                actionTargetType: TargetType,
                modifierMergeStrategy: ModifierMergeStrategy,
                onRef_Action: OnRef_Action,
                onApply_Action: OnApply_Action,
                onRemove_Action: OnRemove_Action
            );
        }
        public enum ActionTargetType
        {
            User,
            Affected,
        }
    }

    public enum ThroughputSource
    {
        Damage,
        Heal,
        Power,
        None,
    }
    public enum CloneMode
    {
        Defaults,
        OverrideFromData,
    }
}