using System.Collections.Generic;
using UnityEngine;
using Utility;
// Physician cards in Fire Bomb style. Keep cards only under Spells, MartialArts, and Items (no Abilities/Curses/Blessings here).
// Call PhysicianCards.RegisterAll() from CardDatabase.RegisterAll().
public static class PhysicianCards
{
    public static void RegisterAll()
    {
        RegisterSpells();
        RegisterMartialArts();
        RegisterAbilities();
        RegisterItems();
        RegisterBlessings();
        RegisterCurses();
    }

    // ID scheme: 14 | TT | II
    // TT: MartialArt=01, Ability=02, Spell=03, Curse=04, Blessing=05, Item=07

    private static void RegisterMartialArts()
    {
        // 140101 – Jade Needle Acupuncture (Single Ally) – boost life regen
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140101,
            cardName = "Jade Needle Acupuncture",
            cardType = CardType.Technique,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.Ranged },

            cost_u = 6,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1, 
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Boost ally life regeneration (TBD: value & duration).",
            CardEffect = (User, Target, d) => { /* TODO: regen buff */ }
        });

        // 140102 – Bloodletting (Single Enemy) – convert Poison → Bleed
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140102,
            cardName = "Bloodletting",
            cardType = CardType.Technique,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.Blood, CardIdentity.Poison },

            cost_u = 4,
            power_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Turn target's Poison into Bleed (TBD: conversion rule).",
            CardEffect = (User, Target, d) => { /* TODO: convert Poison stacks to Bleed stacks */ }
        });

        // 140103 – Formation of the Hundred Remedies (Ring Ally) – heal AOE
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140103,
            cardName = "Formation of the Hundred Remedies",
            cardType = CardType.Technique,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Ring,
                range = 1,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Heal all allies in range (TBD: heal value).",
            CardEffect = (User, Target, d) => { /* TODO: heal allies in ring */ }
        });

        // 140104 – Venomous Grip (Single Enemy) – worsen Poison
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140104,
            cardName = "Venomous Grip",
            cardType = CardType.Technique,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.Poison },

            cost_u = 4,
            power_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
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

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Cleanse ally (remove negative effects).",
            CardEffect = (User, Target, d) => { /* TODO: cleanse implementation */ }
        });
    }

    private static void RegisterSpells()
    {
        // 140301 – Jade Needle Resonance (Sphere, Ally) – non-damage boost per ally
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140301,
            cardName = "Jade Needle Resonance",
            cardType = CardType.Spell,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 8,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Radius,
                range = 2,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Increase boost for every ally in range (TBD: exact buff & scaling).",
            CardEffect = (User, Target, d) =>
            {
                // TODO: For each ally in sphere around chosen tile/user, grant stacking boost.
            }
        });

        // 140302 – Breath of the Jade Lotus (Line, Ally) – non-damage heal line
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140302,
            cardName = "Breath of the Jade Lotus",
            cardType = CardType.Spell,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 3,
            power_u = 0,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.LineSelf,
                range = 3,
                area = 1,
            },

            CardDescription = (User, d) => d.cardDescription = "Heal everyone in a line (TBD: heal value).",
            CardEffect = (User, Target, d) =>
            {
                // TODO: Heal allied entities along the line.
            }
        });
    }

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
                range = 1,
                area = 1,
            },

            CardDescription = (User, d) =>
                d.cardDescription = "Gather materials around you. Value=3 ⇒ create/draw a new card (TODO).",

            CardEffect = (User, Target, d) =>
            {
                // TODO: Sammel-Stack am User erhöhen; bei Stack >= 3 -> neue Karte erzeugen/ziehen und Stack resetten.
            }
        });

        // 140202 – Toxic Remedy Paradox – heilt einen Ally & vergiftet einen Enemy (Selection, 0–1 Range)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140202,
            cardName = "Toxic Remedy Paradox",
            cardType = CardType.Ability,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.Poison },

            cost_u = 4,      // Tabelle: 0–1; falls dynamisch, später anpassen

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.None, // wir wählen Ally & Enemy
                SelectionType = CardTargetSelection.Single,                // Mehrfach-/Zielauswahl
                range = 1,                                          // Tabelle: 0–1
                area = 1,
            },

            CardDescription = (User, d) =>
                d.cardDescription = "Select an ally to heal and an enemy to Poison (TODO amounts).",

            CardEffect = (User, Target, d) =>
            {
                // TODO: Zielauswahl entkoppeln: Ally heilen, Enemy Poison-DoT anwenden.
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
            duration_u = 1,  // wirkt bis Rundenende (ggf. anpassen)

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
                range = 0,
                area = 1,
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
                range = 0,
                area = 1,
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

    private static void RegisterCurses()
    {
        // 140401 – Alchemist's Mishap – failure chance (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140401,
            cardName = "Alchemist's Mishap",
            cardType = CardType.Curse,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.Poison },
            cost_u = 0,
            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
                range = 0,
                area = 1,
            },
            CardDescription = (User, d) => d.cardDescription = "20%: item fails (TODO).",
            CardEffect = (User, Target, d) => { /* TODO */ }
        });
    }

    private static void RegisterBlessings()
    {
        // 140501 – Mythical Herb – items stronger (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140501,
            cardName = "Mythical Herb",
            cardType = CardType.Blessing,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },
            cost_u = 0,
            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
                range = 0,
                area = 1,
            },
            CardDescription = (User, d) => d.cardDescription = "Increase item potency (TODO).",
            CardEffect = (User, Target, d) => { /* TODO */ }
        });
    }

    private static void RegisterItems()
    {
        // 140601 – Brew of a Hundred Herbs – Heal ally
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140601,
            cardName = "Brew of a Hundred Herbs",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },
            cost_u = 3,
            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },
            CardDescription = (User, d) => d.cardDescription = "Heal an ally (amount TBD).",
            CardEffect = (User, Target, d) => { /* TODO: heal */ }
        });

        // 140602 – Elixir of a Hundred Herbs – +Max Health for 3 turns
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140602,
            cardName = "Elixir of a Hundred Herbs",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 3,
            duration_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },
            CardDescription = (User, d) => d.cardDescription = "Increase Max Health for 3 turns (value TBD).",
            CardEffect = (User, Target, d) => { /* TODO: temp max health up */ }
        });

        // 140603 – Pill of a Hundred Herbs – +Max Health
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140603,
            cardName = "Pill of a Hundred Herbs",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },
            cost_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },
            CardDescription = (User, d) => d.cardDescription = "Increase Max Health (duration TBD).",
            CardEffect = (User, Target, d) => { /* TODO: add max health (persist/encounter) */ }
        });

        // 140604 – Crimson Rejuvenation Brew – regenerate stamina
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140604,
            cardName = "Crimson Rejuvenation Brew",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },
            CardDescription = (User, d) => d.cardDescription = "Regenerate stamina (amount TBD).",
            CardEffect = (User, Target, d) => { /* TODO: restore stamina */ }
        });

        // 140605 – Crimson Rejuvenation Elixir – +Max Stamina for 3 turns
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140605,
            cardName = "Crimson Rejuvenation Elixir",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 3,
            duration_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },
            CardDescription = (User, d) => d.cardDescription = "Increase Max Stamina for 3 turns (value TBD).",
            CardEffect = (User, Target, d) => { /* TODO: temp max stamina up */ }
        });

        // 140606 – Crimson Rejuvenation Pill – +Max Stamina
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140606,
            cardName = "Crimson Rejuvenation Pill",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },
            CardDescription = (User, d) => d.cardDescription = "Increase Max Stamina (duration TBD).",
            CardEffect = (User, Target, d) => { /* TODO: add max stamina (persist/encounter) */ }
        });

        // 140607 – Brew of Unbroken Will – boost ally for 1 turn
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140607,
            cardName = "Brew of Unbroken Will",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 3,
            power_u = 5,
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },
            CardDescription = (User, d) => d.cardDescription = "Boost ally for 1 turn (details TBD).",
            CardEffect = (User, Target, d) => 
            { 
                CombatUtility.ApplyBuff(User, Target, Target.entityStats.Armour, new StatModifier(d.Power, ModifierScaling.Flat, new List<gameplayRef> {gameplayRef.onBuffed }, duration: d.Duration, name: "Brew of Unbroken Will"), ModifierMergeStrategy.Merge);
            }
        });

        // 140608 – Elixir of Unbroken Will – +Armor for 3 turns
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140608,
            cardName = "Elixir of Unbroken Will",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 3,
            duration_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },
            CardDescription = (User, d) => d.cardDescription = "Increase ally Armor for 3 turns (value TBD).",
            CardEffect = (User, Target, d) => { /* TODO: armor buff */ }
        });

        // 140609 – Pill of Unbroken Will – +Armor
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140609,
            cardName = "Pill of Unbroken Will",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },
            CardDescription = (User, d) => d.cardDescription = "Increase ally Armor (duration TBD).",
            CardEffect = (User, Target, d) => { /* TODO: armor increase persist/encounter */ }
        });

        // 140610 – Soaring Dragon Brew – +Attack for 1 turn
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140610,
            cardName = "Soaring Dragon Brew",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 3,
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },
            CardDescription = (User, d) => d.cardDescription = "Increase ally Attack for 1 turn (value TBD).",
            CardEffect = (User, Target, d) => { /* TODO: attack buff 1T */ }
        });

        // 140611 – Soaring Dragon Elixir – +Attack for X turns (placeholder 3)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140611,
            cardName = "Soaring Dragon Elixir",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 3,
            duration_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },
            CardDescription = (User, d) => d.cardDescription = "Increase ally Attack for several turns (value/X TBD).",
            CardEffect = (User, Target, d) => { /* TODO: attack buff multi-turn */ }
        });

        // 140612 – Soaring Dragon Pill – +Attack
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140612,
            cardName = "Soaring Dragon Pill",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },
            CardDescription = (User, d) => d.cardDescription = "Increase ally Attack (duration TBD).",
            CardEffect = (User, Target, d) => { /* TODO: attack increase persist/encounter */ }
        });

        // 140613 – Crystal Cleansing Balm – cleanse target
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140613,
            cardName = "Crystal Cleansing Balm",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Ally,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },
            CardDescription = (User, d) => d.cardDescription = "Cleanse target from negative effects.",
            CardEffect = (User, Target, d) => { /* TODO: remove debuffs */ }
        });

        // 140614 – Mandrake Poison Cloud – Sphere Poison DoT (damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 140614,
            cardName = "Mandrake Poison Cloud",
            cardType = CardType.Item,
            cardClass = CardClass.Physician,
            cardIdentities = new() { CardIdentity.Poison },

            cost_u = 4,
            power_u = 2,
            duration_u = 6,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Radius,
                range = 3,
                area = 2,
            },

            CardDescription = (User, d) => d.cardDescription = $"Throw a poison cloud: apply Poison {d.Power} for {d.Duration} turns in area.",
            CardEffect = (User, Target, d) =>
            {
                var poison = new EntityModifier(
                    statName: "Poison",
                    baseValue: d.Power,
                    to_Trigger_refs: new() { gameplayRef.onPoison },
                    duration: d.Duration,
                    target: Target.entityStats.CurrentHealth,
                    triggerConditionRef: new TriggerRef { References = new() { gameplayRef.onTurnStart }, AffectedEntityId = Target.GetInstanceID() },
                    onRefEventAction: (mod, stat, refEv) =>
                    {
                        GameEvents.TriggerRefEvent(new TriggerRef { References = new() { gameplayRef.onPoison }, UserId = User.GetInstanceID(), AffectedEntityId = Target.GetInstanceID() });
                        CombatUtility.ApplyDamage(User, Target, mod.BaseValue);
                    });
                CombatUtility.ApplyEntityModifier(User, Target, poison, ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });
    }
}
