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

        // Percent based effects.

        public Stat IgnoreArmour = new();

        public Stat IgnoreBlock = new();

        public Stat Lifesteal = new();

        // Effect modifiers - flat increases to the base value of an effect, % increases to the base value of an effect, and % multipliers to the entire effect after all other calculations.

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
            Strength_Flat.AddModifier(new StatModifier("BaseValue", Strength_Flat, value: 10f));
            Dexterity_Flat.AddModifier(new StatModifier("BaseValue", Dexterity_Flat, value: 10f));
            Wisdom_Flat.AddModifier(new StatModifier("BaseValue", Wisdom_Flat, value: 10f));
            Foresight_Flat.AddModifier(new StatModifier("BaseValue", Foresight_Flat, value: 10f));
            Endurance_Flat.AddModifier(new StatModifier("BaseValue", Endurance_Flat, value: 10f));
            Tenacity_Flat.AddModifier(new StatModifier("BaseValue", Tenacity_Flat, value: 10f));

            MaxHealth_Flat.AddModifier(new StatModifier("BaseValue", MaxHealth_Flat, value: () => Tenacity_Flat.Value() * 50f));
            MaxStamina_Flat.AddModifier(new StatModifier("BaseValue", MaxStamina_Flat, value: () => Endurance_Flat.Value() * 5f));

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

        // Convenience properties — return the fully computed stat value across all three tiers
        // Base stats
        public float MaxHealth => GetMaxHealthValue();
        public float MaxStamina => GetMaxStaminaValue();
        public float Armour => ApplyStatModifiers(0f, Armour_Flat, Armour_Increase, Armour_Multiplier);
        public float Strength => ApplyStatModifiers(0f, Strength_Flat, Strength_Increase, Strength_Multiplier);
        public float Dexterity => ApplyStatModifiers(0f, Dexterity_Flat, Dexterity_Increase, Dexterity_Multiplier);
        public float Wisdom => ApplyStatModifiers(0f, Wisdom_Flat, Wisdom_Increase, Wisdom_Multiplier);
        public float Foresight => ApplyStatModifiers(0f, Foresight_Flat, Foresight_Increase, Foresight_Multiplier);
        public float Endurance => ApplyStatModifiers(0f, Endurance_Flat, Endurance_Increase, Endurance_Multiplier);
        public float Tenacity => ApplyStatModifiers(0f, Tenacity_Flat, Tenacity_Increase, Tenacity_Multiplier);

        // Other modifiers
        public float MovementCostModifier => ApplyStatModifiers(0f, MovementCostModifier_Flat, MovementCostModifier_Increase, MovementCostModifier_Multiplier);
        public float DamageTakenModifier => ApplyStatModifiers(0f, DamageTakenModifier_Flat, DamageTakenModifier_Increase, DamageTakenModifier_Multiplier);
        public float HealingTakenModifier => ApplyStatModifiers(0f, HealingTakenModifier_Flat, HealingTakenModifier_Increase, HealingTakenModifier_Multiplier);
        public float DamageOutModifier => ApplyStatModifiers(0f, DamageOutModifier_Flat, DamageOutModifier_Increase, DamageOutModifier_Multiplier);
        public float HealingOutModifier => ApplyStatModifiers(0f, HealingOutModifier_Flat, HealingOutModifier_Increase, HealingOutModifier_Multiplier);
        public float CardCostModifier => ApplyStatModifiers(0f, CardCostModifier_Flat, CardCostModifier_Increase, CardCostModifier_Multiplier);
        public float PowerModifier => ApplyStatModifiers(0f, PowerModifier_Flat, PowerModifier_Increase, PowerModifier_Multiplier);
        public float DurationModifier => ApplyStatModifiers(0f, DurationModifier_Flat, DurationModifier_Increase, DurationModifier_Multiplier);

        // Aoe effect modifiers
        public float RangeModifier => ApplyStatModifiers(0f, RangeModifier_Flat, RangeModifier_Increase, RangeModifier_Multiplier);
        public float AreaModifier => ApplyStatModifiers(0f, AreaModifier_Flat, AreaModifier_Increase, AreaModifier_Multiplier);
        public float RadiusModifier => ApplyStatModifiers(0f, RadiusModifier_Flat, RadiusModifier_Increase, RadiusModifier_Multiplier);
        public float MaxTargetModifier => ApplyStatModifiers(0f, MaxTargetModifier_Flat, MaxTargetModifier_Increase, MaxTargetModifier_Multiplier);

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