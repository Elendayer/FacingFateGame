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
            // 110101 â€“ Tempest of a Hundred Spears (Ring 1, repeats=2)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Tech_Tempest_of_a_Hundred_Spears",
                cardName = "Tempest of a Hundred Spears",
                cardType = CardType.Technique,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 20,
                damage_u = 20,
                repeats_u = 2,

                range_u = 6f,
                radius_u = 3f,
                area_u = 1f,

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
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact") { activationCount = d.Repeats});
                },
            });

            // 110102 â€“ Piercing Light (LineSelf 3)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Tech_Piercing_Light",
                cardName = "Piercing Light",
                cardType = CardType.Technique,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical, CardIdentity.Melee },

                cost_u = 25,
                damage_u = 50,

                range_u = 3f,

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
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"));
                },
                CardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAtUnifiedPositions(new VFXData("SpearsFromGround") { positions = Target.targetedPositions});
                }
            });

            // 110103 â€“ Sky-Piercing Leap (LineSelf 2)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Tech_Sky_Piercing_Leap",
                cardName = "Sky-Piercing Leap",
                cardType = CardType.Technique,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 30,
                damage_u = 45,

                range_u = 4f,
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
                    CombatUtility.ApplyDamage(d, Target,  new VFXData ("Impact"));
                },
                CardEffectGround = (User, Target, d) =>
                {
                    MovementUtility.ForcedMove(ForcedMovementType.Jump, User, Target, d.Power, 100f);
                }
            });

            // 110104 â€“ Heaven Piercing Spear (Single, Range 1) â€“ Bleed DoT
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Tech_Heaven_Piercing_Spear",
                cardName = "Heaven Piercing Spear",
                cardType = CardType.Technique,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical, CardIdentity.Blood },

                cost_u = 10,
                damage_u = 4,

                range_u = 4f,
                maxtarget_u = 3,

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

                CardDescription = (User, d) =>
                {
                    d.cardDescription = "Apply Bleed dealing {Damage} for {Duration} turns";
                },

                CardEffect = (User, Target, d) =>
                {
                    EntityModifier entityModifier = EffectDatabase.GetEffectByName("Bleed", CloneMode.Defaults, d, ThroughputSource.Damage, User);

                    Debug.Log($"Applying Bleed modifier from Heaven Piercing Spear to {Target.name} with base damage {entityModifier.BaseValue} for {entityModifier.Duration} turns.");

                    CombatUtility.ApplyEntityModifier(d, Target, entityModifier, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

            // 110105 â€“ Salamander Sweep (Cone 1)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Tech_Salamander_Tail_Sweep",
                cardName = "Salamander Tail Sweep",
                cardType = CardType.Technique,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 20,
                damage_u = 30,

                range_u = 2f,
                area_u = 30f,

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
                    CombatUtility.ApplyDamage(d, Target, new VFXData("SlashImpact") { activationCount = 1});
                },
                CardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAtIndividualPositions(new VFXData("Firestorm"), Target.targetedPositions);
                }
            });

            // 110106 â€“ Dragon Fang Thrust (Cone)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Tech_Snake_Tail_Sweep",
                cardName = "Snake Tail Sweep",
                cardType = CardType.Technique,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 20,
                damage_u = 5,

                //Slow Movement Cost increase
                power_u = 1,

                range_u = 3f,
                area_u = 35f,


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

            // 110107 â€“ Dragon Tail Sweep (cone)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Tech_Dragon_Tail_Sweep",
                cardName = "Dragon Tail Sweep",
                cardType = CardType.Technique,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 30,
                damage_u = 25,

                range_u = 4f,
                area_u = 40f,


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
                    CombatUtility.ApplyDamage(d, Target, new VFXData("SlashImpact") { activationCount = 1 });
                    // To-Do Slow
                }
            });

            // 110108 â€“ Earthshatter Pole 
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Tech_Earthshatter_Pole",
                cardName = "Earthshatter Pole",
                cardType = CardType.Technique,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 10,
                damage_u = 60,

                range_u = 3f,
                radius_u = 2f,

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
                    CombatUtility.ApplyDamage(d, Target, new VFXData ("BurnEffect") , d.Damage);
                },
                CardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAtIndividualPositions(new VFXData("Firestorm"), Target.targetedPositions);
                }

            });

            // 110109 â€“ Azure Dragon's Roar (Self; until end of turn)
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

                CardDescription = (User, d) =>
                {
                    d.cardDescription = "Increses attack damage by {Power}.";
                },

                CardEffect = (User, Target, d) =>
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
                CardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), Target.targetedEntities);
                }
            });

            // 110110 â€“ Pillar of the Earth (Radius Stun)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Tech_Pillar_of_the_Earth",
                cardName = "Pillar of the Earth",
                cardType = CardType.Technique,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 36,
                damage_u = 60,

                duration_u = 2,

                range_u = 2f,
                radius_u = 2f,
                area_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.CombatTile,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Radius,
                },

                CardDescription = (User, d) =>
                {
                    d.cardDescription = "Deal {Damage} damage. Roots for {Duration} turns";
                },

                CardEffect = (User, Target, d) =>
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
                CardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAtUnifiedPositions(new VFXData("SpearsFromGround") {positions = Target.targetedPositions });
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Debuff"), Target.targetedEntities);
                }
            });
        }

        private static void RegisterAbilities()
        {
            // 110201 â€“ Extending Heaven's Lance (Self; +range until end of turn)
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

                CardDescription = (User, d) =>
                {
                    d.cardDescription = "Increase melee range by {Power} until end of turn.";
                },

                CardEffect = (User, Target, d) =>
                {
                    CombatUtility.ApplyStatBuff(d, Target,
                        new StatModifier(
                            name: $"MeleeRangeFlat",
                            stat: Target.entityStats.RangeModifier_Flat,
                            value: d.Power,
                            condition: (e, c) => c.cardIdentities.Contains(CardIdentity.Melee) && c.range_u > 1,
                            to_TriggerRefs: new() { },
                            duration: d.Duration
                            ),
                        ModifierMergeStrategy.RefreshDurationAndMerge
                        );
                },
                CardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), Target.targetedEntities);
                }
            });

            // 110202 â€“ Iron Wall Reversal (Self; fixed melee counter once)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Abil_Iron_Wall_Reversal",
                cardName = "Iron Wall Reversal",
                cardType = CardType.Ability,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 20,
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
                            CombatUtility.ApplyEffectDamage(value, cd.Owner, GameplayRef.onCounterRecieved, new VFXData ("Impact"));
                        }
                    );
                    CombatUtility.ApplyEntityModifier(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
                },
                CardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), Target.targetedEntities);
                }
            });

            // 110203 â€“ Whirling Heaven Ward (Self; deflect ranged once)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Abil_Whirling_Heaven_Ward",
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
                    PowerOverrideValue = 80,
                },

                CardDescription = (User, d) =>
                {
                    d.cardDescription = $"On next ranged technique recieved this round reduce the damage to 0";
                },

                CardEffect = (User, Target, d) =>
                {
                 CombatUtility.ApplyStatBuff(d, Target,
                        new StatModifier
                        (
                            name: "RangedDamage",
                            stat: Target.entityStats.DamageTakenModifier_Flat,
                            value: 0,
                            condition: (e, c) => c.cardType == CardType.Technique && c.cardIdentities.Contains(CardIdentity.Ranged),
                            to_TriggerRefs: new() { },
                            duration: d.Duration,
                            charges: 1
                        ),
                    ModifierMergeStrategy.RefreshDurationAndMerge
                    );

                },
                CardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), Target.targetedEntities);
                }
            });

            // 110204 â€“ Unyielding Spear Stance (Self; Taunt + Armour +10 for 1 turn)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Abil_Unyielding_Spear_Stance",
                cardName = "Unyielding Spear Stance",
                cardType = CardType.Ability,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                range_u = 2f,

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
                        name: "Armour",
                        stat: User.entityStats.Armour_Flat,
                        value: d.Power,
                        duration: d.Duration
                        ),
                        ModifierMergeStrategy.RefreshDurationAndMerge);

                    // Apply Taunt
                    CombatUtility.ApplyEntityModifier(d, Target, EffectDatabase.GetEffectByName("Taunted", CloneMode.Defaults, d, ThroughputSource.Power, User), ModifierMergeStrategy.Override);
                },
                CardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Debuff"), Target.targetedEntities);
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), new List<EntityScript>() { Data.Owner });
                }
            });

            /*
            // 110205 â€“ Sky-Rending Reversal (Self; stronger fixed counter)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Abil_Sky_Rending_Reversal",
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
            */

            // 110206 â€“ Phalanx Guard (Self; defensive placeholder)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Spear_Abil_Phalanx_Guard",
                cardName = "Phalanx Guard",
                cardType = CardType.Ability,
                cardClass = CardClass.Spearman,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 40,
                power_u = 10,
                damage_u = 10,
                duration_u = 3,

                radius_u = 2f,

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
                CardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), Target.targetedEntities);
                }
            });
        }

        private static void RegisterCurses()
        {
            // 110401 â€“ Brittle Courage (Self)
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

                CardDescription = (User, d) =>
                {
                    d.cardDescription = "Reduce Armour by {Power}%.";
                },

                CardEffect = (User, Target, d) =>
                {
                    var stat = Target.entityStats.IgnoreArmour;
                    var mod = new StatModifier(
                        name: "Armour",
                        stat: stat,
                        value: -d.Power,
                        duration: d.Duration);

                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
                },
                CardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Debuff"), Target.targetedEntities);
                }
            });
        }

        private static void RegisterBlessings()
        {
            // 110501 â€“ Brilliant Spear (Self)
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

                CardDescription = (User, d) =>
                {
                    d.cardDescription = "Increase Attack power by {Power}% while active.";
                },

                CardEffect = (User, Target, d) =>
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
                CardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), Target.targetedEntities);
                }
            });
        }
    }
}
