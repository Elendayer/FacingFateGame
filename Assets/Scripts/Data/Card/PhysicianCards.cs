
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
            // 140101 â€“ Jade Needle Acupuncture â€“ HoT on ally
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Tech_Jade_Needle_Acupuncture",
                cardName = "Jade Needle Acupuncture",
                cardType = CardType.Technique,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 20,
                healing_u = 20,
                duration_u = 3,   // for 3 turns
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

                CardDescription = (User, d) =>
                {
                    d.cardDescription = $"Apply a regeneration of {d.Power} for {d.Duration} turns.";
                },

                CardEffect = (User, Target, d) =>
                {
                    // Heal-over-time as positive â€œon turn startâ€ tick
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

            // 140102 â€“ Bloodletting â€“ convert Poison â†’ Bleed
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Tech_Bloodletting",
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

                CardDescription = (User, d) => d.cardDescription = "Turn target's Poison into Bleed.",
                CardEffect = (User, Target, d) => { /* TODO: convert Poison stacks to Bleed stacks */ }
            });

            // 140103 â€“ Formation of the Hundred Remedies â€“ Heal allies in ring
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Tech_Formation_of_the_Hundred_Remedies",
                cardName = "Formation of the Hundred Remedies",
                cardType = CardType.Technique,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 100,
                healing_u = 100,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.CombatTile,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Ring,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = $"Heal allies in range for {d.Healing}.",

                CardEffect = (User, Target, d) =>
                {
                    CombatUtility.ApplyHealing(d, Target, d.Healing);
                }
            });

            // 140104 â€“ Venomous Grip (Single Enemy) â€“ worsen Poison
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Tech_Venomous_Grip",
                cardName = "Venomous Grip",
                cardType = CardType.Technique,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.Poison },

                cost_u = 40,
                power_u = 2,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) => d.cardDescription = "Worsen target's Poison (TBD: add stacks / increase tick).",
                CardEffect = (User, Target, d) => { /* TODO: modify poison modifier on target */ }
            });

            // 140105 â€“ Needle of the Flowing River (Single Ally) â€“ cleanse
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Tech_Needle_of_the_Flowing_River",
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

                CardDescription = (User, d) => d.cardDescription = "Cleanse ally (remove negative effects).",
                CardEffect = (User, Target, d) => { /* TODO: cleanse implementation */ }
            });
        }


        // --------------------------- Abilities ---------------------------
        private static void RegisterAbilities()
        {
            // 140201 â€“ Gather â€“ sammelt Materialien; bei Wert 3 = neue Karte (TODO)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Abil_Gather",
                cardName = "Gather",
                cardType = CardType.Ability,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 3,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.CombatTile,
                    CardTargetAffiliation = CardTargetAffiliation.None,
                    cardTargetingMode = CardTargetingMode.Ring,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = "Gather materials around you. Value=3 â‡’ create/draw a new card (TODO).",

                CardEffect = (User, Target, d) =>
                {
                    // TODO: Sammel-Stack am User erhÃ¶hen; bei Stack >= 3 -> neue Karte erzeugen/ziehen und Stack resetten.
                }
            });

            // 140202 â€“ Toxic Remedy Paradox â€“ heilt einen Ally & vergiftet einen Enemy
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Abil_Toxic_Remedy_Paradox",
                cardName = "Toxic Remedy Paradox",
                cardType = CardType.Ability,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.Poison },

                cost_u = 4,
                range_u = 2f,


                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.None,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = "Select an ally to heal and an enemy to Poison (TODO amounts).",

                CardEffect = (User, Target, d) =>
                {
                    // TODO: Zielauswahl entkoppeln: Ally Cleansen, Enemy Poison-DoT anwenden.
                }
            });

            // 140203 â€“ Poison Barbs â€“ bei Treffer: Thorns + Poison (Self)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Abil_Poison_Barbs",
                cardName = "Poison Barbs",
                cardType = CardType.Ability,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.Poison },

                cost_u = 5,
                power_u = 2,
                duration_u = 2,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = "When attacked this turn, deal Thorns and apply Poison to the attacker.",

                CardEffect = (User, Target, d) =>
                {
                    // TODO: Reaktiven Modifier auf den User legen:
                    // On 'onHitTaken' -> fÃ¼ge fixen Thorns-Schaden zu & apply Poison-DoT auf den Angreifer.
                }
            });

            // 140204 â€“ Doctor's Footwork â€“ halbiert Movement-Kosten (Self)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Abil_Doctors_Footwork",
                cardName = "Doctor's Footwork",
                cardType = CardType.Ability,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 2,
                duration_u = 1,  // bis Rundenende

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = "Halve movement cost until end of turn.",

                CardEffect = (User, Target, d) =>
                {
                    // TODO: TemporÃ¤ren Modifier auf den User anwenden, der die Bewegungs-Kosten halbiert.
                    // (z. B. Multiplikator 0.5 auf Stamina/Movement-Cost; endet am Turn-End.)
                }
            });
        }

        private static void RegisterSpells()
        {
            // 140301 â€“ Jade Needle Resonance â€“ Buff allies in area (Damage up)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Spell_Jade_Needle_Resonance",
                cardName = "Jade Needle Resonance",
                cardType = CardType.Spell,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 5,
                power_u = 20,
                duration_u = 2,
                range_u = 3f,
                area_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.CombatTile,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Radius,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = $"Allies in range gain {d.Power} damage increase for {d.Duration} turns.",

                CardEffect = (User, Target, d) =>
                {
                    // Buff DamageIncrease on each affected ally
                    var stat = Target.entityStats.DamageOutModifier;
                    var mod = new StatModifier(
                        stat: stat,
                        value: d.Power,
                        scaling: ModifierScaling.Percent,
                        duration: d.Duration,
                        name: $"JadeResonance_Dmg+{d.Power}"
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

            // 140302 â€“ Breath of the Jade Lotus â€“ Heals everyone in a line
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Spell_Breath_of_the_Jade_Lotus",
                cardName = "Breath of the Jade Lotus",
                cardType = CardType.Spell,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 30,
                healing_u = 80,
                range_u = 3f,
                area_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.CombatTile,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Cone,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = $"Heal allies in a line for {d.Healing}.",

                CardEffect = (User, Target, d) =>
                {
                    CombatUtility.ApplyHealing(d, Target, d.Healing);
                }
            });
        }
        private static void RegisterItems()
        {
            // 140601 â€“ Brew of a Hundred Herbs â€“ Heal an Ally
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Item_Brew_of_a_Hundred_Herbs",
                cardName = "Brew of a Hundred Herbs",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,
                healing_u = 100,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = $"Heal an ally for {d.Healing}.",

                CardEffect = (User, Target, d) =>
                {
                    CombatUtility.ApplyHealing(d, Target, d.Healing);
                }
            });

            // 140602 â€“ Elixir of a Hundred Herbs â€“ +Max Health for 3 turns
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Item_Elixir_of_a_Hundred_Herbs",
                cardName = "Elixir of a Hundred Herbs",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,
                duration_u = 3,
                power_u = 100, // +MaxHealth
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = $"Increase Max Health by {d.Power} for {d.Duration} turns.",

                CardEffect = (User, Target, d) =>
                {
                    var stat = Target.entityStats.MaxHealth;
                    var mod = new StatModifier(
                        stat: stat,
                        value: d.Power,
                        scaling: ModifierScaling.Flat,
                        duration: d.Duration,
                        name: $"MaxHP+{d.Power}"
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

            // 140603 â€“ Pill of a Hundred Herbs â€“ +Max Health (indefinite)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Item_Pill_of_a_Hundred_Herbs",
                cardName = "Pill of a Hundred Herbs",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,
                duration_u = 0,
                power_u = 100,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = $"Permanently increase Max Health by {d.Power}.",

                CardEffect = (User, Target, d) =>
                {
                    var stat = Target.entityStats.MaxHealth;
                    var mod = new StatModifier(
                        stat: stat,
                        value: d.Power,
                        scaling: ModifierScaling.Flat,
                        duration: d.Duration, // 0 => indefinite in deinem System
                        name: $"MaxHP+{d.Power}_Pill"
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.Merge);
                }
            });

            // 140604 â€“ Crimson Rejuvenation Brew â€“ Regenerates Stamina
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Item_Crimson_Rejuvenation_Brew",
                cardName = "Crimson Rejuvenation Brew",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 1,
                power_u = 150,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = $"Regenerate stamina (value {d.Power}).",

                CardEffect = (User, Target, d) =>
                {
                    // TODO Stamina
                }
            });

            // 140605 â€“ Crimson Rejuvenation Elixir â€“ +Max Stamina for 3 turns
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Item_Crimson_Rejuvenation_Elixir",
                cardName = "Crimson Rejuvenation Elixir",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,
                duration_u = 3,
                power_u = 150,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = $"Increase Max Stamina by {d.Power} for {d.Duration} turns.",

                CardEffect = (User, Target, d) =>
                {
                    var stat = Target.entityStats.MaxStamina;
                    var mod = new StatModifier(
                        stat: stat,
                        value: d.Power,
                        scaling: ModifierScaling.Flat,
                        duration: d.Duration,
                        name: $"MaxStamina+{d.Power}"
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

            // 140606 â€“ Crimson Rejuvenation Pill â€“ +Max Stamina (indefinite)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Item_Crimson_Rejuvenation_Pill",
                cardName = "Crimson Rejuvenation Pill",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,
                duration_u = 0,
                power_u = 150,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = $"Permanently increase Max Stamina by {d.Power}.",

                CardEffect = (User, Target, d) =>
                {
                    var stat = Target.entityStats.MaxStamina;
                    var mod = new StatModifier(
                        stat: stat,
                        value: d.Power,
                        scaling: ModifierScaling.Flat,
                        duration: d.Duration,
                        name: $"MaxStamina+{d.Power}"
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.Merge);
                }
            });

            // 140607 â€“ Brew of Unbroken Will â€“ +Armour for 1 Turn (DamageIncrease)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Item_Brew_of_Unbroken_Will",
                cardName = "Brew of Unbroken Will",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,
                duration_u = 1,
                power_u = 100,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = $"Ally gains +{d.Power} armour for this turn.",

                CardEffect = (User, Target, d) =>
                {
                    var stat = Target.entityStats.DamageOutModifier;
                    var mod = new StatModifier(
                        stat: stat,
                        value: d.Power,
                        scaling: ModifierScaling.Flat,
                        duration: d.Duration,
                        name: $"ArmourIncrease+{d.Power}"
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

            // 140608 â€“ Elixir of Unbroken Will â€“ +Armour for 3 turns
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Item_Elixir_of_Unbroken_Will",
                cardName = "Elixir of Unbroken Will",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,
                duration_u = 3,
                power_u = 100,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = $"Ally gains +{d.Power} Armour for {d.Duration} turns.",

                CardEffect = (User, Target, d) =>
                {
                    var stat = Target.entityStats.Armour;
                    var mod = new StatModifier(
                        stat: stat,
                        value: d.Power,
                        scaling: ModifierScaling.Flat,
                        duration: d.Duration,
                        name: $"ArmourIncrease+{d.Power}"
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

            // 140609 â€“ Pill of Unbroken Will â€“ +Armour (indefinite)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Item_Pill_of_Unbroken_Will",
                cardName = "Pill of Unbroken Will",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,
                duration_u = 0,
                power_u = 100,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = $"Permanently increase Armour by {d.Power}.",

                CardEffect = (User, Target, d) =>
                {
                    var stat = Target.entityStats.Armour;
                    var mod = new StatModifier(
                        stat: stat,
                        value: d.Power,
                        scaling: ModifierScaling.Flat,
                        duration: d.Duration,
                        name: $"ArmourIncrease+{d.Power}"
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.Merge);
                }
            });

            // 140610 â€“ Soaring Dragon Brew â€“ +Attack for 1 turn
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Item_Soaring_Dragon_Brew",
                cardName = "Soaring Dragon Brew",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,
                duration_u = 1,
                power_u = 100,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = $"Ally gains +{d.Power} damage for this turn.",

                CardEffect = (User, Target, d) =>
                {
                    CombatUtility.ApplyStatBuff(d, Target,
                        new StatModifier
                        (
                        stat: User.entityStats.DamageOutModifier,
                        value: d.Power,
                        scaling: ModifierScaling.Flat,
                        condition: true,
                        to_TriggerRefs: new() { },
                        duration: d.Duration,
                        name: $"SoaringDragon"
                    ),
                   ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

            // 140611 â€“ Soaring Dragon Elixir â€“ +Attack for X turns (here 3)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Item_Soaring_Dragon_Elixir",
                cardName = "Soaring Dragon Elixir",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,
                duration_u = 3,
                power_u = 100,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = $"Ally gains +{d.Power} damage for {d.Duration} turns.",

                CardEffect = (User, Target, d) =>
                {
                    var stat = Target.entityStats.DamageOutModifier;
                    var mod = new StatModifier(
                        stat: stat,
                        value: d.Power,
                        scaling: ModifierScaling.Flat,
                        duration: d.Duration,
                        name: $"SoaringDragonElixir_Dmg+{d.Power}"
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

            // 140612 â€“ Soaring Dragon Pill â€“ +Attack (indefinite)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Item_Soaring_Dragon_Pill",
                cardName = "Soaring Dragon Pill",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,
                duration_u = 0,
                power_u = 100,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = $"Permanently increase damage by {d.Power}.",

                CardEffect = (User, Target, d) =>
                {
                    var stat = Target.entityStats.DamageOutModifier;
                    var mod = new StatModifier(
                        stat: stat,
                        value: d.Power,
                        scaling: ModifierScaling.Flat,
                        duration: d.Duration,
                        name: $"SoaringDragonPill_Dmg+{d.Power}"
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.Merge);
                }
            });

            // 140613 â€“ Crystal Cleansing Balm â€“ Cleanse target (status removal) -> TODO
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Item_Crystal_Cleansing_Balm",
                cardName = "Crystal Cleansing Balm",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,
                range_u = 3f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.CombatTile,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Radius,

                },

                CardDescription = (User, d) =>
                    d.cardDescription = $"Cleanses Target of all DoTs.",

                CardEffect = (User, Target, d) =>
                {
                    //TODO Cleanse
                }
            });

            // 140614 â€“ Mandrake Poison Cloud â€“ Throws Poison Cloud
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Item_Mandrake_Poison_Cloud",
                cardName = "Mandrake Poison Cloud",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.Poison },

                cost_u = 30,

                damage_u = 2,
                duration_u = 3,

                range_u = 4f,
                radius_u = 3f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.CombatTile,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Radius,
                },

                CardAiBias = new CardAiBias
                {
                    DamageOverrideValue = 40

                },

                CardDescription = (User, d) => d.cardDescription = "Create a cloud of poison, inflicing Poison dealing {Damage} for {Duration} turns.",

                CardEffectGround = (User, TargetTile, d) =>
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
            // 140401 â€“ Alchemistâ€™s Misstep â€“ failure chance 20% fail for Items
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Curse_Alchemists_Misstep",
                cardName = "Alchemistâ€™s Misstep",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.CombatTile,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Radius,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = $"Cleanses Target of all DoTs.",

                CardEffect = (User, Target, d) =>
                {
                    //Item Fail 20% der Zeit
                }
            });
        }

        private static void RegisterBlessing()
        {
            // 140501 â€“ Mythical Herb â€“ Increases potency of items 
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Phy_Item_Mythical_Herb",
                cardName = "Mythical Herb",
                cardType = CardType.Item,
                cardClass = CardClass.Physician,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.CombatTile,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Radius,
                },

                CardDescription = (User, d) =>
                    d.cardDescription = $"Cleanses Target of all DoTs.",

                CardEffect = (User, Target, d) =>
                {
                    //TODO Improve ItemCards
                }
            });
        }
    }
}
