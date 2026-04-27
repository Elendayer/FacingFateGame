using System.Collections.Generic;
using UnityEngine;
namespace facingfate
{
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
            // 130101 � Mind Shock � direct damage
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Tech_Mind_Shock",
                cardName = "Mind Shock",
                cardType = CardType.Technique,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.Soul },

                cost_u = 40,
                damage_u = 130,
                range_u = 3f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} psychic damage.",
                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData ("Impact"), d.Damage);
                }
            });

            // 130102 � Staff Swing � melee damage
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Tech_Staff_Swing",
                cardName = "Staff Swing",
                cardType = CardType.Technique,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 20,
                damage_u = 50,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} melee damage.",
                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"));
                }
            });

            // 130103 � Absorb Qi � damage + resource (resource part TODO)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Tech_Absorb_Qi",
                cardName = "Absorb Qi",
                cardType = CardType.Technique,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 25,
                damage_u = 75,
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage and absorb Qi from target.",
                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"), d.Damage);
                    // TODO: absorb resource from target
                }
            });
        }

        private static void RegisterAbilities()
        {
            // 130201 � Dancing Shadow � all entities attack if possible (non-damage)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Abil_Dancing_Shadow",
                cardName = "Dancing Shadow",
                cardType = CardType.Ability,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 10,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.All,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "All entities attack if possible (TODO).",
                cardEffectAction = (User, Target, d) => { /* TODO */ }
            });

            // 130202 � Meditation � regenerate mana? (non-damage)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Abil_Meditation",
                cardName = "Meditation",
                cardType = CardType.Ability,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.None },
                cost_u = 0,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Regenerate mana? (TODO).",
                cardEffectAction = (User, Target, d) => { /* TODO */ }
            });
        }
        private static void RegisterSpells()
        {
            // 130301 � Illusionary Double � summon/aggro (non-damage)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Spell_Illusionary_Double",
                cardName = "Illusionary Double",
                cardType = CardType.Spell,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 8,
                range_u = 3f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Create an illusionary double that draws aggro and attacks once.",
                cardEffectGroundAction = (User, Target, d) =>
                {
                    CombatUtility.SpawnEntity(d, Target, "Summon_Double", EntityAffiliation.Neutral, true);
                }
            });

            // 130302 � Phantom Spear Battalion � ring blockers (non-damage)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Spell_Phantom_Spear_Battalion",
                cardName = "Phantom Spear Battalion",
                cardType = CardType.Spell,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 45,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Ring,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Summon 3 phantom spearmen around target. They each attack once.",
                cardEffectAction = (User, Target, d) => { /* TODO spawn 3 entities */ }
            });

            // 130303 � Warp Intention � target attacks someone else (non-damage)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Spell_Warp_Intention",
                cardName = "Warp Intention",
                cardType = CardType.Spell,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.Ranged },

                cost_u = 40,
                range_u = 8f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,

                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Force target to attack someone else this turn.",
                cardEffectAction = (User, Target, d) =>
                {
                    // TODO: Apply a temporary 'Taunt/Confuse' so the target attacks a different target.
                }
            });

            // 130304 � Sleepwalking � force move 2 spaces (non-damage)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Spell_Sleepwalking",
                cardName = "Sleepwalking",
                cardType = CardType.Spell,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.Melee },

                cost_u = 8,
                power_u = 2,
                range_u = 5f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Force target to move 2 spaces randomly.",
                cardEffectAction = (User, Target, d) =>
                {
                    MovementUtility.ForcedMove(ForcedMovementType.Random, Target, Target.transform.position, d.Power);
                }
            });

            // 130305 � Spectral Barrier � blocks 1 space (non-damage)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Spell_Spectral_Barrier",
                cardName = "Spectral Barrier",
                cardType = CardType.Spell,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.Melee },
                cost_u = 8,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.All,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Create a spectral barrier that blocks 1 tile.",
                cardEffectAction = (User, Target, d) =>
                {
                    // TODO: Spawn a blocking entity/obstacle on the chosen tile for N turns.
                }
            });

            // 130306 � Spacial Reversal � swap positions (non-damage)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Spell_Spacial_Reversal",
                cardName = "Spacial Reversal",
                cardType = CardType.Spell,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.Melee },

                cost_u = 40,
                range_u = 5f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.AllyEnemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Switch positions with target.",
                cardEffectAction = (User, Target, d) =>
                {
                    MovementUtility.SwapLocations(User, Target);

                }
            });

            // 130307 � Bloody Hex � proc Bleed (non-damage trigger)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Spell_Bloody_Hex",
                cardName = "Bloody Hex",
                cardType = CardType.Spell,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.Blood },

                cost_u = 25,
                range_u = 6f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Immediately proc Bleed on target.",
                cardEffectAction = (User, Target, d) =>
                {
                    // TODO: Immediately trigger target's Bleed tick(s) or apply Bleed if none.
                }
            });

            // 130308 � Venom Hex � proc Poison (non-damage trigger)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Spell_Venom_Hex",
                cardName = "Venom Hex",
                cardType = CardType.Spell,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.Poison },

                cost_u = 25,
                range_u = 6f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Immediately proc Poison on target.",
                cardEffectAction = (User, Target, d) =>
                {
                    // TODO: Immediately trigger target's Poison tick(s) or apply Poison if none.
                }
            });

            // 130309 � Crimson Hex � Ignite proc (non-damage trigger)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Spell_Crimson_Hex",
                cardName = "Crimson Hex",
                cardType = CardType.Spell,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.Fire },

                cost_u = 25,
                range_u = 6f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Immediately proc Ignite on target.",
                cardEffectAction = (User, Target, d) => { /* TODO */ }
            });

            // 130310 � Mental Chains � cannot attack this turn (non-damage)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Spell_Mental_Chains",
                cardName = "Mental Chains",
                cardType = CardType.Spell,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.Ranged },

                cost_u = 35,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Target cannot attack this turn.",
                cardEffectAction = (User, Target, d) =>
                {
                    // TODO: Apply a 'Silence/Disarm' style modifier preventing attacks until turn end.
                }
            });

            // 130311 � Rainbow Hex � proc all (non-damage trigger, AOE sphere)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Spell_Rainbow_Hex",
                cardName = "Rainbow Hex",
                cardType = CardType.Spell,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.Ranged },

                cost_u = 48,
                range_u = 8f,
                area_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Radius,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Proc all status effects on enemies in area.",
                cardEffectAction = (User, Target, d) =>
                {
                    // TODO: For each affected enemy, trigger existing DoTs/CCs (Bleed/Poison/Ignite/etc.).
                }
            });

            // 130312 � Pure Flames � Ignite DoT
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Spell_Pure_Flames",
                cardName = "Pure Flames",
                cardType = CardType.Spell,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.Fire },

                cost_u = 45,
                damage_u = 17,
                duration_u = 6,
                range_u = 4f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,

                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Burn target for {Damage}/turn over {Duration} turns.",
                cardEffectAction = (User, Target, d) =>
                {
                    var ignite = new EntityModifier(
                        modifierName: "Burn",
                        owner: Target,
                        baseValue: d.Damage,
                        toTriggerRefs: new() { GameplayRef.onBurn },
                        duration: d.Duration,
                        onRef_Trigger: new RelevantTriggerCheck
                        {
                            OnTriggerReference = new() { GameplayRef.onTurnStart },
                            CheckType = CheckEntityType.User,
                            CheckEntity = Target
                        },
                        onRef_Action: (target, cd, value) =>
                        {
                            CombatUtility.ApplyDamage(null, target, new VFXData("Impact"), value);
                        });
                    CombatUtility.ApplyEntityModifier(d, Target, ignite, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

        }

        private static void RegisterCurses()
        {
            // 130401 � Psychic Backlash � spells cost more (non-damage)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Curse_Psychic_Backlash",
                cardName = "Psychic Backlash",
                cardType = CardType.Curse,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.None },
                cost_u = 0,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Using spells costs more.",
                cardEffectAction = (User, Target, d) => { /* TODO */ }
            });
        }

        private static void RegisterBlessings()
        {
            // 130501 � Inner Calm � spells cheaper (non-damage)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Bless_Inner_Calm",
                cardName = "Inner Calm",
                cardType = CardType.Blessing,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.None },
                cost_u = 0,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Spells are cheaper (TODO).",
                cardEffectAction = (User, Target, d) => { /* TODO */ }
            });
        }
    }
}

