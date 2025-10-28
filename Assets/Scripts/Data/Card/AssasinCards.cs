using Mono.Cecil;
using System.Collections.Generic;
using UnityEngine;
using Utility;

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

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.LineSelf,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Line: deal {d.Damage} damage to enemies in line (range {d.Range}).",

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(User, Target, d.Damage);
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
                SelectionType = CardTargetSelection.Ring,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"AOE (adjacent): deal {d.Damage} damage (x{d.Repeats}).",

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(User, Target, d.Damage);
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
            damage_u = 10,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"If target <10% HP: execute (TODO). Otherwise deal {d.Damage} damage.",

            CardEffect = (User, Target, d) =>
            {
                // TODO: Execute-Logik (instant kill) wenn Ziel <10% MaxHP
                CombatUtility.ApplyDamage(User, Target, d.Damage);
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

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
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
                        statName: statName,
                        baseValue: tick,
                        to_Trigger_refs: new() { tickRef },
                        duration: dur,
                        target: Target.entityStats.CurrentHealth,
                        triggerConditionRef: new TriggerRef
                        {
                            UserId = User.GetInstanceID(),
                            References = new() { GameplayRef.onTurnStart },
                            AffectedEntityId = Target.GetInstanceID()
                        },
                        onRefEventAction: (mod, stat, ev) =>
                        {
                            GameEvents.TriggerRefEvent(new TriggerRef
                            {
                                References = new() { tickRef },
                                UserId = User.GetInstanceID(),
                                AffectedEntityId = Target.GetInstanceID()
                            });
                            CombatUtility.ApplyDamage(User, Target, mod.BaseValue);
                        });
                }

                var poison = MakeDot("Poison", GameplayRef.onPoison);
                var burn = MakeDot("Burn", GameplayRef.onBurn);
                var bleed = MakeDot("Bleed", GameplayRef.onBleed);

                // WICHTIG: hier explizit Merge (kein Refresh), damit gleiche Karte nicht verlängert
                CombatUtility.ApplyEntityModifier(User, Target, poison, ModifierMergeStrategy.Merge);
                CombatUtility.ApplyEntityModifier(User, Target, burn, ModifierMergeStrategy.Merge);
                CombatUtility.ApplyEntityModifier(User, Target, bleed, ModifierMergeStrategy.Merge);
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

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Deal {d.Damage} damage to a single enemy.",

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(User, Target, d.Damage);
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

            cost_u = 30,
            damage_u = 100,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.LineSelf,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Line: hit up to {d.Area} enemies for {d.Damage} each (range {d.Range}).",

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(User, Target, d.Damage);
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

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Radius,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Deals {d.Damage} damage in a small area.",

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(User, Target, d.Damage);
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

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Hit for {d.Damage} and bounce to {(d.Repeats > 0 ? d.Repeats : 2)} enemies (≤2 tiles between bounces).",

            CardEffect = (User, Target, d) =>
            {
                // -------- Helpers --------
                bool IsEnemyOf(EntityScript owner, EntityScript candidate)
                {
                    if (owner == null || candidate == null) return false;
                    // Feind = andere Fraktion und nicht Neutral.
                    return candidate.entityAffiliation != owner.entityAffiliation
                           && candidate.entityAffiliation != EntityAffiliation.Neutral;
                }

                int StepsBetween(Vector3Int from, Vector3Int to)
                {
                    var p = TilemapUtilityScript.FindPath(from, to, ignoreCost: true);
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
                CombatUtility.ApplyDamage(User, Target, d.Damage);

                // 2) Bounces
                var hitSet = new HashSet<int> { Target.GetInstanceID() };
                var last = Target;

                int maxBounces = (d.Repeats > 0) ? d.Repeats : 2; 
                const int hopLimit = 2; 

                for (int i = 0; i < maxBounces; i++)
                {
                    var next = FindNextBounceTarget(last, hitSet, hopLimit);
                    if (next == null) break;

                    CombatUtility.ApplyDamage(User, next, d.Damage);
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

            cost_u = 4,
            damage_u = 7,
            duration_u = 6, // Bleed-Dauer

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Radius, // (du hast Sphere -> Radius bereits korrigiert)
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"AOE: deal {d.Damage} damage and inflict Bleed ({d.Duration} turns).",

            CardEffect = (User, Target, d) =>
            {
                // Direktschaden
                CombatUtility.ApplyDamage(User, Target, d.Damage);

                // Bleed-DoT
                var bleed = new EntityModifier(
                    statName: "Bleed",
                    baseValue: d.Damage, // oder etwas kleiner als d.Damage, falls gewünscht
                    to_Trigger_refs: new() { GameplayRef.onBleed },
                    duration: d.Duration,
                    target: Target.entityStats.CurrentHealth,
                    triggerConditionRef: new TriggerRef
                    {
                        References = new() { GameplayRef.onTurnStart },
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    onRefEventAction: (mod, stat, ev) =>
                    {
                        GameEvents.TriggerRefEvent(new TriggerRef
                        {
                            References = new() { GameplayRef.onBleed },
                            UserId = User.GetInstanceID(),
                            AffectedEntityId = Target.GetInstanceID()
                        });
                        CombatUtility.ApplyDamage(User, Target, mod.BaseValue);
                    });

                CombatUtility.ApplyEntityModifier(User, Target, bleed, ModifierMergeStrategy.RefreshDurationAndMerge);
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

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Deal {d.Damage} damage and Stun the target for {d.Duration} turn.",

            CardEffect = (User, Target, d) =>
            {
                // Direkter Schaden
                CombatUtility.ApplyDamage(User, Target, d.Damage);

                // Stun-Modifier (Ein-Zug)
                var stun = new EntityModifier(
                    statName: "Stun",
                    baseValue: 1,
                    to_Trigger_refs: new() { GameplayRef.onStunned },
                    duration: Mathf.Max(1, d.Duration),
                    target: Target.entityStats.CurrentHealth, // Träger ist die Entity; Stat selbst egal
                    triggerConditionRef: new TriggerRef
                    {
                        References = new() { GameplayRef.onTurnStart },
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    onRefEventAction: (mod, stat, ev) =>
                    {
                // Melde „Stun aktiv“ – Turn/AI sollten bei vorhandenem Stun-Modifier Aktionen überspringen (TODO in Turn/AI)
                GameEvents.TriggerRefEvent(new TriggerRef
                        {
                            References = new() { GameplayRef.onStunned },
                            UserId = User.GetInstanceID(),
                            AffectedEntityId = Target.GetInstanceID()
                        });
                // Optional: hier könntest du AP/Stamina auf 0 setzen, falls dein System so arbeitet.
                // Target.entityStats.CurrentStamina?.ApplyFinalValue(0); // <-- nur falls vorhanden/gewünscht
            });

                CombatUtility.ApplyEntityModifier(User, Target, stun, ModifierMergeStrategy.Merge);
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

            cost_u = 60,
            damage_u = 20,   
            duration_u = 3,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Ring,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Apply Bleed ({d.Damage} per turn for {d.Duration}) to adjacent enemies.",

            CardEffect = (User, Target, d) =>
            {
                var bleed = new EntityModifier(
                    statName: "Bleed",
                    baseValue: d.Damage,
                    to_Trigger_refs: new() { GameplayRef.onBleed },
                    duration: d.Duration,
                    target: Target.entityStats.CurrentHealth,
                    triggerConditionRef: new TriggerRef
                    {
                        References = new() { GameplayRef.onTurnStart },
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    onRefEventAction: (mod, stat, ev) =>
                    {
                        GameEvents.TriggerRefEvent(new TriggerRef
                        {
                            References = new() { GameplayRef.onBleed },
                            UserId = User.GetInstanceID(),
                            AffectedEntityId = Target.GetInstanceID()
                        });
                        CombatUtility.ApplyDamage(User, Target, mod.BaseValue);
                    });

                CombatUtility.ApplyEntityModifier(User, Target, bleed, ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });

    }

    private static void RegisterAbilities()
    {
        //120201 - Phantom Step - Moves Behind an Enemy
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120201,
            cardName = "Apply Scorching Blood Venom",
            cardType = CardType.Ability,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Fire, CardIdentity.Poison },

            cost_u = 0,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription = $"Moves behind an Enemy.";
            },

            CardEffect = (User, Target, d) =>
            {

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
            power_u = 3,     // <- Anzahl Angriffe (charges)
            damage_u = 2,    
            duration_u = 3,  

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription =
                    $"Next {d.Power} attacks apply Burn (DoT {d.Damage} for {d.Duration} turns).";
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
            cardIdentities = new() { CardIdentity.Poison },

            cost_u = 0,
            power_u = 3,     
            damage_u = 2,    
            duration_u = 3,  

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
            {
                d.cardDescription =
                    $"Next {d.Power} attacks apply Poison (DoT {d.Damage} for {d.Duration} turns).";
            },

            CardEffect = (User, Target, d) =>
            {
                //VenomUtility.ArmPoisonFromCard(User, d);
            }
        });

        // 120204 – Apply Dazzlying Numbing Venom (Stun for next X attacks)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120204,
            cardName = "Apply Dazzlying Numbing Venom",
            cardType = CardType.Ability,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 0,
            duration_u = 1, 
            power_u = 2,    

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
            {
                int charges = (d.Power > 0 ? d.Power : 3);
                d.cardDescription = $"For the next {charges} attacks: apply Stun for {d.Duration} turn(s).";
            },

            CardEffect = (User, Target, d) =>
            {
                //VenomUtility.ArmStunFromCard(User, d);
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
            power_u = 3, // Ziel-Anzahl an Charges nach dem Top-Up

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, d) =>
            {
                int target = (d.Power > 0 ? d.Power : 3);
                d.cardDescription = $"Reapply your last venom card (top-up to {target} charges).";
            },

            CardEffect = (User, Target, d) =>
            {
                //VenomUtility.ReapplyFromCard(User, d); // top-up auf d.Power (Default 3)
            }
        });

        // 120206 – Eye of the Nighthawk – dmg/crit up (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120206,
            cardName = "Eye of the Nighthawk",
            cardType = CardType.Ability,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 0,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
            },

            CardDescription = (User, data) => data.cardDescription = "Increase damage/crit (TODO).",
            CardEffect = (User, Target, data) => { /* TODO buff */ }
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
                SelectionType = CardTargetSelection.Single,
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
                SelectionType = CardTargetSelection.Single,
            },
            CardDescription = (User, data) => data.cardDescription = "Next attack repeats again (TODO).",
            CardEffect = (User, Target, data) => { /* TODO */ }
        });
    }
}
