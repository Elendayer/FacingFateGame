using UnityEngine;

namespace facingfate
{
    public static class NeutralCards
    {
        public static void RegisterAll()
        {
            RegisterTechniques();
            RegisterAbilities();
            RegisterSpells();
            RegisterItems();
        }

        private static void RegisterTechniques()
        {
             CardDatabase.RegisterCard(new CardData()
             {
                 cardID = "Neutral_Tech_Punch",
                 cardName = "Punch",
                 cardType = CardType.Technique,
                 cardClass = CardClass.Neutral,
                 cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical },

                 cost_u = 5,
                 damage_u = 8,

                 targetingData = new()
                 {
                     CardTargetType = CardTargetType.Entity,
                     CardTargetAffiliation = CardTargetAffiliation.Enemy,
                     cardTargetingMode = CardTargetingMode.Single,
                 },

                 cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} Damage",
                 cardActionSequence = new()
                 {
                     new CardAction(
                         ExecutionMode.AllAtOnce,
                         TargetingMode.Entities,
                         delayBefore: 0f,
                         delayBetween: 0.2f,
                         action: (caster, target, cardData) =>
                         {
                             CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"), cardData.Damage);
                         })
                 },
             });

            // 100101 – Strike – normal attack
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Tech_Strike",
                cardName = "Strike",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical },

                cost_u = 10,

                damageFunc = card =>
                {
                    return Mathf.RoundToInt(card.Owner.entityStats.CurrentStrength);
                },

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal Damage equal to your Strength: {Damage}",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.2f,
                        action: (caster, target, cardData) =>
                        {
                                CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"), cardData.Damage);
                        }
                    )
                }
            });

            // 100102 – Heavy Blow – slow heavy hit with debuff
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Tech_Heavy_Blow",
                cardName = "Heavy Blow",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical },

                cost_u = 20,

                duration_u = 2,

                damageFunc = card =>
                {
                    return Mathf.RoundToInt(card.Owner.entityStats.CurrentStrength * 2);
                },

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal Damage equal to twice your Strength: {Damage}, increase movement cost by 1 for {Duration} turns",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.2f,
                        action: (caster, target, cardData) =>
                        {
        
                                CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"), cardData.Damage);
                            
                        }
                    ),
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0.2f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                                CombatUtility.ApplyStatDebuff(cardData, target,
                                    new StatModifier(
                                        name: "Movement",
                                        stat: target.entityStats.MovementCostModifier_Increase,
                                        value: 1,
                                        condition: true,
                                        duration: cardData.Duration,
                                        modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge
                                    ));
                            
                        }
                    ),
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Ground,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            AssetManager.Instance.CreateVFXAtSinglePosition(new VFXData("Debuff"), target);
                        }
                    )
                }
            });



            // 100103 – Quick Jab – fast poke
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Tech_Quick_Jab",
                cardName = "Quick Jab",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical },

                cost_u = 10,
                damageFunc = card =>
                {
                    return Mathf.RoundToInt(card.Owner.entityStats.CurrentDexterity);
                },

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal damage equal to your Dexterity: {Damage}.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.2f,
                        action: (caster, target, cardData) =>
                        {
                    
                                CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"), cardData.Damage);
                            
                        }
                    )
                }
            });

            // 100104 – Double Cut – strike twice
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Tech_Double_Cut",
                cardName = "Double Cut",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical },

                cost_u = 30,
                damage_u = 50,
                repeats_u = 2,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage, {Repeats} times.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.2f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("SlashImpact") { activationCount = cardData.Repeats }, cardData.Damage);
                        }
                    )
                }
            });

            // 100105 – Shove – push 1 space (minor damage)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Tech_Shove",
                cardName = "Shove",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Melee },

                cost_u = 20,
                damage_u = 10,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} and push enemy 1 meter.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.0f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"), cardData.Damage);
                        }
                    ),
                  new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Coroutine,
                        delayBefore: 0.0f,
                        delayBetween: 0f,
                        coroutine: (caster, targetingData, cardData) =>
                        {
                            var target = targetingData.targetedEntities[0];
                            var pushPath = MovementUtility.GetFurtherPosition(caster.transform.position, 1f, target);
                            return target.EntityOnMap.StartMoveRoutine(pushPath.End);
                        }
                    ),
                }
            });

            // 100106 – Charge – move both 1 space with damage
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Tech_Charge",
                cardName = "Charge",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical },

                cost_u = 40,
                damage_u = 60,

                range_u = 6f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Charge to target and push them back 1 meter.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Coroutine,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        coroutine: (caster, targetingData, cardData) =>
                        {
                            var target = targetingData.targetedEntities[0];
                            return caster.EntityOnMap.StartMoveRoutine(target.transform.position);
                        }
                    ),
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0.2f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"), cardData.Damage);
                        }
                    ),
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Coroutine,
                        delayBefore: 0.1f,
                        delayBetween: 0f,
                        coroutine: (caster, targetingData, cardData) =>
                        {
                            var target = targetingData.targetedEntities[0];
                            var pushPath = MovementUtility.GetFurtherPosition(caster.transform.position, 1f, target);
                            return target.EntityOnMap.StartMoveRoutine(pushPath.End);
                        }
                    )
                }
            });

            // 100107 – Step Back – disengage after attack (Self)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Tech_Step_Back",
                cardName = "Step Back",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 2,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Disengage by 3 meters.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Coroutine,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        coroutine: (caster, targetingData, cardData) =>
                        {
                            var pushPath = MovementUtility.GetFurtherPosition(caster.transform.position, 3f, caster);
                            return caster.EntityOnMap.StartMoveRoutine(pushPath.End);
                        }
                    )
                }
            });

            // 100108 – Bite – small hit
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Tech_Bite",
                cardName = "Bite",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical },

                cost_u = 10,
                damage_u = 50,

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
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.2f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"), cardData.Damage);
                        }
                    )
                }
            });
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Tech_Claw",
                cardName = "Claw",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical },

                cost_u = 5,
                damage_u = 20,

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
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.2f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("SlashImpact"), cardData.Damage);
                        }
                    )
                }
            });

            // 100109 – Gnaw – bite until bleed (repeat 2) – now applies Bleed DoT (with fallback duration)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Tech_Gnaw",
                cardName = "Gnaw",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Melee, CardIdentity.Blood, CardIdentity.Physical },

                cost_u = 50,
                damage_u = 50,
                repeats_u = 2,
                duration_u = 6,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Bite multiple times and apply Bleed over time.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("SlashImpact"), cardData.Damage);
                        }
                    ),
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0.1f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var bleed = new EntityModifier(
                                modifierName: "Bleed",
                                owner: target,
                                baseValue: cardData.Damage,
                                toTriggerRefs: new() { GameplayRef.onBleed },
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge,
                                onRef_Trigger: new RelevantTriggerCheck
                                {
                                    OnTriggerReference = new() { GameplayRef.onTurnStart },
                                    CheckType = CheckEntityType.User,
                                    CheckEntity = target,
                                },
                                onRef_Action: (targetEntity, cd, value) =>
                                {
                                    CombatUtility.ApplyEffectDamage(value, targetEntity, GameplayRef.onBleed, new VFXData("BleedEffect"));
                                });

                            CombatUtility.ApplyEntityModifier(cardData, target, bleed);
                        }
                    )
                }
            });

            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Tech_Jump",
                cardName = "Jump",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { },

                cost_u = 20,
                range_u = 3f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Jump to a location within range.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Coroutine,
                        delayBefore: 0.0f,
                        delayBetween: 0f,
                        coroutine: (caster, targetingData, cardData) =>
                        {
                            return caster.EntityOnMap.StartJumpRoutine(targetingData.aimPosition);
                        }
                    )
                }
            });

            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Tech_Sunder",
                cardName = "Sunder",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 40,
                damage_u = 100,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Cone,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.15f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"), cardData.Damage);
                        }
                    )
                }
            });



            /*
            // 100110 – Sting – damage + Poison DoT
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Tech_Sting",
                cardName = "Sting",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical, CardIdentity.Poison },

                cost_u = 50,
                damage_u = 5,
                duration_u = 6,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) => d.cardDescription = "Deal {Damage} damage and apply Poison for {Duration} turns.",
                cardEffect = (User, Target, d) =>
                {

                    // Poison DoT + immediate tick
                    string name = $"Poison";
                    var poison = new EntityModifier(
                        modifierName: name,
                        owner: Target,
                        baseValue: d.Damage,
                        toTriggerRefs: new() { GameplayRef.onPoison },
                        duration: d.Duration,
                        onRef_Trigger: new RelevantTriggerCheck
                        {
                            OnTriggerReference = new() { GameplayRef.onTurnStart },
                            CheckType = CheckEntityType.User,
                            CheckEntity = Target,
                        },
                        onRef_Action: (target, cd, value) =>
                        {
                            CombatUtility.ApplyDamage(null, target, new VFXData("Poison", true), value);

                        });

                    CombatUtility.ApplyEntityModifier(d, Target, poison, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });*/
        }

        private static void RegisterAbilities()
        {
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Abil_Recover",
                cardName = "Recover",
                cardType = CardType.Ability,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 30,
                healing_u = 50,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
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
                            CombatUtility.ApplyHealing(cardData, target, cardData.Healing);
                        }
                    ),
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), target );
                        }
                    )
                }
            });

            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Abil_Meditate",
                cardName = "Meditate",
                cardType = CardType.Ability,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Soul },

                cost_u = 10,
                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Draw a Card",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            target.DrawCards(1);
                        }
                    )
                }
            });

            // Rethink Discard then draw
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Abil_Rethink",
                cardName = "Rethink",
                cardType = CardType.Ability,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Soul },
                cost_u = 20,
                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Discard your hand and draw that many cards.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            if (target is PlayerScript player)
                            {
                                DeckManager.Instance.Player_DiscardRandomCardFromHand();
                                DeckManager.Instance.Player_DrawTopCard();
                            }
                            else if (target is NonPlayerScript npc)
                            {
                                npc.DiscardCards(1);
                                npc.DrawCards(1);
                            }
                        }
                    )
                }
            });

            // 100201 – Focus – empower next attack (Self)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Abil_Focus",
                cardName = "Focus",
                cardType = CardType.Ability,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 30,
                power_u = 30,
                duration_u = 3,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Your next attack is empowered by {Power}.",
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
                                name: "Focus",
                                stat: stat,
                                value: cardData.Power,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge
                            );
                            CombatUtility.ApplyStatBuff(cardData, target, mod);
                        }
                    )
                }
            });

            // 100202 – Growl – demoralize enemies (AOE debuff)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Abil_Growl",
                cardName = "Growl",
                cardType = CardType.Ability,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 20,
                power_u = 5,
                duration_u = 2,
                range_u = 4f,
                area_u = 4f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Ring,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Demoralize enemies reduces damage by {Power} and their Armor by 10%.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.1f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyStatDebuff(cardData, target, new StatModifier(
                                name: "Growl Damage Reduction",
                                stat: target.entityStats.DamageOutModifier_Flat,
                                value: cardData.Power,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge
                            ));

                            CombatUtility.ApplyStatDebuff(cardData, target, new StatModifier(
                                name: "Growl Armour Reduction",
                                stat: target.entityStats.Armour_Increase,
                                value: 10,
                                condition: true,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge
                            ));
                        })
                }
            });

            // 100203 – Howl – improve allies' stats (AOE buff)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Abil_Howl",
                cardName = "Howl",
                cardType = CardType.Ability,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 20,
                power_u = 5,
                duration_u = 2,

                range_u = 0f,
                radius_u = 4f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Sphere,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Bolster allies damage {Power}.",
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
                                    name: "Damage",
                                    stat: target.entityStats.DamageOutModifier_Flat,
                                    value: cardData.Power,
                                    duration: cardData.Duration,
                                    modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge
                                ));
                        })
                }
            });

            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Abil_Warcry",
                cardName = "Warcry",
                cardType = CardType.Ability,
                cardClass = CardClass.Neutral,

                cardIdentities = new() { },

                cost_u = 25,

                range_u = 4f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.All,
                    cardTargetingMode = CardTargetingMode.All,
                },
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.1f,
                        action: (caster, target, cardData) =>
                        {
                            if (TargetingUtility.isEnemyOf(caster, target))
                            {
                                CombatUtility.ApplyStatDebuff(cardData, target,
                                    new StatModifier(
                                        name: "Warcry Damage Reduction",
                                        stat: target.entityStats.DamageOutModifier_Increase,
                                        value: -10,
                                        duration: 2,
                                        modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge
                                    ));
                            }
                            else
                            {
                                CombatUtility.ApplyStatBuff(cardData, target,
                                    new StatModifier(
                                        name: "Warcry Damage Taken Reduction",
                                        stat: target.entityStats.DamageTakenModifier_Increase,
                                        value: -20,
                                        duration: 2,
                                        modifierMergeStrategy: ModifierMergeStrategy.Override
                                    ));
                            }
                        }
                    )
                }
            });

            // 100204 – Guard Up – raise defense until end of turn
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Abil_Guard_Up",
                cardName = "Guard Up",
                cardType = CardType.Ability,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 15,
                powerFunc = card =>
                {
                    return Mathf.RoundToInt(card.Owner.entityStats.CurrentTenacity * 5);
                },
                duration_u = 2,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Increase your Armour until end of turn by {Power}.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var stat = target.entityStats.Armour_Increase;
                            var mod = new StatModifier(
                                name: "Guard_Up",
                                stat: stat,
                                value: cardData.Power,
                                condition: true,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.Override
                            );
                            CombatUtility.ApplyStatBuff(cardData, target, mod);
                        }
                    )
                }
            });
        }

        private static void RegisterSpells()
        {
            // 100301 – Summon Wolf – ground-targeted summoning
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Spell_Summon_Wolf",
                cardName = "Summon Wolf",
                cardType = CardType.Spell,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Summon },

                cost_u = 50,
                range_u = 2,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Summon a Wolf ally",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Ground,
                        delayBefore: 0f,
                        delayBetween: 0.2f,
                        action: (caster, position, cardData) =>
                        {
                            CombatUtility.SpawnEntity(cardData, position, "Npc_Wolf_Summon", caster.entityAffiliation, true);
                        }
                    )
                }
            });
        }

        private static void RegisterItems()
        {   
            /*
            // 100601 – Throw Poison – Single/Radius; apply Poison DoT (with immediate tick)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Item_Throw_Poison",
                cardName = "Throw Poison",
                cardType = CardType.Item,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Poison, CardIdentity.Ranged },

                cost_u = 10,
                damage_u = 20,
                duration_u = 6,
                range_u = 4f,
                area_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Radius,
                },

                CardDescription = (User, d) => d.cardDescription = "Apply Poison 2 for 6 turns",
                cardEffect = (User, Target, d) =>
                {
                    var poison = new EntityModifier(
                        modifierName: "Poison",
                        owner: Target,
                        baseValue: d.Damage,
                        toTriggerRefs: new() { GameplayRef.onPoison },
                        duration: d.Duration,
                        onRef_Trigger: new RelevantTriggerCheck
                        {
                            OnTriggerReference = new() { GameplayRef.onTurnStart },
                            CheckType = CheckEntityType.User,
                            CheckEntity = Target,
                        },
                        onRef_Action: (target, cd, value) =>
                        {
                            CombatUtility.ApplyDamage(null, target, new VFXData ("Poison",true), value);
                        });

                    CombatUtility.ApplyEntityModifier(d, Target, poison, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

            // 100602 – Throw Firebomb – Single/Radius; apply Burn DoT (with immediate tick)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Item_Throw_Firebomb",
                cardName = "Throw Firebomb",
                cardType = CardType.Item,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Fire, CardIdentity.Ranged },

                cost_u = 20,
                damage_u = 20,
                duration_u = 6,
                range_u = 4f,
                area_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Radius, // keep as defined in your file
                },

                CardDescription = (User, d) => d.cardDescription = $"Apply Burn {d.Damage} for {d.Duration} turns.",
                cardEffect = (User, Target, d) =>
                {
                    string name = "Burn";
                    var burn = new EntityModifier(
                        modifierName: name,
                        owner: Target,
                        baseValue: d.Damage,
                        toTriggerRefs: new() { GameplayRef.onBurn },
                        duration: d.Duration,
                        onRef_Trigger: new RelevantTriggerCheck
                        {
                            OnTriggerReference = new() { GameplayRef.onTurnStart },
                            CheckType = CheckEntityType.User,
                            CheckEntity = Target,
                        },
                        onRef_Action: (target, cd, value) =>
                        {
                            CombatUtility.ApplyDamage(null, target, new VFXData ("Burn",true) , value);
                        });

                    CombatUtility.ApplyEntityModifier(d, Target, burn, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });*/
        }
    }
}