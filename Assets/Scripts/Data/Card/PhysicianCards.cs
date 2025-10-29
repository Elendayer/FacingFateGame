using System.Collections.Generic;
using UnityEngine;
using Utility;

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
            cardID = 140201,
            cardName = "Jade Needle Acupuncture",
            cardType = CardType.Technique,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 20,
            healing_u = 20,      
            duration_u = 3,   // for 3 turns
            range_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Apply a regeneration of {d.Power} for {d.Duration} turns.",

            CardEffect = (User, Target, d) =>
            {
                // Heal-over-time as positive “on turn start” tick
                var regen = new EntityModifier(
                    statName: "Regeneration",
                    baseValue: d.Healing,
                    to_Trigger_refs: new() { GameplayRef.onHeal },
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
                            References = new() { GameplayRef.onHeal },
                            UserId = User.GetInstanceID(),
                            AffectedEntityId = Target.GetInstanceID()
                        });
                        CombatUtility.ApplyHealing(User, Target, mod.BaseValue);
                    }
                );

                CombatUtility.ApplyEntityModifier(User, Target, regen, ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

        // 140102 – Bloodletting – convert Poison → Bleed
        CardDatabase.RegisterCard(new CardData()
    {
        cardID = 140102,
            cardName = "Bloodletting",
            cardType = CardType.Technique,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.Blood, CardIdentity.Poison },

            cost_u = 10,
            power_u = 1,
            range_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) => d.cardDescription = "Turn target's Poison into Bleed.",
            CardEffect = (User, Target, d) => { /* TODO: convert Poison stacks to Bleed stacks */ }
        });

        // 140103 – Formation of the Hundred Remedies – Heal allies in ring
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140203,
            cardName = "Formation of the Hundred Remedies",
            cardType = CardType.Technique,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 100,
            healing_u = 100,
            range_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Ring,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Heal allies in range for {d.Healing}.",

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyHealing(User, Target, d.Healing);
            }
        });

        // 140104 – Venomous Grip (Single Enemy) – worsen Poison
        CardDatabase.RegisterCard(new CardData()
    {
            cardID = 140104,
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
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) => d.cardDescription = "Worsen target's Poison (TBD: add stacks / increase tick).",
            CardEffect = (User, Target, d) => { /* TODO: modify poison modifier on target */ }
        });

        // 140105 – Needle of the Flowing River (Single Ally) – cleanse
        CardDatabase.RegisterCard(new CardData()
    {
            cardID = 140105,
            cardName = "Needle of the Flowing River",
            cardType = CardType.Technique,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 2,
            range_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) => d.cardDescription = "Cleanse ally (remove negative effects).",
            CardEffect = (User, Target, d) => { /* TODO: cleanse implementation */ }
        });
    }


    // --------------------------- Abilities ---------------------------
    private static void RegisterAbilities()
    {
        // 140201 – Gather – sammelt Materialien; bei Wert 3 = neue Karte (TODO)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140201,
            cardName = "Gather",
            cardType = CardType.Ability,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.None,
                SelectionType = CardTargetSelection.Ring,
            },

            CardDescription = (User, d) =>
                d.cardDescription = "Gather materials around you. Value=3 ⇒ create/draw a new card (TODO).",

            CardEffect = (User, Target, d) =>
            {
                // TODO: Sammel-Stack am User erhöhen; bei Stack >= 3 -> neue Karte erzeugen/ziehen und Stack resetten.
            }
        });

        // 140202 – Toxic Remedy Paradox – heilt einen Ally & vergiftet einen Enemy
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140202,
            cardName = "Toxic Remedy Paradox",
            cardType = CardType.Ability,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.Poison },

            cost_u = 4,    
            range_u = 2,


            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.None, 
                SelectionType = CardTargetSelection.Single,                
            },

            CardDescription = (User, d) =>
                d.cardDescription = "Select an ally to heal and an enemy to Poison (TODO amounts).",

            CardEffect = (User, Target, d) =>
            {
                // TODO: Zielauswahl entkoppeln: Ally Cleansen, Enemy Poison-DoT anwenden.
            }
        });

        // 140203 – Poison Barbs – bei Treffer: Thorns + Poison (Self)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140203,
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
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = "When attacked this turn, deal Thorns and apply Poison to the attacker.",

            CardEffect = (User, Target, d) =>
            {
                // TODO: Reaktiven Modifier auf den User legen:
                // On 'onHitTaken' -> füge fixen Thorns-Schaden zu & apply Poison-DoT auf den Angreifer.
            }
        });

        // 140204 – Doctor's Footwork – halbiert Movement-Kosten (Self)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140204,
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
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = "Halve movement cost until end of turn.",

            CardEffect = (User, Target, d) =>
            {
                // TODO: Temporären Modifier auf den User anwenden, der die Bewegungs-Kosten halbiert.
                // (z. B. Multiplikator 0.5 auf Stamina/Movement-Cost; endet am Turn-End.)
            }
        });
    }

    private static void RegisterSpells()
    {
        // 140301 – Jade Needle Resonance – Buff allies in area (Damage up)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140101,
            cardName = "Jade Needle Resonance",
            cardType = CardType.Spell,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 5,
            power_u = 20,     
            duration_u = 2,
            range_u = 3,
            area_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Radius,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Allies in range gain {d.Power} damage increase for {d.Duration} turns.",

            CardEffect = (User, Target, d) =>
            {
                // Buff DamageIncrease on each affected ally
                var stat = Target.entityStats.DamageIncrease;
                var mod = new StatModifier(
                    value: d.Power,
                    scaling: ModifierScaling.Percent,
                    duration: d.Duration,
                    on_triggerConditionRef: new TriggerRef { References = new() { GameplayRef.onTurnStart }, AffectedEntityId = Target.GetInstanceID() },
                    name: $"JadeResonance_Dmg+{d.Power}"
                );
                CombatUtility.ApplyBuff(User, Target, stat, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

        // 140302 – Breath of the Jade Lotus – Heals everyone in a line
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140102,
            cardName = "Breath of the Jade Lotus",
            cardType = CardType.Spell,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 30,
            healing_u = 80,
            range_u = 3,
            area_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Cone,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Heal allies in a line for {d.Healing}.",

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyHealing(User, Target, d.Healing);
            }
        });
    }
    private static void RegisterItems()
    {
        // 140601 – Brew of a Hundred Herbs – Heal an Ally
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140601,
            cardName = "Brew of a Hundred Herbs",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 10,
            healing_u = 100,
            range_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Heal an ally for {d.Healing}.",

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyHealing(User, Target, d.Healing);
            }
        });

        // 140602 – Elixir of a Hundred Herbs – +Max Health for 3 turns
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140602,
            cardName = "Elixir of a Hundred Herbs",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 10,
            duration_u = 3,
            power_u = 100, // +MaxHealth
            range_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Increase Max Health by {d.Power} for {d.Duration} turns.",

            CardEffect = (User, Target, d) =>
            {
                var stat = Target.entityStats.MaxHealth;
                var mod = new StatModifier(
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration,
                    on_triggerConditionRef: new TriggerRef { References = new() { GameplayRef.onTurnStart }, AffectedEntityId = Target.GetInstanceID() },
                    name: $"MaxHP+{d.Power}"
                );
                CombatUtility.ApplyBuff(User, Target, stat, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

        // 140603 – Pill of a Hundred Herbs – +Max Health (indefinite)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140603,
            cardName = "Pill of a Hundred Herbs",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 10,
            duration_u = 0,
            power_u = 100,
            range_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Permanently increase Max Health by {d.Power}.",

            CardEffect = (User, Target, d) =>
            {
                var stat = Target.entityStats.MaxHealth;
                var mod = new StatModifier(
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration, // 0 => indefinite in deinem System
                    on_triggerConditionRef: new TriggerRef(), // kein Ablauf-Trigger nötig
                    name: $"MaxHP+{d.Power}_Pill"
                );
                CombatUtility.ApplyBuff(User, Target, stat, mod, ModifierMergeStrategy.Merge);
            }
        });

        // 140604 – Crimson Rejuvenation Brew – Regenerates Stamina
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140604,
            cardName = "Crimson Rejuvenation Brew",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 1,
            power_u = 150,
            range_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Regenerate stamina (value {d.Power}).",

            CardEffect = (User, Target, d) =>
            {
                // TODO Stamina
            }
        });

        // 140605 – Crimson Rejuvenation Elixir – +Max Stamina for 3 turns
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140605,
            cardName = "Crimson Rejuvenation Elixir",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 10,
            duration_u = 3,
            power_u = 150,
            range_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Increase Max Stamina by {d.Power} for {d.Duration} turns.",

            CardEffect = (User, Target, d) =>
            {
                var stat = Target.entityStats.MaxStamina;
                var mod = new StatModifier(
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration,
                    on_triggerConditionRef: new TriggerRef { References = new() { GameplayRef.onTurnStart }, AffectedEntityId = Target.GetInstanceID() },
                    name: $"MaxStamina+{d.Power}"
                );
                CombatUtility.ApplyBuff(User, Target, stat, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

        // 140606 – Crimson Rejuvenation Pill – +Max Stamina (indefinite)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140606,
            cardName = "Crimson Rejuvenation Pill",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 10,
            duration_u = 0,
            power_u = 150,
            range_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Permanently increase Max Stamina by {d.Power}.",

            CardEffect = (User, Target, d) =>
            {
                var stat = Target.entityStats.MaxStamina;
                var mod = new StatModifier(
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration,
                    on_triggerConditionRef: new TriggerRef(),
                    name: $"MaxStamina+{d.Power}"
                );
                CombatUtility.ApplyBuff(User, Target, stat, mod, ModifierMergeStrategy.Merge);
            }
        });

        // 140607 – Brew of Unbroken Will – +Armour for 1 Turn (DamageIncrease)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140607,
            cardName = "Brew of Unbroken Will",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 10,
            duration_u = 1,
            power_u = 100,
            range_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Ally gains +{d.Power} armour for this turn.",

            CardEffect = (User, Target, d) =>
            {
                var stat = Target.entityStats.DamageIncrease;
                var mod = new StatModifier(
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration,
                    on_triggerConditionRef: new TriggerRef { References = new() { GameplayRef.onTurnStart }, AffectedEntityId = Target.GetInstanceID() },
                    name: $"ArmourIncrease+{d.Power}"
                );
                CombatUtility.ApplyBuff(User, Target, stat, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

        // 140608 – Elixir of Unbroken Will – +Armour for 3 turns
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140608,
            cardName = "Elixir of Unbroken Will",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 10,
            duration_u = 3,
            power_u = 100,
            range_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Ally gains +{d.Power} Armour for {d.Duration} turns.",

            CardEffect = (User, Target, d) =>
            {
                var stat = Target.entityStats.Armour;
                var mod = new StatModifier(
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration,
                    on_triggerConditionRef: new TriggerRef { References = new() { GameplayRef.onTurnStart }, AffectedEntityId = Target.GetInstanceID() },
                    name: $"ArmourIncrease+{d.Power}"
                );
                CombatUtility.ApplyBuff(User, Target, stat, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

        // 140609 – Pill of Unbroken Will – +Armour (indefinite)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140609,
            cardName = "Pill of Unbroken Will",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 10,
            duration_u = 0,
            power_u = 100,
            range_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Permanently increase Armour by {d.Power}.",

            CardEffect = (User, Target, d) =>
            {
                var stat = Target.entityStats.Armour;
                var mod = new StatModifier(
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration,
                    on_triggerConditionRef: new TriggerRef(),
                    name: $"ArmourIncrease+{d.Power}"
                );
                CombatUtility.ApplyBuff(User, Target, stat, mod, ModifierMergeStrategy.Merge);
            }
        });

        // 140610 – Soaring Dragon Brew – +Attack for 1 turn
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140610,
            cardName = "Soaring Dragon Brew",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 10,
            duration_u = 1,
            power_u = 100,
            range_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Ally gains +{d.Power} damage for this turn.",

            CardEffect = (User, Target, d) =>
            {
                var stat = Target.entityStats.DamageIncrease;
                var mod = new StatModifier(
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration,
                    on_triggerConditionRef: new TriggerRef { References = new() { GameplayRef.onTurnStart }, AffectedEntityId = Target.GetInstanceID() },
                    name: $"SoaringDragon_Dmg+{d.Power}"
                );
                CombatUtility.ApplyBuff(User, Target, stat, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

        // 140611 – Soaring Dragon Elixir – +Attack for X turns (here 3)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140611,
            cardName = "Soaring Dragon Elixir",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 10,
            duration_u = 3,
            power_u = 100,
            range_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Ally gains +{d.Power} damage for {d.Duration} turns.",

            CardEffect = (User, Target, d) =>
            {
                var stat = Target.entityStats.DamageIncrease;
                var mod = new StatModifier(
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration,
                    on_triggerConditionRef: new TriggerRef { References = new() { GameplayRef.onTurnStart }, AffectedEntityId = Target.GetInstanceID() },
                    name: $"SoaringDragonElixir_Dmg+{d.Power}"
                );
                CombatUtility.ApplyBuff(User, Target, stat, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

        // 140612 – Soaring Dragon Pill – +Attack (indefinite)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140612,
            cardName = "Soaring Dragon Pill",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 10,
            duration_u = 0,
            power_u = 100,
            range_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Permanently increase damage by {d.Power}.",

            CardEffect = (User, Target, d) =>
            {
                var stat = Target.entityStats.DamageIncrease;
                var mod = new StatModifier(
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration,
                    on_triggerConditionRef: new TriggerRef(),
                    name: $"SoaringDragonPill_Dmg+{d.Power}"
                );
                CombatUtility.ApplyBuff(User, Target, stat, mod, ModifierMergeStrategy.Merge);
            }
        });

        // 140613 – Crystal Cleansing Balm – Cleanse target (status removal) -> TODO
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140613,
            cardName = "Crystal Cleansing Balm",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 10,
            range_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Radius,

            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Cleanses Target of all DoTs.",

            CardEffect = (User, Target, d) =>
            {
                //TODO Cleanse
            }
        });

        // 140614 – Mandrake Poison Cloud – Throws Poison Cloud
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140614,
            cardName = "Mandrake Poison Cloud",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.Poison },

            cost_u = 30,
            damage_u = 2,
            duration_u = 3,
            range_u = 4,
            area_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Radius,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Deal {d.Damage} poison damage in a small area. A poisonous clod stays on the field.",

            CardEffect = (User, Target, d) =>
            {
                string name = $"Poison#{d.cardID}";
                var poison = new EntityModifier(
                    statName: name,
                    baseValue: d.Damage,
                    to_Trigger_refs: new() { GameplayRef.onPoison },
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
                            References = new() { GameplayRef.onPoison },
                            UserId = User.GetInstanceID(),
                            AffectedEntityId = Target.GetInstanceID()
                        });
                        CombatUtility.ApplyDamage(User, Target, mod.BaseValue);
                    });

                CombatUtility.ApplyEntityModifier(User, Target, poison, ModifierMergeStrategy.RefreshDurationAndMerge);

                // ToDO: Posion Cloud sollte länger auf dem Spielfeld und ALLE vergiften die durchgehen wollen
            }
        });
    }

    private static void RegisterCurse()
    {
        // 140401 – Alchemist’s Misstep – failure chance 20% fail for Items
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140401,
            cardName = "Alchemist’s Misstep",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 10,
            range_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Radius,
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
        // 140501 – Mythical Herb – Increases potency of items 
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140501,
            cardName = "Mythical Herb",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 10,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Radius,
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
