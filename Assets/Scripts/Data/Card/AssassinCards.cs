using facingfate;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility;
using static TimelineManager;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using Object = UnityEngine.Object;

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
            cardID = 120101,
            cardName = "Shadowfang Strike",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 3,
            damage_u = 10,
            range_u = 2,
            area_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.LineSelf,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Line: deal {d.Damage} damage to enemies in line (range {d.Range}).",

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target);
            }
        });

        // 120102 – Dance of a Hundred Cuts (Ring, repeats)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120102,
            cardName = "Dance of a Hundred Cuts",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 80,
            damage_u = 30,
            repeats_u = 4,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Ring,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"AOE: deal {d.Damage} damage (x{d.Repeats}).",

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target);
            }
        });

        // 120103 – Lotus Death Kiss (Execute <10% HP)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120103,
            cardName = "Lotus Death Kiss",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 2,
            radius_u= 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = "If target has less than 10% of their maximum Health, Execute them. Otherwise deal 1 damage.",

            CardEffect = (User, Target, d) =>
            {
                if (Target.entityStats.CurrentHealth <= Target.entityStats.MaxHealth.Value() / 10)
                {
                    CombatUtility.ApplyEffectDamage(99999, Target, GameplayRef.Physical);
                }
                else
                {
                    CombatUtility.ApplyEffectDamage(1, Target, GameplayRef.Physical);
                }
            }
        });

        // 120104 – Moonlit Needlestorm (3 DoTs)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120104,
            cardName = "Moonlit Needlestorm",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Poison, CardIdentity.Fire, CardIdentity.Blood },

            cost_u = 60,
            damage_u = 10,     // Tickhöhe
            duration_u = 2,   // Dauer der DoTs
            range_u = 4,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Inflict Poison, Burn and Bleed ({d.Damage} per turn for {d.Duration} turns).",

            CardEffect = (User, Target, d) =>
            {
                int tick = Mathf.Max(1, d.Damage);
                int dur = d.Duration > 0 ? d.Duration : 6;

                // Helper zum Erstellen eines DoTs
                EntityModifier MakeDot(string statName, GameplayRef tickRef)
                {
                    return new EntityModifier(
                        modifierName: statName,
                        owner: Target,
                        baseValue: tick,
                        toTriggerRefs: new() { tickRef },
                        duration: dur,
                        onRef_Trigger: new RelevantTriggerCheck
                        {
                            OnTriggerReference = new() { GameplayRef.onTurnStart },
                            CheckType = CheckEntityType.Target,
                            CheckEntity = User,
                        },
                        actionTargetType: EntityModifier.ActionTargetType.Affected,
                    onRef_Action: (target, cd, value) =>
                    {
                        CombatUtility.ApplyDamage(null, target, value);
                    });
                }

                var poison = MakeDot("Poison", GameplayRef.onPoison);
                var burn = MakeDot("Burn", GameplayRef.onBurn);
                var bleed = MakeDot("Bleed", GameplayRef.onBleed);

                // WICHTIG: hier explizit Merge (kein Refresh), damit gleiche Karte nicht verlängert
                CombatUtility.ApplyEntityModifier(d, Target, poison, ModifierMergeStrategy.Merge);
                CombatUtility.ApplyEntityModifier(d, Target, burn, ModifierMergeStrategy.Merge);
                CombatUtility.ApplyEntityModifier(d, Target, bleed, ModifierMergeStrategy.Merge);
            }
        });

        // 120105 – Black Lotus Needle (Single heavy hit)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120105,
            cardName = "Black Lotus Needle",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 80,
            damage_u = 200,
            range_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Deal {d.Damage} damage to a single enemy.",

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target);
            }
        });

        // 120106 – Moon Piercing Arrow (LineSelf; multi-hit along the line)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120106,
            cardName = "Moon Piercing Arrow",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 60,
            damage_u = 100,
            range_u = 5,
            area_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.LineSelf,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Line: hit up to {d.Area} enemies for {d.Damage} each (range {d.Range}).",

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target);
            }
        });


        // 120107 – Midnight Rain (Small AOE)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120107,
            cardName = "Midnight Rain",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 4,
            damage_u = 5,
            range_u = 5,
            area_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Radius,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Deals {d.Damage} damage in a small area.",

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(d, Target);
            }
        });

        // 120108 – Bouncing Shot (chain with ally/enemy + hop distance <= 2)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120108,
            cardName = "Bouncing Shot",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 3,
            damage_u = 4,
            repeats_u = 2, // Anzahl Bounces (Standard 2, kann per Stat skaliert werden)
            range_u = 4,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Hit for {d.Damage} and bounce to {(d.Repeats > 0 ? d.Repeats : 2)} enemies (≤2 tiles between bounces).",

            CardEffect = (User, Target, d) =>
            {
                bool IsEnemyOf(EntityScript owner, EntityScript candidate)
                {
                    if (owner == null || candidate == null) return false;
                    // Feind = andere Fraktion und nicht Neutral.
                    return candidate.entityAffiliation != owner.entityAffiliation
                           && candidate.entityAffiliation != EntityAffiliation.Neutral;
                }

                int StepsBetween(Vector3Int from, Vector3Int to)
                {
                    var p = MovementUtility.FindPath(from, to, ignoreCost: true);
                    // Path.Count = Anzahl Knoten; Schritte = Knoten - 1
                    return (p.Path == null || p.Path.Count == 0) ? int.MaxValue : Mathf.Max(0, p.Path.Count - 1);
                }

                EntityScript FindNextBounceTarget(EntityScript from, HashSet<int> alreadyHit, int maxHopSteps)
                {
                    var allOnMap = Object.FindObjectsByType<EntityOnMap>(
                        FindObjectsInactive.Exclude, FindObjectsSortMode.None);

                    var fromCell = from.GetComponent<EntityOnMap>()?.currentCell ?? TilemapUtilityScript.InvalidPosition;

                    EntityScript best = null;
                    int bestSteps = int.MaxValue;

                    foreach (var eom in allOnMap)
                    {
                        var cand = eom.GetComponent<EntityScript>();
                        if (cand == null) continue;
                        if (alreadyHit.Contains(cand.GetInstanceID())) continue;
                        if (!IsEnemyOf(User, cand)) continue;

                        int steps = StepsBetween(fromCell, eom.currentCell);
                        if (steps <= maxHopSteps && steps < bestSteps)
                        {
                            best = cand;
                            bestSteps = steps;
                        }
                    }
                    return best;
                }

                // 1) Starttreffer
                CombatUtility.ApplyDamage(d, Target);

                // 2) Bounces
                var hitSet = new HashSet<int> { Target.GetInstanceID() };
                var last = Target;

                int maxBounces = (d.Repeats > 0) ? d.Repeats : 2; 
                const int hopLimit = 2; 

                for (int i = 0; i < maxBounces; i++)
                {
                    var next = FindNextBounceTarget(last, hitSet, hopLimit);
                    if (next == null) break;

                    CombatUtility.ApplyDamage(d, next);
                    hitSet.Add(next.GetInstanceID());
                    last = next;
                }
            }
        });

        // 120109 – Barbed Needle Volley (Damage + Bleed)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120109,
            cardName = "Barbed Needle Volley",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Physical, CardIdentity.Blood },

            cost_u = 60,
            damage_u = 50,
            duration_u = 6,
            range_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Cone,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"AOE: deal {d.Damage} damage and inflict Bleed ({d.Duration} turns)";
            },
            CardEffect = (User, Target, d) =>
            {
                // Direktschaden
                CombatUtility.ApplyDamage(d, Target);

                CombatUtility.ApplyEntityModifier(d, Target, EffectDatabase.GetEffectByName("Bleed", CloneMode.OverrideFromData, d, ThroughputSource.Damage, User), ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

        // 120111 – Merciful Headshot (Stun + Damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120111,
            cardName = "Merciful Headshot",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 2,
            damage_u = 6,
            duration_u = 1, // Stun für 1 Zug
            range_u = 5,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Deal {d.Damage} damage and Stun the target for {d.Duration} turn.",

            CardEffect = (User, Target, d) =>
            {
                // Direkter Schaden
                CombatUtility.ApplyDamage(d, Target);

                // Stun-Modifier (Ein-Zug)
                var stun = new EntityModifier(
                    modifierName: "Stun",
                    owner: Target,
                    baseValue: 1,
                    toTriggerRefs: new() { GameplayRef.onStunned },
                    duration: Mathf.Max(1, d.Duration),
                    onRef_Trigger: new RelevantTriggerCheck
                    {
                        OnTriggerReference = new() { GameplayRef.onTurnStart },
                        CheckType = CheckEntityType.Target,
                        CheckEntity = User,
                    },
                    onRef_Action: (target, cd, value) =>
                    {
                        GameEvents.TriggerRefEvent(new ToSendTriggerReference
                        {
                            OnTriggerReference = new() { GameplayRef.onStunned },
                            AffectedEntities = new() { Target },
                            UserEntity = User
                        });
                        // Optional: hier könntest du AP/Stamina auf 0 setzen, falls dein System so arbeitet.
                        // Target.entityStats.CurrentStamina?.ApplyFinalValue(0); // <-- nur falls vorhanden/gewünscht
                    });

                CombatUtility.ApplyEntityModifier(d, Target, stun, ModifierMergeStrategy.Merge);
            }
        });

        // 120112 – Crimson Thorn Array (Bleed DoT AOE)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120112,
            cardName = "Crimson Thorn Array",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Physical, CardIdentity.Blood },

            cost_u = 50,
            damage_u = 20,   
            duration_u = 3,

            range_u = 10,
            radius_u = 2,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Ring,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Deal {d.Damage} Damage at the Start of your turn to targets in the effect.",

            CardEffectGround = (User, Target, d) =>
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
                    onRef: (target) => { CombatUtility.ApplyDamage(null, target, d.Damage); AssetManager.Instance.CreateFX("BloodEffect", Target);
                    }
                   )); 
            }
        });
    }

    private static void RegisterAbilities()
    {
        //120201 - Phantom Step - Moves Behind an Enemy
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120201,
            cardName = "Phantom Step",
            cardType = CardType.Ability,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Fire, CardIdentity.Poison },

            cost_u = 40,
            range_u = 5,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                cardTargetingMode = CardTargetingMode.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"Moves behind an Enemy.";
            },

            CardEffect = (User, Target, d) =>
            {
                MovementUtility.ForcedMove(ForcedMovementType.Jump, User, Target.GetComponent<EntityOnMap>().currentCell);
            }
        });

        // 120202 – Apply Scorching Blood Venom – next X hits apply Burn DoT
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120202,
            cardName = "Apply Scorching Blood Venom",
            cardType = CardType.Ability,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Fire, CardIdentity.Poison },

            cost_u = 0,
            charges_u = 3,
            damage_u = 2,
            duration_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                cardTargetingMode = CardTargetingMode.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"Next {d.Power} attacks apply Burn (DoT {d.Damage} for {d.Duration} turns).";
            },

            CardEffect = (User, Target, d) =>
            {
                //VenomUtility.ArmBurnFromCard(User, d);
            }
        });

        // 120203 – Apply Black Lotus Venom – next X hits apply Poison DoT
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120203,
            cardName = "Apply Black Lotus Venom",
            cardType = CardType.Ability,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Venom },

            cost_u = 5,
            duration_u = 3,
            charges_u = 2,
            damage_u = 10,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                cardTargetingMode = CardTargetingMode.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = "Next {Charges} attacks apply {Damage} Poison";
            },

            CardEffect = (User, Target, cd) =>
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
            cardID = 120204,
            cardName = "Apply Dazzlying Numbing Venom",
            cardType = CardType.Ability,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Venom },

            cost_u = 15,
            charges_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                cardTargetingMode = CardTargetingMode.Single,
            },

            CardDescription = (User, cd) =>
            {
                cd.cardDescription = "The next {Charges} attacks apply Stun";
            },

            CardEffect = (User, Target, cd) =>
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
                            CombatUtility.ApplyEntityModifier( d, t, EffectDatabase.GetEffectByName("Stun", CloneMode.Defaults, d, ThroughputSource.Power, t), ModifierMergeStrategy.RefreshDurationAndMerge);
                        }),
                    ModifierMergeStrategy.Override);
            }
        });

        // 120205 – Reapply Venom (top-up to X charges of your last venom card)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120205,
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

            CardDescription = (User, d) =>
            {
                int target = (d.Power > 0 ? d.Power : 3);
                d.cardDescription = $"Reapply your last venom card.";
            },

            CardEffect = (User, Target, d) =>
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
                lastVenomCard.CardData.CardEffect.Invoke(User, Target, lastVenomCard.CardData);
            }
        });

            // 120206 – Eye of the Nighthawk – dmg/crit up (non-damage)
            CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120206,
            cardName = "Eye of the Nighthawk",
            cardType = CardType.Ability,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Shadow },

            cost_u = 0,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                cardTargetingMode = CardTargetingMode.Single,
            },

            CardDescription = (User, data) => data.cardDescription = "Your next damage dealt is doubled",
            CardEffect = (User, Target, data) =>
            {
                CombatUtility.ApplyStatBuff(data, Target, new StatModifier
                    (
                        name: "CritOnNextTechnique",
                        stat: Target.entityStats.DamageOutModifier,
                        value: 200,
                        scaling: ModifierScaling.Multiplier,
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
            cardID = 120401,
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
            CardDescription = (User, data) => data.cardDescription = "Randomly inflict a negative effect on yourself (TODO).",
            CardEffect = (User, Target, data) => { /* TODO */ }
        });
    }

    private static void RegisterBlessings()
    {
        // 120501 – Lucky Strike – next attack repeats (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120501,
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
            CardDescription = (User, data) => data.cardDescription = "Next attack repeats again (TODO).",
            CardEffect = (User, Target, data) => { /* TODO */ }
        });
    }
}
