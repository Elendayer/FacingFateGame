using UnityEngine;

[System.Serializable]
public class EntityStats
{
    public EntityScript Owner;
   
    [Header("Base Stats")]
    [SerializeField]
    public Stat MaxHealth = new();
    public Stat CurrentHealth = new();
    public Stat MaxStamina = new();
    public Stat CurrentStamina = new();

    [Header("Defense")]
    public Stat Block = new();
    public Stat Armour = new();

    [Header("Attributes")]
    public Stat Strength = new();
    public Stat Dexterity = new ();
    public Stat Wisdom = new ();
    public Stat Foresight  = new ();
    public Stat Endurance = new ();
    public Stat Tenacity  = new ();

    [Header ("Combat Modifiers")]
    public Stat DamageTakenReduction = new();

    public Stat DamageIncrease = new();
    public Stat HealingIncrease = new();
    public Stat CostIncrease = new();
    public Stat PowerIncrease = new();
    public Stat DurationIncrease = new();

    public Stat IgnoreArmour = new();
    public Stat IgnoreBlock = new();

    public Stat Lifesteal = new();

    public void StartUp(EntityScript entityScript)
    {
        Owner = entityScript;

        Strength.AddModifier(new StatModifier(value: 10, ModifierScaling.Flat, name: "BaseValue"));
        Dexterity.AddModifier(new StatModifier(value: 10, ModifierScaling.Flat, name: "BaseValue"));
        Wisdom.AddModifier(new StatModifier(value: 10, ModifierScaling.Flat, name: "BaseValue"));
        Foresight.AddModifier(new StatModifier(value: 10, ModifierScaling.Flat, name: "BaseValue"));
        Endurance.AddModifier(new StatModifier(value: 10, ModifierScaling.Flat, name: "BaseValue"));
        Tenacity.AddModifier(new StatModifier(value: 10, ModifierScaling.Flat, name: "BaseValue"));



        MaxHealth.AddModifier(new StatModifier(value: () => Tenacity.Value * 50, ModifierScaling.Flat, name: "BaseValue"));
        MaxStamina.AddModifier(new StatModifier(value: () => Endurance.Value * 5, ModifierScaling.Flat, name: "BaseValue"));

        CurrentHealth.AddModifier(new StatModifier(MaxHealth.Value, ModifierScaling.Flat, name: "BaseValue"));
        CurrentStamina.AddModifier(new StatModifier(MaxStamina.Value, ModifierScaling.Flat, name: "BaseValue"));

    }
}
