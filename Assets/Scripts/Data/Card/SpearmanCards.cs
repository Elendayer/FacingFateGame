using UnityEngine;
using System.Collections.Generic;

namespace facingfate
{
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
                cardID = "Spear_Tech_Tempest_of_a_Hundred_Spears",
                cardName = "Tempest of a Hundred Spears",
                cardType = CardType.Technique,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 25,
                damage_u = 12,
                repeats_u = 2,

                range_u = 6f,
                radius_u = 3f,
                area_u = 1f,

                targetingData = new()
                {
                    TargetingUsesVision = true,
                    EffectUsesVision = true,
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Radius,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage {Repeats} times to all enemies in the area.",

                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact") { activationCount = d.Repeats});
                },
            });

            // 110102 – Piercing Light (LineSelf 3)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Tech_Piercing_The_Boulder",
                cardName = "Piercing the Boulder",
                cardType = CardType.Technique,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical, CardIdentity.Melee },

                cost_u = 25,
                damage_u = 40,

                range_u = 5f,
                area_u = 1f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.LineSelf,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage",

                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"));
                },
                cardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAtSinglePosition(new VFXData("SpearsFromGround") { area = Data.Area, start = Data.Owner.transform.position, end = Target.targetedPositions[0]}, Target.targetedPositions[0]);
                }
            });

            // 110103 – Sky-Piercing Leap (LineSelf 2)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Tech_Sky_Reaching_Leap",
                cardName = "Sky-Reaching Leap",
                cardType = CardType.Technique,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 30,
                damage_u = 70,

                range_u = 4f,
                power_u = 3,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage.",

                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target,  new VFXData ("Impact"));
                },
                cardEffectGroundAction = (User, Target, d) =>
                {
                    MovementUtility.ForcedMove(ForcedMovementType.Jump, User, Target, d.Power, 100f);
                }
            });

            // 110104 – Heaven Piercing Spear (Single, Range 1) – Bleed DoT
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Tech_Heavens_Spear",
                cardName = "Heavens Spear",
                cardType = CardType.Technique,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical, CardIdentity.Blood },

                cost_u = 10,
                damage_u = 4,

                range_u = 4f,
                maxtarget_u = 3,
                duration_u = 3,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Select,
                },

                CardAiBias = new()
                {
                    DamageOverrideValue = 40,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Apply Bleed dealing {Damage} for {Duration} turns",

                cardEffectAction = (User, Target, d) =>
                {
                    EntityModifier entityModifier = EffectDatabase.GetEffectByName("Bleed", CloneMode.Defaults, d, ThroughputSource.Damage, User);
                    CombatUtility.ApplyEntityModifier(d, Target, entityModifier, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

            // 110105 – Salamander Sweep (Cone 1)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Tech_Salamander_Tail_Sweep",
                cardName = "Salamander Tail Sweep",
                cardType = CardType.Technique,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 20,
                damage_u = 12,

                range_u = 1.2f,
                area_u = 1f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Ring,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage to enemies in a ring. Inflict 5 Burn.",

                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("SlashImpact") );

                    EntityModifier mod = EffectDatabase.GetEffectByName("Burn", CloneMode.Defaults, d, ThroughputSource.Damage, User);
                    CombatUtility.ApplyEntityModifier(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);

                },
                cardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAtSinglePosition(new VFXData("Firestorm_Ring") {radius = Data.Radius, area = Data.Area}, Target.targetedPositions[0]);
                }
            });

            // 110106 – Dragon Fang Thrust (Cone)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Tech_Snake_Tail_Sweep",
                cardName = "Snake Tail Sweep",
                cardType = CardType.Technique,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 22,
                damage_u = 14,

                //Slow Movement Cost increase
                power_u = 1,

                range_u = 3f,
                area_u = 35f,


                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Ring,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage in a ring and increase movement cost of enemies hit.",

                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("SlashImpact") { activationCount = 1 });

                    CombatUtility.ApplyStatDebuff(d, Target,
                        new StatModifier
                        (
                            name: "MovementFlat",
                            stat: Target.entityStats.MovementCostModifier_Flat,
                            value: d.Power,
                            duration: d.Duration,
                            to_TriggerRefs: new() { GameplayRef.onSlowed }
                        ),
                        ModifierMergeStrategy.RefreshDurationAndOverride);
                }
            });

            // 110107 – Dragon Tail Sweep (cone)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Tech_Dragon_Tail_Sweep",
                cardName = "Dragon Tail Sweep",
                cardType = CardType.Technique,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 30,
                damage_u = 22,

                range_u = 4f,
                area_u = 40f,


                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Ring,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage to all enemies in a ring.",

                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("SlashImpact") { activationCount = 1 });
                    // To-Do Slow
                }
            });

            // 110108 – Earthshatter Pole 
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Tech_Earthshatter_Pole",
                cardName = "Earthshatter Pole",
                cardType = CardType.Technique,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 20,
                damage_u = 12,

                range_u = 3f,
                radius_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Radius,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage.",

                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData ("BurnEffect") , d.Damage);
                },
                cardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAtIndividualPositions(new VFXData("Firestorm") { radius = Data.Radius}, Target.targetedPositions);
                }

            });

            // 110109 – Azure Dragon's Roar (Self; until end of turn)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Tech_Azure_Dragons_Roar",
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
                    PowerOverrideValue = 80,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Increses attack damage by {Power}.",
    
                cardEffectAction = (User, Target, d) =>
                {
                    var mod = new StatModifier
                    (
                        name: "Damage",
                        stat: Target.entityStats.DamageOutModifier_Increase,
                        value: d.Power,
                        duration: d.Duration
                        );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.AddUnique);
                },
                cardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), Target.targetedEntities);
                }
            });

            // 110110 – Pillar of the Earth (Radius Stun)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Tech_Pillar_of_the_Earth",
                cardName = "Pillar of the Earth",
                cardType = CardType.Technique,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 36,
                damage_u = 20,

                duration_u = 2,

                range_u = 2f,
                radius_u = 2f,
                area_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Radius,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage. Roots for {Duration} turns",

                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyEntityModifier(d, Target, new EntityModifier
                        (
                            modifierName: "Rooted",
                            owner: Target,
                            baseValue: d.Power,
                            duration: d.Duration,
                            onApply_Action: (target, cd, value) =>
                            {
                                target.entityStats.IsRooted = true;
                            },
                            onRemove_Action: (target, cd, value) =>
                            {
                                target.entityStats.IsRooted = false;
                            }
                        ),
                     ModifierMergeStrategy.RefreshDurationAndMerge);

                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"));
                },
                cardVfx = (Data, Target) =>
                {
                    //AssetManager.Instance.CreateVFXAtSinglePosition(new VFXData("SpearsFromGround") {positions = Target.targetedPositions });
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Debuff"), Target.targetedEntities);
                }
            });
        }

        private static void RegisterAbilities()
        {
            // 110201 – Extending Heaven's Lance (Self; +range until end of turn)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Abil_Extending_Heavens_Lance",
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

                cardDescriptionAction = (User, d) => d.cardDescription = "Increase melee range by {Power} until end of turn.",

                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyStatBuff(d, Target,
                        new StatModifier(
                            name: $"MeleeRangeFlat",
                            stat: Target.entityStats.RangeModifier_Flat,
                            value: d.Power,
                            to_TriggerRefs: new(),
                            duration: d.Duration,
                            condition: "Melee"
                            ),
                        ModifierMergeStrategy.RefreshDurationAndMerge
                        );
                },
                cardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), Target.targetedEntities);
                }
            });

            // 110202 – Iron Wall Reversal (Self; fixed melee counter once)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Abil_Iron_Wall_Reversal",
                cardName = "Iron Wall Reversal",
                cardType = CardType.Ability,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 20,
                damage_u = 60,

                duration_u = 1,
                charges_u = 1,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) =>
                {
                    d.cardDescription =     "On next hit taken this round, counter for {Damage}.";
                },

                cardEffectAction = (User, Target, d) =>
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
                            CombatUtility.ApplyEffectDamage(value, cd.Owner, GameplayRef.onCounterRecieved, new VFXData ("Impact"));
                        }
                    );
                    CombatUtility.ApplyEntityModifier(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
                },
                cardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), Target.targetedEntities);
                }
            });

            // 110203 – Whirling Heaven Ward (Self; deflect ranged once)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Abil_Whirling_Ward",
                cardName = "Whirling Ward",
                cardType = CardType.Ability,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 12,
                duration_u = 1,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardAiBias = new()
                {
                    PowerOverrideValue = 80,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = $"On next ranged technique received this round, reduce the damage to 0.",

                cardEffectAction = (User, Target, d) =>
                {
                 CombatUtility.ApplyStatBuff(d, Target,
                        new StatModifier
                        (
                            name: "RangedDamage",
                            stat: Target.entityStats.DamageTakenModifier_Flat,
                            value: 0,
                            to_TriggerRefs: new(),
                            duration: d.Duration,
                            charges: 1,
                            "Technique", "Ranged"
                        ),
                    ModifierMergeStrategy.RefreshDurationAndMerge
                    );

                },
                cardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), Target.targetedEntities);
                }
            });

            // 110204 – Unyielding Spear Stance (Self; Taunt + Armour +10 for 1 turn)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Abil_Unyielding_Spear_Stance",
                cardName = "Unyielding Spear Stance",
                cardType = CardType.Ability,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                range_u = 2f,

                cost_u = 20,
                power_u = 30,
                duration_u = 2,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Taunt target and gain {Power} Armour for {Duration} turns.",

                cardEffectAction = (User, Target, d) =>
                {
                    // Apply Armour
                    CombatUtility.ApplyStatBuff(d, User,
                        new StatModifier
                        (
                        name: "Armour",
                        stat: User.entityStats.Armour_Flat,
                        value: d.Power,
                        duration: d.Duration
                        ),
                        ModifierMergeStrategy.RefreshDurationAndMerge);

                    // Apply Taunt
                    CombatUtility.ApplyEntityModifier(d, Target, EffectDatabase.GetEffectByName("Taunted", CloneMode.Defaults, d, ThroughputSource.Power, User), ModifierMergeStrategy.Override);
                },
                cardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Debuff"), Target.targetedEntities);
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), new List<EntityScript>() { Data.Owner });
                }
            });

            
            //110205 – Sky-Rending Reversal (Self; stronger fixed counter)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Abil_Sky_Rending_Reversal",
                cardName = "Sky-Rending Reversal",
                cardType = CardType.Ability,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 12,
                damage_u = 30,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) =>
                {
                    d.cardDescription = "On next hit received this turn, counter for {Damage}.";
                },

                cardEffectAction = (User, Target, d) =>
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
                            CombatUtility.ApplyDamage(null, target, new VFXData("SlashImpact"), value);
                        }
                     );
                    CombatUtility.ApplyEntityModifier(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

            // 110206 – Phalanx Guard (Self; defensive placeholder)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Abil_Phalanx_Guard",
                cardName = "Phalanx Guard",
                cardType = CardType.Ability,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 40,
                power_u = 15,
                damage_u = 15,
                duration_u = 3,

                radius_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Radius,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Increses armour by {Power}) for adjacent allies and gives them thorns {Damage}.",

                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyStatBuff(d, Target,
                        new StatModifier(
                        name: "ArmourFlat",
                        stat: Target.entityStats.Armour_Flat,
                        value: d.Power,
                        duration: d.Duration),
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
                            CombatUtility.ApplyEffectDamage(value, cd.Owner, GameplayRef.onThorns, new VFXData ("Impact"));
                        }),
                        ModifierMergeStrategy.RefreshDurationAndMerge);
                },
                cardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), Target.targetedEntities);
                }
            });
        }

        private static void RegisterCurses()
        {
            // 110401 – Brittle Courage (Self)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Curse_Brittle_Courage",
                cardName = "Brittle Courage",
                cardType = CardType.Curse,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 0,
                power_u = 50,

                range_u = 5f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Reduce Armour by {Power}%.",

                cardEffectAction = (User, Target, d) =>
                {
                    var stat = Target.entityStats.IgnoreArmour;
                    var mod = new StatModifier(
                        name: "Armour",
                        stat: stat,
                        value: -d.Power,
                        duration: d.Duration);

                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
                },
                cardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Debuff"), Target.targetedEntities);
                }
            });
        }

        private static void RegisterBlessings()
        {
            // 110501 – Brilliant Spear (Self)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Bless_Brilliant_Spear",
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

                cardDescriptionAction = (User, d) => d.cardDescription = "Increase Attack power by {Power}% while active.",

                cardEffectAction = (User, Target, d) =>
                {
                    var stat = Target.entityStats.DamageOutModifier_Increase;
                    var mod = new StatModifier(
                        name: "Damage",
                        stat: stat,
                        value: d.Power,
                        duration: d.Duration);

                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);

                    // To-Do Increase Aggro
                },
                cardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), Target.targetedEntities);
                }
            });
        }
    }
}

