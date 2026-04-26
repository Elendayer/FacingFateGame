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

                cardEffectAction = (User, Target, d) =>
                {
                    // Heal-over-time as positive “on turn start” tick
                    var regen = new EntityModifier(
                        modifierName: "Regeneration",
                        owner: Target,
                        baseValue: d.Healing,
                        toTriggerRefs: new() { GameplayRef.onHealRecieved },
                        duration: d.Duration,
                        onRef_Trigger: new RelevantTriggerCheck
                        {
                            OnTriggerReference = new() { GameplayRef.onTurnStart },
                            CheckType = CheckEntityType.User,
                            CheckEntity = Target,
                        },
                        onRef_Action: (target, cd, value) =>
                        {
                            CombatUtility.ApplyHealing(cd, target, value);
                        }
                    );

                    CombatUtility.ApplyEntityModifier(d, Target, regen, ModifierMergeStrategy.RefreshDurationAndMerge);
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
                cardEffectAction = (User, Target, d) => { /* TODO: convert Poison stacks to Bleed stacks */ }
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

                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyHealing(d, Target, d.Healing);
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
                cardEffectAction = (User, Target, d) => { /* TODO: modify poison modifier on target */ }
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
                cardEffectAction = (User, Target, d) => { /* TODO: cleanse implementation */ }
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

                cardEffectAction = (User, Target, d) =>
                {
                    // TODO: Sammel-Stack am User erhöhen; bei Stack >= 3 -> neue Karte erzeugen/ziehen und Stack resetten.
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

                cardEffectAction = (User, Target, d) =>
                {
                    // TODO: Zielauswahl entkoppeln: Ally Cleansen, Enemy Poison-DoT anwenden.
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

                cardEffectAction = (User, Target, d) =>
                {
                    // TODO: Reaktiven Modifier auf den User legen:
                    // On 'onHitTaken' -> füge fixen Thorns-Schaden zu & apply Poison-DoT auf den Angreifer.
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

                cardEffectAction = (User, Target, d) =>
                {
                    var mod = new StatModifier(
                        name: "MovementCostMultiplier",
                        stat: Target.entityStats.MovementCostModifier_Multiplier,
                        value: 0.5f,
                        duration: d.Duration
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
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

                cardEffectAction = (User, Target, d) =>
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
                        roll < 0.60f ? brews  :
                        roll < 0.90f ? elixirs :
                                       pills;

                    string selectedID = pool[UnityEngine.Random.Range(0, pool.Count)];
                    CardData handCardData = CardDatabase.GetCardById(selectedID, User);

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
                    CardData deckCardData = CardDatabase.GetCardById(selectedID, User);
                    GameObject deckGO = UnityEngine.Object.Instantiate(
                        DeckManager.Instance.cardPrefab,
                        DeckManager.Instance.deckParent);
                    deckGO.name = deckCardData.cardName;
                    CardScript deckCS = deckGO.GetComponent<CardScript>();
                    deckCS.cardData = deckCardData;
                    deckCS.SetHidden();
                    TransformUtility.ZeroLocalRectTransform(deckGO.transform as RectTransform);
                    DeckManager.Instance.cardStack.Push(deckGO);
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
                    cardTargetingMode = CardTargetingMode.Radius,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Allies in range gain {Power} increased damage for {Duration} turns.",

                cardEffectAction = (User, Target, d) =>
                {
                    // Buff DamageIncrease on each affected ally
                    var mod = new StatModifier(
                        name: "Damage",
                        stat: Target.entityStats.DamageOutModifier_Increase,
                        value: d.Power,
                        duration: d.Duration
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
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

                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyHealing(d, Target, d.Healing);
                }
            });
        }
        private static void RegisterItems()
        {
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

                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyHealing(d, Target, d.Healing);
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

                cardEffectAction = (User, Target, d) =>
                {
                    var mod = new StatModifier(
                        name: "Health",
                        stat: Target.entityStats.MaxHealth_Flat,
                        value: d.Power,
                        duration: d.Duration
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
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

                cardEffectAction = (User, Target, d) =>
                {
                    var mod = new StatModifier(
                        name: "Health",
                        stat: Target.entityStats.MaxHealth_Flat,
                        value: d.Power,
                        duration: d.Duration
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.Merge);
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

                cardEffectAction = (User, Target, d) =>
                {
                    // TODO Stamina
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

                cardEffectAction = (User, Target, d) =>
                {
                    var mod = new StatModifier(
                        name: "Stamina",
                        stat: Target.entityStats.MaxStamina_Flat,
                        value: d.Power,
                        duration: d.Duration
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
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

                cardEffectAction = (User, Target, d) =>
                {
                    var mod = new StatModifier(
                        name: "Stamina",
                        stat: Target.entityStats.MaxStamina_Flat,
                        value: d.Power,
                        duration: d.Duration
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.Merge);
                }
            });

            // 140607 – Brew of Unbroken Will – +Armour for 1 Turn (DamageIncrease)
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

                cardEffectAction = (User, Target, d) =>
                {
                    var mod = new StatModifier(
                        name: "Armour",
                        stat: Target.entityStats.Armour_Flat,
                        value: d.Power,
                        duration: d.Duration
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
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

                cardEffectAction = (User, Target, d) =>
                {
                    var mod = new StatModifier(
                        name: $"ArmourIncrease",
                        stat: Target.entityStats.Armour_Increase,
                        value: d.Power,
                        duration: d.Duration
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
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

                cardEffectAction = (User, Target, d) =>
                {
                    var mod = new StatModifier(
                        name: "Armour",
                        stat: Target.entityStats.Armour_Flat,
                        value: d.Power,
                        duration: d.Duration
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.Merge);
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

                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyStatBuff(d, Target,
                        new StatModifier
                        (
                        name: $"SoaringDragon",
                        stat: Target.entityStats.DamageOutModifier_Flat,
                        value: d.Power,
                        duration: d.Duration
                    ),
                   ModifierMergeStrategy.RefreshDurationAndMerge);
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

                cardEffectAction = (User, Target, d) =>
                {
                    var mod = new StatModifier(
                        name: "Damage",
                        stat: Target.entityStats.DamageOutModifier_Increase,
                        value: d.Power,
                        duration: d.Duration
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
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

                cardEffectAction = (User, Target, d) =>
                {
                    var mod = new StatModifier(
                        name: "Damage",
                        stat: Target.entityStats.DamageOutModifier_Increase,
                        value: d.Power,
                        duration: d.Duration
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.Merge);
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
                    cardTargetingMode = CardTargetingMode.Radius,

                },

                cardDescriptionAction = (User, d) => d.cardDescription = $"Cleanses Target of all DoTs.",

                cardEffectAction = (User, Target, d) =>
                {
                    //TODO Cleanse
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
                    cardTargetingMode = CardTargetingMode.Radius,
                },

                CardAiBias = new CardAiBias
                {
                    DamageOverrideValue = 40

                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Create a cloud of poison, inflicing Poison dealing {Damage} for {Duration} turns.",

                cardEffectGroundAction = (User, TargetTile, d) =>
                {
                    var mod = new EntityModifier(
                        modifierName: "Poison",
                        owner: User,
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
                            CombatUtility.ApplyEffectDamage(value, cd.Owner, GameplayRef.onBleed, new VFXData("BleedEffect", true));
                        });

                    CombatUtility.SpawnGroundEffect(d, TargetTile, new GroundEffect_Enter_EntityData
                    (
                        cardData: d,
                        relevantTrigger: new RelevantTriggerCheck
                        {
                            OnTriggerReference = new() { GameplayRef.onTurnStart },
                            CheckType = CheckEntityType.User,
                            CheckEntity = User,
                        },
                        duration: d.Duration,
                        removeOnExit: false,
                        removeOnEnd: false,
                        modifier: mod,
                        onEnter: (modifier, target) => { },
                        onExit: (modifier, target) => { }));
                }
            });
        }

        private static void RegisterCurse()
        {
            // 140401 – Alchemist’s Misstep – failure chance 20% fail for Items
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Physician_Curse_Alchemists_Misstep",
                cardName = "Alchemist’s Misstep",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Radius,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = $"Cleanses Target of all DoTs.",

                cardEffectAction = (User, Target, d) =>
                {
                    //Item Fail 20% der Zeit
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
                    cardTargetingMode = CardTargetingMode.Radius,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = $"Cleanses Target of all DoTs.",

                cardEffectAction = (User, Target, d) =>
                {
                    //TODO Improve ItemCards
                }
            });
        }
    }
}

