using System.Collections.Generic;
using UnityEngine;
using Utility;

// Assassin cards in Fire Bomb style. Damage effects implemented; complex ones left as comments.
// Call AssassinCards.RegisterAll() from CardDatabase.RegisterAll().
public static class AssassinCards
{
    public static void RegisterAll()
    {
        RegisterMartialArts();
        RegisterAbilities();
        RegisterSpells();
        RegisterCurses();
        RegisterBlessings();
    }

    // Class code = 12|TT|II

    private static void RegisterMartialArts()
    {
        // 120101 – Shadowfang Strike (Line)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120101,
            cardName = "Shadowfang Strike",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 5,
            power_u = 10,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                areaType = CardTargetArea.LineSelf,
                range = 3,
                area = 1,
            },

            SetCardDescription = (User, data) =>
                data.cardDescription = $"Line: Deal {data.Power} damage to enemies in a line.",

            CardEffect = (User, Target, data) =>
            {
                CombatUtility.ApplyDamage(User, Target, data.Power);
            }
        });

        // 120102 – Dance of a Hundred Cuts (AOE Ring)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120102,
            cardName = "Dance of a Hundred Cuts",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 8,
            power_u = 1,
            repeats_u = 4,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                areaType = CardTargetArea.Ring,
                range = 1,
                area = 1,
            },

            SetCardDescription = (User, data) =>
                data.cardDescription = $"AOE: Deal {data.Power} damage to adjacent enemies.",

            CardEffect = (User, Target, data) =>
            {
                // Damage is applied per resolved target around the user.
                CombatUtility.ApplyDamage(User, Target, data.Power);
            }
        });

        // 120103 – Lotus Death Kiss (Execute <10%) – complex
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120103,
            cardName = "Lotus Death Kiss",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 6,
            power_u = 10,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                areaType = CardTargetArea.Single,
                range = 1,
                area = 1,
            },

            SetCardDescription = (User, data) =>
                data.cardDescription = "Execute enemy below 10% HP (TODO: implement condition & kill).",

            CardEffect = (User, Target, data) =>
            {
                // TODO: If Target health < 10% -> execute/kill; else deal damage.
            }
        });

        // 120104 – Moonlit Needlestorm – complex (status)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120104,
            cardName = "Moonlit Needlestorm",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Ranged, CardIdentity.Blood, CardIdentity.Poison, CardIdentity.Fire },

            cost_u = 6,
            power_u = 1,
            repeats_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                areaType = CardTargetArea.Single, // if unsupported, switch to Single later
                range = 3,
                area = 1,
            },

            SetCardDescription = (User, data) =>
                data.cardDescription = "Throw needles at enemies and inflict status (TODO).",

            CardEffect = (User, Target, data) =>
            {
                // Fallback, falls duration_u noch nicht gesetzt ist
                var dur = data.Duration > 0 ? data.Duration : 6;
                var tick = data.Power;

                // POISON
                var poison = new FunctionModifier(
                    statName: "Poison",
                    baseValue: tick,
                    statScaling: ModifierScaling.Flat,
                    to_Trigger_refs: new() { gameplayRef.onPoison },
                    duration: dur,
                    target: Target.CurrentHealth,
                    triggerConditionRef: new TriggerRef
                    {
                        References = new() { gameplayRef.onTurnStart },
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    onRefEventAction: (mod, stat, toTrigger) =>
                    {
                        GameEvents.TriggerRefEvent(new TriggerRef
                        {
                            References = new() { gameplayRef.onPoison },
                            UserId = User.GetInstanceID(),
                            AffectedEntityId = Target.GetInstanceID()
                        });
                        CombatUtility.ApplyDamage(User, Target, mod.BaseValue);
                    });

                // BURN
                var burn = new FunctionModifier(
                    statName: "Burn",
                    baseValue: tick,
                    statScaling: ModifierScaling.Flat,
                    to_Trigger_refs: new() { gameplayRef.onBurn },
                    duration: dur,
                    target: Target.CurrentHealth,
                    triggerConditionRef: new TriggerRef
                    {
                        References = new() { gameplayRef.onTurnStart },
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    onRefEventAction: (mod, stat, toTrigger) =>
                    {
                        GameEvents.TriggerRefEvent(new TriggerRef
                        {
                            References = new() { gameplayRef.onBurn },
                            UserId = User.GetInstanceID(),
                            AffectedEntityId = Target.GetInstanceID()
                        });
                        CombatUtility.ApplyDamage(User, Target, mod.BaseValue);
                    });

                // BLEED
                var bleed = new FunctionModifier(
                    statName: "Bleed",
                    baseValue: tick,
                    statScaling: ModifierScaling.Flat,
                    to_Trigger_refs: new() { gameplayRef.onBleed },
                    duration: dur,
                    target: Target.CurrentHealth,
                    triggerConditionRef: new TriggerRef
                    {
                        References = new() { gameplayRef.onTurnStart },
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    onRefEventAction: (mod, stat, toTrigger) =>
                    {
                        GameEvents.TriggerRefEvent(new TriggerRef
                        {
                            References = new() { gameplayRef.onBleed },
                            UserId = User.GetInstanceID(),
                            AffectedEntityId = Target.GetInstanceID()
                        });
                        CombatUtility.ApplyDamage(User, Target, mod.BaseValue);
                    });

                CombatUtility.ApplyModifier(User, Target, Target.CurrentHealth, poison, ModifierMergeStrategy.RefreshIncrease);
                CombatUtility.ApplyModifier(User, Target, Target.CurrentHealth, burn, ModifierMergeStrategy.RefreshIncrease);
                CombatUtility.ApplyModifier(User, Target, Target.CurrentHealth, bleed, ModifierMergeStrategy.RefreshIncrease);
            }

        });

        // 120105 – Black Lotus Needle (Single, increased crit)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120105,
            cardName = "Black Lotus Needle",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Physical, CardIdentity.Ranged },

            cost_u = 5,
            power_u = 20,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                areaType = CardTargetArea.Single,
                range = 5,
                area = 1,
            },

            SetCardDescription = (User, data) =>
                data.cardDescription = $"Deal {data.Power} damage (increased crit; TODO).",

            CardEffect = (User, Target, data) =>
            {
                // Direct hit; crit handling later.
                CombatUtility.ApplyDamage(User, Target, data.Power);
            }
        });

        // 120106 – Moon Piercing Arrow (Line 2 targets)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120106,
            cardName = "Moon Piercing Arrow",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Ranged },

            cost_u = 5,
            power_u = 10,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                areaType = CardTargetArea.LineSelf,
                range = 3,
                area = 2,
            },

            SetCardDescription = (User, data) =>
                data.cardDescription = $"Pierce up to 2 enemies in a line for {data.Power} damage (TODO: cap=2).",

            CardEffect = (User, Target, data) =>
            {
                // Damage each target along the line; cap to 2 separately if needed.
                CombatUtility.ApplyDamage(User, Target, data.Power);
            }
        });

        // 120107 – Midnight Rain (AOE Sphere)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120107,
            cardName = "Midnight Rain",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 7,
            power_u = 5,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                areaType = CardTargetArea.Radius,
                range = 2,
                area = 1,
            },

            SetCardDescription = (User, data) =>
                data.cardDescription = $"AOE: Deal {data.Power} damage in a small area.",

            CardEffect = (User, Target, data) =>
            {
                CombatUtility.ApplyDamage(User, Target, data.Power);
            }
        });

        // 120108 – Bouncing Shot (Attack Bounce)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120108,
            cardName = "Bouncing Shot",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 3,
            power_u = 4,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                areaType = CardTargetArea.Single,
                range = 3,
                area = 1,
            },

            SetCardDescription = (User, data) =>
                data.cardDescription = $"AOE ring: Deal {data.Power} damage around you.",

            CardEffect = (User, Target, data) =>
            {
                CombatUtility.ApplyDamage(User, Target, data.Power);
                //To-Do Bouncing logic
            }
        });

        // 120109 – Barbed Needle Volley – complex selection + bleed
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120109,
            cardName = "Barbed Needle Volley",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Physical, CardIdentity.Blood },

            cost_u = 5,
            power_u = 7,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                areaType = CardTargetArea.Single, // TODO: selection targeting
                range = 3,
                area = 1,
            },

            SetCardDescription = (User, data) =>
                data.cardDescription = "Hit multiple enemies with needles; applies Bleed (TODO).",

            CardEffect = (User, Target, data) =>
            {
                CombatUtility.ApplyDamage(User, Target, data.Power);
                // TODO: multi-target selection + apply Bleed DoT like Fire Bomb.
            }
        });

        // 120110 – Serpent Coil Shot – immobilize (CC)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120110,
            cardName = "Serpent Coil Shot",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 2,
            power_u = 0,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                areaType = CardTargetArea.Single,
                range = 3,
                area = 1,
            },

            SetCardDescription = (User, data) =>
                data.cardDescription = "Immobilize an enemy (TODO).",

            CardEffect = (User, Target, data) =>
            {
                // TODO: Apply immobilize debuff.
            }
        });

        // 120111 – Merciful Headshot – stun (CC)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120111,
            cardName = "Merciful Headshot",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Physical,CardIdentity.Ranged },

            cost_u = 3,
            power_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                areaType = CardTargetArea.Single,
                range = 3,
                area = 1,
            },

            SetCardDescription = (User, data) =>
                data.cardDescription = "Stun an enemy (TODO).",

            CardEffect = (User, Target, data) =>
            {
                CombatUtility.ApplyDamage(User, Target, data.Power);
                // TODO: Apply stun.
            }
        });

        // 120112 – Crimson Thorn Array – Bleed AOE ring
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120112,
            cardName = "Crimson Thorn Array",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Blood },

            cost_u = 8,
            power_u = 8,
            duration_u = 6,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                areaType = CardTargetArea.Ring,
                range = 1,
                area = 1,
            },

            SetCardDescription = (User, data) =>
                data.cardDescription = $"Apply Bleed (DoT {data.Power} for {data.Duration}) to adjacent enemies.",

            CardEffect = (User, Target, data) =>
            {
                // DoT like Fire Bomb but using onBleed
                var bleed = new FunctionModifier(
                    statName: "Bleed",
                    baseValue: data.Power,
                    statScaling: ModifierScaling.Flat,
                    to_Trigger_refs: new() { gameplayRef.onBleed },
                    duration: data.Duration,
                    target: Target.CurrentHealth,
                    triggerConditionRef: new TriggerRef { References = new() { gameplayRef.onTurnStart }, AffectedEntityId = Target.GetInstanceID() },
                    onRefEventAction: (mod, stat, refEv) =>
                    {
                        GameEvents.TriggerRefEvent(new TriggerRef { References = new() { gameplayRef.onBleed }, UserId = User.GetInstanceID(), AffectedEntityId = Target.GetInstanceID() });
                        CombatUtility.ApplyDamage(User, Target, mod.BaseValue);
                    });
                CombatUtility.ApplyModifier(User, Target, Target.CurrentHealth, bleed, ModifierMergeStrategy.RefreshIncrease);
            }
        });
    }

    private static void RegisterAbilities()
    {
        // 120201 – Phantom Step – movement (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120201,
            cardName = "Phantom Step",
            cardType = CardType.Ability,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 0,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                areaType = CardTargetArea.Single,
                range = 2,
                area = 1,
            },

            SetCardDescription = (User, data) => data.cardDescription = "Move behind an enemy (TODO).",
            CardEffect = (User, Target, data) => { /* TODO movement */ }
        });

        // 120202 – Apply Scorching Blood Venom – ignite next attack (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120202,
            cardName = "Apply Scorching Blood Venom",
            cardType = CardType.Ability,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Fire },

            cost_u = 0,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                areaType = CardTargetArea.Single,
                range = 0,
                area = 1,
            },

            SetCardDescription = (User, data) => data.cardDescription = "Next attack inflicts Ignite (TODO).",
            CardEffect = (User, Target, data) => { /* TODO next-attack buff */ }
        });

        // 120203 – Apply Black Lotus Venom – poison next attack (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120203,
            cardName = "Apply Black Lotus Venom",
            cardType = CardType.Ability,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Poison },

            cost_u = 0,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                areaType = CardTargetArea.Single,
                range = 0,
                area = 1,
            },

            SetCardDescription = (User, data) => data.cardDescription = "Next attack inflicts Poison (TODO).",
            CardEffect = (User, Target, data) => { /* TODO next-attack buff */ }
        });

        // 120204 – Apply Dazzlying Numbing Venom – stun next attack (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120204,
            cardName = "Apply Dazzlying Numbing Venom",
            cardType = CardType.Ability,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 0,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                areaType = CardTargetArea.Single,
                range = 0,
                area = 1,
            },

            SetCardDescription = (User, data) => data.cardDescription = "Next attack inflicts Stun (TODO).",
            CardEffect = (User, Target, data) => { /* TODO next-attack buff */ }
        });

        // 120205 – Eye of the Nighthawk – dmg/crit up (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120205,
            cardName = "Eye of the Nighthawk",
            cardType = CardType.Ability,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 0,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                areaType = CardTargetArea.Single,
                range = 0,
                area = 1,
            },

            SetCardDescription = (User, data) => data.cardDescription = "Increase damage/crit (TODO).",
            CardEffect = (User, Target, data) => { /* TODO buff */ }
        });

        // 120206 – Reapply Venom – reapplies last venom (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120206,
            cardName = "Reapply Venom",
            cardType = CardType.Ability,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 0,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                areaType = CardTargetArea.Single,
                range = 0,
                area = 1,
            },

            SetCardDescription = (User, data) => data.cardDescription = "Reapply the last venom used (TODO).",
            CardEffect = (User, Target, data) => { /* TODO */ }
        });
    }

    private static void RegisterSpells()
    {
        // no explicit damage spells listed for Assassin; add later if needed
    }

    private static void RegisterCurses()
    {
        // 120401 – Fumble – self-random (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120401,
            cardName = "Fumble",
            cardType = CardType.Curse,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.None },
            cost_u = 0,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                areaType = CardTargetArea.Single,
                range = 0,
                area = 1,
            },
            SetCardDescription = (User, data) => data.cardDescription = "Randomly inflict a negative effect on yourself (TODO).",
            CardEffect = (User, Target, data) => { /* TODO */ }
        });
    }

    private static void RegisterBlessings()
    {
        // 120501 – Lucky Strike – next attack repeats (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120501,
            cardName = "Lucky Strike",
            cardType = CardType.Blessing,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.None },
            cost_u = 0,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                areaType = CardTargetArea.Single,
                range = 0,
                area = 1,
            },
            SetCardDescription = (User, data) => data.cardDescription = "Next attack repeats again (TODO).",
            CardEffect = (User, Target, data) => { /* TODO */ }
        });
    }
}
