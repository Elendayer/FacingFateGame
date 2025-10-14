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

    // --------------------------- Spells ---------------------------
    private static void RegisterSpells()
    {
        // 140101 – Jade Needle Resonance – Buff allies in area (Damage up)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140101,
            cardName = "Jade Needle Resonance",
            cardType = CardType.Spell,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 5,
            power_u = 2,     // DamageIncrease +2
            duration_u = 2,  // 2 turns

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Radius,
                range = 2,
                area = 1,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Allies in range gain +{d.Power} damage for {d.Duration} turns.",

            CardEffect = (User, Target, d) =>
            {
                // Buff DamageIncrease on each affected ally
                var stat = Target.entityStats.DamageIncrease;
                var mod = new StatModifier(
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration,
                    on_triggerConditionRef: new TriggerRef { References = new() { gameplayRef.onTurnStart }, AffectedEntityId = Target.GetInstanceID() },
                    target: stat,
                    name: $"JadeResonance_Dmg+{d.Power}"
                );
                CombatUtility.ApplyBuff(User, Target, stat, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

        // 140102 – Breath of the Jade Lotus – Heals everyone in a line
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140102,
            cardName = "Breath of the Jade Lotus",
            cardType = CardType.Spell,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 3,
            healing_u = 8,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.LineSelf,
                range = 3,
                area = 1,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Heal allies in a line for {d.Healing}.",

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyHealing(User, Target, d.Healing);
            }
        });

        // 140112 – Pure Flames – Ignite (single-target damage-over-time)  —— optional simple damage
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140112,
            cardName = "Pure Flames",
            cardType = CardType.Spell,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.Fire },

            cost_u = 2,
            damage_u = 4,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
                range = 2,
                area = 1,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Deal {d.Damage} fire damage.",

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(User, Target, d.Damage);
                // (Optional) DoT Ignite könnte später ergänzt werden.
            }
        });

        // Andere Spells mit Speziallogik (Warp Intention, Sleepwalking, etc.) -> TODO
    }

    // --------------------------- Martial Arts ---------------------------
    private static void RegisterMartialArts()
    {
        // 140201 – Jade Needle Acupuncture – HoT on ally
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140201,
            cardName = "Jade Needle Acupuncture",
            cardType = CardType.Technique,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 2,
            power_u = 2,      // HoT tick
            duration_u = 3,   // for 3 turns

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Apply a regeneration of {d.Power} for {d.Duration} turns.",

            CardEffect = (User, Target, d) =>
            {
                // Heal-over-time as positive “on turn start” tick
                var regen = new EntityModifier(
                    statName: "Regeneration",
                    baseValue: d.Power,
                    to_Trigger_refs: new() { gameplayRef.onHeal },
                    duration: d.Duration,
                    target: Target.entityStats.CurrentHealth,
                    triggerConditionRef: new TriggerRef
                    {
                        References = new() { gameplayRef.onTurnStart },
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    onRefEventAction: (mod, stat, ev) =>
                    {
                        GameEvents.TriggerRefEvent(new TriggerRef
                        {
                            References = new() { gameplayRef.onHeal },
                            UserId = User.GetInstanceID(),
                            AffectedEntityId = Target.GetInstanceID()
                        });
                        CombatUtility.ApplyHealing(User, Target, mod.BaseValue);
                    }
                );

                CombatUtility.ApplyEntityModifier(User, Target, regen, ModifierMergeStrategy.RefreshDurationAndMerge);
                // Optional: immediate small heal on play? -> we skip for now.
            }
        });

        // 140203 – Formation of the Hundred Remedies – Heal allies in ring
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140203,
            cardName = "Formation of the Hundred Remedies",
            cardType = CardType.Technique,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 1,
            healing_u = 6,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Ring,
                range = 2,
                area = 1,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Heal allies in range for {d.Healing}.",

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyHealing(User, Target, d.Healing);
            }
        });

        // 140202 Bloodletting (Poison->Bleed), 140204 Venomous Grip (worsen Poison), 140205 Cleanse Ally -> TODO
    }

    // --------------------------- Abilities ---------------------------
    private static void RegisterAbilities()
    {
        // 140401 – Gather – TODO (creates a new card based on environment)
        // 140402 – Toxic Remedy Paradox – Heal ally + poison enemy (selection) -> TODO
        // 140403 – Poison Barbs – Thorns on hit -> TODO
        // 140404 – Doctor's Footwork – Halve movement cost -> TODO
    }

    // --------------------------- Items ---------------------------
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

            cost_u = 0,
            healing_u = 10,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
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

            cost_u = 1,
            duration_u = 3,
            power_u = 10, // +MaxHealth

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
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
                    on_triggerConditionRef: new TriggerRef { References = new() { gameplayRef.onTurnStart }, AffectedEntityId = Target.GetInstanceID() },
                    target: stat,
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

            cost_u = 1,
            duration_u = 0,  // 0 -> indefinite
            power_u = 10,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
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
                    target: stat,
                    name: $"MaxHP+{d.Power}_Pill"
                );
                CombatUtility.ApplyBuff(User, Target, stat, mod, ModifierMergeStrategy.Merge);
            }
        });

        // 140604 – Crimson Rejuvenation Brew – Regenerates Stamina (TODO exact API)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140604,
            cardName = "Crimson Rejuvenation Brew",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 1,
            power_u = 5, // intended stamina amount

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Regenerate stamina (value {d.Power}).",

            CardEffect = (User, Target, d) =>
            {
                // TODO: if you have a helper like CombatUtility.RestoreStamina, use it here:
                // CombatUtility.RestoreStamina(User, Target, d.Power);
                // Fallback: small HoT on stamina stat could be added later.
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

            cost_u = 1,
            duration_u = 3,
            power_u = 5,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
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
                    on_triggerConditionRef: new TriggerRef { References = new() { gameplayRef.onTurnStart }, AffectedEntityId = Target.GetInstanceID() },
                    target: stat,
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

            cost_u = 1,
            duration_u = 0,
            power_u = 5,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
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
                    target: stat,
                    name: $"MaxStamina+{d.Power}_Pill"
                );
                CombatUtility.ApplyBuff(User, Target, stat, mod, ModifierMergeStrategy.Merge);
            }
        });

        // 140607 – Brew of Unbroken Will – Boost Ally for 1 Turn (DamageIncrease)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140607,
            cardName = "Brew of Unbroken Will",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 1,
            duration_u = 1,
            power_u = 2, 

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
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
                    on_triggerConditionRef: new TriggerRef { References = new() { gameplayRef.onTurnStart }, AffectedEntityId = Target.GetInstanceID() },
                    target: stat,
                    name: $"UnbrokenWill_Dmg+{d.Power}"
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

            cost_u = 1,
            duration_u = 3,
            power_u = 10,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
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
                    on_triggerConditionRef: new TriggerRef { References = new() { gameplayRef.onTurnStart }, AffectedEntityId = Target.GetInstanceID() },
                    target: stat,
                    name: $"Armour+{d.Power}"
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

            cost_u = 1,
            duration_u = 0,
            power_u = 10,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
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
                    target: stat,
                    name: $"Armour+{d.Power}_Pill"
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

            cost_u = 1,
            duration_u = 1,
            power_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
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
                    on_triggerConditionRef: new TriggerRef { References = new() { gameplayRef.onTurnStart }, AffectedEntityId = Target.GetInstanceID() },
                    target: stat,
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

            cost_u = 1,
            duration_u = 3,
            power_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
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
                    on_triggerConditionRef: new TriggerRef { References = new() { gameplayRef.onTurnStart }, AffectedEntityId = Target.GetInstanceID() },
                    target: stat,
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

            cost_u = 1,
            duration_u = 0,
            power_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
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
                    target: stat,
                    name: $"SoaringDragonPill_Dmg+{d.Power}"
                );
                CombatUtility.ApplyBuff(User, Target, stat, mod, ModifierMergeStrategy.Merge);
            }
        });

        // 140613 – Crystal Cleansing Balm – Cleanse target (status removal) -> TODO

        // 140614 – Mandrake Poison Cloud – Throws Poison Cloud (AoE damage placeholder)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140614,
            cardName = "Mandrake Poison Cloud",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.Poison },

            cost_u = 3,
            damage_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Radius,
                range = 2,
                area = 1,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Deal {d.Damage} poison damage in a small area.",

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(User, Target, d.Damage);
                // Persisting hazard / DoT field could be added later.
            }
        });
    }

    // --------------------------- Curse ---------------------------
    private static void RegisterCurse()
    {
        // 140701 – Alchemist’s Misstep – failure chance -> TODO (20% fail)
    }

    // --------------------------- Blessing ---------------------------
    private static void RegisterBlessing()
    {
        // 140801 – Mythical Herb – Increases potency of items -> TODO
    }
}
