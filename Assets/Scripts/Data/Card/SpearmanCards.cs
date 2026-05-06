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
                    cardTargetingMode = CardTargetingMode.Sphere,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage {Repeats} times to all enemies in the area.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Ground,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, position, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, position, new VFXData("Impact") { activationCount = cardData.Repeats});
                        })
                }
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

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"));
                            AssetManager.Instance.CreateVFXAtSinglePosition(new VFXData("SpearsFromGround") { area = cardData.Area, start = caster.transform.position, end = target.transform.position}, target.transform.position);
                        })
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

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Coroutine,
                        delayBefore: 0.0f,
                        delayBetween: 0f,
                        coroutine: (caster, targetingData, cardData) =>
                        {
                            var target = targetingData.targetedEntities[0];
                            return caster.EntityOnMap.StartJumpRoutine(target.transform.position);
                        }
                    ),
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0.0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"));
                        }
                    ),

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

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.1f,
                        action: (caster, target, cardData) =>
                        {
                            EntityModifier entityModifier = EffectDatabase.GetEffectByName("Bleed", CloneMode.Defaults, cardData, ThroughputSource.Damage, caster);
                            entityModifier.ModifierMergeStrategy = ModifierMergeStrategy.RefreshDurationAndMerge;
                            CombatUtility.ApplyEntityModifier(cardData, target, entityModifier);
                        }
                    )
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

                range_u = 0.5f,

                radius_u = 2f,
                area_u = 1f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.RingSelf,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage to enemies in a ring. Inflict 5 Burn.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Caster,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, cardData) =>
                        {
                            AssetManager.Instance.CreateVFXAtSinglePosition(new VFXData("Firestorm_Ring") {radius = cardData.Radius, area = cardData.Area}, caster.transform.position);
                            
                        }
                    ),
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.1f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("SlashImpact"));


                            EntityModifier mod = EffectDatabase.GetEffectByName("Burn", CloneMode.Defaults, cardData, ThroughputSource.Damage, caster);
                            CombatUtility.ApplyEntityModifier(cardData, target, mod);
                        }
                    )
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

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.1f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("SlashImpact") { activationCount = 1 });

                            CombatUtility.ApplyStatDebuff(cardData, target,
                                new StatModifier
                                (
                                    name: "MovementFlat",
                                    stat: target.entityStats.MovementCostModifier_Flat,
                                    value: cardData.Power,
                                    duration: cardData.Duration,
                                    modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndOverride,
                                    to_TriggerRefs: new() { GameplayRef.onSlowed }
                                ));
                        }
                    )
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

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.1f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("SlashImpact") { activationCount = 1 });
                        }
                    )
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
                    cardTargetingMode = CardTargetingMode.Sphere,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Aim,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, position, cardData) =>
                        {
                            AssetManager.Instance.CreateVFXAtSinglePosition(new VFXData("Firestorm") { radius = cardData.Radius}, position);
                        }
                    ),
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0.1f,
                        delayBetween: 0.0f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("BurnEffect"), cardData.Damage);
                        }
                    )
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
                power_u = 20,
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

                cardDescriptionAction = (User, d) => d.cardDescription = "Increases attack damage by {Power}%.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var mod = new StatModifier
                            (
                                name: "Damage",
                                stat: target.entityStats.DamageOutModifier_Increase,
                                value: cardData.Power,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.AddUnique
                            );
                            CombatUtility.ApplyStatBuff(cardData, target, mod);
                        }
                    ),
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), target);
                        }
                    )
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
                    cardTargetingMode = CardTargetingMode.Sphere,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage. Roots for {Duration} turns",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.1f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyEntityModifier(cardData, target, new EntityModifier
                                (
                                    modifierName: "Rooted",
                                    owner: target,
                                    baseValue: cardData.Power,
                                    duration: cardData.Duration,
                                    modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge,
                                    onApply_Action: (targetEntity, cd, value) =>
                                    {
                                        targetEntity.entityStats.IsRooted = true;
                                    },
                                    onRemove_Action: (targetEntity, cd, value) =>
                                    {
                                        targetEntity.entityStats.IsRooted = false;
                                    }
                                ));

                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"));
                        }
                    ),
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Debuff"), target);
                        }
                    )
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

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyStatBuff(cardData, target,
                                new StatModifier(
                                    name: $"MeleeRangeFlat",
                                    stat: target.entityStats.RangeModifier_Flat,
                                    value: cardData.Power,
                                    to_TriggerRefs: new(),
                                    duration: cardData.Duration,
                                    modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge,
                                    condition: "Melee"
                                ));
                        }
                    ),
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), target);
                        }
                    )
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
                    d.cardDescription = "On next hit taken this round, counter for {Damage}.";
                },

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var mod = new EntityModifier(
                                modifierName: "SpearmanIronWallReversalCounter",
                                owner: target,
                                baseValue: cardData.Damage,
                                toTriggerRefs: new(),
                                duration: 1,
                                charges: 1,
                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge,
                                onRef_Trigger: new RelevantTriggerCheck
                                {
                                    OnTriggerReference = new() { GameplayRef.onHitLanded },
                                    CheckType = CheckEntityType.Target,
                                    CheckEntity = caster,
                                },
                                onRef_Action: (targetEntity, cd, value) =>
                                {
                                    Debug.Log($"[Counter] Spearman Iron Wall Reversal counter triggered for {value} damage.");
                                    CombatUtility.ApplyEffectDamage(value, cd.Owner, GameplayRef.onCounterRecieved, new VFXData("Impact"));
                                }
                            );
                            CombatUtility.ApplyEntityModifier(cardData, target, mod);
                        }
                    ),
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), target);
                        }
                    )
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

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyStatBuff(cardData, target,
                                new StatModifier
                                (
                                    "RangedDamage",
                                    target.entityStats.DamageTakenModifier_Flat,
                                    0f,
                                    new(),
                                    cardData.Duration,
                                    1,
                                    ModifierMergeStrategy.RefreshDurationAndMerge,
                                    "Technique", "Ranged"
                                ));
                        }
                    ),
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), target);
                        }
                    )
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

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {

                            // Apply Taunt
                            var taunt = new EntityModifier
                            (
                                modifierName: "Taunt",
                                owner: caster,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.Override,
                                onApply_Action: (targetEntity, cd, value) =>
                                {
                                    targetEntity.entityStats.tauntTarget = cardData.Owner;
                                },
                                onRemove_Action: (targetEntity, cd, value) =>
                                {
                                    targetEntity.entityStats.tauntTarget = null;
                                }
                            );

                            AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Debuff"), target);
                            CombatUtility.ApplyEntityModifier(cardData, target, taunt);
                        }
                    ),
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            EntityScript e = target;
                            // Apply Armour
                            CombatUtility.ApplyStatBuff(cardData, caster,
                                new StatModifier
                                (
                                    name: "Armour",
                                    stat: caster.entityStats.Armour_Flat,
                                    value: cardData.Power,
                                    duration: cardData.Duration,
                                    modifierMergeStrategy: ModifierMergeStrategy.AddUnique
                                ));
                            AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), caster);
                        }
                    )
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
                    d.cardDescription = "On next hit received this turn, counter for {Damage}.";
                },

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var mod = new EntityModifier(
                                modifierName: "SpearmanSkyRendingReversalCounter",
                                owner: target,
                                baseValue: cardData.Damage,
                                toTriggerRefs: new() { },
                                duration: cardData.Duration,
                                charges: cardData.Charges,
                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge,
                                onRef_Trigger: new RelevantTriggerCheck
                                {
                                    OnTriggerReference = new() { GameplayRef.onHitLanded },
                                    CheckType = CheckEntityType.Target,
                                    CheckEntity = caster,
                                },
                                onRef_Action: (targetEntity, cd, value) =>
                                {
                                    CombatUtility.ApplyEffectDamage(cardData.Damage, cd.Owner, dotType: GameplayRef.onCounterRecieved, new VFXData("Impact"));
                                }
                            );
                            CombatUtility.ApplyEntityModifier(cardData, target, mod);
                        }
                    )
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
                    cardTargetingMode = CardTargetingMode.Sphere,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Increses armour by {Power}) for adjacent allies and gives them thorns {Damage}.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.1f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyStatBuff(cardData, target,
                                new StatModifier(
                                    name: "Armour",
                                    stat: target.entityStats.Armour_Flat,
                                    value: cardData.Power,
                                    duration: cardData.Duration,
                                    modifierMergeStrategy: ModifierMergeStrategy.AddUnique));

                            CombatUtility.ApplyEntityModifier(cardData, target,
                                new EntityModifier(
                                    modifierName: "SpearmanPhalanxGuardThorns",
                                    owner: target,
                                    baseValue: cardData.Damage,
                                    duration: cardData.Duration,
                                    modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge,
                                    onRef_Trigger: new RelevantTriggerCheck
                                    {
                                        OnTriggerReference = new() { GameplayRef.onDamageRecieved },
                                        CheckType = CheckEntityType.Target,
                                        CheckEntity = caster,
                                    },
                                    onRef_Action: (targetEntity, cd, value) =>
                                    {
                                        CombatUtility.ApplyEffectDamage(value, cd.Owner, GameplayRef.onThorns, new VFXData("Impact"));
                                    }));
                        }
                    ),
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), target);
                        }
                    )
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

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var stat = target.entityStats.IgnoreArmour;
                            var mod = new StatModifier(
                                name: "Armour",
                                stat: stat,
                                value: -cardData.Power,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge);

                            CombatUtility.ApplyStatDebuff(cardData, target, mod);
                                                        AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Debuff"), target);

                        }
                    ),
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

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var stat = target.entityStats.DamageOutModifier_Increase;
                            var mod = new StatModifier(
                                name: "Damage",
                                stat: stat,
                                value: cardData.Power,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.AddUnique);

                            CombatUtility.ApplyStatBuff(cardData, target, mod);
                        }
                    ),
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), target);
                        }
                    )
                }
            });
        }
    }
}