using System;
using System.Collections.Generic;
using UnityEngine;

namespace facingfate
{
    public interface IStatModifier
    {
        string ModifierName { get; }
        float BaseValue { get; set; }
        int Duration { get; set; }
        int Charges { get; set; }
        List<GameplayRef> To_TriggerGameplayRefs { get; }
        bool IsExpired { get; }
        bool IsSpend { get; }

        RelevantTriggerCheck On_RefTrigger { get; set; }
        void RefAction(ToSendTriggerReference reference);

        Stat Stat { get; }
        ConditionalModifierInfo Condition { get; }

        public void Init();
        void Tick();
        void UpdateStatModifier();
        void OnRemove();
    }

    public class StatModifier : IStatModifier
    {
        #region Public Properties

        public string ModifierName { get; private set; }
        public Stat Stat { get; private set; }
        public ConditionalModifierInfo Condition { get; private set; }

        public int Duration { get; set; }
        public int Charges { get; set; }
        public bool IsExpired => Duration <= 0;
        public bool IsSpend => Charges <= 0;

        public List<GameplayRef> To_TriggerGameplayRefs { get; private set; }
        public RelevantTriggerCheck On_RefTrigger { get; set; }
        public Action<StatModifier, EntityScript, CardData, int> On_RefAction { get; set; }

        public float BaseValue
        {
            get => _dynamicValueFunc != null ? _dynamicValueFunc.Invoke() : _staticValue ?? 0;
            set
            {
                if (_dynamicValueFunc != null)
                    throw new InvalidOperationException("Cannot set BaseValue when using dynamic value function.");
                _staticValue = value;
            }
        }

        #endregion

        #region Private Fields

        private float? _staticValue;
        private Func<float> _dynamicValueFunc;
        private bool? _staticCondition;
        private Func<EntityScript, CardData, bool> _dynamicConditionFunc;

        #endregion

        #region Public Methods

        /// <summary>Evaluates the condition logic for this modifier</summary>
        public bool EvaluateCondition(EntityScript entityScript = null, CardData cardData = null) =>
            _dynamicConditionFunc != null
                ? _dynamicConditionFunc.Invoke(entityScript, cardData)
                : _staticCondition ?? false;

        public int GetRemainingDuration() => Duration;

        public void Init()
        {
            if (On_RefTrigger.OnTriggerReference == null || On_RefTrigger.OnTriggerReference.Count == 0)
                return;

            GameEvents.OnGameplayReference += RefAction;
        }

        public void Tick()
        {
            Duration--;

            if (IsExpired)
            {
                TimelineManager.GlobalActionQueue.Enqueue(OnRemove);
            }
        }

        public void UpdateStatModifier()
        {
            if (IsSpend)
            {
                TimelineManager.GlobalActionQueue.Enqueue(OnRemove);
            }
        }

        public void OnRemove()
        {
            GameEvents.OnGameplayReference -= RefAction;
            Stat.statModifiers.Remove(this);
        }

        public void RefAction(ToSendTriggerReference trigger)
        {
            if (!GameEvents.CheckIfRelevantTrigger(trigger, On_RefTrigger))
                return;

            Debug.Log($"StatModifier '{ModifierName}' triggered. Charges: {Charges}");
            Charges--;

            On_RefAction?.Invoke(this, trigger.UserEntity, trigger.CardData, trigger.Throughput);
        }

        #endregion

        #region Constructors

        /// <summary>Static float + static bool condition</summary>
        public StatModifier(
            string name,
            Stat stat,
            float value,
            bool condition = true,
            List<GameplayRef> to_TriggerRefs = null,
            int duration = 99999,
            int charges = 99999,
            RelevantTriggerCheck on_RefTrigger = new(),
            Action<StatModifier, EntityScript, CardData, int> on_RefAction = null,
            ConditionalModifierInfo conditionMetadata = null)
        {
            InitializeCommon(name, stat, to_TriggerRefs, duration, charges, on_RefTrigger, on_RefAction, conditionMetadata);
            _staticValue = value;
            _staticCondition = condition;
        }

        /// <summary>Static float + dynamic condition</summary>
        public StatModifier(
            string name,
            Stat stat,
            float value,
            Func<EntityScript, CardData, bool> condition,
            List<GameplayRef> to_TriggerRefs = null,
            int duration = 99999,
            int charges = 99999,
            RelevantTriggerCheck on_RefTrigger = new(),
            Action<StatModifier, EntityScript, CardData, int> on_RefAction = null,
            ConditionalModifierInfo conditionMetadata = null)
        {
            InitializeCommon(name, stat, to_TriggerRefs, duration, charges, on_RefTrigger, on_RefAction, conditionMetadata);
            _staticValue = value;
            _dynamicConditionFunc = condition;
        }

        /// <summary>Static float + string condition (resolves metadata automatically)</summary>
        public StatModifier(
            string name,
            Stat stat,
            float value,
            string condition)
        {
            var metadata = ConditionalModifierInfo.Get(condition);
            InitializeCommon(name, stat, null, 99999, 99999, new(), null, metadata);
            _staticValue = value;
            _dynamicConditionFunc = ConditionalModifierInfo.GetCombinedConditionFunc(condition);
        }

        /// <summary>Static float + string condition with optional parameters</summary>
        public StatModifier(
            string name,
            Stat stat,
            float value,
            List<GameplayRef> to_TriggerRefs,
            int duration,
            string condition)
        {
            var metadata = ConditionalModifierInfo.Get(condition);
            InitializeCommon(name, stat, to_TriggerRefs, duration, 99999, new(), null, metadata);
            _staticValue = value;
            _dynamicConditionFunc = ConditionalModifierInfo.GetCombinedConditionFunc(condition);
        }

        /// <summary>Static float + multiple string conditions (AND logic)</summary>
        public StatModifier(
            string name,
            Stat stat,
            float value,
            List<GameplayRef> to_TriggerRefs,
            int duration,
            int charges,
            params string[] conditions)
        {
            if (conditions == null || conditions.Length == 0)
            {
                InitializeCommon(name, stat, to_TriggerRefs, duration, charges, new(), null, null);
                _staticValue = value;
                _dynamicConditionFunc = (e, d) => true;
            }
            else
            {
                var metadata = conditions.Length == 1 
                    ? ConditionalModifierInfo.Get(conditions[0])
                    : ConditionalModifierInfo.CreateCombinedMetadata(
                        string.Join(" + ", conditions),
                        $"Bonus applies when: {string.Join(" AND ", conditions)}",
                        conditions);

                InitializeCommon(name, stat, to_TriggerRefs, duration, charges, new(), null, metadata);
                _staticValue = value;
                _dynamicConditionFunc = ConditionalModifierInfo.GetCombinedConditionFunc(conditions);
            }
        }

        /// <summary>Dynamic float + static bool condition</summary>
        public StatModifier(
            string name,
            Stat stat,
            Func<float> value,
            bool condition = true,
            List<GameplayRef> to_TriggerRefs = null,
            int duration = 99999,
            int charges = 99999,
            RelevantTriggerCheck on_RefTrigger = new(),
            Action<StatModifier, EntityScript, CardData, int> on_RefAction = null,
            ConditionalModifierInfo conditionMetadata = null)
        {
            InitializeCommon(name, stat, to_TriggerRefs, duration, charges, on_RefTrigger, on_RefAction, conditionMetadata);
            _dynamicValueFunc = value;
            _staticCondition = condition;
        }

        /// <summary>Dynamic float + dynamic condition</summary>
        public StatModifier(
            string name,
            Stat stat,
            Func<float> value,
            Func<EntityScript, CardData, bool> condition,
            List<GameplayRef> to_TriggerRefs = null,
            int duration = 99999,
            int charges = 99999,
            RelevantTriggerCheck on_RefTrigger = new(),
            Action<StatModifier, EntityScript, CardData, int> on_RefAction = null,
            ConditionalModifierInfo conditionMetadata = null)
        {
            InitializeCommon(name, stat, to_TriggerRefs, duration, charges, on_RefTrigger, on_RefAction, conditionMetadata);
            _dynamicValueFunc = value;
            _dynamicConditionFunc = condition;
        }

        /// <summary>Dynamic float + string condition (resolves metadata automatically)</summary>
        public StatModifier(
            string name,
            Stat stat,
            Func<float> value,
            string condition)
        {
            var metadata = ConditionalModifierInfo.Get(condition);
            InitializeCommon(name, stat, null, 99999, 99999, new(), null, metadata);
            _dynamicValueFunc = value;
            _dynamicConditionFunc = ConditionalModifierInfo.GetCombinedConditionFunc(condition);
        }

        /// <summary>Dynamic float + multiple string conditions (AND logic)</summary>
        public StatModifier(
            string name,
            Stat stat,
            Func<float> value,
            List<GameplayRef> to_TriggerRefs,
            int duration,
            int charges,
            params string[] conditions)
        {
            if (conditions == null || conditions.Length == 0)
            {
                InitializeCommon(name, stat, to_TriggerRefs, duration, charges, new(), null, null);
                _dynamicValueFunc = value;
                _dynamicConditionFunc = (e, d) => true;
            }
            else
            {
                var metadata = conditions.Length == 1 
                    ? ConditionalModifierInfo.Get(conditions[0])
                    : ConditionalModifierInfo.CreateCombinedMetadata(
                        string.Join(" + ", conditions),
                        $"Bonus applies when: {string.Join(" AND ", conditions)}",
                        conditions);

                InitializeCommon(name, stat, to_TriggerRefs, duration, charges, new(), null, metadata);
                _dynamicValueFunc = value;
                _dynamicConditionFunc = ConditionalModifierInfo.GetCombinedConditionFunc(conditions);
            }
        }

        private void InitializeCommon(
            string name,
            Stat stat,
            List<GameplayRef> to_TriggerRefs,
            int duration,
            int charges,
            RelevantTriggerCheck on_RefTrigger,
            Action<StatModifier, EntityScript, CardData, int> on_RefAction,
            ConditionalModifierInfo conditionMetadata)
        {
            ModifierName = name;
            Stat = stat;
            To_TriggerGameplayRefs = to_TriggerRefs ?? new List<GameplayRef>();
            Duration = duration;
            Charges = charges;
            On_RefTrigger = on_RefTrigger;
            On_RefAction = on_RefAction;
            Condition = conditionMetadata;
        }

        #endregion
    }
}