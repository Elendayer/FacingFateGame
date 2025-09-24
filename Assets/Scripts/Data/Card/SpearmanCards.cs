using System.Collections.Generic;
using UnityEngine;

// Spearman cards are registered here to keep CardDatabase clean per class.
// Call SpearmanCards.RegisterAll() from CardDatabase.RegisterAll() (or your Startup).
public static class SpearmanCards
{
    // Type codes according to your schema: Martial Art/Ability/Spell/Curse/Blessing
    private const int TYPE_MARTIAL_ART = 01;
    private const int TYPE_ABILITY = 02;
    private const int TYPE_SPELL = 03; // (unused for Spearman for now)
    private const int TYPE_CURSE = 04;
    private const int TYPE_BLESSING = 05;

    // Spearman class prefix (11|TT|II)
    private const int CLASS_BASE = 110000;

    private static int ComposeId(int typeCode, int index)
        => CLASS_BASE + typeCode * 100 + index;

    public static void RegisterAll()
    {
        RegisterMartialArts();
        RegisterAbilities();
        RegisterCurses();
        RegisterBlessings();
    }

    // ---------------- Martial Arts (TT = 01) ----------------
    private static void RegisterMartialArts()
    {
        // Helper to reduce repetition and match your card layout pattern
        CardData MakeMA(
            string name,
            int index,
            int cost,
            int power,
            CardTargetType targetType,
            CardTargetAffiliation targetAff,
            CardTargetArea areaType,
            int range,
            int area = 1,
            int repeats = 0,
            List<CardIdentity> ids = null)
        {
            var data = new CardData
            {
                cardID = ComposeId(TYPE_MARTIAL_ART, index),
                cardName = name,
                cardType = CardType.Technique, // Martial Art → Technique
                cardClass = CardClass.Spearman,
                cardIdentities = ids ?? new List<CardIdentity> { CardIdentity.Physical }, // Melee = Physical

                cost_u = cost,
                power_u = power,
                repeats_u = repeats,

                targetingData = new TargetArea
                {
                    CardTargetType = targetType,
                    CardTargetAffiliation = targetAff,
                    areaType = areaType,
                    range = range,
                    area = area,
                },

                SetCardDescription = (User, d) =>
                {
                    d.cardDescription = $"{name}: Cost {d.Cost}, Power {d.Power}.";
                },

                CardEffect = (User, Target, d) =>
                {
                    // TODO: fill with real effect logic (damage/slow/knockback/taunt etc.)
                    // Example for pure damage cards later:
                    // CombatUtility.ApplyDamage(User, Target, d.Power, true);
                },
            };
            return data;
        }

        // Index & values per sheet (Range taken from your table; Line = LineSelf)
        CardDatabase.RegisterCard(MakeMA(
            name: "Tempest of a Hundred Spears", index: 07, cost: 10, power: 3,
            targetType: CardTargetType.CombatTile, targetAff: CardTargetAffiliation.Enemy,
            areaType: CardTargetArea.Ring, range: 1, area: 1, repeats: 2));

        CardDatabase.RegisterCard(MakeMA(
            name: "Piercing Light", index: 02, cost: 5, power: 3,
            targetType: CardTargetType.CombatTile, targetAff: CardTargetAffiliation.Enemy,
            areaType: CardTargetArea.LineSelf, range: 3));

        CardDatabase.RegisterCard(MakeMA(
            name: "Sky-Piercing Leap", index: 03, cost: 3, power: 2,
            targetType: CardTargetType.CombatTile, targetAff: CardTargetAffiliation.Enemy,
            areaType: CardTargetArea.LineSelf, range: 2));

        CardDatabase.RegisterCard(MakeMA(
            name: "Dragon Fang Thrust", index: 01, cost: 2, power: 10,
            targetType: CardTargetType.Entity, targetAff: CardTargetAffiliation.Enemy,
            areaType: CardTargetArea.Single, range: 1));

        CardDatabase.RegisterCard(MakeMA(
            name: "Heaven Piercing Spear", index: 04, cost: 3, power: 10,
            targetType: CardTargetType.Entity, targetAff: CardTargetAffiliation.Enemy,
            areaType: CardTargetArea.Single, range: 1,
            ids: new List<CardIdentity> { CardIdentity.Physical, CardIdentity.Blood })); // Bleed

        CardDatabase.RegisterCard(MakeMA(
            name: "Earth-Sundering Sweep", index: 05, cost: 5, power: 5,
            targetType: CardTargetType.CombatTile, targetAff: CardTargetAffiliation.Enemy,
            areaType: CardTargetArea.Ring, range: 1));

        CardDatabase.RegisterCard(MakeMA(
            name: "Dragon's Tail Sweep", index: 08, cost: 5, power: 3,
            targetType: CardTargetType.Entity, targetAff: CardTargetAffiliation.Enemy,
            areaType: CardTargetArea.Single, range: 1));

        CardDatabase.RegisterCard(MakeMA(
            name: "Earthshatter Pole", index: 09, cost: 10, power: 2,
            targetType: CardTargetType.CombatTile, targetAff: CardTargetAffiliation.Enemy,
            areaType: CardTargetArea.Ring, range: 1));

        CardDatabase.RegisterCard(MakeMA(
            name: "Azure Dragon's Roar", index: 10, cost: 3, power: 0,
            targetType: CardTargetType.Entity, targetAff: CardTargetAffiliation.Self,
            areaType: CardTargetArea.Single, range: 0));

        CardDatabase.RegisterCard(MakeMA(
            name: "Pillar of the Earth", index: 11, cost: 10, power: 4,
            targetType: CardTargetType.CombatTile, targetAff: CardTargetAffiliation.Enemy,
            areaType: CardTargetArea.LineSelf, range: 4));
    }

    // ---------------- Abilities (TT = 02) ----------------
    private static void RegisterAbilities()
    {
        CardData MakeAbility(
            string name,
            int index,
            int cost,
            int power,
            CardTargetType targetType,
            CardTargetAffiliation targetAff,
            CardTargetArea areaType,
            int range,
            int area = 1,
            List<CardIdentity> ids = null)
        {
            var data = new CardData
            {
                cardID = ComposeId(TYPE_ABILITY, index),
                cardName = name,
                cardType = CardType.Ability,
                cardClass = CardClass.Spearman,
                cardIdentities = ids ?? new List<CardIdentity> { CardIdentity.Physical },

                cost_u = cost,
                power_u = power,

                targetingData = new TargetArea
                {
                    CardTargetType = targetType,
                    CardTargetAffiliation = targetAff,
                    areaType = areaType,
                    range = range,
                    area = area,
                },

                SetCardDescription = (User, d) =>
                {
                    d.cardDescription = $"{name}: Cost {d.Cost}, Power {d.Power}.";
                },

                CardEffect = (User, Target, d) =>
                {
                    // TODO: implement stance/guard/reversal/deflect/taunt specifics later
                },
            };
            return data;
        }

        CardDatabase.RegisterCard(MakeAbility(
            name: "Extending Heaven's Lance", index: 01, cost: 2, power: 0,
            targetType: CardTargetType.Entity, targetAff: CardTargetAffiliation.Self,
            areaType: CardTargetArea.Single, range: 0));

        CardDatabase.RegisterCard(MakeAbility(
            name: "Iron Wall Reversal", index: 02, cost: 2, power: 0,
            targetType: CardTargetType.Entity, targetAff: CardTargetAffiliation.Self,
            areaType: CardTargetArea.Single, range: 0));

        CardDatabase.RegisterCard(MakeAbility(
            name: "Whirling Heaven Ward", index: 03, cost: 2, power: 0,
            targetType: CardTargetType.Entity, targetAff: CardTargetAffiliation.Self,
            areaType: CardTargetArea.Single, range: 0));

        CardDatabase.RegisterCard(MakeAbility(
            name: "Unyielding Spear Stance", index: 04, cost: 4, power: 0,
            targetType: CardTargetType.Entity, targetAff: CardTargetAffiliation.Self,
            areaType: CardTargetArea.Single, range: 0));

        CardDatabase.RegisterCard(MakeAbility(
            name: "Sky-Rending Reversal", index: 05, cost: 8, power: 0,
            targetType: CardTargetType.Entity, targetAff: CardTargetAffiliation.Self,
            areaType: CardTargetArea.Single, range: 0));

        // Phalanx Guard – aura-like, but start as Self target for now
        CardDatabase.RegisterCard(MakeAbility(
            name: "Phalanx Guard", index: 06, cost: 5, power: 1,
            targetType: CardTargetType.Entity, targetAff: CardTargetAffiliation.Self,
            areaType: CardTargetArea.Single, range: 0));
    }

    // ---------------- Curse (TT = 04) ----------------
    private static void RegisterCurses()
    {
        var data = new CardData()
        {
            cardID = ComposeId(TYPE_CURSE, 01),
            cardName = "Brittle Courage",
            cardType = CardType.Curse,
            cardClass = CardClass.Spearman,
            cardIdentities = new List<CardIdentity> { CardIdentity.Physical },

            cost_u = 0,
            power_u = 10, // defaulted per your instruction

            targetingData = new TargetArea
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                areaType = CardTargetArea.Single,
                range = 0,
            },

            SetCardDescription = (User, d) =>
            {
                d.cardDescription = $"{d.cardName}: Cost {d.Cost}, Power {d.Power}.";
            },

            CardEffect = (User, Target, d) =>
            {
                // TODO: implement 50% less Armour (temporary modifier)
            }
        };
        CardDatabase.RegisterCard(data);
    }

    // ---------------- Blessing (TT = 05) ----------------
    private static void RegisterBlessings()
    {
        var data = new CardData()
        {
            cardID = ComposeId(TYPE_BLESSING, 01),
            cardName = "Brilliant Spear",
            cardType = CardType.Blessing,
            cardClass = CardClass.Spearman,
            cardIdentities = new List<CardIdentity> { CardIdentity.Physical },

            cost_u = 0,
            power_u = 10, // defaulted per your instruction

            targetingData = new TargetArea
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                areaType = CardTargetArea.Single,
                range = 0,
            },

            SetCardDescription = (User, d) =>
            {
                d.cardDescription = $"{d.cardName}: Cost {d.Cost}, Power {d.Power}.";
            },

            CardEffect = (User, Target, d) =>
            {
                // TODO: implement Aggro up & attack harder
            }
        };
        CardDatabase.RegisterCard(data);
    }
}
