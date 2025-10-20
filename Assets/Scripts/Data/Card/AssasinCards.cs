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
                range = 2,
                area = 1,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Line: deal {d.Damage} damage to enemies in line (range {d.targetingData.range}).",

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

            cost_u = 4,
            damage_u = 2,
            repeats_u = 4, // System-seitig wiederholt abhandeln

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Ring,
                range = 1,
                area = 1,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"AOE (adjacent): deal {d.Damage} damage (x{d.Repeats}).",

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyDamage(User, Target, d.Damage);
            }
        });

        // 120103 – Lotus Death Kiss (Execute <10% HP) – damage fallback jetzt
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
                range = 1,
                area = 1,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"If target <10% HP: execute (TODO). Otherwise deal {d.Damage} damage.",

            CardEffect = (User, Target, d) =>
            {
                // TODO: Execute-Logik (instant kill) wenn Ziel <10% MaxHP
                CombatUtility.ApplyDamage(User, Target, d.Damage);
            }
        });

        // 120104 – Moonlit Needlestorm (3 DoTs, Merge)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120104,
            cardName = "Moonlit Needlestorm",
            cardType = CardType.Technique,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Poison, CardIdentity.Fire, CardIdentity.Blood },

            cost_u = 3,
            damage_u = 1,     // Tickhöhe
            duration_u = 6,   // Dauer der DoTs

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
                range = 2,
                area = 1,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Inflict Poison, Burn and Bleed ({d.Damage} per turn for {d.Duration} turns).",

            CardEffect = (User, Target, d) =>
            {
                int tick = Mathf.Max(1, d.Damage);
                int dur = d.Duration > 0 ? d.Duration : 6;

                // Helper zum Erstellen eines DoTs
                EntityModifier MakeDot(string statName, gameplayRef tickRef)
                {
                    return new EntityModifier(
                        statName: statName,
                        baseValue: tick,
                        to_Trigger_refs: new() { tickRef },
                        duration: dur,
                        target: Target.entityStats.CurrentHealth,
                        triggerConditionRef: new TriggerRef
                        {
                            References = new() { gameplayRef.onTurnStart },
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

                var poison = MakeDot("Poison", gameplayRef.onPoison);
                var burn = MakeDot("Burn", gameplayRef.onBurn);
                var bleed = MakeDot("Bleed", gameplayRef.onBleed);

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

            cost_u = 4,
            damage_u = 20,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
                range = 2,
                area = 1,
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

            cost_u = 3,
            damage_u = 10,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.LineSelf,
                range = 3,
                area = 3, // <= trifft bis zu X Gegner in der Linie
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Line: hit up to {d.targetingData.area} enemies for {d.Damage} each (range {d.targetingData.range}).",

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
                range = 2,
                area = 1,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"AOE: deal {d.Damage} damage in a small area.",

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
                range = 3,   // Reichweite zum ERSTEN Ziel
                area = 1,
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
                range = 2,
                area = 1,
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
                    to_Trigger_refs: new() { gameplayRef.onBleed },
                    duration: d.Duration,
                    target: Target.entityStats.CurrentHealth,
                    triggerConditionRef: new TriggerRef
                    {
                        References = new() { gameplayRef.onTurnStart },
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    onRefEventAction: (mod, stat, ev) =>
                    {
                        GameEvents.TriggerRefEvent(new TriggerRef
                        {
                            References = new() { gameplayRef.onBleed },
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
                range = 3,
                area = 1,
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
                    to_Trigger_refs: new() { gameplayRef.onStunned },
                    duration: Mathf.Max(1, d.Duration),
                    target: Target.entityStats.CurrentHealth, // Träger ist die Entity; Stat selbst egal
                    triggerConditionRef: new TriggerRef
                    {
                        References = new() { gameplayRef.onTurnStart },
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    onRefEventAction: (mod, stat, ev) =>
                    {
                // Melde „Stun aktiv“ – Turn/AI sollten bei vorhandenem Stun-Modifier Aktionen überspringen (TODO in Turn/AI)
                GameEvents.TriggerRefEvent(new TriggerRef
                        {
                            References = new() { gameplayRef.onStunned },
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

            cost_u = 3,
            damage_u = 8,   // Bleed-Tickhöhe
            duration_u = 6,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Ring,
                range = 1,
                area = 1,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Apply Bleed ({d.Damage} per turn for {d.Duration}) to adjacent enemies.",

            CardEffect = (User, Target, d) =>
            {
                var bleed = new EntityModifier(
                    statName: "Bleed",
                    baseValue: d.Damage,
                    to_Trigger_refs: new() { gameplayRef.onBleed },
                    duration: d.Duration,
                    target: Target.entityStats.CurrentHealth,
                    triggerConditionRef: new TriggerRef
                    {
                        References = new() { gameplayRef.onTurnStart },
                        AffectedEntityId = Target.GetInstanceID()
                    },
                    onRefEventAction: (mod, stat, ev) =>
                    {
                        GameEvents.TriggerRefEvent(new TriggerRef
                        {
                            References = new() { gameplayRef.onBleed },
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
        // 120202 – Apply Scorching Blood Venom (Next attack applies Ignite/Burn)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120202,
            cardName = "Apply Scorching Blood Venom",
            cardType = CardType.Ability,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Fire },

            cost_u = 0,           // Tabelle: 0–1 → erstmal 0
            damage_u = 3,         // Tick pro Runde
            duration_u = 6,       // 6 Runden

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
                range = 0,
                area = 1,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Next attack applies Ignite: {d.Damage} damage for {d.Duration} turns.",

            CardEffect = (User, Target, d) =>
            {
                // Ignite == Burn
                CombatUtility.ApplyNextHitDot(User, d.Damage, d.Duration, "Burn", gameplayRef.onBurn);
                CombatUtility.SetLastVenom(User, "Burn", d.Damage, d.Duration, gameplayRef.onBurn); // <— merken für Reapply
            }

    });

        // 120203 – Apply Black Lotus Venom (Next attack applies Poison)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120203,
            cardName = "Apply Black Lotus Venom",
            cardType = CardType.Ability,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.Poison },

            cost_u = 0,
            damage_u = 3,
            duration_u = 6,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
                range = 0,
                area = 1,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Next attack applies Poison: {d.Damage} damage for {d.Duration} turns.",

            CardEffect = (User, Target, d) =>
            {
                CombatUtility.ApplyNextHitDot(User, d.Damage, d.Duration, "Poison", gameplayRef.onPoison);
                CombatUtility.SetLastVenom(User, "Poison", d.Damage, d.Duration, gameplayRef.onPoison);
            }
        });

        // 120204 – Apply Dazzlying Numbing Venom (Next attack applies Stun)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120204,
            cardName = "Apply Dazzlying Numbing Venom",
            cardType = CardType.Ability,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 0,
            damage_u = 0,
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
                range = 0,
                area = 1,
            },

            CardDescription = (User, d) =>
                d.cardDescription = $"Next attack applies Stun (duration {d.Duration}).",

            CardEffect = (User, Target, d) =>
            {
                // TODO: NextHit → apply Stun to the hit target (no damage-over-time).
                // CombatUtility.ApplyNextHitStatus(User, duration: d.Duration, "Stun", gameplayRef.onStunned);
                // CombatUtility.SetLastVenom(User, "Stun", d.Damage, d.Duration, gameplayRef.onStunned);
            }
        });

        // 120205 – Eye of the Nighthawk – dmg/crit up (non-damage)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120205,
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
                range = 0,
                area = 1,
            },

            CardDescription = (User, data) => data.cardDescription = "Increase damage/crit (TODO).",
            CardEffect = (User, Target, data) => { /* TODO buff */ }
        });

        // 120206 – Reapply Venom – reapplies the last used venom
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 120206,
            cardName = "Reapply Venom",
            cardType = CardType.Ability,
            cardClass = CardClass.Assassin,
            cardIdentities = new() { CardIdentity.None },

            cost_u = 0,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
                range = 0,
                area = 1,
            },

            CardDescription = (User, d) =>
                d.cardDescription = "Reapply the last venom used; your next attack applies it again.",

            CardEffect = (User, Target, d) =>
            {
                if (CombatUtility.TryGetLastVenom(User, out var ven))
                {
                    CombatUtility.ApplyNextHitDot(User, ven.tick, ven.duration, ven.effectName, ven.tickRef);
                }
                else
                {
                    // Kein gespeichertes Venom vorhanden → kein Effekt (oder optional Default)
                    // CombatUtility.ApplyNextHitDot(User, 3, 6, "Poison", gameplayRef.onPoison);
                }
            }
        });

    }

    private static void RegisterSpells()
    {
        // no explicit damage spells listed for Assassin; add later if needed
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
                range = 0,
                area = 1,
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
                range = 0,
                area = 1,
            },
            CardDescription = (User, data) => data.cardDescription = "Next attack repeats again (TODO).",
            CardEffect = (User, Target, data) => { /* TODO */ }
        });
    }
}
