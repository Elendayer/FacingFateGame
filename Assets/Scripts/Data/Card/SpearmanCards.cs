using System.Collections.Generic;
using UnityEngine;
using Utility;

// Spearman cards in Fire Bomb style (explicit RegisterCard with CardData initializers).
// Call SpearmanCards.RegisterAll() from CardDatabase.RegisterAll() or your startup.
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

            cost_u = 1,
            power_u = 3,
            repeats_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Ring,
                range = 1,
                area = 1,
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Twice deal {data.Power} damage to adjacent enemies.";
            },

            CardEffect = (User, Target, data) =>
            {
                // Resolve twice: each time fetch adjacent enemies and deal data.Power to each.
                CombatUtility.ApplyDamage(User, Target, data.Power);
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

            cost_u = 5,
            power_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.LineSelf,
                range = 3,
                area = 2,
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Deal {data.Power} damage to all enemies in a line (range {data.targetingData.range}).";
            },

            CardEffect = (User, Target, data) =>
            {
                CombatUtility.ApplyDamage(User, Target, data.Power);
                // Fetch all enemies along a 3-tile line from User and deal data.Power to each.
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

            cost_u = 3,
            power_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.LineSelf,
                range = 2,
                area = 1,
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Deal {data.Power} damage along a short line (range {data.targetingData.range}).";
            },

            CardEffect = (User, Target, data) =>
            {
                CombatUtility.ApplyDamage(User, Target, data.Power);
                // Fetch enemies along a 2-tile line from User; deal data.Power to each. (No movement/knockback.)
            }
        });

        // 110104 – Heaven Piercing Spear (Single, Range 1) – Bleed DoT
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110104,
            cardName = "Heaven Piercing Spear",
            cardType = CardType.Technique,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical, CardIdentity.Blood },

            cost_u = 3,
            power_u = 10,
            duration_u = 6,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 3,
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Apply Bleed dealing {data.Power} for {data.Duration} turns.";
            },

            CardEffect = (User, Target, data) =>
            {
                // Create a Bleed FunctionModifier like Fire Bomb: tick on onTurnStart for data.Duration; deal data.Power each tick.
                // Merge strategy should be RefreshIncrease.

                var bleed = new EntityModifier(
                    statName: "Bleed",
                    baseValue: data.Power,
                    to_Trigger_refs: new() { GameplayRef.onBleed },
                    duration: data.Duration,
                    target: Target.entityStats.CurrentHealth,
                    triggerConditionRef: new TriggerRef
                    {
                        References = new() { GameplayRef.onTurnStart },
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    onRefEventAction: (modifier, stat, toTrigger_Reference) =>
                    {
                        GameEvents.TriggerRefEvent(new TriggerRef
                        {
                            References = new() { GameplayRef.onBleed },
                            UserId = User.GetInstanceID(),
                            AffectedEntityId = Target.GetInstanceID()
                        });
                        CombatUtility.ApplyDamage(User, Target, modifier.BaseValue);
                    }
                    );

                CombatUtility.ApplyEntityModifier(User, Target,  bleed, ModifierMergeStrategy.RefreshDurationAndMerge);
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

            cost_u = 5,
            power_u = 5,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Ring,
                range = 1,
                area = 3,
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Deal {data.Power} damage to adjacent enemies.";
            },

            CardEffect = (User, Target, data) =>
            {
                CombatUtility.ApplyDamage(User, Target, data.Power);
                // Fetch all adjacent enemies (ring=1) and deal data.Power to each. No knockback.
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

            cost_u = 2,
            power_u = 10,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Deal {data.Power} damage.";
            },

            CardEffect = (User, Target, data) =>
            {
                CombatUtility.ApplyDamage(User, Target, data.Power);
                // Deal data.Power direct damage to Target.
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
            power_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Deal {data.Power} damage.";
            },

            CardEffect = (User, Target, data) =>
            {
                CombatUtility.ApplyDamage(User, Target, data.Power);
                // Single-target damage: deal data.Power to Target. No knockback.
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

            cost_u = 10,
            power_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Ring,
                range = 1,
                area = 1,
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Deal {data.Power} damage to adjacent enemies.";
            },

            CardEffect = (User, Target, data) =>
            {
                CombatUtility.ApplyDamage(User, Target, data.Power);
                // Fetch all adjacent enemies and deal data.Power to each.
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

            cost_u = 3,
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

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Self-buff until end of turn (e.g., Taunt/Damage up).";
            },

            CardEffect = (User, Target, data) =>
            {
               
                // Self-buff until end of turn (comment-only as requested). No concrete stat changes yet.
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

            cost_u = 10,
            power_u = 4,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.LineSelf,
                range = 4,
                area = 1,
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Deal {data.Power} damage along a long line (range {data.targetingData.range}).";
            },

            CardEffect = (User, Target, data) =>
            {
                CombatUtility.ApplyDamage(User, Target, data.Power);
                // Fetch enemies along a 4-tile line from User; deal data.Power to each.
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

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Increase melee range by +1 until end of turn.";
            },

            CardEffect = (User, Target, data) =>
            {
                // Add temporary +1 melee range to User until end of turn.
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
                range = 0,
                area = 1,
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"On next melee hit taken this turn, counter for {data.Power}.";
            },

            CardEffect = (User, Target, data) =>
            {
                
                // Give User 1 counter charge for this turn; when hit by melee, deal data.Power to attacker.
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
                range = 0,
                area = 1,
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"On next ranged hit this turn, deflect/negate (details TBD).";
            },

            CardEffect = (User, Target, data) =>
            {
               
                // Give User 1 ranged-deflect charge for this turn; exact behavior TBD.
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

            cost_u = 4,
            power_u = 10,
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
                range = 0,
                area = 1,
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Taunt and gain +10 Armour for 1 turn.";
            },

            CardEffect = (User, Target, data) =>
            {
               
                // Apply Taunt to User and +10 Armour for 1 turn.
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
                range = 0,
                area = 1,
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"On next hit this turn, counter for {data.Power}.";
            },

            CardEffect = (User, Target, data) =>
            {
                
                // Give User 1 counter charge for this turn; when hit by any attack, deal data.Power to attacker.
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

            cost_u = 5,
            power_u = 1,
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
                range = 0,
                area = 1,
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Defensive stance this turn (mitigation {data.Power}).";
            },

            CardEffect = (User, Target, data) =>
            {
                // Reduce incoming damage by data.Power for this turn (exact rule TBD).
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
            power_u = 10,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
                range = 0,
                area = 1,
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Reduce Armour by 50% (details TBD).";
            },

            CardEffect = (User, Target, data) =>
            {
                // Reduce Target Armour by 50% (duration/stacking TBD).
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
            power_u = 10,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
                range = 0,
                area = 1,
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Increase Aggro and attack power while active (details TBD).";
            },

            CardEffect = (User, Target, data) =>
            {
                // Increase Aggro and attack power while active (exact scaling/duration TBD).
            }
        });
    }
}