using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace facingfate
{
    public static class PhysicianCards
    {
        public static void RegisterAll()
        {
            RegisterSpells();
            RegisterMartialArts();
            RegisterAbilities();
            RegisterItems();
            RegisterCurse();
            RegisterBlessing();
        }


        private static void RegisterMartialArts()
        {
            // 140101 – Jade Needle Acupuncture – HoT on ally
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Tech_Jade_Needle_Acupuncture",
                cardName = "Jade Needle Acupuncture",
                cardType = CardType.Technique,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 20,
                healing_u = 15,
                duration_u = 3,
                range_u = 3f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardAiBias = new()
                {
                    triggerConditionTargets = (entitiy) => entitiy.HasCondition(GameplayCondition.isDamaged)
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Apply Regeneration ({Healing}/turn) for {Duration} turns.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var regen = new EntityModifier(
                                modifierName: "Regeneration",
                                owner: target,
                                baseValue: cardData.Healing,
                                toTriggerRefs: new() { GameplayRef.onHealRecieved },
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
                                    CombatUtility.ApplyHealing(cd, targetEntity, value);
                                }
                            );
                            CombatUtility.ApplyEntityModifier(cardData, target, regen);
                        }
                    )
                }
            });

            // 140102 – Bloodletting – convert Poison → Bleed
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Tech_Bloodletting",
                cardName = "Bloodletting",
                cardType = CardType.Technique,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.Blood, CardIdentity.Poison },

                cost_u = 10,
                power_u = 1,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Turn target's Poison into Bleed.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            // TODO: convert Poison stacks to Bleed stacks
                        })
                    )
                }
            });

            // 140103 – Formation of the Hundred Remedies – Heal allies in ring
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Tech_Formation_of_the_Hundred_Remedies",
                cardName = "Formation of the Hundred Remedies",
                cardType = CardType.Technique,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 45,
                healing_u = 40,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Ring,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = $"Heal allies in range for {d.Healing}.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.1f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyHealing(cardData, target, cardData.Healing);
                        }
                    )
                }
            });

            // 140104 – Venomous Grip (Single Enemy) – worsen Poison
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Tech_Venomous_Grip",
                cardName = "Venomous Grip",
                cardType = CardType.Technique,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.Poison },

                cost_u = 30,
                power_u = 2,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Worsen target's Poison stacks.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            // TODO: modify poison modifier on target
                        })
                    )
                }
            });

            // 140105 – Needle of the Flowing River (Single Ally) – cleanse
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Tech_Needle_of_the_Flowing_River",
                cardName = "Needle of the Flowing River",
                cardType = CardType.Technique,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 2,
                range_u = 3f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Cleanse ally (remove negative effects).",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            // TODO: cleanse implementation
                        })
                    )
                }
            });
        }


        // --------------------------- Abilities ---------------------------
        private static void RegisterAbilities()
        {
            // 140201 – Gather – sammelt Materialien; bei Wert 3 = neue Karte (TODO)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Abil_Gather",
                cardName = "Gather",
                cardType = CardType.Ability,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 5,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.None,
                    cardTargetingMode = CardTargetingMode.Ring,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Gather materials around you. Value=3 ⇒ create/draw a new card (TODO).",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Ground,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, Vector3, CardData>)((caster, position, cardData) =>
                        {
                            // TODO: Sammel-Stack am User erhöhen; bei Stack >= 3 -> neue Karte erzeugen/ziehen und Stack resetten.
                        })
                    )
                }
            });

            // 140202 – Toxic Remedy Paradox – heilt einen Ally & vergiftet einen Enemy
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Abil_Toxic_Remedy_Paradox",
                cardName = "Toxic Remedy Paradox",
                cardType = CardType.Ability,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.Poison },

                cost_u = 8,
                range_u = 2f,


                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.None,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Select an ally to heal and an enemy to Poison (TODO amounts).",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            // TODO: Zielauswahl entkoppeln: Ally Cleansen, Enemy Poison-DoT anwenden.
                        })
                    )
                }
            });

            // 140203 – Poison Barbs – bei Treffer: Thorns + Poison (Self)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Abil_Poison_Barbs",
                cardName = "Poison Barbs",
                cardType = CardType.Ability,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.Poison },

                cost_u = 8,
                power_u = 5,
                duration_u = 2,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "When attacked this turn, deal Thorns and apply Poison to the attacker.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            EntityModifier barbs = null;
                            barbs = new EntityModifier(
                                modifierName: "PoisonBarbs",
                                owner: target,
                                baseValue: cardData.Power,
                                duration: 9999,
                                charges: 9999,
                                onRef_Trigger: new RelevantTriggerCheck
                                {
                                    OnTriggerReference = new() { GameplayRef.onHitLanded },
                                    CheckType = CheckEntityType.Target,
                                    CheckEntity = target,
                                },
                                onRef_Action: (entity, cd, value) =>
                                {
                                    var attacker = GameEvents.LastGameplayTrigger.UserEntity;
                                    if (attacker == null) return;
                                    CombatUtility.ApplyEffectDamage(value, attacker, GameplayRef.onCounterRecieved, new VFXData("Impact"));
                                    var poison = EffectDatabase.GetEffectByName("Poison", cd, ThroughputSource.Damage, attacker);
                                    if (poison != null)
                                    {
                                        poison.ModifierMergeStrategy = ModifierMergeStrategy.RefreshDurationAndMerge;
                                        CombatUtility.ApplyEntityModifier(cd, attacker, poison);
                                    }
                                }
                            );
                            CombatUtility.ApplyEntityModifier(cardData, target, barbs);

                            Action<ToSendTriggerReference> expireHandler = null;
                            expireHandler = (trigger) =>
                            {
                                if (trigger.UserEntity != target) return;
                                if (trigger.OnTriggerReference == null || !trigger.OnTriggerReference.Contains(GameplayRef.onTurnStart)) return;
                                barbs?.OnRemove();
                                GameEvents.OnGameplayReference -= expireHandler;
                            };
                            GameEvents.OnGameplayReference += expireHandler;
                        }
                    )
                }
            });

            // 140204 – Doctor's Footwork – halbiert Movement-Kosten (Self)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Abil_Doctors_Footwork",
                cardName = "Doctor's Footwork",
                cardType = CardType.Ability,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 5,
                duration_u = 1,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Halve movement cost until end of turn.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var mod = new StatModifier(
                                name: "MovementCostMultiplier",
                                stat: target.entityStats.MovementCostModifier_Multiplier,
                                value: 0.5f,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge
                            );
                            CombatUtility.ApplyStatBuff(cardData, target, mod);
                        }
                    )
                }
            });

            // 140205 – Combat Alchemy – random Brew/Elixir/Pill added to hand and deck (Self)
            // Brew 60% · Elixir 30% · Pill 10%
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Abil_Combat_Alchemy",
                cardName = "Combat Alchemy",
                cardType = CardType.Ability,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 20,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) =>
                    d.cardDescription = "Brew 60% · Elixir 30% · Pill 10% — add a random concoction to your hand and deck.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            // Pools by tier (Mandrake Poison Cloud and Crystal Cleansing Balm excluded — ground actions)
                            var brews = new System.Collections.Generic.List<string>
                            {
                                "Physician_Item_Brew_of_a_Hundred_Herbs",
                                "Physician_Item_Brew_of_Unbroken_Will",
                                "Physician_Item_Soaring_Dragon_Brew",
                                "Physician_Item_Crimson_Rejuvenation_Brew",
                            };
                            var elixirs = new System.Collections.Generic.List<string>
                            {
                                "Physician_Item_Elixir_of_a_Hundred_Herbs",
                                "Physician_Item_Elixir_of_Unbroken_Will",
                                "Physician_Item_Soaring_Dragon_Elixir",
                                "Physician_Item_Crimson_Rejuvenation_Elixir",
                            };
                            var pills = new System.Collections.Generic.List<string>
                            {
                                "Physician_Item_Pill_of_a_Hundred_Herbs",
                                "Physician_Item_Pill_of_Unbroken_Will",
                                "Physician_Item_Soaring_Dragon_Pill",
                                "Physician_Item_Crimson_Rejuvenation_Pill",
                            };

                            float roll = UnityEngine.Random.value;
                            System.Collections.Generic.List<string> pool =
                                roll < 0.60f ? brews :
                                roll < 0.90f ? elixirs :
                                               pills;

                            string selectedID = pool[UnityEngine.Random.Range(0, pool.Count)];
                            CardData handCardData = CardDatabase.GetCardById(selectedID, caster);

                            if (handCardData == null)
                            {
                                UnityEngine.Debug.LogWarning($"[Combat Alchemy] Card not found: {selectedID}");
                                return;
                            }

                            UnityEngine.Debug.Log($"[Combat Alchemy] Generated: {handCardData.cardName}");

                            // Instantiate into hand for immediate use
                            GameObject handGO = UnityEngine.Object.Instantiate(
                                DeckManager.Instance.cardPrefab,
                                DeckManager.Instance.deckParent);
                            handGO.name = handCardData.cardName;
                            CardScript handCS = handGO.GetComponent<CardScript>();
                            handCS.cardData = handCardData;
                            HandManager.Instance.AddCard(handGO);
                            handCS.SetRevealed();

                            // Instantiate a second copy into the deck (persists for future draws)
                            CardData deckCardData = CardDatabase.GetCardById(selectedID, caster);
                            GameObject deckGO = UnityEngine.Object.Instantiate(
                                DeckManager.Instance.cardPrefab,
                                DeckManager.Instance.deckParent);
                            deckGO.name = deckCardData.cardName;
                            CardScript deckCS = deckGO.GetComponent<CardScript>();
                            deckCS.cardData = deckCardData;
                            deckCS.SetHidden();
                            TransformUtility.ZeroLocalRectTransform(deckGO.transform as RectTransform);
                            DeckManager.Instance.cardStack.Push(deckGO);
                            DeckManager.Instance.deckParent.GetComponent<facingfate.DiscardPileVisualizer>()?.Refresh();
                        })
                    )
                }
            });
        }

        private static void RegisterSpells()
        {
            // 140301 – Jade Needle Resonance – Buff allies in area (Damage up)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Spell_Jade_Needle_Resonance",
                cardName = "Jade Needle Resonance",
                cardType = CardType.Spell,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,
                power_u = 15,
                duration_u = 2,
                range_u = 3f,
                area_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Sphere,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Allies in range gain {Power} increased damage for {Duration} turns.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.1f,
                        action: (caster, target, cardData) =>
                        {
                            var mod = new StatModifier(
                                name: "Damage",
                                stat: target.entityStats.DamageOutModifier_Increase,
                                value: cardData.Power,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge
                            );
                            CombatUtility.ApplyStatBuff(cardData, target, mod);
                        }
                    )
                }
            });

            // 140302 – Breath of the Jade Lotus – Heals everyone in a line
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Spell_Breath_of_the_Jade_Lotus",
                cardName = "Breath of the Jade Lotus",
                cardType = CardType.Spell,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 30,
                healing_u = 25,
                range_u = 3f,
                area_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Cone,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Heal allies in a line for {Healing}.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.1f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyHealing(cardData, target, cardData.Healing);
                        }
                    ),
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Caster,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, cardData) =>
                        {
                            CombatUtility.ApplyHealing(cardData, caster, cardData.Healing);
                        }
                    )
                }
            });
        }

        private static void RegisterItems()
        {
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Item_Spiderweb_Bomb",
                cardName = "Spiderweb Bomb",
                cardType = CardType.Ability,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.Alchemical, CardIdentity.Physical },

                cost_u = 25,
                damage_u = 12,
                power_u = 1,
                duration_u = 2,
                range_u = 5f,
                area_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Sphere,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage and Root enemies for {Duration} turns.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.1f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"), cardData.Damage);
                        }
                    ),
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0.1f,
                        delayBetween: 0.1f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyEntityModifier(cardData, target, new EntityModifier(
                                modifierName: "Rooted",
                                owner: target,
                                baseValue: cardData.Power,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge,
                                onApply_Action: (targetEntity, cd, value) => { targetEntity.entityStats.IsRooted = true; },
                                onRemove_Action: (targetEntity, cd, value) => { targetEntity.entityStats.IsRooted = false; }
                            ));
                        }
                    )
                }
            });

            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Item_Alchemic_Fire",
                cardName = "Alchemic Fire",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.Fire },

                cost_u = 30,
                damage_u = 20,
                secondaryDamage_u = 6,
                duration_u = 3,
                range_u = 5f,
                area_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Sphere,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage and Burn for {SecondaryDamage}/turn over {Duration} turns.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.1f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"), cardData.Damage);
                        }
                    ),
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0.1f,
                        delayBetween: 0.1f,
                        action: (caster, target, cardData) =>
                        {
                            var burn = new EntityModifier(
                                modifierName: "Burn",
                                owner: target,
                                baseValue: cardData.SecondaryDamage,
                                toTriggerRefs: new() { GameplayRef.onBurn },
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge,
                                onRef_Trigger: new RelevantTriggerCheck
                                {
                                    OnTriggerReference = new() { GameplayRef.onTurnStart },
                                    CheckType = CheckEntityType.User,
                                    CheckEntity = target
                                },
                                onRef_Action: (targetEntity, cd, value) =>
                                {
                                    CombatUtility.ApplyDamage(null, targetEntity, new VFXData("BurnEffect", true), value);
                                });
                            CombatUtility.ApplyEntityModifier(cardData, target, burn);
                        }
                    )
                }
            });

            // 140601 – Brew of a Hundred Herbs – Heal an Ally
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Item_Brew_of_a_Hundred_Herbs",
                cardName = "Brew of a Hundred Herbs",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 15,
                healing_u = 40,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Heal an ally for {Healing}.",

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
                    )
                }
            });

            // 140602 – Elixir of a Hundred Herbs – +Max Health for 3 turns
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Item_Elixir_of_a_Hundred_Herbs",
                cardName = "Elixir of a Hundred Herbs",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 20,
                duration_u = 3,
                power_u = 75,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Increase Max Health by {Power} for {Duration} turns.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var mod = new StatModifier(
                                name: "Health",
                                stat: target.entityStats.MaxHealth_Flat,
                                value: cardData.Power,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge
                            );
                            CombatUtility.ApplyStatBuff(cardData, target, mod);
                        }
                    )
                }
            });

            // 140603 – Pill of a Hundred Herbs – +Max Health (indefinite)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Item_Pill_of_a_Hundred_Herbs",
                cardName = "Pill of a Hundred Herbs",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 25,
                duration_u = 0,
                power_u = 50,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Permanently increase Max Health by {Power}.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var mod = new StatModifier(
                                name: "Health",
                                stat: target.entityStats.MaxHealth_Flat,
                                value: cardData.Power,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.Merge
                            );
                            CombatUtility.ApplyStatBuff(cardData, target, mod);
                        }
                    )
                }
            });

            // 140604 – Crimson Rejuvenation Brew – Regenerates Stamina
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Item_Crimson_Rejuvenation_Brew",
                cardName = "Crimson Rejuvenation Brew",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 5,
                power_u = 30,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Regenerate stamina (value {Power}).",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            target.entityStats.CurrentStamina += cardData.Power;
                            HandUI.RefreshHandLocks(target);

                            var mod = new EntityModifier(
                                modifierName: "CrimsonRejuvenationBrewOverheal",
                                owner: target,
                                baseValue: cardData.Power,
                                duration: 1,
                                onRef_Trigger: new RelevantTriggerCheck
                                {
                                    OnTriggerReference = new() { GameplayRef.onTurnStart },
                                    CheckType = CheckEntityType.User,
                                    CheckEntity = target,
                                },
                                onRemove_Action: (entity, cd, value) =>
                                {
                                    entity.entityStats.CurrentStamina = Mathf.Min(
                                        entity.entityStats.CurrentStamina, entity.entityStats.MaxStamina);
                                    HandUI.RefreshHandLocks(entity);
                                }
                            );
                            target.AddModifier(mod);
                        })
                    )
                }
            });

            // 140605 – Crimson Rejuvenation Elixir – +Max Stamina for 3 turns
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Item_Crimson_Rejuvenation_Elixir",
                cardName = "Crimson Rejuvenation Elixir",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 20,
                duration_u = 3,
                power_u = 25,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = $"Increase Max Stamina by {d.Power} for {d.Duration} turns.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var mod = new StatModifier(
                                name: "Stamina",
                                stat: target.entityStats.MaxStamina_Flat,
                                value: cardData.Power,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge
                            );
                            CombatUtility.ApplyStatBuff(cardData, target, mod);
                        }
                    )
                }
            });

            // 140606 – Crimson Rejuvenation Pill – +Max Stamina (indefinite)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Item_Crimson_Rejuvenation_Pill",
                cardName = "Crimson Rejuvenation Pill",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 20,
                duration_u = 1,
                power_u = 20,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Permanently increase Max Stamina by {Power}.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var mod = new StatModifier(
                                name: "Stamina",
                                stat: target.entityStats.MaxStamina_Flat,
                                value: cardData.Power,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.Merge
                            );
                            CombatUtility.ApplyStatBuff(cardData, target, mod);
                        }
                    )
                }
            });

            // 140607 – Brew of Unbroken Will – +Armour for 1 Turn
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Item_Brew_of_Unbroken_Will",
                cardName = "Brew of Unbroken Will",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,
                duration_u = 1,
                power_u = 50,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Ally gains +{Power} Armour for this turn.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var mod = new StatModifier(
                                name: "Armour",
                                stat: target.entityStats.Armour_Flat,
                                value: cardData.Power,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge
                            );
                            CombatUtility.ApplyStatBuff(cardData, target, mod);
                        }
                    )
                }
            });

            // 140608 – Elixir of Unbroken Will – +Armour for 3 turns
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Item_Elixir_of_Unbroken_Will",
                cardName = "Elixir of Unbroken Will",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 18,
                duration_u = 3,
                power_u = 50,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Ally gains +{Power} Armour for {Duration} turns.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var mod = new StatModifier(
                                name: "ArmourIncrease",
                                stat: target.entityStats.Armour_Increase,
                                value: cardData.Power,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge
                            );
                            CombatUtility.ApplyStatBuff(cardData, target, mod);
                        }
                    )
                }
            });

            // 140609 – Pill of Unbroken Will – +Armour (indefinite)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Item_Pill_of_Unbroken_Will",
                cardName = "Pill of Unbroken Will",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 22,
                duration_u = 0,
                power_u = 35,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Permanently increase Armour by {Power}.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var mod = new StatModifier(
                                name: "Armour",
                                stat: target.entityStats.Armour_Flat,
                                value: cardData.Power,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.Merge
                            );
                            CombatUtility.ApplyStatBuff(cardData, target, mod);
                        }
                    )
                }
            });

            // 140610 – Soaring Dragon Brew – +Attack for 1 turn
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Item_Soaring_Dragon_Brew",
                cardName = "Soaring Dragon Brew",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,
                duration_u = 1,
                power_u = 30,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Ally gains +{Power} damage for this turn.",

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
                                    name: "SoaringDragon",
                                    stat: target.entityStats.DamageOutModifier_Flat,
                                    value: cardData.Power,
                                    duration: cardData.Duration,
                                    modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge
                                ));
                        }
                    )
                }
            });

            // 140611 – Soaring Dragon Elixir – +Attack for X turns (here 3)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Item_Soaring_Dragon_Elixir",
                cardName = "Soaring Dragon Elixir",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 18,
                duration_u = 3,
                power_u = 30,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Ally gains +{Power} damage for {Duration} turns.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var mod = new StatModifier(
                                name: "Damage",
                                stat: target.entityStats.DamageOutModifier_Increase,
                                value: cardData.Power,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge
                            );
                            CombatUtility.ApplyStatBuff(cardData, target, mod);
                        }
                    )
                }
            });

            // 140612 – Soaring Dragon Pill – +Attack (indefinite)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Item_Soaring_Dragon_Pill",
                cardName = "Soaring Dragon Pill",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 22,
                duration_u = 0,
                power_u = 20,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Permanently increase damage by {Power}.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var mod = new StatModifier(
                                name: "Damage",
                                stat: target.entityStats.DamageOutModifier_Increase,
                                value: cardData.Power,
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.Merge
                            );
                            CombatUtility.ApplyStatBuff(cardData, target, mod);
                        }
                    )
                }
            });

            // 140613 – Crystal Cleansing Balm – Cleanse target (status removal) -> TODO
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Item_Crystal_Cleansing_Balm",
                cardName = "Crystal Cleansing Balm",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,
                range_u = 3f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Sphere,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = $"Cleanses Target of all DoTs.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.1f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            //TODO Cleanse
                        })
                    )
                }
            });

            // 140614 – Mandrake Poison Cloud – Throws Poison Cloud
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Item_Mandrake_Poison_Cloud",
                cardName = "Mandrake Poison Cloud",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.Poison },

                cost_u = 30,
                damage_u = 5,
                duration_u = 4,
                range_u = 4f,
                radius_u = 3f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Sphere,
                },

                CardAiBias = new CardAiBias
                {
                    DamageOverrideValue = 40
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Create a cloud of poison lasting {Duration} turns, inflicting Poison dealing {Damage} for 3 turns.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Aim,
                        delayBefore: 0f,
                        delayBetween: 0.0f,
                        action: (caster, target, cardData) =>
                        {
                            // Shared factory — creates a correctly-wired, per-entity poison modifier.
                            // Used by modifierFactory (applied on enter) and onRef (refreshes each caster turn).
                            Func<EntityScript, EntityModifier> poisonFactory = (entity) => new EntityModifier(
                                modifierName: "Poison",
                                owner: entity,
                                baseValue: cardData.Damage,
                                toTriggerRefs: new() { GameplayRef.onPoison },
                                duration: 3,
                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge,
                                onRef_Trigger: new RelevantTriggerCheck
                                {
                                    OnTriggerReference = new() { GameplayRef.onTurnStart },
                                    CheckType = CheckEntityType.User,
                                    CheckEntity = entity,
                                },
                                onRef_Action: (poisonedEntity, cd, value) =>
                                {
                                    CombatUtility.ApplyEffectDamage(value, poisonedEntity, GameplayRef.onPoison, new VFXData("PoisonEffect", true));
                                }
                            );

                            var groundEffect = new GroundEffectData
                            (
                                cardData: cardData,
                                relevantTrigger: new RelevantTriggerCheck
                                {
                                    OnTriggerReference = new() { GameplayRef.onTurnStart },
                                    CheckType = CheckEntityType.User,
                                    CheckEntity = caster,
                                },
                                duration: cardData.Duration,
                                targetingMode: CardTargetingMode.Sphere,
                                removeOnExit: false,
                                removeOnEnd: false,
                                // Applies a fresh poison modifier when an entity first enters the zone
                                modifierFactory: poisonFactory,
                                // Refreshes the poison modifier for every entity already in the zone at the start of each caster turn
                                onRef: (entity) => entity.AddModifier(poisonFactory(entity))
                            );
                            CombatUtility.SpawnGroundEffect(cardData, target, groundEffect, new VFXData("PoisonCloud", true) { radius = cardData.Radius });
                        })
                }
            });
        }

        private static void RegisterCurse()
        {
            // 140401 – Alchemist's Misstep – failure chance 20% fail for Items
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Curse_Alchemists_Misstep",
                cardName = "Alchemist's Misstep",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Sphere,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = $"Cleanses Target of all DoTs.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.1f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            //Item Fail 20% der Zeit
                        })
                    )
                }
            });
        }


        private static void RegisterBlessing()
        {
            // 140501 – Mythical Herb – Increases potency of items
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Item_Mythical_Herb",
                cardName = "Mythical Herb",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Sphere,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = $"Cleanses Target of all DoTs.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.1f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            //TODO Improve ItemCards
                        })
                    )
                }
            });
        }
    }}