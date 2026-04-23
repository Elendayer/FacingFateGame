using UnityEngine;

namespace facingfate
{
    // Neutral cards (Class = 10)
    // ID-Schema: 10 | TT | II   (TT: MartialArt=01, Ability=02, Spell=03, Curse=04, Blessing=05, Item=06)
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
                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"), d.Damage);
                },
            });

            // 100102 – Heavy Blow – slow heavy hit (TODO: -1 Movement)
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
                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"), d.Damage);
                    CombatUtility.ApplyStatDebuff(d, Target,
                        new StatModifier(
                        name: "Movement",
                        stat: Target.entityStats.MovementCostModifier_Increase,
                        value: 1,
                        condition: true,
                        duration: d.Duration
                    ), ModifierMergeStrategy.RefreshDurationAndMerge);
                },
                cardVfx = (Data,Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData ("Debuff"), Target.targetedEntities);
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
                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData ("Impact") ,d.Damage);
                },
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
                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("SlashImpact") { activationCount = d.Repeats }, d.Damage);
                },
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
                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData ("Impact") , d.Damage);
                    MovementUtility.ForcedMove(ForcedMovementType.Push, Target, User.transform.position, 1);
                }
            });

            // 100106 – Charge – move both 1 space 
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
                cardEffectAction = (User, Target, d) =>
                {
                    MovementUtility.ForcedMove(ForcedMovementType.Pull, User, Target.transform.position);
                    MovementUtility.ForcedMove(ForcedMovementType.Push, Target, User.transform.position, 1);

                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"), d.Damage);
                    // TODO: User/Target bewegen und Stun anwenden
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

                cardDescriptionAction = (User, d) => d.cardDescription = "Disengage by 2.5 meters.",
                cardEffectAction = (User, Target, d) =>
                {
                    MovementUtility.ForcedMove(ForcedMovementType.Push, User, User.transform.position, 2.5f);
                    // TODO: User 1 Feld rückwärts bewegen
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

                cardDescriptionAction = (User, d) => d.cardDescription = $"Deal {d.Damage} damage.",
                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"), d.Damage);
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
                cardEffectAction = (User, Target, d) =>
                {
                    // direct hit (per repeat)
                    CombatUtility.ApplyDamage(d, Target, new VFXData ("SlashImpact"), d.Damage);

                    var bleed = new EntityModifier(
                        modifierName: "Bleed",
                        owner: Target,
                        baseValue: d.Damage,
                        toTriggerRefs: new() { GameplayRef.onBleed },
                        duration: d.Duration,
                        onRef_Trigger: new RelevantTriggerCheck
                        {
                            OnTriggerReference = new() { GameplayRef.onTurnStart },
                            CheckType = CheckEntityType.User,
                            CheckEntity = Target,
                        },
                        onRef_Action: (target, cd, value) =>
                        {
                            CombatUtility.ApplyEffectDamage(value, target, GameplayRef.onBleed , new VFXData ("BleedEffect"));
                        });

                    CombatUtility.ApplyEntityModifier(d, Target, bleed, ModifierMergeStrategy.RefreshDurationAndMerge);
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
                cardEffectAction = (User, Target, d) =>
                {
                    MovementUtility.ForcedMove(ForcedMovementType.Jump, User, Target.transform.position);
                },
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
                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"));
                },
                cardVfx = (Data, Target) =>
                {
                    //Wip VFX for Cone
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

                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyHealing(d, Target, d.Healing);
                },
                cardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Buff"), Target.targetedEntities);
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
                cardEffectAction = (User, Target, d) =>
                {
                    User.DrawCards(1);
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
                cardEffectAction = (User, Target, d) =>
                {
                    var stat = Target.entityStats.DamageOutModifier_Increase;
                    var mod = new StatModifier(
                        name: "Focus",
                        stat: stat,
                        value: d.Power,
                        duration: d.Duration
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
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
                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyStatDebuff(d, Target, new StatModifier(
                        name: "Growl Damage Reduction",
                        stat: Target.entityStats.DamageOutModifier_Flat,
                        value: d.Power,
                        duration: d.Duration
                    ), ModifierMergeStrategy.RefreshDurationAndMerge);

                    CombatUtility.ApplyStatDebuff(d, Target, new StatModifier(
                        name: "Growl Armour Reduction",
                        stat: Target.entityStats.Armour_Increase,
                        value: 10,
                        condition: true,
                        duration: d.Duration
                    ), ModifierMergeStrategy.RefreshDurationAndMerge);
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
                    cardTargetingMode = CardTargetingMode.Radius,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Bolster allies damage {Power}.",
                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyStatBuff(d, Target,
                        new StatModifier
                        (
                            name: "Damage",
                            stat: Target.entityStats.DamageOutModifier_Flat,
                            value: d.Power,
                            duration: d.Duration
                            ),
                        ModifierMergeStrategy.RefreshDurationAndMerge);
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
                cardEffectAction = (User, Target, d) =>
                {
                    if (TargetingUtility.isEnemyOf(User, Target))
                    {
                        CombatUtility.ApplyStatDebuff(d, Target,
                            new StatModifier
                            (
                                name: "Warcry Damage Reduction",
                                stat: Target.entityStats.DamageOutModifier_Increase,
                                value: -10,
                                duration: 2
                            ), ModifierMergeStrategy.RefreshDurationAndMerge);
                    }
                    else
                    {
                        CombatUtility.ApplyStatBuff(d, Target,
                            new StatModifier
                            (
                                name: "Warcry Damage Taken Reduction",
                                stat: Target.entityStats.DamageTakenModifier_Increase,
                                value: -20,
                                duration: 2
                            ), ModifierMergeStrategy.Override);
                    }
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
                cardEffectAction = (User, Target, d) =>
                {
                    var stat = Target.entityStats.Armour_Increase;
                    var mod = new StatModifier(
                        name: "Guard_Up",
                        stat: stat,
                        value: d.Power,
                        condition: true,
                        duration: d.Duration
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.Override);
                }
            });
        }

        private static void RegisterSpells()
        {
            // 100301 – Fireball – ranged hit + Burn DoT
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
                cardEffectGroundAction = (User, Target, d) =>
                {
                    CombatUtility.SpawnEntity(d, Target, "Npc_Wolf_Summon", User.entityAffiliation, true);
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