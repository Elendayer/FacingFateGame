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

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "NoActionType"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "Gong"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "NoElementType"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 40,
                damage_u = 100,
                range_u = 5f,

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
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData ("Impact"), cardData.Damage);
                        })
                    )
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

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "Attack"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "NoWeaponType"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "NoElementType"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 20,
                damage_u = 50,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} melee damage.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"));
                        })
                    )
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
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"), cardData.Damage);
                            // TODO: absorb resource from target
                        })
                    )
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
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            // TODO
                        })
                    )
                }
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
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            // TODO
                        })
                    )
                }
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
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Ground,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, Vector3, CardData>)((caster, position, cardData) =>
                        {
                            CombatUtility.SpawnEntity(cardData, position, "Summon_Double", EntityAffiliation.Neutral, true);
                        })
                    )
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
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Ground,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, Vector3, CardData>)((caster, position, cardData) =>
                        {
                            // TODO spawn 3 entities
                        })
                    )
                }
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
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            // TODO: Apply a temporary 'Taunt/Confuse' so the target attacks a different target.
                        })
                    )
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

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "NoActionType"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "Gong"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "NoElementType"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

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
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            MovementUtility.ForcedMove(ForcedMovementType.Random, target, target.transform.position, cardData.Power);
                        })
                    )
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
                    CardTargetAffiliation = CardTargetAffiliation.None,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Create a spectral barrier that blocks 1 tile.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Ground,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, Vector3, CardData>)((caster, position, cardData) =>
                        {
                            CombatUtility.SpawnEntity(cardData, position, "Spectral_Barrier", EntityAffiliation.Neutral, false);
                        })
                    )
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

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "Magic"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "NoWeaponType"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "NoElementType"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 25,
                range_u = 5f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.All,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Switch positions with target.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            MovementUtility.SwapLocations(caster, target);
                        }
                    )
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

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "Magic"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "NoWeaponType"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "Blood"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 25,
                range_u = 6f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Trigger Bleed on the target.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var bleed = target.GetModifierByName("Bleed");
                            if (bleed != null)
                                CombatUtility.ApplyEffectDamage(bleed.BaseValue, target, GameplayRef.onBleed, new VFXData("BleedEffect", true));
                        }
                    )
                }
            });

            // 130308 � Venom Hex � proc Poison (non-damage trigger)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Spell_Toxic_Hex",
                cardName = "Toxic Hex",
                cardType = CardType.Spell,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.Poison },

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "Magic"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "NoWeaponType"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "Poison"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 25,
                range_u = 6f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Trigger Poison on the target.",
                cardActionSequence = new()
                {
                   new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var poison = target.GetModifierByName("Poison");
                            if (poison != null)
                                CombatUtility.ApplyEffectDamage(poison.BaseValue, target, GameplayRef.onPoison, new VFXData("PoisonEffect", true));
                        }
                    )
                }
            });

            // 130309 � Crimson Hex � Ignite proc (non-damage trigger)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Spell_Blazing_Hex",
                cardName = "Blazing Hex",
                cardType = CardType.Spell,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.Fire },

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "Magic"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "NoWeaponType"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "Fire"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 25,
                range_u = 6f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Trigger Burn on the target.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            var burn = target.GetModifierByName("Burn");
                            if (burn != null)
                                CombatUtility.ApplyEffectDamage(burn.BaseValue, target, GameplayRef.onBurn, new VFXData("BurnEffect", true));
                        }
                    )
                }
            });

            // 130310 � Mental Chains � cannot attack this turn (non-damage)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Spell_Mental_Chains",
                cardName = "Mental Chains",
                cardType = CardType.Spell,
                cardClass = CardClass.Mystic,
                cardIdentities = new() {},

                cost_u = 35,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Target cannot attack this turn.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            // TODO: Apply a 'Silence/Disarm' style modifier preventing attacks until turn end.
                        })
                    )
                }
            });

            // 130311 � Rainbow Hex � proc all (non-damage trigger, AOE sphere)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Spell_Rainbow_Hex",
                cardName = "Rainbow Hex",
                cardType = CardType.Spell,
                cardClass = CardClass.Mystic,
                cardIdentities = new() {},

                cost_u = 48,
                range_u = 8f,
                area_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Sphere,
                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Proc all status effects on enemies in area.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.1f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            // TODO: For each affected enemy, trigger existing DoTs/CCs (Bleed/Poison/Ignite/etc.).
                        })
                    )
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

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "Magic"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "NoWeaponType"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "Fire"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 30,
                damage_u = 20,
                duration_u = 6,
                range_u = 4f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,

                },
                cardDescriptionAction = (User, d) => d.cardDescription = "Burn target for {Damage}.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            var ignite = new EntityModifier(
                                modifierName: "Burn",
                                owner: target,
                                baseValue: cardData.Damage,
                                // toTriggerRefs omitted: ApplyEffectDamage already fires onBurn via HandlePostCombatTrigger
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
                                    CombatUtility.ApplyEffectDamage(value, targetEntity, GameplayRef.onBurn, new VFXData("BurnEffect", true));
                                });
                            CombatUtility.ApplyEntityModifier(cardData, target, ignite);
                        })
                    )
                }
            });

        }

        private static void RegisterCurses()
        {
            // 130401 – Psychic Backlash – spells cost more (non-damage)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Mystic_Curse_Psychic_Backlash",
                cardName = "Psychic Backlash",
                cardType = CardType.Curse,
                cardClass = CardClass.Mystic,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 0,
                duration_u = 2,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Spells cost 5 more for {Duration} turns",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
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
                                        name: $"Inner Calm",
                                        stat: target.entityStats.CardCostModifier_Flat,
                                        value: 5,
                                        to_TriggerRefs: new(),
                                        duration: cardData.Duration,
                                        modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge,
                                        condition: "Spell"
                                        )
                                    );
                            });
                        }))
                }
            });
        }





        private static void RegisterBlessings()
        {
            // 130501 – Inner Calm – spells cheaper (non-damage)
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
                cardDescriptionAction = (User, d) => d.cardDescription = "Spells cost 5 less for 2 turns",
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
                                    name: $"Inner Calm",
                                    stat: target.entityStats.CardCostModifier_Flat,
                                    value: -5,
                                    to_TriggerRefs: new(),
                                    duration: cardData.Duration,
                                    modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge,
                                    condition: "Spell"
                                ));

                        }
                    )
                }
            });
        }
    }
}

