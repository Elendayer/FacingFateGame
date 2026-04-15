using UnityEngine;

namespace facingfate
{
    // Neutral cards (Class = 10)
    // ID-Schema: 10 | TT | II   (TT: MartialArt=01, Ability=02, Spell=03, Curse=04, Blessing=05, Item=06)
    public static class NeutralCards
    {
        public static void RegisterAll()
        {
            RegisterMartialArts();
            RegisterAbilities();
            RegisterItems();
        }

        private static void RegisterMartialArts()
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
                    return Mathf.RoundToInt(card.Owner.entityStats.Strength / 2);
                },

                range_u = 1f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) => d.cardDescription = "Deal Damage equal to half your Strength: ({Damage})",
                CardEffect = (User, Target, d) =>
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

                cost_u = 40,
                damage_u = 75,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) => d.cardDescription = "Deal {Damage} Damage, Increase Movement Cost by 1 for {Duration turns",
                CardEffect = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"), d.Damage);
                    CombatUtility.ApplyStatDebuff(d, Target,
                        new StatModifier(
                        name: "Movement",
                        stat: Target.entityStats.MovementCostModifier_Increase,
                        value: 1,
                        condition: true,
                        duration: 2
                    ), ModifierMergeStrategy.RefreshDurationAndMerge);
                },
                CardVfx = (Data,Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData ("Debuff"), Target.targetedEntities);
                }
            });

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

                CardEffect = (User, Target, d) =>
                {
                    CombatUtility.ApplyHealing(d, Target, d.Healing);
                },
                CardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData ("Buff"), Target.targetedEntities);
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
                damage_u = 30,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) => d.cardDescription = "Deal {Damage} damage.",
                CardEffect = (User, Target, d) =>
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

                CardDescription = (User, d) => d.cardDescription = "Hit the target {Repeats} for {Damage}.",
                CardEffect = (User, Target, d) =>
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
                range_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) => d.cardDescription = $"Push enemy 1 space and deals {d.Damage}.",
                CardEffect = (User, Target, d) =>
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

                cost_u = 50,
                damage_u = 60,
                range_u = 2f,
                power_u = 1,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) => d.cardDescription = "Charge and move both 1 space.",
                CardEffect = (User, Target, d) =>
                {
                    MovementUtility.ForcedMove(ForcedMovementType.Push, Target, User.transform.position, d.Power);
                    MovementUtility.ForcedMove(ForcedMovementType.Pull, User, Target.transform.position, d.Power);
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
                power_u = 1,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) => d.cardDescription = "Disengage after your attack (step back).",
                CardEffect = (User, Target, d) =>
                {
                    MovementUtility.ForcedMove(ForcedMovementType.Push, User, User.transform.position, d.Power);
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

                CardDescription = (User, d) => d.cardDescription = $"Deal {d.Damage} damage.",
                CardEffect = (User, Target, d) =>
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

                CardDescription = (User, d) => d.cardDescription = "Bite multiple times and apply Bleed over time.",
                CardEffect = (User, Target, d) =>
                {
                    // direct hit (per repeat)
                    CombatUtility.ApplyDamage(d, Target, new VFXData ("SlashImpact"), d.Damage);

                    // Bleed DoT + immediate tick
                    int dur = d.Duration > 0 ? d.Duration : 6;
                    string name = $"Bleed#{d.cardID}";
                    var bleed = new EntityModifier(
                        modifierName: name,
                        owner: Target,
                        baseValue: d.Damage,
                        toTriggerRefs: new() { GameplayRef.onBleed },
                        duration: dur,
                        onRef_Trigger: new RelevantTriggerCheck
                        {
                            OnTriggerReference = new() { GameplayRef.onTurnStart },
                            CheckType = CheckEntityType.User,
                            CheckEntity = Target,
                        },
                        onRef_Action: (target, cd, value) =>
                        {
                            CombatUtility.ApplyDamage(null, target, new VFXData ("BleedEffect"), value);
                        });

                    CombatUtility.ApplyEntityModifier(d, Target, bleed, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

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
                CardEffect = (User, Target, d) =>
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
            });

            // 100111 – Arrowshot – ranged hit
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Tech_Arrowshot",
                cardName = "Arrowshot",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Ranged, CardIdentity.Physical },

                cost_u = 30,
                damage_u = 100,
                range_u = 5f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) => d.cardDescription = $"Shoot an arrow dealing {d.Damage} damage.",
                CardEffect = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData ("Impact"), d.Damage);
                }
            });

            // 100112 – Multishot – multi-target (repeat 2)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Tech_Multishot",
                cardName = "Multishot",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Ranged },

                cost_u = 80,
                damage_u = 30,
                repeats_u = 5,
                range_u = 5f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Select,
                },

                CardDescription = (User, d) => d.cardDescription = "Shoot multiple arrows at selected enemies (2 shots).",
                CardEffect = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData ("Impact"),d.Damage);
                }
            });
        }

        private static void RegisterAbilities()
        {
            // 100201 – Focus – empower next attack (Self)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Ab_Focus",
                cardName = "Focus",
                cardType = CardType.Ability,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 30,
                power_u = 100,
                duration_u = 3,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) => d.cardDescription = $"Your next attack is empowered by {d.power_u}.",
                CardEffect = (User, Target, d) =>
                {
                    var stat = Target.entityStats.DamageOutModifier_Increase;
                    var mod = new StatModifier(
                        name: "Damage",
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
                cardID = "Neutral_Ab_Growl",
                cardName = "Growl",
                cardType = CardType.Ability,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 20,
                power_u = 10,
                duration_u = 2,
                range_u = 4f,
                area_u = 4f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Ring,
                },

                CardDescription = (User, d) => d.cardDescription = $"Demoralize enemies in an area and reduces damage by {d.Power}.",
                CardEffect = (User, Target, d) =>
                {
                    CombatUtility.ApplyStatDebuff(d, Target, new StatModifier(
                        name: "Damage",
                        stat: Target.entityStats.DamageOutModifier_Increase,
                        value: d.Power,
                        duration: d.Duration
                    ), ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

            // 100203 – Howl – improve allies' stats (AOE buff)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Ab_Howl",
                cardName = "Howl",
                cardType = CardType.Ability,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 50,
                power_u = 50, // stat buff (non-damage)
                duration_u = 2,
                range_u = 4f,
                area_u = 4f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Ring,
                },

                CardDescription = (User, d) => d.cardDescription = $"Bolster allies damage in range by {d.Power} ).",
                CardEffect = (User, Target, d) =>
                {
                    CombatUtility.ApplyStatBuff(d, Target,
                        new StatModifier
                        (
                            name: "Damage",
                            stat: Target.entityStats.DamageOutModifier_Increase,
                            value: d.Power,
                            duration: d.Duration
                            ),
                        ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

            // 100204 – Guard Up – raise defense until end of turn
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Neutral_Ab_Guard_Up",
                cardName = "Guard Up",
                cardType = CardType.Ability,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 50,
                power_u = 50,
                duration_u = 2,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) => d.cardDescription = $"Increase your defense until end of turn by {d.Power}.",
                CardEffect = (User, Target, d) =>
                {
                    var stat = Target.entityStats.Armour_Increase;
                    var mod = new StatModifier(
                        name: "Armour",
                        stat: stat,
                        value: d.Power,
                        condition: true,
                        duration: d.Duration
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });
        }

        private static void RegisterItems()
        {
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
                CardEffect = (User, Target, d) =>
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
                CardEffect = (User, Target, d) =>
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
            });
        }
    }
}

