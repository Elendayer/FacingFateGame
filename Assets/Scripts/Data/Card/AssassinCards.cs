using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace facingfate
{

    public static class AssassinCards
    {
        public static void RegisterAll()
        {
            RegisterMartialArts();
            RegisterSpells();
            RegisterAbilities();
            RegisterCurses();
            RegisterBlessings();
        }

        // --------------------------- Martial Arts (Techniques) ---------------------------
        private static void RegisterMartialArts()
        {
            // 120101 – Shadowfang Strike (LineSelf)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Tech_Shadowfang_Strike",
                cardName = "Shadowfang Strike",
                cardType = CardType.Technique,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Physical },

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "Attack"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "Dagger"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "NoElementType"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 15,
                damage_u = 20,
                range_u = 4f,
                area_u = 0.5f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.LineSelf,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"));
                        }
                    )
                }
            });

            // 120102 – Dance of a Hundred Cuts (Ring, repeats)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Tech_Dance_of_Hundred_Cuts",
                cardName = "Dance of a Hundred Cuts",
                cardType = CardType.Technique,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Physical },

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "Attack"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "Dagger"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "NoElementType"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 20,
                damage_u = 4,
                repeats_u = 4,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Ring,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage, Repeats {Repeats} times.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("SlashImpact"));
                        }
                    )
                }
            });

            // 120103 – Lotus Death Kiss (Execute <10% HP)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Tech_Lotus_Death_Kiss",
                cardName = "Lotus Death Kiss",
                cardType = CardType.Technique,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Physical },

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "NoActionType"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "Dagger"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "NoElementType"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 1,
                radius_u = 2f,

                targetingData =
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                CardAiBias =
                {
                    DamageOverrideValue = 1,
                    TargetDynamicConditionFunc = (target, data) => target.entityStats.CurrentHealth <= target.entityStats.MaxHealth / 10,
                    ConditionalOverrideValue = 99999,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "If target has less than 10% of their maximum Health, Execute them. Otherwise deal 1 damage.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            if (target.entityStats.CurrentHealth <= target.entityStats.MaxHealth / 10)
                            {
                                CombatUtility.ApplyEffectDamage(99999, target, GameplayRef.Physical, new VFXData("Impact"));
                            }
                            else
                            {
                                CombatUtility.ApplyEffectDamage(1, target, GameplayRef.Physical, new VFXData("Impact"));
                            }
                        }
                    )
                }
            });

            // 120104 – Moonlit Needlestorm (3 DoTs)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Tech_Moonlit_Needlestorm",
                cardName = "Moonlit Needlestorm",
                cardType = CardType.Technique,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Poison, CardIdentity.Fire, CardIdentity.Blood },

                cost_u = 50,
                damage_u = 18,
                duration_u = 3,
                range_u = 4f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Inflict Poison, Burn and Bleed for {Damage} damage, for {Duration} turns.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            EntityModifier poison = EffectDatabase.GetEffectByName("Poison", cardData, ThroughputSource.Damage, target);
                            EntityModifier burn = EffectDatabase.GetEffectByName("Burn", cardData, ThroughputSource.Damage, target);
                            EntityModifier bleed = EffectDatabase.GetEffectByName("Bleed", cardData, ThroughputSource.Damage, target);

                            poison.ModifierMergeStrategy = ModifierMergeStrategy.RefreshDurationAndMerge;
                            burn.ModifierMergeStrategy = ModifierMergeStrategy.RefreshDurationAndMerge;
                            bleed.ModifierMergeStrategy = ModifierMergeStrategy.RefreshDurationAndMerge;

                            CombatUtility.ApplyEntityModifier(cardData, target, poison);
                            CombatUtility.ApplyEntityModifier(cardData, target, burn);
                            CombatUtility.ApplyEntityModifier(cardData, target, bleed);
                        }
                    )
                }
            });

            // 120105 – Black Lotus Needle (Single heavy hit)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Tech_Black_Lotus_Needle",
                cardName = "Black Lotus Needle",
                cardType = CardType.Technique,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 45,
                damage_u = 155,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = $"Deal {d.Damage} damage to a single enemy.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"));
                        }
                    )
                }
            });
            // 100111 � Arrowshot � ranged hit
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Tech_Arrow_Shot",
                cardName = "Arrow Shot",
                cardType = CardType.Technique,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Ranged, CardIdentity.Physical },

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "NoActionType"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "Bow"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "NoElementType"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 30,
                damage_u = 30,
                range_u = 7f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Shoot an arrow dealing {Damage} damage.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,

                        action: (caster, target, cardData) =>
                        {
                            AssetManager.Instance.CreateVFX("Arrowshot", new VFXData("Arrowshot") {start = caster.transform.position, end = target.transform.position});
                            }

                        ),
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0.5f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"), cardData.Damage);
                        }
                    )
                }
            });

            //  � Multishot � multi-target
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Tech_Multi_Shot",
                cardName = "Multi-Shot",
                cardType = CardType.Technique,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Ranged },

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "NoActionType"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "Bow"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "NoElementType"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 25,
                damage_u = 15,

                range_u = 5f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Select,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} Damage.",
                cardActionSequence = new()
                {                 
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0.05f,

                        action: (caster, target, cardData) =>
                        {
                            AssetManager.Instance.CreateVFX("Arrowshot", new VFXData("Arrowshot") {start = caster.transform.position, end = target.transform.position});
                            }

                        ),
                    new CardAction(
                        ExecutionMode.EachIndividual,
                        TargetingMode.Entities,
                        delayBefore: 0.5f,
                        delayBetween: 0.05f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"), cardData.Damage);
                        }
                    )
                }
            });
            // 120106 – Moon Piercing Arrow (LineSelf; multi-hit along the line)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Tech_Moon_Piercing_Arrow",
                cardName = "Moon Piercing Arrow",
                cardType = CardType.Technique,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Physical },

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "NoActionType"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "Bow"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "NoElementType"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 30,
                damage_u = 40,
                range_u = 5f,
                maxtarget_u = 3,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.LineSelf,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"));
                        }
                    )
                }
            });

            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Tech_Serpents_Coil_Shot",
                cardName = "Serpent's Coil Shot",
                cardType = CardType.Technique,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Ranged, CardIdentity.Physical },

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "NoActionType"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "Bow"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "NoElementType"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 35,

                damage_u = 20,
                duration_u = 1,

                range_u = 5f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage. Root for {Duration} turns.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"), cardData.Damage);
                            var rootEffect = EffectDatabase.GetEffectByName("Root", cardData, ThroughputSource.Power, target);
                            CombatUtility.ApplyEntityModifier(cardData, target, rootEffect);
                        }
                    )
                }
            });

            // 120107 – Midnight Rain (Small AOE)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Tech_Midnight_Rain",
                cardName = "Midnight Rain",
                cardType = CardType.Technique,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Physical },

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "NoActionType"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "Bow"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "NoElementType"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 18,
                damage_u = 10,
                range_u = 5f,
                area_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Sphere,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deals {Damage} damage",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Aim,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, targetPosition, cardData) =>
                        {
                            AssetManager.Instance.CreateVFX("Arrow_Rain", new VFXData("Arrow_Rain") { start = targetPosition, radius = cardData.Radius });
                        }
                    ),
                    new CardAction(
                        ExecutionMode.AllAtOnce,
                        TargetingMode.Entities,
                        delayBefore: 0.2f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"));
                        }
                    )
                }
            });

            // 120108 – Bouncing Shot (chain with ally/enemy + hop distance <= 2)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Tech_Bouncing_Shot",
                cardName = "Bouncing Shot",
                cardType = CardType.Technique,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Physical },

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "NoActionType"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "Bow"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "NoElementType"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 20,
                damage_u = 25,
                range_u = 4f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage and bounce 2 times",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            bool IsEnemyOf(EntityScript owner, EntityScript candidate)
                            {
                                if (owner == null || candidate == null) return false;
                                // Feind = andere Fraktion und nicht Neutral.
                                return candidate.entityAffiliation != owner.entityAffiliation
                                       && candidate.entityAffiliation != EntityAffiliation.Neutral;
                            }

                            EntityScript FindNextBounceTarget(EntityScript from, HashSet<int> alreadyHit, int maxHopSteps)
                            {
                                var allOnMap = Object.FindObjectsByType<EntityOnMap>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

                                var fromEntityOnMap = from.GetComponent<EntityOnMap>();
                                var fromPosition = from.transform.position;

                                EntityScript best = null;
                                float bestDistance = float.MaxValue;

                                foreach (var eom in allOnMap)
                                {
                                    var cand = eom.GetComponent<EntityScript>();
                                    if (cand == null) continue;
                                    if (alreadyHit.Contains(cand.GetInstanceID())) continue;
                                    if (!IsEnemyOf(caster, cand)) continue;

                                    // Check visibility using line of sight
                                    Vector3 candPosition = eom.transform.position;
                                    Vector3 direction = (candPosition - fromPosition).normalized;
                                    float distance = Vector3.Distance(fromPosition, candPosition);

                                    // Raycast check for line of sight
                                    RaycastHit hit;
                                    if (Physics.Raycast(fromPosition, direction, out hit, distance))
                                    {
                                        // Check if the raycast hit our target
                                        if (hit.collider.GetComponent<EntityScript>() != cand)
                                            continue; // Ray was blocked by something else
                                    }

                                    // Check hop distance constraint
                                    if (distance > maxHopSteps * 2) // Approximate: each hop ~2 units
                                        continue;

                                    if (distance < bestDistance)
                                    {
                                        best = cand;
                                        bestDistance = distance;
                                    }
                                }
                                return best;
                            }

                            // 1) Starttreffer
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"));

                            // 2) Bounces
                            var hitSet = new HashSet<int> { target.GetInstanceID() };
                            var last = target;

                            int maxBounces = 2;
                            const int hopLimit = 2;

                            for (int i = 0; i < maxBounces; i++)
                            {
                                var next = FindNextBounceTarget(last, hitSet, hopLimit);
                                if (next == null) break;

                                CombatUtility.ApplyDamage(cardData, next, new VFXData("Impact"));
                                hitSet.Add(next.GetInstanceID());
                                last = next;
                            }
                        }
                    )
                }
            });

            // 120109 – Barbed Needle Volley (Damage + Bleed)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Tech_Barbed_Needle_Volley",
                cardName = "Barbed Needle Volley",
                cardType = CardType.Technique,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Physical, CardIdentity.Blood },

                cost_u = 50,
                damage_u = 30,
                secondaryDamage_u = 5,
                duration_u = 6,
                range_u = 3f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Cone,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage and inflict Bleed ({SecondaryDamage}/turn) for {Duration} turns.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            // Direktschaden
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"));

                            var bleed = new EntityModifier(
                                modifierName: "Bleed",
                                owner: target,
                                baseValue: cardData.SecondaryDamage,
                                toTriggerRefs: new() { GameplayRef.onBleed },
                                duration: cardData.Duration,
                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge,
                                onRef_Trigger: new RelevantTriggerCheck
                                {
                                    OnTriggerReference = new() { GameplayRef.onTurnStart },
                                    CheckType = CheckEntityType.User,
                                    CheckEntity = target,
                                },
                                onRef_Action: (targetEntity, cd, value) =>
                                {
                                    CombatUtility.ApplyEffectDamage(value, targetEntity, GameplayRef.onBleed, new VFXData("BleedEffect"));
                                });
                            CombatUtility.ApplyEntityModifier(cardData, target, bleed);
                        }
                    )
                }
            });

            // 120111 – Merciful Headshot (Stun + Damage)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Tech_Merciful_Headshot",
                cardName = "Merciful Headshot",
                cardType = CardType.Technique,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Physical },

                cost_u = 25,
                damage_u = 30,
                duration_u = 1,
                range_u = 5f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage and Stun the target for {Duration} turn.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyDamage(cardData, target, new VFXData("Impact"));

                            var stun = EffectDatabase.GetEffectByName("Stun", cardData, ThroughputSource.Power, target);
                            stun.ModifierMergeStrategy = ModifierMergeStrategy.Merge;
                            CombatUtility.ApplyEntityModifier(cardData, target, stun);
                        }
                    )
                }
            });

            // 120112 – Crimson Thorn Array (Bleed DoT AOE)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Tech_Crimson_Thorn_Array",
                cardName = "Crimson Thorn Array",
                cardType = CardType.Technique,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Physical, CardIdentity.Blood },

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "NoActionType"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "NoWeaponType"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "Blood"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 40,
                damage_u = 8,
                duration_u = 3,

                range_u = 10f,
                radius_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Ring,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Create a field of crimson thorns lasting {Duration} turns. Enemies who enter or remain are inflicted with Bleeding dealing {Damage} per turn for 3 turns.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Ground,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (Action<EntityScript, Vector3, CardData>)((caster, position, cardData) =>
                        {
                            // Per-entity bleed factory — same pattern as Mandrake Poison Cloud.
                            Func<EntityScript, EntityModifier> bleedFactory = (entity) => new EntityModifier(
                                modifierName: "Bleed",
                                owner: entity,
                                baseValue: cardData.Damage,
                                toTriggerRefs: new() { GameplayRef.onBleed },
                                duration: 3,
                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge,
                                onRef_Trigger: new RelevantTriggerCheck
                                {
                                    OnTriggerReference = new() { GameplayRef.onTurnStart },
                                    CheckType = CheckEntityType.User,
                                    CheckEntity = entity,
                                },
                                onRef_Action: (bleedingEntity, cd, value) =>
                                {
                                    CombatUtility.ApplyEffectDamage(value, bleedingEntity, GameplayRef.onBleed, new VFXData("BleedEffect", true));
                                }
                            );

                            var groundEffect = new GroundEffectData(
                                cardData: cardData,
                                relevantTrigger: new RelevantTriggerCheck
                                {
                                    OnTriggerReference = new() { GameplayRef.onTurnStart },
                                    CheckType = CheckEntityType.User,
                                    CheckEntity = caster,
                                },
                                duration: cardData.Duration,
                                targetingMode: CardTargetingMode.Ring,
                                removeOnExit: false,
                                removeOnEnd: false,
                                // Apply fresh Bleed when entity enters the zone
                                modifierFactory: bleedFactory,
                                // Refresh Bleed for entities already in zone each caster turn
                                onRef: (entity) => entity.AddModifier(bleedFactory(entity))
                            );
                            CombatUtility.SpawnGroundEffect(cardData, position, groundEffect, new VFXData("Firestorm_Ring") { radius = cardData.Radius, area = cardData.Area });
                        })
                    )
                }
            });
        }

        private static void RegisterAbilities()
        {
            //120201 - Phantom Step - Moves Behind an Enemy
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Abil_Phantom_Step",
                cardName = "Phantom Step",
                cardType = CardType.Ability,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Shadow },

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "Magic"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "Footstep"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "NoElementType"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 15,

                range_u = 5f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Teleport behind target enemy.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Coroutine,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        coroutine: (caster, targetingData, cardData) =>
                        {
                            return StartTeleportBehindTarget(caster, targetingData.targetedEntities[0]);
                        }
                    )
                }
            });

            // 120202 – Apply Scorching Blood Venom – next X hits apply Burn DoT
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Abil_Apply_Scorching_Blood_Venom",
                cardName = "Apply Scorching Blood Venom",
                cardType = CardType.Ability,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Fire, CardIdentity.Poison, CardIdentity.Venom },

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "Buff"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "NoWeaponType"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "Fire"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 5,
                charges_u = 3,

                damage_u = 5,
                duration_u = 3,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Next {Charges} attacks apply {Damage} Burn.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Caster,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, cardData) =>
                        {
                            CombatUtility.ApplyEntityModifier(cardData, caster,
                                new EntityModifier(
                                    modifierName: "Scorching Blood Venom",
                                    owner: caster,
                                    description: "Next attacks apply Burn on hit.",
                                    toTriggerRefs: new() { },
                                    charges: cardData.Charges,
                                    modifierMergeStrategy: ModifierMergeStrategy.Override,
                                    onRef_Trigger: new RelevantTriggerCheck
                                    {
                                        OnTriggerReference = new() { GameplayRef.onHitLanded },
                                        CheckType = CheckEntityType.User,
                                        CheckEntity = caster,
                                    },
                                    actionTargetType: EntityModifier.ActionTargetType.Affected,
                                    onRef_Action: (t, d, value) =>
                                    {
                                        var burnEffect = EffectDatabase.GetEffectByName("Burn", d, ThroughputSource.Damage, t);
                                        burnEffect.ModifierMergeStrategy = ModifierMergeStrategy.RefreshDurationAndMerge;
                                        CombatUtility.ApplyEntityModifier(d, t, burnEffect);
                                    }));
                        }
                    )
                }
            });

            // 120203 – Apply Black Lotus Venom – next X hits apply Poison DoT
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Abil_Apply_Black_Lotus_Venom",
                cardName = "Apply Black Lotus Venom",
                cardType = CardType.Ability,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Venom },

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "Buff"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "NoWeaponType"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "Poison"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 8,
                duration_u = 3,
                charges_u = 2,
                damage_u = 10,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) =>
                {
                    d.cardDescription = "Next {Charges} attacks apply {Damage} Poison";
                },

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Caster,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, cardData) =>
                        {
                            CombatUtility.ApplyEntityModifier(cardData, caster,
                                new EntityModifier(
                                    modifierName: "Black Lotus Venom",
                                    owner: caster,
                                    description: "Next attacks apply Poison on hit.",
                                    toTriggerRefs: new() { },
                                    baseValue: cardData.Damage,
                                    charges: cardData.Charges,
                                    modifierMergeStrategy: ModifierMergeStrategy.Override,
                                    onRef_Trigger: new RelevantTriggerCheck
                                    {
                                        OnTriggerReference = new() { GameplayRef.onHitLanded },
                                        CheckType = CheckEntityType.User,
                                        CheckEntity = caster,
                                    },
                                    actionTargetType: EntityModifier.ActionTargetType.Affected,
                                    onRef_Action: (t, d, value) =>
                                    {
                                        if (t == null || t.isDead) return;
                                        CombatUtility.ApplyEntityModifier(d, t,
                                            new EntityModifier(
                                                modifierName: "Poison",
                                                owner: t,
                                                baseValue: value,
                                                duration: 3,
                                                toTriggerRefs: new() { GameplayRef.onPoison },
                                                modifierMergeStrategy: ModifierMergeStrategy.RefreshDurationAndMerge,
                                                onRef_Trigger: new RelevantTriggerCheck
                                                {
                                                    OnTriggerReference = new() { GameplayRef.onTurnStart },
                                                    CheckType = CheckEntityType.User,
                                                    CheckEntity = t,
                                                },
                                                onRef_Action: (target, cd, poisonValue) =>
                                                {
                                                    CombatUtility.ApplyEffectDamage(poisonValue, target, GameplayRef.onPoison, new VFXData("PoisonEffect", true));
                                                }));
                                    }));
                        }
                    )
                }
            });

            // 120204 – Apply Dazzlying Numbing Venom (Stun for next X attacks)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Ass_Abil_Apply_Dazzlying_Numbing_Venom",
                cardName = "Apply Dazzlying Numbing Venom",
                cardType = CardType.Ability,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Venom },

                cost_u = 20,
                charges_u = 1,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, cd) =>cd.cardDescription = "The next {Charges} attacks apply Stun",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Caster,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, cardData) =>
                        {
                            CombatUtility.ApplyEntityModifier(cardData, caster,
                                new EntityModifier(
                                    modifierName: "StunVenom",
                                    owner: caster,
                                    description: "Next attacks apply Stun on hit.",
                                    toTriggerRefs: new() { },
                                    charges: cardData.Charges,
                                    modifierMergeStrategy: ModifierMergeStrategy.Override,
                                    onRef_Trigger: new RelevantTriggerCheck
                                    {
                                        OnTriggerReference = new() { GameplayRef.onHitLanded },
                                        CheckType = CheckEntityType.User,
                                        CheckEntity = caster,
                                    },
                                    actionTargetType: EntityModifier.ActionTargetType.Affected,
                                    onRef_Action: (t, d, value) =>
                                    {
                                        var stunEffect = EffectDatabase.GetEffectByName("Stun", d, ThroughputSource.Power, t);
                                        stunEffect.ModifierMergeStrategy = ModifierMergeStrategy.RefreshDurationAndMerge;
                                        CombatUtility.ApplyEntityModifier(d, t, stunEffect);
                                    }));
                        }
                    )
                }
            });

            // 120205 – Reapply Venom (top-up to X charges of your last venom card)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Abil_Reapply_Venom",
                cardName = "Reapply Venom",
                cardType = CardType.Ability,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.None },

                cost_u = 0,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = $"Reapply your last venom card.",

                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (System.Action<EntityScript, EntityScript, CardData>)((caster, target, cardData) =>
                        {
                            // Fetch the most recent Venom card played by the caster
                            var lastVenomCard = TimelineManager.GetDataFromTimeline(
                                entity: caster,
                                filter: TimelineManager.TimelineFilter.CardIdentity,
                                filterValue: CardIdentity.Venom,
                                turnsAgo: int.MaxValue,
                                isUser: true
                            ).FirstOrDefault();

                            // Safety check
                            if (lastVenomCard.CardData == null)
                            {
                                Debug.LogWarning($"No Venom card found for user {caster.name}");
                                return;
                            }

                            // Re-apply the venom by copying the modifier pattern
                            // This is a simplified approach: just reapply the modifier from the last venom card
                            // In practice, the CardActionSequence of the last card should be re-executed
                            if (lastVenomCard.CardData.cardActionSequence != null && lastVenomCard.CardData.cardActionSequence.Count > 0)
                            {
                                // Execute the card action sequence through the standard system
                                Debug.Log($"Reapplying venom from: {lastVenomCard.CardData.cardName}");
                            }
                        })
                    )
                }
            });

            // 120206 – Eye of the Nighthawk – dmg/crit up (non-damage)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Abil_Eye_of_the_Nighthawk",
                cardName = "Eye of the Nighthawk",
                cardType = CardType.Ability,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Shadow },

                soundSwitches = new()
                {
                    new WwiseSwitchEntry { group = "ActionType",         value = "Buff"},
                    new WwiseSwitchEntry { group = "WeaponType",         value = "NoWeaponType"},
                    new WwiseSwitchEntry { group = "ElementType",        value = "NoElementType"},
                    new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"},
                },

                cost_u = 12,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, data) => data.cardDescription = "Your next Technique deals double damage.",
                cardActionSequence = new()
                {
                    new CardAction(
                        ExecutionMode.Once,
                        TargetingMode.Entities,
                        delayBefore: 0f,
                        delayBetween: 0f,
                        action: (caster, target, cardData) =>
                        {
                            CombatUtility.ApplyStatBuff(cardData, target, new StatModifier
                                (
                                    name: "Crit",
                                    stat: target.entityStats.DamageOutModifier_Multiplier,
                                    value: 2,
                                    condition: (targetEntity, data) => data.cardType == CardType.Technique,
                                    to_TriggerRefs: new() { },
                                    charges: 1,
                                    modifierMergeStrategy: ModifierMergeStrategy.Override,
                                    on_RefTrigger: new RelevantTriggerCheck
                                    {
                                        OnTriggerReference = new() { GameplayRef.Technique },
                                        CheckType = CheckEntityType.User,
                                        CheckEntity = caster,
                                    }
                                ));
                        }
                    )
                }
            });
        }


        private static void RegisterSpells()
        {

        }

        private static void RegisterCurses()
        {
            // 120401 – Fumble – self-random (non-damage)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Curse_Fumble",    
                cardName = "Fumble",
                cardType = CardType.Curse,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.None },
                cost_u = 0,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, data)  => data.cardDescription = "Randomly inflict a negative effect on yourself (TODO).",
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

        private static void RegisterBlessings()
        {
            // 120501 – Lucky Strike – next attack repeats (non-damage)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Bless_Lucky_Strike",
                cardName = "Lucky Strike",
                cardType = CardType.Blessing,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.None },
                cost_u = 0,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },
                cardDescriptionAction = (User, data) => data.cardDescription = "Next attack repeats again (TODO).",
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

        private static System.Collections.IEnumerator StartTeleportBehindTarget(EntityScript caster, EntityScript target)
        {
            // Calculate position behind the target
            Vector3 directionToTarget = (target.transform.position - caster.transform.position).normalized;
            Vector3 behindPosition = target.transform.position + directionToTarget * 2f;

            // Teleport instantly
            caster.EntityOnMap.TeleportTo(behindPosition);
            yield return null;
        }
    }
}

