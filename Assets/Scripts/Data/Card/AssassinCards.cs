using System;
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
                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"));
                }
            });

            // 120102 – Dance of a Hundred Cuts (Ring, repeats)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Tech_Dance_of_a_Hundred_Cuts",
                cardName = "Dance of a Hundred Cuts",
                cardType = CardType.Technique,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Physical },

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

                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("SlashImpact"));
                },
            });

            // 120103 – Lotus Death Kiss (Execute <10% HP)
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Tech_Lotus_Death_Kiss",
                cardName = "Lotus Death Kiss",
                cardType = CardType.Technique,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Physical },

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

                cardEffectAction = (User, Target, d) =>
                {
                    if (Target.entityStats.CurrentHealth <= Target.entityStats.MaxHealth / 10)
                    {
                        CombatUtility.ApplyEffectDamage(99999, Target, GameplayRef.Physical, new VFXData("Impact"));
                    }
                    else
                    {
                        CombatUtility.ApplyEffectDamage(1, Target, GameplayRef.Physical, new VFXData("Impact"));
                    }
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

                cardEffectAction = (User, Target, d) =>
                {
                    EntityModifier poison = EffectDatabase.GetEffectByName("Poison", CloneMode.Defaults, d, ThroughputSource.Damage, Target);
                    EntityModifier burn = EffectDatabase.GetEffectByName("Burn", CloneMode.Defaults, d, ThroughputSource.Damage, Target);
                    EntityModifier bleed = EffectDatabase.GetEffectByName("Bleed", CloneMode.Defaults, d, ThroughputSource.Damage, Target);

                    CombatUtility.ApplyEntityModifier(d, Target, poison, ModifierMergeStrategy.RefreshDurationAndMerge);
                    CombatUtility.ApplyEntityModifier(d, Target, burn, ModifierMergeStrategy.RefreshDurationAndMerge);
                    CombatUtility.ApplyEntityModifier(d, Target, bleed, ModifierMergeStrategy.RefreshDurationAndMerge);
                },
                cardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("Impact"), Target.targetedEntities);

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

                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"));
                },
            });
            // 100111 � Arrowshot � ranged hit
            CardDatabase.RegisterCard(new CardData()
            {
                cardID = "Assassin_Tech_Arrow_Shot",
                cardName = "Arrow Shot",
                cardType = CardType.Technique,
                cardClass = CardClass.Assassin,
                cardIdentities = new() { CardIdentity.Ranged, CardIdentity.Physical },

                cost_u = 30,
                damage_u = 75,
                range_u = 8f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Shoot an arrow dealing {Damage} damage.",
                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"), d.Damage);
                },
                cardVfx = (Data, Target) =>
                {
                    AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("ArrowShot") { start = Data.Owner.transform.position }, Target.targetedEntities);
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
                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"), d.Damage);
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

                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"));
                },
                cardVfx = (Data, Target) =>
                {
                    foreach (var entity in Target.targetedEntities)
                    {
                        AssetManager.Instance.CreateVFXAttachedToGameObjects(new VFXData("ArrowShot") { start = Data.Owner.transform.position }, Target.targetedEntities);
                    }
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

                cost_u = 18,
                damage_u = 10,
                range_u = 5f,
                area_u = 2f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Ground,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Radius,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deals {Damage} damage",

                cardEffectAction = (User, Target, d) =>
                {
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"));
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

                cost_u = 20,
                damage_u = 25,
                range_u = 4f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Deal {Damage} damage and bounce to 2 times",

                cardEffectAction = (User, Target, d) =>
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
                            if (!IsEnemyOf(User, cand)) continue;

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
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"));

                    // 2) Bounces
                    var hitSet = new HashSet<int> { Target.GetInstanceID() };
                    var last = Target;

                    int maxBounces = 2;
                    const int hopLimit = 2;

                    for (int i = 0; i < maxBounces; i++)
                    {
                        var next = FindNextBounceTarget(last, hitSet, hopLimit);
                        if (next == null) break;

                        CombatUtility.ApplyDamage(d, next, new VFXData("Impact"));
                        hitSet.Add(next.GetInstanceID());
                        last = next;
                    }
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
                cardEffectAction = (User, Target, d) =>
                {
                    // Direktschaden
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"));

                    var bleed = new EntityModifier(
                        modifierName: "Bleed",
                        owner: Target,
                        baseValue: d.SecondaryDamage,
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
                            CombatUtility.ApplyEffectDamage(value, target, GameplayRef.onBleed, new VFXData("BleedEffect"));
                        });
                    CombatUtility.ApplyEntityModifier(d, Target, bleed, ModifierMergeStrategy.RefreshDurationAndMerge);
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

                cardEffectAction = (User, Target, d) =>
                {
                    // Direkter Schaden
                    CombatUtility.ApplyDamage(d, Target, new VFXData("Impact"));


                    CombatUtility.ApplyEntityModifier(d, Target, EffectDatabase.GetEffectByName("Stun", CloneMode.OverrideFromData, d, ThroughputSource.Power, Target), ModifierMergeStrategy.Merge);
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

                cardDescriptionAction = (User, d) => d.cardDescription = $"Deal {d.Damage} Damage at the Start of your turn to targets in the effect.",

                cardEffectGroundAction = (User, Target, d) =>
                {
                    CombatUtility.SpawnGroundEffect(d, Target, new GroundEffect_Ref_Effect
                        (
                        cardData: d,
                        relevantTrigger: new RelevantTriggerCheck
                        {
                            OnTriggerReference = new() { GameplayRef.onTurnStart },
                            CheckType = CheckEntityType.User,
                            CheckEntity = User,
                        },
                        duration: d.Duration,
                        onRef: (target) => CombatUtility.ApplyDamage(null, target, new VFXData("Impact"), d.Damage)
                        ),
                        vfxData: null
                        );

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

                cost_u = 30,

                range_u = 5f,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Enemy,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, d) => d.cardDescription = "Teleport behind target enemy.",

                cardEffectAction = (User, Target, d) =>
                {
                    MovementUtility.ForcedMove(ForcedMovementType.Jump, User, Target.transform.position);
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

                cardDescriptionAction = (User, d) => d.cardDescription = $"Next {d.Power} attacks apply Burn (DoT {d.Damage} for {d.Duration} turns).",

                cardEffectAction = (User, Target, d) =>
                {
                    //VenomUtility.ArmBurnFromCard(User, d);
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

                cardEffectAction = (User, Target, cd) =>
                {
                    CombatUtility.ApplyEntityModifier(cd, User,
                        new EntityModifier(
                            modifierName: "PosionVenom",
                            owner: User,
                            baseValue: cd.Duration,
                            toTriggerRefs: new() { },
                            charges: cd.Charges,
                            onRef_Trigger: new RelevantTriggerCheck
                            {
                                OnTriggerReference = new() { GameplayRef.onHitLanded },
                                CheckType = CheckEntityType.User,
                                CheckEntity = User,
                            },
                            actionTargetType: EntityModifier.ActionTargetType.Affected,
                            onRef_Action: (t, d, value) =>
                            {
                                CombatUtility.ApplyEntityModifier(d, t, EffectDatabase.GetEffectByName("Poison", CloneMode.Defaults, d, ThroughputSource.Damage, t), ModifierMergeStrategy.RefreshDurationAndMerge);
                            }),
                        ModifierMergeStrategy.Override);
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

                cardEffectAction = (User, Target, cd) =>
                {
                    CombatUtility.ApplyEntityModifier(cd, User,
                        new EntityModifier(
                            modifierName: "StunVenom",
                            owner: User,
                            baseValue: cd.Duration,
                            toTriggerRefs: new() { },
                            charges: cd.Charges,
                            onRef_Trigger: new RelevantTriggerCheck
                            {
                                OnTriggerReference = new() { GameplayRef.onHitLanded },
                                CheckType = CheckEntityType.User,
                                CheckEntity = User,
                            },
                            actionTargetType: EntityModifier.ActionTargetType.Affected,
                            onRef_Action: (t, d, value) =>
                            {
                                CombatUtility.ApplyEntityModifier(d, t, EffectDatabase.GetEffectByName("Stun", CloneMode.Defaults, d, ThroughputSource.Power, t), ModifierMergeStrategy.RefreshDurationAndMerge);
                            }),
                        ModifierMergeStrategy.Override);
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

                cardEffectAction = (User, Target, d) =>
                {
                    // Fetch the most recent Venom card played by the User
                    var lastVenomCard = TimelineManager.GetDataFromTimeline(
                        entity: User,
                        filter: TimelineManager.TimelineFilter.CardIdentity,
                        filterValue: CardIdentity.Venom,
                        turnsAgo: int.MaxValue,
                        isUser: true
                    ).FirstOrDefault();

                    // Safety check
                    if (lastVenomCard.CardData == null)
                    {
                        Debug.LogWarning($"No Venom card found for user {User.name}");
                        return;
                    }

                    // Execute the CardEffect from the fetched card
                    lastVenomCard.CardData.cardEffectAction.Invoke(User, Target, lastVenomCard.CardData);
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

                cost_u = 12,

                targetingData = new()
                {
                    CardTargetType = CardTargetType.Entity,
                    CardTargetAffiliation = CardTargetAffiliation.Self,
                    cardTargetingMode = CardTargetingMode.Single,
                },

                cardDescriptionAction = (User, data) => data.cardDescription = "Your next Technique deals double damage.",
                cardEffectAction = (User, Target, data) =>
                {
                    CombatUtility.ApplyStatBuff(data, Target, new StatModifier
                        (
                            name: "Crit",
                            stat: Target.entityStats.DamageOutModifier_Multiplier,
                            value: 2,
                            to_TriggerRefs: new() { },
                            charges: 1,
                            condition: (target, data) => data.cardType == CardType.Technique,
                            on_RefTrigger: new RelevantTriggerCheck
                            {
                                OnTriggerReference = new() { GameplayRef.Technique },
                                CheckType = CheckEntityType.User,
                                CheckEntity = User,
                            }
                            ), ModifierMergeStrategy.Override);
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
                cardEffectAction = (User, Target, data) => { /* TODO */ }
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
                cardEffectAction = (User, Target, data) => { /* TODO */ }
            });
        }
    }
}

