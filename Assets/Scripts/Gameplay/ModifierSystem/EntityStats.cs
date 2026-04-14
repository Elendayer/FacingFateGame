using System.Linq;
using UnityEngine;

namespace facingfate
{
    [System.Serializable]
    public class EntityStats
    {
        public EntityScript Owner;

        [Header("Base Stats")]
        [SerializeField]
        public Stat MaxHealth_Flat = new();
        public Stat MaxHealth_Increase = new();
        public Stat MaxHealth_Multiplier = new();
        public float CurrentHealth;

        public Stat MaxStamina_Flat = new();
        public Stat MaxStamina_Increase = new();
        public Stat MaxStamina_Multiplier = new();
        public float CurrentStamina;

        [Header("Defense")]
        public float Block = 0;
        public Stat Armour_Flat = new();
        public Stat Armour_Increase = new();
        public Stat Armour_Multiplier = new();

        [Header("Attributes")]
        public Stat Strength_Flat = new();
        public Stat Strength_Increase = new();
        public Stat Strength_Multiplier = new();

        public Stat Dexterity_Flat = new();
        public Stat Dexterity_Increase = new();
        public Stat Dexterity_Multiplier = new();

        public Stat Wisdom_Flat = new();
        public Stat Wisdom_Increase = new();
        public Stat Wisdom_Multiplier = new();

        public Stat Foresight_Flat = new();
        public Stat Foresight_Increase = new();
        public Stat Foresight_Multiplier = new();

        public Stat Endurance_Flat = new();
        public Stat Endurance_Increase = new();
        public Stat Endurance_Multiplier = new();

        public Stat Tenacity_Flat = new();
        public Stat Tenacity_Increase = new();
        public Stat Tenacity_Multiplier = new();

        [Header("Other Stats")]
        public Stat MovementCostModifier_Flat = new();
        public Stat MovementCostModifier_Increase = new();
        public Stat MovementCostModifier_Multiplier = new();

        [Header("Combat Modifiers")]
        public Stat DamageTakenModifier_Flat = new();
        public Stat DamageTakenModifier_Increase = new();
        public Stat DamageTakenModifier_Multiplier = new();

        public Stat HealingTakenModifier_Flat = new();
        public Stat HealingTakenModifier_Increase = new();
        public Stat HealingTakenModifier_Multiplier = new();

        public Stat DamageOutModifier_Flat = new();
        public Stat DamageOutModifier_Increase = new();
        public Stat DamageOutModifier_Multiplier = new();

        public Stat HealingOutModifier_Flat = new();
        public Stat HealingOutModifier_Increase = new();
        public Stat HealingOutModifier_Multiplier = new();

        public Stat CardCostModifier_Flat = new();
        public Stat CardCostModifier_Increase = new();
        public Stat CardCostModifier_Multiplier = new();

        public Stat PowerModifier_Flat = new();
        public Stat PowerModifier_Increase = new();
        public Stat PowerModifier_Multiplier = new();

        public Stat DurationModifier_Flat = new();
        public Stat DurationModifier_Increase = new();
        public Stat DurationModifier_Multiplier = new();

        public Stat IgnoreArmour_Flat = new();
        public Stat IgnoreArmour_Increase = new();
        public Stat IgnoreArmour_Multiplier = new();

        public Stat IgnoreBlock_Flat = new();
        public Stat IgnoreBlock_Increase = new();
        public Stat IgnoreBlock_Multiplier = new();

        public Stat Lifesteal_Flat = new();
        public Stat Lifesteal_Increase = new();
        public Stat Lifesteal_Multiplier = new();

        public Stat RangeModifier_Flat = new();
        public Stat RangeModifier_Increase = new();
        public Stat RangeModifier_Multiplier = new();

        public Stat AreaModifier_Flat = new();
        public Stat AreaModifier_Increase = new();
        public Stat AreaModifier_Multiplier = new();

        public Stat RadiusModifier_Flat = new();
        public Stat RadiusModifier_Increase = new();
        public Stat RadiusModifier_Multiplier = new();

        public Stat MaxTargetModifier_Flat = new();
        public Stat MaxTargetModifier_Increase = new();
        public Stat MaxTargetModifier_Multiplier = new();

        [Header("StatusConditions")]
        public bool IsStunned = false;
        public bool IsRooted = false;
        public EntityScript tauntTarget;

        public void StartUp(EntityScript entityScript)
        {
            Owner = entityScript;

            // Base attribute values - Flat
            Strength_Flat.AddModifier(new StatModifier("BaseValue", Strength_Flat, value: 10f, ModifierScaling.Flat));
            Dexterity_Flat.AddModifier(new StatModifier("BaseValue", Dexterity_Flat, value: 10f, ModifierScaling.Flat));
            Wisdom_Flat.AddModifier(new StatModifier("BaseValue", Wisdom_Flat, value: 10f, ModifierScaling.Flat));
            Foresight_Flat.AddModifier(new StatModifier("BaseValue", Foresight_Flat, value: 10f, ModifierScaling.Flat));
            Endurance_Flat.AddModifier(new StatModifier("BaseValue", Endurance_Flat, value: 10f, ModifierScaling.Flat));
            Tenacity_Flat.AddModifier(new StatModifier("BaseValue", Tenacity_Flat, value: 10f, ModifierScaling.Flat));

            MaxHealth_Flat.AddModifier(new StatModifier("BaseValue", MaxHealth_Flat, value: () => Tenacity_Flat.Value() * 50f, ModifierScaling.Flat));
            MaxStamina_Flat.AddModifier(new StatModifier("BaseValue", MaxStamina_Flat, value: () => Endurance_Flat.Value() * 5f, ModifierScaling.Flat));

            CurrentHealth = GetMaxHealthValue();
            CurrentStamina = GetMaxStaminaValue();

            // Initial tick to set all stats correctly
            ActionQueueUtility.EnqueueAction(() =>
            {
                TickAllStats();
            });
        }

        private float GetMaxHealthValue()
        {
            float flat = MaxHealth_Flat.Value();
            float increase = 1f + (MaxHealth_Increase.Value() / 100f);
            float multipliers = GetMultiplierProduct(MaxHealth_Multiplier);
            return flat * increase * multipliers;
        }

        private float GetMaxStaminaValue()
        {
            float flat = MaxStamina_Flat.Value();
            float increase = 1f + (MaxStamina_Increase.Value() / 100f);
            float multipliers = GetMultiplierProduct(MaxStamina_Multiplier);
            return flat * increase * multipliers;
        }

        private float GetMultiplierProduct(Stat multiplierStat)
        {
            float product = 1f;
            var multipliers = multiplierStat.GetAllValues();
            foreach (var mult in multipliers)
            {
                product *= (mult / 100f);
            }
            return product;
        }

        public float ApplyStatModifiers(float baseValue, Stat flatStat, Stat increaseStat, Stat multiplierStat, EntityScript entityScript = null, CardData cardData = null)
        {
            float flat = flatStat.ApplyFinalValue(0, entityScript, cardData);
            float increase = increaseStat.ApplyFinalValue(0, entityScript, cardData);
            float multipliers = GetMultiplierProduct(multiplierStat);

            return (baseValue + flat) * (1f + (increase / 100f)) * multipliers;
        }

        // Convenience properties for backward compatibility
        public Stat MaxHealth => MaxHealth_Flat;
        public Stat MaxStamina => MaxStamina_Flat;
        public Stat Armour => Armour_Flat;
        public Stat Strength => Strength_Flat;
        public Stat Dexterity => Dexterity_Flat;
        public Stat Wisdom => Wisdom_Flat;
        public Stat Foresight => Foresight_Flat;
        public Stat Endurance => Endurance_Flat;
        public Stat Tenacity => Tenacity_Flat;
        public Stat MovementCostModifier => MovementCostModifier_Flat;
        public Stat DamageTakenModifier => DamageTakenModifier_Flat;
        public Stat HealingTakenModifier => HealingTakenModifier_Flat;
        public Stat DamageOutModifier => DamageOutModifier_Flat;
        public Stat HealingOutModifier => HealingOutModifier_Flat;
        public Stat CardCostModifier => CardCostModifier_Flat;
        public Stat PowerModifier => PowerModifier_Flat;
        public Stat DurationModifier => DurationModifier_Flat;
        public Stat IgnoreArmour => IgnoreArmour_Flat;
        public Stat IgnoreBlock => IgnoreBlock_Flat;
        public Stat Lifesteal => Lifesteal_Flat;
        public Stat RangeModifier => RangeModifier_Flat;
        public Stat AreaModifier => AreaModifier_Flat;
        public Stat RadiusModifier => RadiusModifier_Flat;
        public Stat MaxTargetModifier => MaxTargetModifier_Flat;
        public void TickAllStats()
        {
            var statFields = typeof(EntityStats).GetFields().Where(f => f.FieldType == typeof(Stat));

            foreach (var field in statFields)
            {
                var stat = (Stat)field.GetValue(this);
                stat.owner = Owner;
                stat?.Tick();
            }
        }

        public void UpdateStats()
        {
            var statFields = typeof(EntityStats).GetFields().Where(f => f.FieldType == typeof(Stat));

            foreach (var field in statFields)
            {
                var stat = (Stat)field.GetValue(this);
                stat?.UpdateStat();
            }

            if (CurrentHealth <= 0)
            {
                ActionQueueUtility.EnqueueAction(() =>
                {
                    TurnManager.Instance.RemoveTurn(Owner);
                    Owner.GetComponent<EntityOnMap>().enabled = false;
                    Owner.enabled = false;

                    GameEvents.TriggerRefEvent(new ToSendTriggerReference(new() { GameplayRef.onDeath }, Owner, new() { Owner }));
                    Owner.RemoveAllModifiers();

                    Owner.EntityModel.transform.rotation = new();
                });
            }
        }
    }
}