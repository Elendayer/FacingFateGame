using UnityEngine;

namespace facingfate
{
    [System.Serializable]
    public class EntityStats
    {
        public EntityScript Owner;

        [Header("Base Stats")]
        [SerializeField]
        public Stat MaxHealth = new();
        public int CurrentHealth;
        public Stat MaxStamina = new();
        public int CurrentStamina;

        [Header("Defense")]
        public int Block = new();
        public Stat Armour = new();

        [Header("Attributes")]
        public Stat Strength = new();
        public Stat Dexterity = new();
        public Stat Wisdom = new();
        public Stat Foresight = new();
        public Stat Endurance = new();
        public Stat Tenacity = new();

        [Header("Other Stats")]
        public Stat MovementCostModifier = new();

        [Header("Combat Modifiers")]
        public Stat DamageTakenModifier = new();
        public Stat HealingTakenModifier = new();

        public Stat DamageOutModifier = new();
        public Stat HealingOutModifier = new();
        public Stat CardCostModifier = new();
        public Stat PowerModifier = new();
        public Stat DurationModifier = new();

        public Stat IgnoreArmour = new();
        public Stat IgnoreBlock = new();

        public Stat Lifesteal = new();

        public Stat RangeModifier = new();
        public Stat AreaModifier = new();
        public Stat RadiusModifier = new();
        public Stat MaxTargetModifier = new();

        [Header("StatusConditions")]
        public bool IsStunned = false;
        public bool IsRooted = false;
        public EntityScript tauntTarget;

        public void StartUp(EntityScript entityScript)
        {
            Owner = entityScript;

            Strength.AddModifier(new StatModifier(stat: Strength, value: 10, ModifierScaling.Flat, name: "BaseValue"));
            Dexterity.AddModifier(new StatModifier(stat: Dexterity, value: 10, ModifierScaling.Flat, name: "BaseValue"));
            Wisdom.AddModifier(new StatModifier(stat: Wisdom, value: 10, ModifierScaling.Flat, name: "BaseValue"));
            Foresight.AddModifier(new StatModifier(stat: Foresight, value: 10, ModifierScaling.Flat, name: "BaseValue"));
            Endurance.AddModifier(new StatModifier(stat: Endurance, value: 10, ModifierScaling.Flat, name: "BaseValue"));
            Tenacity.AddModifier(new StatModifier(stat: Tenacity, value: 10, ModifierScaling.Flat, name: "BaseValue"));

            MaxHealth.AddModifier(new StatModifier(stat: MaxHealth, value: () => Tenacity.Value() * 50, ModifierScaling.Flat, name: "BaseValue"));
            MaxStamina.AddModifier(new StatModifier(stat: MaxStamina, value: () => Endurance.Value() * 5, ModifierScaling.Flat, name: "BaseValue"));

            CurrentHealth = MaxHealth.Value();
            CurrentStamina = MaxStamina.Value();
            GameEvents.OnGameplayReference += OnTurnStart;
        }
        private void OnTurnStart(TriggerRef reference)
        {
            if (GameEvents.CheckIfRelevantTrigger(reference, new TriggerRef(new() { GameplayRef.onTurnStart }, Owner, new() { Owner })))
            {
                CurrentStamina = MaxStamina.Value();
            }

            if (IsStunned)
            {
                GameEvents.TriggerTurnEnd();
            }
        }
    }
}