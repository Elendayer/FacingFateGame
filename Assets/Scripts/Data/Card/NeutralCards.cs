using System.Collections.Generic;
using UnityEngine;
using Utility;

// Neutral cards (Class = 10)
// ID-Schema: 10 | TT | II   (TT: MartialArt=01, Ability=02, Spell=03, Curse=04, Blessing=05, Item=06)
public static class NeutralCards
{
    public static void RegisterAll()
    {
        RegisterMartialArts();
        RegisterAbilities();
        RegisterItems();
    }

    private static void RegisterMartialArts()
    {
        // 100101 – Strike – normal attack
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100101,
            cardName = "Strike",
            cardType = CardType.Technique,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.Melee },

            cost_u = 1,
            damage_u = 10,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = $"Deal {d.Damage} damage.",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(User, Target, d.Damage);
            }
        });

        // 100102 – Heavy Blow – slow heavy hit (TODO: -1 Movement)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100102,
            cardName = "Heavy Blow",
            cardType = CardType.Technique,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical },

            cost_u = 5,
            damage_u = 15,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Deal 15 damage. (TODO: apply -1 Movement)",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(User, Target, d.Damage);
                // TODO: Movement-Cost/Range senken
            }
        });

        // 100103 – Quick Jab – fast poke
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100103,
            cardName = "Quick Jab",
            cardType = CardType.Technique,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical },

            cost_u = 1,
            damage_u = 5,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Deal 5 damage.",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(User, Target, d.Damage);
            }
        });

        // 100104 – Double Cut – strike twice
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100104,
            cardName = "Double Cut",
            cardType = CardType.Technique,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical },

            cost_u = 2,
            damage_u = 3,
            repeats_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Hit the target twice.",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(User, Target, d.Damage);
                // Wiederholungen werden vom System abgewickelt
            }
        });

        // 100105 – Shove – push 1 space (minor damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100105,
            cardName = "Shove",
            cardType = CardType.Technique,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.Melee },

            cost_u = 2,
            damage_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Push enemy 1 space (deals minor damage).",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(User, Target, d.Damage);
                // TODO: 1 Feld wegschieben
            }
        });

        // 100106 – Charge – move both 1 space (maybe Stun)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100106,
            cardName = "Charge",
            cardType = CardType.Technique,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical },

            cost_u = 5,
            damage_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Charge and move both 1 space. (TODO: stun)",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(User, Target, d.Damage);
                // TODO: User/Target bewegen und Stun anwenden
            }
        });

        // 100107 – Step Back – disengage after attack (Self)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100107,
            cardName = "Step Back",
            cardType = CardType.Technique,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 2,
            damage_u = 0,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
                range = 0,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Disengage after your attack (step back).",
            CardEffect = (User, Target, d) =>
            {
                // TODO: User 1 Feld rückwärts bewegen
            }
        });

        // 100108 – Bite – small hit
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100108,
            cardName = "Bite",
            cardType = CardType.Technique,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical },

            cost_u = 2,
            damage_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
                range = 2,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Deal 1 damage.",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(User, Target, d.Damage);
            }
        });

        // 100109 – Gnaw – bite until bleed (repeat 2) – TODO: Bleed DoT
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100109,
            cardName = "Gnaw",
            cardType = CardType.Technique,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.Melee, CardIdentity.Blood, CardIdentity.Physical },

            cost_u = 5,
            damage_u = 1,
            repeats_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
                range = 2,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Bite multiple times (applies Bleed – TODO).",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(User, Target, d.Damage);
                // TODO: Bleed-DoT wie Poison/Burn anwenden (CardIdentity.Blood)
            }
        });

        // 100110 – Sting – damage + Poison DoT
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100110,
            cardName = "Sting",
            cardType = CardType.Technique,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical, CardIdentity.Poison },

            cost_u = 5,
            damage_u = 5,
            duration_u = 6,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
                range = 2,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Deal 5 damage and apply Poison for 6 turns.",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(User, Target, d.Damage);

                var poison = new EntityModifier(
                    statName: "Poison",
                    baseValue: d.Damage,
                    to_Trigger_refs: new() { gameplayRef.onPoison },
                    duration: d.Duration,
                    target: Target.entityStats.CurrentHealth,
                    triggerConditionRef: new TriggerRef
                    {
                        References = new() { gameplayRef.onTurnStart },
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    onRefEventAction: (mod, stat, ev) =>
                    {
                        GameEvents.TriggerRefEvent(new TriggerRef
                        {
                            References = new() { gameplayRef.onPoison },
                            UserId = User.GetInstanceID(),
                            AffectedEntityId = Target.GetInstanceID()
                        });
                        CombatUtility.ApplyDamage(User, Target, mod.BaseValue);
                    });
                CombatUtility.ApplyEntityModifier(User, Target, poison, ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

        // 100111 – Arrowshot – ranged hit
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100111,
            cardName = "Arrowshot",
            cardType = CardType.Technique,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.Ranged, CardIdentity.Physical },

            cost_u = 2,
            damage_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
                range = 3,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Shoot an arrow dealing 3 damage.",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(User, Target, d.Damage);
            }
        });

        // 100112 – Multishot – multi-target (repeat 2)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100112,
            cardName = "Multishot",
            cardType = CardType.Technique,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.Ranged },

            cost_u = 5,
            damage_u = 3,
            repeats_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Select,
                range = 3,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Shoot multiple arrows at selected enemies (2 shots).",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(User, Target, d.Damage);
            }
        });
    }

    private static void RegisterAbilities()
    {
        // 100201 – Focus – empower next attack (Self)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100201,
            cardName = "Focus",
            cardType = CardType.Ability,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 3,
            damage_u = 0,
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
                range = 0,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Your next attack is empowered (TODO).",
            CardEffect = (User, Target, d) => { /* TODO: One-hit buff */ }
        });

        // 100202 – Growl – demoralize enemies (AOE debuff)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100202,
            cardName = "Growl",
            cardType = CardType.Ability,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 3,
            power_u = 3, // stat debuff (non-damage)
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Radius,
                range = 3,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Demoralize enemies in an area (reduce stats).",
            CardEffect = (User, Target, d) => { /* TODO: Apply debuff to enemies */ }
        });

        // 100203 – Howl – improve allies' stats (AOE buff)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100203,
            cardName = "Howl",
            cardType = CardType.Ability,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 3,
            power_u = 3, // stat buff (non-damage)
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Radius,
                range = 3,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Bolster allies in range (raise stats).",
            CardEffect = (User, Target, d) => { /* TODO: Apply buff to allies */ }
        });

        // 100204 – Guard Up – raise defense until end of turn
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100204,
            cardName = "Guard Up",
            cardType = CardType.Ability,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 2,
            power_u = 0,
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
                range = 0,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Increase your defense until end of turn.",
            CardEffect = (User, Target, d) => { /* TODO: Armor up */ }
        });
    }

    private static void RegisterItems()
    {
        // 100601 – Throw Poison – Single; apply Poison DoT
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100601,
            cardName = "Throw Poison",
            cardType = CardType.Item,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.Poison, CardIdentity.Ranged },

            cost_u = 3,
            damage_u = 2,
            duration_u = 6,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Radius,
                range = 3,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Apply Poison 2 for 6 turns.",
            CardEffect = (User, Target, d) =>
            {
                var poison = new EntityModifier(
                    statName: "Poison",
                    baseValue: d.Damage,
                    to_Trigger_refs: new() { gameplayRef.onPoison },
                    duration: d.Duration,
                    target: Target.entityStats.CurrentHealth,
                    triggerConditionRef: new TriggerRef
                    {
                        References = new() { gameplayRef.onTurnStart },
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    onRefEventAction: (mod, stat, ev) =>
                    {
                        GameEvents.TriggerRefEvent(new TriggerRef
                        {
                            References = new() { gameplayRef.onPoison },
                            UserId = User.GetInstanceID(),
                            AffectedEntityId = Target.GetInstanceID()
                        });
                        CombatUtility.ApplyDamage(User, Target, mod.BaseValue);
                    });
                CombatUtility.ApplyEntityModifier(User, Target, poison, ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

        // 100602 – Throw Firebomb – Single; apply Burn DoT
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100602,
            cardName = "Throw Firebomb",
            cardType = CardType.Item,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.Fire, CardIdentity.Ranged },

            cost_u = 2,
            damage_u = 3,
            duration_u = 6,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Radius,
                range = 3,
                area = 2,
            },

            CardDescription = (User, d) => d.cardDescription = "Apply Burn 2 for 6 turns.",
            CardEffect = (User, Target, d) =>
            {
                var burn = new EntityModifier(
                    statName: "Burn",
                    baseValue: d.Damage,
                    to_Trigger_refs: new() { gameplayRef.onBurn },
                    duration: d.Duration,
                    target: Target.entityStats.CurrentHealth,
                    triggerConditionRef: new TriggerRef
                    {
                        References = new() { gameplayRef.onTurnStart },
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    onRefEventAction: (mod, stat, ev) =>
                    {
                        GameEvents.TriggerRefEvent(new TriggerRef
                        {
                            References = new() { gameplayRef.onBurn },
                            UserId = User.GetInstanceID(),
                            AffectedEntityId = Target.GetInstanceID()
                        });
                        CombatUtility.ApplyDamage(User, Target, mod.BaseValue);
                    });
                CombatUtility.ApplyEntityModifier(User, Target, burn, ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });
    }
}
