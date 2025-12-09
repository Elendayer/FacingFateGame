using System.Collections.Generic;
using UnityEngine;
using Utility;

// Mystic cards in Fire Bomb style. Damage-focused; complex utility left as comments.
// Call MysticCards.RegisterAll() from CardDatabase.RegisterAll().
public static class MysticCards
{
    public static void RegisterAll()
    {
        RegisterSpells();
        RegisterMartialArts();
        RegisterAbilities();
        RegisterCurses();
        RegisterBlessings();
    }

    // Class code = 13|TT|II


    private static void RegisterMartialArts()
    {
        // 130101 – Mind Shock – direct damage
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 130101,
            cardName = "Mind Shock",
            cardType = CardType.Technique,
            cardClass = CardClass.Mystic,
            cardIdentities = new() { CardIdentity.Soul },

            cost_u = 50,
            power_u = 70,
            range_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardSelectionType = CardTargetingModeType.Single,
            },
            CardDescription = (User, d) => d.cardDescription = $"Deal {d.Power} damage.",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target, d.Power);
            }
        });

        // 130102 – Staff Swing – melee damage
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 130102,
            cardName = "Staff Swing",
            cardType = CardType.Technique,
            cardClass = CardClass.Mystic,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 20,
            power_u = 40,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardSelectionType = CardTargetingModeType.Single,
            },
            CardDescription = (User, d) => d.cardDescription = $"Deal {d.Power} melee damage.",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target);
            }
        });

        // 130103 – Absorb Qi – damage + resource (resource part TODO)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 130103,
            cardName = "Absorb Qi",
            cardType = CardType.Technique,
            cardClass = CardClass.Mystic,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 30,
            power_u = 30,
            range_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardSelectionType = CardTargetingModeType.Single,
            },
            CardDescription = (User, d) => d.cardDescription = $"Deal {d.Power} damage.",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target, d.Power);
                // TODO: absorb resource from target
            }
        });
    }

    private static void RegisterAbilities()
    {
        // 130201 – Dancing Shadow – all entities attack if possible (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 130201,
            cardName = "Dancing Shadow",
            cardType = CardType.Ability,
            cardClass = CardClass.Mystic,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 10,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.All,
                cardSelectionType = CardTargetingModeType.Single,
            },
            CardDescription = (User, d) => d.cardDescription = "All entities attack if possible (TODO).",
            CardEffect = (User, Target, d) => { /* TODO */ }
        });

        // 130202 – Meditation – regenerate mana? (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 130202,
            cardName = "Meditation",
            cardType = CardType.Ability,
            cardClass = CardClass.Mystic,
            cardIdentities = new() { CardIdentity.None },
            cost_u = 0,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                cardSelectionType = CardTargetingModeType.Single,
            },
            CardDescription = (User, d) => d.cardDescription = "Regenerate mana? (TODO).",
            CardEffect = (User, Target, d) => { /* TODO */ }
        });
    }
    private static void RegisterSpells()
    {
        // 130301 – Illusionary Double – summon/aggro (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 130301,
            cardName = "Illusionary Double",
            cardType = CardType.Spell,
            cardClass = CardClass.Mystic,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 0,
            range_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Ground,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardSelectionType = CardTargetingModeType.Single,
            },
            CardDescription = (User, d) => d.cardDescription = "Create a double that draws aggro and attacks once (TODO).",
            CardEffectGround = (User, Target, d) => 
            {
                CombatUtility.SpawnEntity(d, Target, "0001", EntityAffiliation.Neutral); 
            }
        });

        // 130302 – Phantom Spear Battalion – ring blockers (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 130302,
            cardName = "Phantom Spear Battalion",
            cardType = CardType.Spell,
            cardClass = CardClass.Mystic,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 80,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardSelectionType = CardTargetingModeType.Ring,
            },
            CardDescription = (User, d) => d.cardDescription = "Create 3 blocking entities around target; they attack once (TODO).",
            CardEffect = (User, Target, d) => { /* TODO spawn 3 entities */ }
        });

        // 130303 – Warp Intention – target attacks someone else (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 130303,
            cardName = "Warp Intention",
            cardType = CardType.Spell,
            cardClass = CardClass.Mystic,
            cardIdentities = new() { CardIdentity.Ranged },

            cost_u = 50,
            range_u = 8,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardSelectionType = CardTargetingModeType.Single,

            },
            CardDescription = (User, d) => d.cardDescription = "Force the target to attack someone else this turn.",
            CardEffect = (User, Target, d) =>
            {
                // TODO: Apply a temporary 'Taunt/Confuse' so the target attacks a different target.
            }
        });

        // 130304 – Sleepwalking – force move 2 spaces (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 130304,
            cardName = "Sleepwalking",
            cardType = CardType.Spell,
            cardClass = CardClass.Mystic,
            cardIdentities = new() { CardIdentity.Melee },

            cost_u = 0,
            power_u = 2,
            range_u = 5,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardSelectionType = CardTargetingModeType.Single,
            },
            CardDescription = (User, d) => d.cardDescription = "Force the target to move 2 spaces.",
            CardEffect = (User, Target, d) =>
            {
                MovementUtility.ForcedMove(ForcedMovementType.Random, Target, Target.GetComponent<EntityOnMap>().currentCell, d.Power);
            }
        });

        // 130305 – Spectral Barrier – blocks 1 space (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 130305,
            cardName = "Spectral Barrier",
            cardType = CardType.Spell,
            cardClass = CardClass.Mystic,
            cardIdentities = new() { CardIdentity.Melee },
            cost_u = 0,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,  // block a tile
                CardTargetAffiliation = CardTargetAffiliation.All,
                cardSelectionType = CardTargetingModeType.Single,
            },

            CardDescription = (User, d) => d.cardDescription = "Create a barrier that blocks 1 space.",
            CardEffect = (User, Target, d) =>
            {
                // TODO: Spawn a blocking entity/obstacle on the chosen tile for N turns.
            }
        });

        // 130306 – Spacial Reversal – swap positions (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 130306,
            cardName = "Spacial Reversal",
            cardType = CardType.Spell,
            cardClass = CardClass.Mystic,
            cardIdentities = new() { CardIdentity.Melee },
            
            cost_u = 70,
            range_u = 5,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardSelectionType = CardTargetingModeType.Single,
            },
            CardDescription = (User, d) => d.cardDescription = "Switch positions with the target.",
            CardEffect = (User, Target, d) =>
            {
                MovementUtility.SwapLocations(User, Target);
                
            }
        });

        // 130307 – Bloody Hex – proc Bleed (non-damage trigger)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 130307,
            cardName = "Bloody Hex",
            cardType = CardType.Spell,
            cardClass = CardClass.Mystic,
            cardIdentities = new() { CardIdentity.Blood },

            cost_u = 30,
            range_u = 6,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardSelectionType = CardTargetingModeType.Single,
            },
            CardDescription = (User, d) => d.cardDescription = "Proc Bleed on the target.",
            CardEffect = (User, Target, d) =>
            {
                // TODO: Immediately trigger target's Bleed tick(s) or apply Bleed if none.
            }
        });

        // 130308 – Venom Hex – proc Poison (non-damage trigger)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 130308,
            cardName = "Venom Hex",
            cardType = CardType.Spell,
            cardClass = CardClass.Mystic,
            cardIdentities = new() { CardIdentity.Poison },

            cost_u = 30,
            range_u = 6,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardSelectionType = CardTargetingModeType.Single,
            },
            CardDescription = (User, d) => d.cardDescription = "Proc Poison on the target.",
            CardEffect = (User, Target, d) =>
            {
                // TODO: Immediately trigger target's Poison tick(s) or apply Poison if none.
            }
        });

        // 130309 – Crimson Hex – Ignite proc (non-damage trigger)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 130309,
            cardName = "Crimson Hex",
            cardType = CardType.Spell,
            cardClass = CardClass.Mystic,
            cardIdentities = new() { CardIdentity.Fire },

            cost_u = 30,
            range_u = 6,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardSelectionType = CardTargetingModeType.Single,
            },
            CardDescription = (User, d) => d.cardDescription = "Proc Ignite on target (TODO).",
            CardEffect = (User, Target, d) => { /* TODO */ }
        });

        // 130310 – Mental Chains – cannot attack this turn (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 130310,
            cardName = "Mental Chains",
            cardType = CardType.Spell,
            cardClass = CardClass.Mystic,
            cardIdentities = new() { CardIdentity.Ranged },
            
            cost_u = 50,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardSelectionType = CardTargetingModeType.Single,
            },
            CardDescription = (User, d) => d.cardDescription = "Target cannot attack this turn.",
            CardEffect = (User, Target, d) =>
            {
                // TODO: Apply a 'Silence/Disarm' style modifier preventing attacks until turn end.
            }
        });

        // 130311 – Rainbow Hex – proc all (non-damage trigger, AOE sphere)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 130311,
            cardName = "Rainbow Hex",
            cardType = CardType.Spell,
            cardClass = CardClass.Mystic,
            cardIdentities = new() { CardIdentity.Ranged },
            
            cost_u = 80,
            range_u = 8,
            area_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardSelectionType = CardTargetingModeType.Radius,
            },
            CardDescription = (User, d) => d.cardDescription = "Proc all status effects on enemies in area.",
            CardEffect = (User, Target, d) =>
            {
                // TODO: For each affected enemy, trigger existing DoTs/CCs (Bleed/Poison/Ignite/etc.).
            }
        });

        // 130312 – Pure Flames – Ignite DoT
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 130312,
            cardName = "Pure Flames",
            cardType = CardType.Spell,
            cardClass = CardClass.Mystic,
            cardIdentities = new() { CardIdentity.Fire },

            cost_u = 60,
            power_u = 80,
            duration_u = 6,
            range_u = 4,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardSelectionType = CardTargetingModeType.Single,

            },
            CardDescription = (User, d) => d.cardDescription = $"Burn target for {d.Power} over {d.Duration} turns.",
            CardEffect = (User, Target, d) =>
            {
                var ignite = new EntityModifier(
                    modifierName: "Burn",
                    baseValue: d.Power,
                    toTriggerRefs: new() { GameplayRef.onBurn },
                    duration: d.Duration,
                    onRef_Trigger: new TriggerRef { OnTriggerReference = new() { GameplayRef.onTurnStart }, AffectedEntities = { Target }},
                    onRef_Action: (data, target) =>
                    {
                        CombatUtility.ApplyDamage(null, target, data.Value);
                    });
                CombatUtility.ApplyEntityModifier(d, Target, ignite, ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

    }

    private static void RegisterCurses()
    {
        // 130401 – Psychic Backlash – spells cost more (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 130401,
            cardName = "Psychic Backlash",
            cardType = CardType.Curse,
            cardClass = CardClass.Mystic,
            cardIdentities = new() { CardIdentity.None },
            cost_u = 0,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                cardSelectionType = CardTargetingModeType.Single,
            },
            CardDescription = (User, d) => d.cardDescription = "Using spells costs more.",
            CardEffect = (User, Target, d) => { /* TODO */ }
        });
    }

    private static void RegisterBlessings()
    {
        // 130501 – Inner Calm – spells cheaper (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 130501,
            cardName = "Inner Calm",
            cardType = CardType.Blessing,
            cardClass = CardClass.Mystic,
            cardIdentities = new() { CardIdentity.None },
            cost_u = 0,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                cardSelectionType = CardTargetingModeType.Single,
            },
            CardDescription = (User, d) => d.cardDescription = "Spells are cheaper (TODO).",
            CardEffect = (User, Target, d) => { /* TODO */ }
        });
    }
}
