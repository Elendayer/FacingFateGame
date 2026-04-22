using System;
using System.Collections.Generic;
using System.Linq;

namespace facingfate
{
    public class ConditionalModifierInfo
    {
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }

        public ConditionalModifierInfo(string name, string displayName, string description)
        {
            Name = name;
            DisplayName = displayName;
            Description = description;
        }

        /// <summary>Gets the condition evaluation function for this modifier condition</summary>
        public Func<EntityScript, CardData, bool> GetConditionFunc()
        {
            return Name switch
            {
                // Type-based conditions check for the presence of the corresponding CardIdentity in the card's identities or match the card type
                "Melee" => (e, d) => d != null && d.cardIdentities.Contains(CardIdentity.Melee),
                "Ranged" => (e, d) => d != null && d.cardIdentities.Contains(CardIdentity.Ranged),
                "Magic" => (e, d) => d != null && d.cardIdentities.Contains(CardIdentity.Magic),
                "Healing" => (e, d) => d != null && d.cardIdentities.Contains(CardIdentity.Healing),
                "Buff" => (e, d) => d != null && d.cardIdentities.Contains(CardIdentity.Buff),
                "Debuff" => (e, d) => d != null && d.cardIdentities.Contains(CardIdentity.Debuff),

                // Card type conditions check if the card's type matches the specified type
                "Spell" => (e, d) => d != null && d.cardType == CardType.Spell,
                "Skill" => (e, d) => d != null && d.cardType == CardType.Skill,
                "Ability" => (e, d) => d != null && d.cardType == CardType.Ability,
                "Technique" => (e, d) => d != null && d.cardType == CardType.Technique,

                // Elemental conditions check for the presence of the corresponding CardIdentity in the card's identities
                "Fire" => (e, d) => d != null && d.cardIdentities.Contains(CardIdentity.Fire),
                "Ice" => (e, d) => d != null && d.cardIdentities.Contains(CardIdentity.Ice),
                "Air" => (e, d) => d != null && d.cardIdentities.Contains(CardIdentity.Air),
                "Earth" => (e, d) => d != null && d.cardIdentities.Contains(CardIdentity.Earth),
                "Physical" => (e, d) => d != null && d.cardIdentities.Contains(CardIdentity.Physical),
                "Shadow" => (e, d) => d != null && d.cardIdentities.Contains(CardIdentity.Shadow),
                "Light" => (e, d) => d != null && d.cardIdentities.Contains(CardIdentity.Light),
                "Arcane" => (e, d) => d != null && d.cardIdentities.Contains(CardIdentity.Arcane),

                // Entity Checks
                "isBurning" => (e, d) => e != null && e.HasModifier("Burning"),

                // Conditions based on entity stats
                "isStunned" => (e, d) => e != null && e.entityStats.IsStunned,
                "isRooted" => (e, d) => e != null && e.entityStats.IsRooted,
                _ => (e, d) => true  // Default to always true for unknown conditions
            };
        }

        // ── Predefined Conditions ──────────────────────────────────────────────
        // These are UI-focused metadata definitions. The actual condition logic
        // remains in the StatModifier's Func<EntityScript, CardData, bool>

        public static readonly ConditionalModifierInfo Melee = new("Melee", "Melee Damage Dealt", "Bonus applies only to Melee attacks");
        public static readonly ConditionalModifierInfo Ranged = new("Ranged", "Ranged Damage Dealt", "Bonus applies only to Ranged attacks");
        public static readonly ConditionalModifierInfo Magic = new("Magic", "Magic Damage Dealt", "Bonus applies only to Magic attacks");
        public static readonly ConditionalModifierInfo Healing = new("Healing", "Healing Dealt", "Bonus applies only to Healing cards");
        public static readonly ConditionalModifierInfo Buff = new("Buff", "Buff Effect", "Bonus applies only to Buff cards");
        public static readonly ConditionalModifierInfo Debuff = new("Debuff", "Debuff Effect", "Bonus applies only to Debuff cards");

        public static readonly ConditionalModifierInfo Spell = new("Spell", "Spell Damage Dealt", "Bonus applies only to Spells");
        public static readonly ConditionalModifierInfo Skill = new("Skill", "Skill Effect", "Bonus applies only to Skills");
        public static readonly ConditionalModifierInfo Ability = new("Ability", "Ability Effect", "Bonus applies only to Abilities");
        public static readonly ConditionalModifierInfo Technique = new("Technique", "Technique Effect", "Bonus applies only to Techniques");

        public static readonly ConditionalModifierInfo Fire = new("Fire", "Fire Damage Dealt", "Bonus applies only to Fire damage");
        public static readonly ConditionalModifierInfo Ice = new("Ice", "Ice Damage Dealt", "Bonus applies only to Ice damage");
        public static readonly ConditionalModifierInfo Air = new("Air", "Air Damage Dealt", "Bonus applies only to Air damage");
        public static readonly ConditionalModifierInfo Earth = new("Earth", "Earth Damage Dealt", "Bonus applies only to Earth damage");
        public static readonly ConditionalModifierInfo Physical = new("Physical", "Physical Damage Dealt", "Bonus applies only to Physical damage");
        public static readonly ConditionalModifierInfo Shadow = new("Shadow", "Shadow Damage Dealt", "Bonus applies only to Shadow damage");
        public static readonly ConditionalModifierInfo Light = new("Light", "Light Damage Dealt", "Bonus applies only to Light damage");
        public static readonly ConditionalModifierInfo Arcane = new("Arcane", "Arcane Damage Dealt", "Bonus applies only to Arcane damage");

        public static readonly ConditionalModifierInfo IsBurning = new("isBurning", "Is Burning", "Bonus applies only if the entity is currently Burning");
        public static readonly ConditionalModifierInfo IsStunned = new("isStunned", "Is Stunned", "Bonus applies only if the entity is currently Stunned");
        public static readonly ConditionalModifierInfo IsRooted = new("isRooted", "Is Rooted", "Bonus applies only if the entity is currently Rooted");

        /// <summary>Gets a condition metadata by name. Returns null if not found.</summary>
        public static ConditionalModifierInfo Get(string conditionName)
        {
            if (string.IsNullOrEmpty(conditionName)) return null;

            return conditionName switch
            {
                "Melee" => Melee,
                "Ranged" => Ranged,
                "Magic" => Magic,
                "Healing" => Healing,
                "Buff" => Buff,
                "Debuff" => Debuff,

                "Spell" => Spell,
                "Skill" => Skill,
                "Ability" => Ability,
                "Technique" => Technique,

                "Fire" => Fire,
                "Ice" => Ice,
                "Air" => Air,
                "Earth" => Earth,
                "Physical" => Physical,
                "Shadow" => Shadow,
                "Light" => Light,
                "Arcane" => Arcane,

                "isBurning" => IsBurning,
                "isStunned" => IsStunned,
                "isRooted" => IsRooted,
                _ => null
            };
        }

        /// <summary>Creates a custom condition metadata for advanced use cases</summary>
        public static ConditionalModifierInfo CreateCustom(string name, string displayName, string description)
        {
            return new ConditionalModifierInfo(name, displayName, description);
        }

        /// <summary>Gets the condition evaluation function for a named condition. Returns always-true if condition not found.</summary>
        public static Func<EntityScript, CardData, bool> GetConditionFuncByName(string conditionName)
        {
            if (string.IsNullOrEmpty(conditionName))
                return (e, d) => true;

            var info = Get(conditionName);
            return info?.GetConditionFunc() ?? ((e, d) => true);
        }

        /// <summary>Gets a combined condition function that requires ALL specified conditions to be true (AND logic)</summary>
        public static Func<EntityScript, CardData, bool> GetCombinedConditionFunc(params string[] conditionNames)
        {
            if (conditionNames == null || conditionNames.Length == 0)
                return (e, d) => true;

            // Get all condition functions and store them
            var conditions = conditionNames
                .Select(GetConditionFuncByName)
                .ToList();

            // Return a combined function that requires ALL conditions to be true
            return (e, d) => conditions.All(cond => cond(e, d));
        }

        /// <summary>Creates combined metadata for multiple conditions (useful for display when multiple conditions are required)</summary>
        public static ConditionalModifierInfo CreateCombinedMetadata(string displayName, string description, params string[] conditionNames)
        {
            // Use a combined name for internal reference
            string combinedName = string.Join("+", conditionNames);
            return new ConditionalModifierInfo(combinedName, displayName, description);
        }
    }
}