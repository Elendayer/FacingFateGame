using System.Collections.Generic;
using UnityEngine;
using Utility;

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
            // 100101 � Strike � normal attack
            CardDatabase.RegisterCard(new CardData()
            cardID = 100101,
            cardName = "Strike",
            cardType = CardType.Technique,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical },

            cost_u = 10,
            damageFunc = card =>
            {
                return card.Owner.entityStats.Strength.Value() / 2;
            },

            range_u = 2,

            targetingData = new()
            {
                cardID = 100101,
                cardName = "Strike",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical },

            CardDescription = (User, d) => d.cardDescription = "Deal Damage equal to half your Strength: ({Damage})",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target, d.Damage);
            }
        });

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) => d.cardDescription = $"Deal {d.Damage} damage.",
                CardEffect = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, d.Damage);
                }
            });

            // 100102 � Heavy Blow � slow heavy hit (TODO: -1 Movement)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = 100102,
                cardName = "Heavy Blow",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical },

            CardDescription = (User, d) => d.cardDescription = $"Deal {d.Damage}, reduces Movement",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target, d.Damage);
                CombatUtility.ApplyStatDebuff(d, Target,
                    new StatModifier(
                    stat: Target.entityStats.MovementCostModifier,
                    value: 1,
                    condition: true,
                    scaling: ModifierScaling.Flat,
                    duration: 2,
                    name: $"HeavyBlowMovementDecrease"
                ), ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) => d.cardDescription = $"Deal {d.Damage}, reduces Movement",
                CardEffect = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, d.Damage);
                    // TODO: Movement-Cost/Range senken
                }
            });

            // 100103 � Quick Jab � fast poke
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = 100103,
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

                CardDescription = (User, d) => d.cardDescription = $"Deal {d.Damage} damage.",
                CardEffect = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, d.Damage);
                }
            });

            // 100104 � Double Cut � strike twice
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = 100104,
                cardName = "Double Cut",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical },

            CardDescription = (User, d) => d.cardDescription = "Hit the target {Repeats} for {Damage}.",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target, d.Damage);
            }
        });

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

            cost_u = 20,
            damage_u = 10,
            range_u = 2,

            // 100105 � Shove � push 1 space (minor damage)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = 100105,
                cardName = "Shove",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Melee },

            CardDescription = (User, d) => d.cardDescription = $"Push enemy 1 space and deals {d.Damage}.",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target, d.Damage);
                MovementUtility.ForcedMove(ForcedMovementType.Push, Target, User.GetComponent<EntityOnMap>().currentCell, 1);
                // TODO: 1 Feld wegschieben
            }
        });

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) => d.cardDescription = $"Push enemy 1 space and deals {d.Damage}.",
                CardEffect = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, d.Damage);
                    MovementUtility.ForcedMove(ForcedMovementType.Push, Target, User.GetComponent<EntityOnMap>().currentCell, d.Power);
                    // TODO: 1 Feld wegschieben
                }
            });

            // 100106 � Charge � move both 1 space 
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = 100106,
                cardName = "Charge",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical },

                cost_u = 50,
                damage_u = 60,
                range_u = 2,
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
                    MovementUtility.ForcedMove(ForcedMovementType.Push, Target, User.GetComponent<EntityOnMap>().currentCell, d.Power);
                    MovementUtility.ForcedMove(ForcedMovementType.Pull, User, Target.GetComponent<EntityOnMap>().currentCell, d.Power);
                    CombatUtility.ApplyDamage(d, Target, d.Damage);
                    // TODO: User/Target bewegen und Stun anwenden
                }
            });

            // 100107 � Step Back � disengage after attack (Self)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = 100107,
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
                    MovementUtility.ForcedMove(ForcedMovementType.Push, User, User.GetComponent<EntityOnMap>().currentCell, d.Power);
                    // TODO: User 1 Feld r�ckw�rts bewegen
                }
            });

            // 100108 � Bite � small hit
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = 100108,
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
                    CombatUtility.ApplyDamage(d, Target, d.Damage);
                }
            });

            // 100109 � Gnaw � bite until bleed (repeat 2) � now applies Bleed DoT (with fallback duration)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = 100109,
                cardName = "Gnaw",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Melee, CardIdentity.Blood, CardIdentity.Physical },

                cost_u = 50,
                damage_u = 50,
                repeats_u = 2,
                duration_u = 6,

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
                        CombatUtility.ApplyDamage(null, target, value);
                    });

                CardDescription = (User, d) => d.cardDescription = "Bite multiple times and apply Bleed over time.",
                CardEffect = (User, Target, d) =>
                {
                    // direct hit (per repeat)
                    CombatUtility.ApplyDamage(d, Target, d.Damage);

                    // Bleed DoT + immediate tick
                    int dur = d.Duration > 0 ? d.Duration : 6;
                    string name = $"Bleed#{d.cardID}";
                    var bleed = new EntityModifier(
                        modifierName: name,
                        baseValue: d.Damage,
                        toTriggerRefs: new() { GameplayRef.onBleed },
                        duration: dur,
                        onRef_Trigger: new TriggerRef
                        {
                            OnTriggerReference = new() { GameplayRef.onTurnStart },
                            AffectedEntities = new() { Target },
                            UserEntity = User
                        },
                        onRef_Action: (data, target) =>
                        {
                            CombatUtility.ApplyDamage(null, target, data.Value);
                        });

                    CombatUtility.ApplyEntityModifier(d, Target, bleed, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

            // 100110 � Sting � damage + Poison DoT
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = 100110,
                cardName = "Sting",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Melee, CardIdentity.Physical, CardIdentity.Poison },

                cost_u = 50,
                damage_u = 150,
                duration_u = 6,

                // Poison DoT + immediate tick
                string name = $"Poison#{d.cardID}";
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
                        CombatUtility.ApplyDamage(null, target, value);

                CardDescription = (User, d) => d.cardDescription = "Deal 5 damage and apply Poison for 6 turns (immediate tick).",
                CardEffect = (User, Target, d) =>
                {

                    // Poison DoT + immediate tick
                    string name = $"Poison#{d.cardID}";
                    var poison = new EntityModifier(
                        modifierName: name,
                        baseValue: d.Damage,
                        toTriggerRefs: new() { GameplayRef.onPoison },
                        duration: d.Duration,
                        onRef_Trigger: new TriggerRef
                        {
                            OnTriggerReference = new() { GameplayRef.onTurnStart },
                            AffectedEntities = new() { Target },
                            UserEntity = User
                        },
                        onRef_Action: (data, target) =>
                        {
                            CombatUtility.ApplyDamage(null, target, data.Value);

                        });

                    CombatUtility.ApplyEntityModifier(d, Target, poison, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

            // 100111 � Arrowshot � ranged hit
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = 100111,
                cardName = "Arrowshot",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Ranged, CardIdentity.Physical },

                cost_u = 30,
                damage_u = 100,
                range_u = 5,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardDescription = (User, d) => d.cardDescription = $"Shoot an arrow dealing {d.Damage} damage.",
                CardEffect = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, d.Damage);
                }
            });

            // 100112 � Multishot � multi-target (repeat 2)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = 100112,
                cardName = "Multishot",
                cardType = CardType.Technique,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Ranged },

                cost_u = 80,
                damage_u = 30,
                repeats_u = 5,
                range_u = 5,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.CombatTile,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Select,
                },

                CardDescription = (User, d) => d.cardDescription = "Shoot multiple arrows at selected enemies (2 shots).",
                CardEffect = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, d.Damage);
                }
            });
        }

        private static void RegisterAbilities()
        {
            // 100201 � Focus � empower next attack (Self)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = 100201,
                cardName = "Focus",
                cardType = CardType.Ability,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.None },

            CardDescription = (User, d) => d.cardDescription = $"Your next attack is empowered by {d.power_u}.",
            CardEffect = (User, Target, d) =>
            {
                var stat = Target.entityStats.DamageOutModifier;
                var mod = new StatModifier(
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration,
                    stat: stat,
                    name: $"SoaringDragonElixir"
                );
                CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

        // 100202 � Growl � demoralize enemies (AOE debuff)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100202,
            cardName = "Growl",
            cardType = CardType.Ability,
            cardClass = CardClass.Neutral,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 20,
            power_u = 10,
            duration_u = 2,
            range_u = 4,
            area_u = 4,

            // 100202 � Growl � demoralize enemies (AOE debuff)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = 100202,
                cardName = "Growl",
                cardType = CardType.Ability,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.None },

            CardDescription = (User, d) => d.cardDescription = $"Demoralize enemies in an area and reduces damage by {d.Power}.",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyStatDebuff(d, Target, new StatModifier(
                    stat: Target.entityStats.DamageOutModifier,
                    value: d.Power,
                    scaling: ModifierScaling.Flat,
                    duration: d.Duration,
                    name: $"GrowlDecrease{d.Power}"
                ), ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

                targetingData = new()
                {
                    CardTargetType = CardTargetType.CombatTile,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Ring,
                },

                CardDescription = (User, d) => d.cardDescription = $"Demoralize enemies in an area and reduces attack damage by {d.Power}.",
                CardEffect = (User, Target, d) =>
                {
                    var stat = Target.entityStats.DamageOutModifier;
                    var mod = new StatModifier(
                        stat: stat,
                        value: d.Power,
                        scaling: ModifierScaling.Flat,
                        duration: d.Duration,
                        name: $"GrowlDecrease{d.Power}"
                    );
                    CombatUtility.ApplyStatDebuff(d, Target, stat, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

            // 100203 � Howl � improve allies' stats (AOE buff)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = 100203,
                cardName = "Howl",
                cardType = CardType.Ability,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.None },

            CardDescription = (User, d) => d.cardDescription = $"Bolster allies damage in range by {d.Power} ).",
            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyStatBuff(d, Target,
                    new StatModifier
                    (
                        stat: Target.entityStats.DamageOutModifier,
                        value: d.Power,
                        scaling: ModifierScaling.Flat,
                        duration: d.Duration,
                        name: $"HowlIncrease{d.Power}"
                        ),
                    ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

                targetingData = new()
                {
                    CardTargetType = CardTargetType.CombatTile,
                    CardTargetAffiliation = CardTargetAffiliation.Ally,
                    cardTargetingMode = CardTargetingMode.Ring,
                },

                CardDescription = (User, d) => d.cardDescription = $"Bolster allies damage in range by {d.Power} ).",
                CardEffect = (User, Target, d) =>
                {
                    var stat = Target.entityStats.DamageOutModifier;
                    var mod = new StatModifier(
                        stat: stat,
                        value: d.Power,
                        scaling: ModifierScaling.Flat,
                        duration: d.Duration,
                        name: $"HowlIncrease{d.Power}"
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

            // 100204 � Guard Up � raise defense until end of turn
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = 100204,
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
                    var stat = Target.entityStats.Armour;
                    var mod = new StatModifier(
                        stat: stat,
                        value: d.Power,
                        condition: true,
                        scaling: ModifierScaling.Flat,
                        duration: d.Duration,
                        name: $"ArmourIncrease{d.Power}"
                    );
                    CombatUtility.ApplyStatBuff(d, Target, mod, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });
        }

        private static void RegisterItems()
        {
            // 100601 � Throw Poison � Single/Radius; apply Poison DoT (with immediate tick)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = 100601,
                cardName = "Throw Poison",
                cardType = CardType.Item,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Poison, CardIdentity.Ranged },

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
                    onRef_Action: (target,cd,value) =>
                    {
                        CombatUtility.ApplyDamage(null, target, value);
                    });

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
                        baseValue: d.Damage,
                        toTriggerRefs: new() { GameplayRef.onPoison },
                        duration: d.Duration,
                        onRef_Trigger: new TriggerRef
                        {
                            OnTriggerReference = new() { GameplayRef.onTurnStart },
                            AffectedEntities = new() { Target },
                            UserEntity = User
                        },
                        onRef_Action: (data, target) =>
                        {
                            CombatUtility.ApplyDamage(null, target, data.Value);
                        });

                    CombatUtility.ApplyEntityModifier(d, Target, poison, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });

            // 100602 � Throw Firebomb � Single/Radius; apply Burn DoT (with immediate tick)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = 100602,
                cardName = "Throw Firebomb",
                cardType = CardType.Item,
                cardClass = CardClass.Neutral,
                cardIdentities = new() { CardIdentity.Fire, CardIdentity.Ranged },

            CardDescription = (User, d) => d.cardDescription = $"Apply Burn {d.Damage} for {d.Duration} turns.",
            CardEffect = (User, Target, d) =>
            {
                string name = "Burn";
                var burn = new EntityModifier(
                    modifierName: name,
                    owner: Target ,
                    baseValue: d.Damage,
                    toTriggerRefs: new() { GameplayRef.onBurn },
                    duration: d.Duration,
                    onRef_Trigger: new RelevantTriggerCheck
                    {
                        OnTriggerReference = new() { GameplayRef.onTurnStart},
                        CheckType = CheckEntityType.User,
                        CheckEntity = Target,
                    },
                    onRef_Action: (target, cd, value) =>
                    {
                        CombatUtility.ApplyDamage(null, target, value);
                    });

                CardDescription = (User, d) => d.cardDescription = $"Apply Burn {d.Damage} for {d.Duration} turns.",
                CardEffect = (User, Target, d) =>
                {
                    string name = $"Burn#{d.cardID}";
                    var burn = new EntityModifier(
                        modifierName: name,
                        baseValue: d.Damage,
                        toTriggerRefs: new() { GameplayRef.onBurn },
                        duration: d.Duration,
                        onRef_Trigger: new TriggerRef
                        {
                            OnTriggerReference = new() { GameplayRef.onTurnStart, GameplayRef.onModifierApplied },
                            AffectedEntities = new() { Target },
                            UserEntity = User

                        },
                        onRef_Action: (data, target) =>
                        {
                            CombatUtility.ApplyDamage(null, target, data.Value);
                        });

                    CombatUtility.ApplyEntityModifier(d, Target, burn, ModifierMergeStrategy.RefreshDurationAndMerge);
                }
            });
        }
    }
}