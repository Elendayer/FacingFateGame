using System;
using System.Linq;
using UnityEngine;

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

        // Base attribute values
        Strength.AddModifier(new StatModifier("BaseValue",  Strength, value: 10, ModifierScaling.Flat));
        Dexterity.AddModifier(new StatModifier("BaseValue", Dexterity, value: 10, ModifierScaling.Flat));
        Wisdom.AddModifier(new StatModifier("BaseValue", Wisdom, value: 10, ModifierScaling.Flat));
        Foresight.AddModifier(new StatModifier("BaseValue", Foresight, value: 10, ModifierScaling.Flat));
        Endurance.AddModifier(new StatModifier("BaseValue", Endurance, value: 10, ModifierScaling.Flat));
        Tenacity.AddModifier(new StatModifier("BaseValue", Tenacity, value: 10, ModifierScaling.Flat));

        MaxHealth.AddModifier(new StatModifier("BaseValue", MaxHealth, value: () => Tenacity.Value() * 50, ModifierScaling.Flat));
        MaxStamina.AddModifier(new StatModifier("BaseValue", MaxStamina, value: () => Endurance.Value() * 5, ModifierScaling.Flat));

        CurrentHealth = MaxHealth.Value();
        CurrentStamina = MaxStamina.Value();

        // Initial tick to set all stats correctly
        ActionQueueUtility.EnqueueAction(() =>
        {
            TickAllStats();
        });
    }
    public void TickAllStats()
    {
        var statFields = typeof(EntityStats).GetFields()
                            .Where(f => f.FieldType == typeof(Stat));

        foreach (var field in statFields)
        {
            var stat = (Stat)field.GetValue(this);
            stat.owner = Owner;
            stat?.Tick();
        }
    }

    internal void UpdateStats()
    {
        var statFields = typeof(EntityStats).GetFields()
                                   .Where(f => f.FieldType == typeof(Stat));

        foreach (var field in statFields)
        {
            var stat = (Stat)field.GetValue(this);
            stat?.UpdateStat();
        }
    }
}