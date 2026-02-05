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
            damage_u = 20,
            repeats_u = 2,

            range_u = 16,
            radius_u = 3,
            area_u = 1,

            targetingData = new()
            {
                TargetingUsesVision = true,
                EffectUsesVision = true,
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Radius,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = "Deal {Damage} damage {Repeats} times";
            },

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target);
            },
            CardEffectGround = (User, Target, d) =>
            {
                AssetManager.Instance.CreateFX("SpearFx", Target);
            }
        });

        // 110102 – Piercing Light (LineSelf 3)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110102,
            cardName = "Piercing Light",
            cardType = CardType.Technique,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical, CardIdentity.Melee },

            cost_u = 25,
            damage_u = 50,

            range_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.LineSelf,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = "Deal {Damage} damage";
            },

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target);
            },
            CardEffectGround = (User, Target, d) =>
            {
                AssetManager.Instance.CreateFX("SpearFx", Target);
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
            power_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = "Deal {Damage} damage";
            },

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target);
            },
            CardEffectGround = (User, Target, d) =>
            {
                MovementUtility.ForcedMove(ForcedMovementType.Targeted, User, Target, d.Power, 100f);
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

            cost_u = 10,
            damage_u = 4,

            range_u = 4,
            maxtarget_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Select,
            },

            CardAiBias = new()
            {
                DamageOverride = 40,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = "Apply Bleed dealing {Damage} for {Duration} turns";
            },

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyEntityModifier(d, Target, EffectDatabase.GetEffectByName("Bleed", CloneMode.Defaults, d, ThroughputSource.Damage, User),ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

        // 110105 – Salamander Sweep (Cone 1)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110105,
            cardName = "Salamander Tail Sweep",
            cardType = CardType.Technique,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 20,
            damage_u = 30,

            range_u = 2,
            area_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Cone,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = "Deal {Damage} damage";
            },

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target);
            },
            CardEffectGround = (User, Target, d) =>
            {
                AssetManager.Instance.CreateFX("FirestormFx", Target);
            }
        });

        // 110106 – Dragon Fang Thrust (Cone)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110106,
            cardName = "Snake Tail Sweep",
            cardType = CardType.Technique,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 20,
            damage_u = 5,

            //Slow Movement Cost increase
            power_u = 1,

            range_u = 3,
            area_u = 3,


            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Cone,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = "Deal {Damage} damage.";
            },

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target);

                CombatUtility.ApplyStatDebuff(d, Target,
                    new StatModifier
                    (
                        stat: Target.entityStats.MovementCostModifier,
                        value: d.Power,
                        scaling: ModifierScaling.Flat,
                        duration: d.Duration,
                        to_TriggerRefs: new() { GameplayRef.onSlowed},
                        name: $"MovementCostIncrease"
                    ),
                    ModifierMergeStrategy.RefreshDurationAndOverride);
            }
        });

        // 110107 – Dragon Tail Sweep (cone)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110107,
            cardName = "Dragon Tail Sweep",
            cardType = CardType.Technique,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 30,
            damage_u = 25,

            range_u = 4,
            area_u = 4,


            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Cone,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = "Deal {Damage} damage.";
            },

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target);
                // To-Do Slow
            }
        });

        // 110108 – Earthshatter Pole 
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110108,
            cardName = "Earthshatter Pole",
            cardType = CardType.Technique,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 40,
            damage_u = 90,

            range_u = 3,
            area_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Radius,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = "Deal {Damage} damage";
            },

            CardEffect = (User, Target, d) =>
            {
                 CombatUtility.ApplyDamage(d, Target, d.Damage);
            },
            CardEffectGround = (User, Target, d) =>
            {
                AssetManager.Instance.CreateFX("FirestormFx", Target);
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
                cardTargetingMode = CardTargetingMode.Single,
            },

            CardAiBias = new()
            {
                PowerOverride = 80,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = "Increses attack damage by {Power}.";
            },

            CardEffect = (User, Target, d) =>
            {
                var mod = new StatModifier
                (
                    stat: Target.entityStats.DamageOutModifier,
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration,
                    name: $"DamageIncrease"
                    );

                Debug.Log("Applying Azure Dragon's Roar buff: +{Power} Damage for {Duration} turns.");

                CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndOverride);
            }
        });

        // 110110 – Pillar of the Earth (Radius Stun)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 110110,
            cardName = "Pillar of the Earth",
            cardType = CardType.Technique,
            cardClass = CardClass.Spearman,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 36,
            damage_u = 60,

            duration_u = 2,

            range_u = 2,
            radius_u = 2,
            area_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Radius,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = "Deal {Damage} damage";
            },

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyEntityModifier(d, Target, new EntityModifier
                    (
                        modifierName: "Rooted",
                        owner: Target,
                        baseValue: d.Power,
                        duration: d.Duration,
                        onApply_Action: (target,cd,value) =>
                        {
                            target.entityStats.IsRooted = true;
                        },
                        onRemove_Action: (target, cd, value) =>
                        {
                            target.entityStats.IsRooted = false;
                        }
                    ),
                 ModifierMergeStrategy.RefreshDurationAndMerge);

                CombatUtility.ApplyDamage(d, Target);
            },
            CardEffectGround = (User, Target, d) =>
            {
                AssetManager.Instance.CreateFX("SpearFx", Target);
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
                cardTargetingMode = CardTargetingMode.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = "Increase melee range by {Power} until end of turn.";
            },

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyStatBuff(d, Target,
                    new StatModifier(
                        stat: Target.entityStats.RangeModifier,
                        value: d.Power,
                        scaling: ModifierScaling.Flat,
                        condition: (e, c) => c.cardIdentities.Contains(CardIdentity.Melee) && c.range_u > 1,
                        to_TriggerRefs: new() { },
                        duration: d.Duration,
                        name: $"MeleeRangeIncrease"
                        ),
                    ModifierMergeStrategy.RefreshDurationAndMerge
                    );
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

            cost_u = 22,
            damage_u = 50,
            duration_u = 1,
            charges_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                cardTargetingMode = CardTargetingMode.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = "On next hit taken this round, counter for {Damage}.";
            },

            CardEffect = (User, Target, d) =>
            {
                var mod = new EntityModifier(
                    modifierName: "SpearmanIronWallReversalCounter",
                    owner: Target,
                    baseValue: d.Damage,
                    toTriggerRefs: new(),
                    duration: d.Duration,
                    charges: d.Charges,
                    onRef_Trigger: new RelevantTriggerCheck
                    {
                        OnTriggerReference = new() { GameplayRef.onHitLanded },
                        CheckType = CheckEntityType.Target,
                        CheckEntity = User,
                    },
                    onRef_Action: (target, cd, value) =>
                    {
                        Debug.Log($"Spearman Iron Wall Reversal counter triggered for {value} damage.");
                        CombatUtility.ApplyEffectDamage(value, cd.Owner, GameplayRef.onCounterRecieved);
                    }
                );
                CombatUtility.ApplyEntityModifier(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
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

            cost_u = 20,
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                cardTargetingMode = CardTargetingMode.Single,
            },

            CardAiBias = new()
            {
                PowerOverride = 80,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"On next ranged hit this round reduce damage 100%";
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

            range_u = 2,

            cost_u = 20,
            power_u = 50,
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = "Taunt target and gain {Power} Armour for {Duration} turn.";
            },

            CardEffect = (User, Target, d) =>
            {
                // Apply Armour
                CombatUtility.ApplyStatBuff(d, User,
                    new StatModifier
                    (
                    stat: User.entityStats.Armour,
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration,
                    name: $"ArmourIncrease"
                    ),
                    ModifierMergeStrategy.RefreshDurationAndMerge);

                // Apply Taunt
                CombatUtility.ApplyEntityModifier(d, Target, EffectDatabase.GetEffectByName("Taunted", CloneMode.Defaults, d, ThroughputSource.Power, User), ModifierMergeStrategy.Override);
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
            damage_u = 10,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                cardTargetingMode = CardTargetingMode.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = "On next hit this turn, counter for {Damage}.";
            },
             
            CardEffect = (User, Target, d) =>
            {
                var mod = new EntityModifier(
                     modifierName: "SpearmanSkyRendingReversalCounter",
                     owner: Target,
                     baseValue: d.Damage,
                     toTriggerRefs: new() { },
                     duration: d.Duration,
                     onRef_Trigger: new RelevantTriggerCheck
                     {
                         OnTriggerReference = new() { GameplayRef.onDamageRecieved },
                            CheckType = CheckEntityType.Target,
                            CheckEntity = User,
                     },
                    onRef_Action: (target, cd, value) =>
                    {
                        CombatUtility.ApplyDamage(null, target, value);
                     }
                 );
                CombatUtility.ApplyEntityModifier(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
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
            power_u = 10,
            damage_u = 10,
            duration_u = 3,

            radius_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                cardTargetingMode = CardTargetingMode.Radius,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = "Increses armour by {Power}) for adjacent allies and gives them thorns {Damage}.";
            },

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyStatBuff(d, Target, 
                    new StatModifier(
                    stat: Target.entityStats.Armour,
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration,
                    name: $"ArmourIncrease#{d.cardID}"),
                    ModifierMergeStrategy.RefreshDurationAndMerge);
                
                CombatUtility.ApplyEntityModifier(d, Target, 
                    new EntityModifier(
                    modifierName: "SpearmanSkyRendingReversalCounter",
                    owner: Target,
                    baseValue: d.Damage,
                    duration: d.Duration,
                    onRef_Trigger: new RelevantTriggerCheck
                    {
                        OnTriggerReference = new() { GameplayRef.onDamageRecieved },
                        CheckType = CheckEntityType.Target,
                        CheckEntity = User,
                    },
                    onRef_Action: (target, cd, value) =>
                    {
                        CombatUtility.ApplyEffectDamage(value, cd.Owner, GameplayRef.onThorns);
                    }),
                    ModifierMergeStrategy.RefreshDurationAndMerge);
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

            range_u = 5,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"Reduce Armour by {d.Power}%.";
            },

            CardEffect = (User, Target, d) =>
            {
                var stat = Target.entityStats.IgnoreArmour;
                var mod = new StatModifier(
                    stat: stat,
                    value: d.Power,
                    scaling: ModifierScaling.Percent,
                    duration: d.Duration,
                    name: $"ArmourReducution");

                CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
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
                cardTargetingMode = CardTargetingMode.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = "Increase Aggro and attack power by {Power}% while active.";
            },

            CardEffect = (User, Target, d) =>
            {
                var stat = Target.entityStats.DamageOutModifier;
                var mod = new StatModifier(
                    stat: stat,
                    value: d.Power,
                    scaling: ModifierScaling.Percent,
                    duration: d.Duration,
                    name: $"AttackIncrease#{d.cardID}");

                CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
                
                // To-Do Increase Aggro
            }
        });
    }
}
