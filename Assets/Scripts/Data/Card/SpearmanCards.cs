using System.Collections.Generic;
using UnityEngine;
using Utility;

public static class SpearmanCards
{
    public static void RegisterAll()
    {
        RegisterMartialArts();
        RegisterAbilities();
        RegisterCurses();
        RegisterBlessings();
    }

    private static void RegisterMartialArts()
    {
        // 110101 – Tempest of a Hundred Spears (Ring 1, repeats=2)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110101,
            cardName = "Tempest of a Hundred Spears",
            cardType = CardType.Technique,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 20,
            damage_u = 75,
            repeats_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Ring,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"Twice deal {d.Damage} damage to adjacent enemies.";
            },

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target, d.Damage);
                Debug.Log("Tempest of a Hundred Spears used");
            }
        });

        // 110102 – Piercing Light (LineSelf 3)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110102,
            cardName = "Piercing Light",
            cardType = CardType.Technique,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 25,
            damage_u = 120,
            range_u = 3,
            area_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.LineSelf,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"Deal {d.Damage} damage to all enemies in a line (range {d.Range}).";
            },

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target, d.Damage);
            }
        });

        // 110103 – Sky-Piercing Leap (LineSelf 2)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110103,
            cardName = "Sky-Piercing Leap",
            cardType = CardType.Technique,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 30,
            damage_u = 85,
            range_u = 4,
            power_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.LineSelf,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"Deal {d.Damage} damage along a short line (range {d.Range}).";
            },

            CardEffect = (User, Target, d) =>
            {
                MovementUtility.ForcedMove(ForcedMovementType.Push, User, User.GetComponent<EntityOnMap>().currentCell, d.Power);
                CombatUtility.ApplyDamage(d, Target, d.Damage);
                // To-Do: Movement nach Vorne
            }
        });

        // 110104 – Heaven Piercing Spear (Single, Range 1) – Bleed DoT + immediate tick
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110104,
            cardName = "Heaven Piercing Spear",
            cardType = CardType.Technique,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical, CardIdentity.Blood },

            cost_u = 50,
            damage_u = 80,
            duration_u = 3,
            range_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"Apply Bleed dealing {d.Damage} for {d.Duration} turns (immediate tick).";
            },

            CardEffect = (User, Target, d) =>
            {
                string name = $"Bleed#{d.cardID}";
                var bleed = new EntityModifier(
                    statName: "Bleed",
                    baseValue: d.Damage,
                    to_Trigger_refs: new() { GameplayRef.onBleed },
                    duration: d.Duration,
                    target: Target.entityStats.CurrentHealth,
                    triggerConditionRef: new TriggerRef
                    {
                        References = new() { GameplayRef.onTurnStart },
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    onRefEventAction: (mod, stat, ev) =>
                    {
                        GameEvents.TriggerRefEvent(new TriggerRef
                        {
                            References = new() { GameplayRef.onBleed },
                            UserId = User.GetInstanceID(),
                            AffectedEntityId = Target.GetInstanceID()
                        });
                        CombatUtility.ApplyDamage(d, Target, mod.BaseValue);
                    }
                );

                CombatUtility.ApplyEntityModifier(d, Target, bleed, ModifierMergeStrategy.RefreshDurationAndMerge);

                // immediate tick on play
                CombatUtility.ApplyDamage(d, Target, d.Damage);
                GameEvents.TriggerRefEvent(new TriggerRef
                {
                    References = new() { GameplayRef.onBleed },
                    UserId = User.GetInstanceID(),
                    AffectedEntityId = Target.GetInstanceID()
                });
            }
        });

        // 110105 – Earth-Sundering Sweep (Ring 1)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110105,
            cardName = "Earth-Sundering Sweep",
            cardType = CardType.Technique,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 70,
            damage_u = 250,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Ring,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"Deal {d.Damage} damage to adjacent enemies.";
            },

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target, d.Damage);
            }
        });

        // 110106 – Dragon Fang Thrust (Single, Range 1)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110106,
            cardName = "Dragon Fang Thrust",
            cardType = CardType.Technique,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 25,
            damage_u = 50,
            range_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"Deal {d.Damage} damage.";
            },

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target, d.Damage);
            }
        });

        // 110107 – Dragon's Tail Sweep (Single)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110107,
            cardName = "Dragon's Tail Sweep",
            cardType = CardType.Technique,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 5,
            damage_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"Deal {d.Damage} damage.";
            },

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target, d.Damage);
                // To-Do Slow
            }
        });

        // 110108 – Earthshatter Pole (Ring 1)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110108,
            cardName = "Earthshatter Pole",
            cardType = CardType.Technique,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 100,
            damage_u = 200,
            area_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Ring,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"Deal {d.Damage} damage to adjacent enemies.";
            },

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target, d.Damage);
                // Slow all Enemies oder Stun
            }
        });

        // 110109 – Azure Dragon's Roar (Self; until end of turn)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110109,
            cardName = "Azure Dragon's Roar",
            cardType = CardType.Technique,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 10,
            power_u = 10,
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"Increses attack damage by {d.Power}.";
            },

            CardEffect = (User, Target, d) =>
            {
                var mod = new StatModifier(
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration,
                    on_triggerConditionRef: new TriggerRef
                    {
                        References = new() { GameplayRef.onTurnStart },   
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    name: $"DamageIncrease#{d.cardID}");                   

                CombatUtility.ApplyBuff(d, Target, Target.entityStats.DamageIncrease, mod,
                    ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

        // 110110 – Pillar of the Earth (LineSelf 4)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110110,
            cardName = "Pillar of the Earth",
            cardType = CardType.Technique,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 100,
            damage_u = 100,
            range_u = 3,
            area_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.LineFree,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"Deal {d.Damage} damage along a long line (range {d.Range}).";
            },

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target, d.Damage);
            }
        });
    }

    private static void RegisterAbilities()
    {
        // 110201 – Extending Heaven's Lance (Self; +range until end of turn)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110201,
            cardName = "Extending Heaven's Lance",
            cardType = CardType.Ability,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 20,
            power_u = 2,
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"Increase melee range by +1 until end of turn.";
            },

            CardEffect = (User, Target, d) =>
            {

                var mod = new StatModifier(
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration,
                    on_triggerConditionRef: new TriggerRef
                    {
                        References = new() { GameplayRef.onTurnStart },
                        AffectedEntityId = User.GetInstanceID()
                    },
                    name: $"MeleeRangeIncrease"
                );

                CombatUtility.ApplyBuff(d, Target, Target.entityStats.RangeIncrease, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
            }     
        });

        // 110202 – Iron Wall Reversal (Self; fixed melee counter once)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110202,
            cardName = "Iron Wall Reversal",
            cardType = CardType.Ability,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 2,
            power_u = 10,
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"On next melee hit taken this turn, counter for {d.Power}.";
            },

            CardEffect = (User, Target, d) =>
            {
                // Counter Melee
            }
        });

        // 110203 – Whirling Heaven Ward (Self; deflect ranged once)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110203,
            cardName = "Whirling Heaven Ward",
            cardType = CardType.Ability,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 2,
            power_u = 0,
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"On next ranged hit this turn, deflect/negate (details TBD).";
            },

            CardEffect = (User, Target, d) =>
            {
                //To-Do Deflect Ranged
            }
        });

        // 110204 – Unyielding Spear Stance (Self; Taunt + Armour +10 for 1 turn)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110204,
            cardName = "Unyielding Spear Stance",
            cardType = CardType.Ability,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 20,
            power_u = 50,
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"Taunt and gain {d.Power} Armour for {d.Duration} turn.";
            },

            CardEffect = (User, Target, d) =>
            {
                var mod = new StatModifier(
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration,
                    on_triggerConditionRef: new TriggerRef
                    {
                        References = new() { GameplayRef.onTurnStart },
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    name: $"ArmourIncrease#{d.cardID}");

                    CombatUtility.ApplyBuff(d, Target, Target.entityStats.DamageIncrease, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
                // Apply Taunt
            }
        });

        // 110205 – Sky-Rending Reversal (Self; stronger fixed counter)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110205,
            cardName = "Sky-Rending Reversal",
            cardType = CardType.Ability,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 8,
            power_u = 10,
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"On next hit this turn, counter for {d.Power}.";
            },

            CardEffect = (User, Target, d) =>
            {
                // Damage zurückwerfen
            }
        });

        // 110206 – Phalanx Guard (Self; defensive placeholder)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110206,
            cardName = "Phalanx Guard",
            cardType = CardType.Ability,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 100,
            power_u = 25,
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"Increses armour by {d.Power}) for adjacent allies and gives them thorns.";
            },

            CardEffect = (User, Target, d) =>
            {
                var mod = new StatModifier(
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration,
                    on_triggerConditionRef: new TriggerRef
                    {
                        References = new() { GameplayRef.onTurnStart },
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    name: $"ArmourIncrease#{d.cardID}");
                //To-DO Thorns
            }
        });
    }

    private static void RegisterCurses()
    {
        // 110401 – Brittle Courage (Self)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110401,
            cardName = "Brittle Courage",
            cardType = CardType.Curse,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 0,
            power_u = -50,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"Reduce Armour by {d.Power}%.";
            },

            CardEffect = (User, Target, d) =>
            {
                var stat = Target.entityStats.DamageIncrease;
                var mod = new StatModifier(
                    value: d.Power,
                    scaling: ModifierScaling.Percent,
                    duration: d.Duration,
                    on_triggerConditionRef: new TriggerRef
                    {
                        References = new() { GameplayRef.onTurnStart },
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    name: $"ArmourIncrease#{d.cardID}");

                CombatUtility.ApplyBuff(    d, Target, stat, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });
    }

    private static void RegisterBlessings()
    {
        // 110501 – Brilliant Spear (Self)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110501,
            cardName = "Brilliant Spear",
            cardType = CardType.Blessing,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 0,
            power_u = 100,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"Increase Aggro and attack power by {d.Power}% while active.";
            },

            CardEffect = (User, Target, d) =>
            {
                var stat = Target.entityStats.DamageIncrease;
                var mod = new StatModifier(
                    value: d.Power,
                    scaling: ModifierScaling.Percent,
                    duration: d.Duration,
                    on_triggerConditionRef: new TriggerRef
                    {
                        References = new() { GameplayRef.onTurnStart },
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    name: $"AttackIncrease#{d.cardID}");

                CombatUtility.ApplyBuff(d, Target, stat, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
                
                // To-Do Increase Aggro
            }
        });
    }
}
