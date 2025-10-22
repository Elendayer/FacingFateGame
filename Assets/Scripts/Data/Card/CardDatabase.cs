using System.Collections.Generic;
using UnityEngine;
using Utility;

public static class CardDatabase
{
    private static Dictionary<int, CardData> cardLookup = new Dictionary<int, CardData>();

    public static void RegisterCard(CardData card)
    {
        if (!cardLookup.ContainsKey(card.cardID))
        {
            cardLookup[card.cardID] = card;
            //Debug.Log($"Registered card: {card.cardName} (ID: {card.cardID})");
        }
        else
        {
            Debug.LogWarning($"Duplicate card ID detected: {card.cardID}");
        }
    }
    public static CardData GetCardById(int id, EntityScript owner)
    {
        if (!cardLookup.TryGetValue(id, out var blueprint) || blueprint == null)
            return null;

        CardData cd = blueprint.Clone();   // <-- frische Instanz mit richtigem Owner
        cd.Owner = owner;
        return cd;
    }

    public static List<CardData> GetAllCards()
    {
        return new List<CardData>(cardLookup.Values);
    }

    public static void RegisterAll()
    {
        RegisterKnightCards();
        RegisterEnemyCards();
        SpearmanCards.RegisterAll();
        AssassinCards.RegisterAll();
        MysticCards.RegisterAll();
        PhysicianCards.RegisterAll(); 
    }

    private static void RegisterKnightCards()
    {
        RegisterCard(new CardData()
        {
            cardID = 100001,
            cardName = "Strike",
            cardType = CardType.Technique,
            cardClass = CardClass.Knight,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 1,
            power_u = 10,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Deal {data.Power} Damage";
            },

            CardEffect = (User, target, data) =>
            {
                CombatUtility.ApplyDamage(User,target, data.Power, true);
            }
        });

        RegisterCard(new CardData()
        {
            cardID = 100002,
            cardName = "Thrust",
            cardType = CardType.Technique,
            cardClass = CardClass.Knight,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 2,
            power_u = 6,

            targetingData = new()
            { 
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.LineSelf,
                range = 3,
                area = 6,
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Deal {data.Power} Damage in a {data.targetingData.range} Tile Line";
            },

            CardEffect = (User, target, data) =>
            {
                CombatUtility.ApplyDamage(User, target, data.Power, true);
            }
        });

        RegisterCard(new CardData()
        {
            cardID = 100003,
            cardName = "Defend",
            cardType = CardType.Skill,
            cardClass = CardClass.Knight,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 1,
            power_u = 5,
            duration_u = 2,


            CardDescription = (user, data) =>
            {
                data.cardDescription = $"Gain {data.Power} Block for {data.Duration} turns.";
            },

            CardEffect = (user, target, data) =>
            {
                user.entityStats.Block.AddModifier(new StatModifier( data.Power,ModifierScaling.Flat, new List<gameplayRef>() { gameplayRef.onBlocking }, data.Duration));
            }
        });

        RegisterCard(new CardData()
        {
            cardID = 100004,
            cardName = "Empowering Scream",
            cardType = CardType.Ability,
            cardClass = CardClass.Knight,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 3,
            power_u = 3,

            EffectTargetTypes = new() { CardType.Technique },


            CardDescription = (user, data) =>
            {
                data.cardDescription = $"Increase the power of a Technique by {data.Power}";
            },

            CardEffect = (user, target, data) =>
            {
                //data.targetCard.cardData.power_s.AddModifier(new StatModifier(data.Power, StatScaling.Flat, new List<gameplayRef>() { gameplayRef.onBuffedRef }, name: "Empower"), ModifierMergeStrategy.RefreshIncrease);
            }
        });

        RegisterCard(new CardData()
        {
            cardID = 100005,
            cardName = "Bash",
            cardType = CardType.Technique,
            cardClass = CardClass.Knight,
            cardIdentities = new() { CardIdentity.Physical },

            cost_u = 2,
            duration_u = 1,


            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Stun for {data.Duration} turn";
            },

            CardEffect = (User, target, data) =>
            {

            }
        });

        RegisterCard(new CardData()
        {
            cardID = 100006,
            cardName = "Valiant Blessing",
            cardType = CardType.Blessing,
            cardClass = CardClass.Knight,
            cardIdentities = new() { CardIdentity.Light },

            cost_u = 2,
            power_u = 50,
            healing_u = 20,
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                SelectionType = CardTargetSelection.Single,
                range = 1,
                area = 1,
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Restore {data.Healing} Health. Increase your Maximum Health by {data.Power}";
            },

            CardEffect = (User, Target, data) =>
            {
            
                var mod = new StatModifier(data.Power, ModifierScaling.Flat, new List<gameplayRef>() { }, name: "Valiant Blessing");

                CombatUtility.ApplyBuff(User, Target, Target.entityStats.MaxHealth, mod, ModifierMergeStrategy.Merge);
                CombatUtility.ApplyHealing(User, Target, data.Healing);
            }

        });
        RegisterCard(new CardData()
        {
            cardID = 100007,
            cardName = "Fire Bomb",
            cardType = CardType.Spell,
            cardClass = CardClass.Knight,
            cardIdentities = new() { CardIdentity.Fire },

            cost_u = 2,
            power_u = 3,
            duration_u = 6,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.All,
                SelectionType = CardTargetSelection.Radius,
                range = 3,
                area = 2,
            },

            CardAiBias = new()
            {
            },

            CardDescription = (User, data) =>
            {
                data.cardDescription = $"Apply Burn dealing {data.Power} for {data.Duration} turns";
            },

            CardEffect = (User, Target, data) =>
            {
                // FunctionModifier that applies damage over time on each turn start
                var burnModifier = new EntityModifier(
                    statName: "Burn",
                    baseValue: data.Power,
                    to_Trigger_refs : new() { gameplayRef.onBurn },
                    duration: data.Duration,
                    target: Target.entityStats.CurrentHealth,
                    statModifier : new StatModifier(data.Power, ModifierScaling.Flat, new List<gameplayRef>() { gameplayRef.onBurn }, name: "Burn"),
                    triggerConditionRef: new TriggerRef() { References = new() { gameplayRef.onTurnStart }, AffectedEntityId = Target.GetInstanceID()},
                    onRefEventAction: (modifier, stat, toTrigger_Reference) =>
                    {
                        GameEvents.TriggerRefEvent(new TriggerRef() { References = new() { gameplayRef.onBurn }, UserId = User.GetInstanceID(), AffectedEntityId = Target.GetInstanceID() });
                        CombatUtility.ApplyDamage(User, Target, modifier.BaseValue);
                    });

                CombatUtility.ApplyEntityModifier(User, Target, burnModifier, ModifierMergeStrategy.RefreshDurationAndMerge);
            }
        });
    }
    private static void RegisterEnemyCards()
    {
        RegisterCard(new CardData()
        {
            cardID = 990101,
            cardName = "Bite",
            cardIdentities = new() { CardIdentity.Physical },
            cardClass = CardClass.Monster,

            power_u = 5,


            CardEffect = (User, target, data) =>
            {
            }
        });
        RegisterCard(new CardData()
        {
            cardID = 990102,
            cardName = "Claw",
            cardIdentities = new() { CardIdentity.Physical },
            cardClass = CardClass.Monster,

            power_u = 0,


            CardEffect = (User, target, data) =>
            {
                }
        });
        // 100602 � Throw Firebomb � Single/Radius; apply Burn DoT (with immediate tick)
        CardDatabase.RegisterCard(new CardData()
        {
            cardID = 100602,
            cardName = "Throw Firebomb",
            cardType = CardType.Item,
            cardClass = CardClass.Monster,
            cardIdentities = new() { CardIdentity.Fire, CardIdentity.Ranged },

            cost_u = 2,
            damage_u = 3,
            duration_u = 6,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                SelectionType = CardTargetSelection.Radius, // keep as defined in your file
                range = 3,
                area = 2,
            },

            CardDescription = (User, d) => d.cardDescription = $"Apply Burn {d.Damage} for {d.Duration} turns (immediate tick).",
            CardEffect = (User, Target, d) =>
            {
                string name = $"Burn#{d.cardID}";
                var burn = new EntityModifier(
                    statName: name,
                    baseValue: d.Damage,
                    to_Trigger_refs: new() { gameplayRef.onBurn },
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
                            References = new() { gameplayRef.onBurn },
                            UserId = User.GetInstanceID(),
                            AffectedEntityId = Target.GetInstanceID()
                        });
                        CombatUtility.ApplyDamage(User, Target, mod.BaseValue);
                    });

                CombatUtility.ApplyEntityModifier(User, Target, burn, ModifierMergeStrategy.RefreshDurationAndMerge);

                // immediate tick on play
                CombatUtility.ApplyDamage(User, Target, d.Damage);
                GameEvents.TriggerRefEvent(new TriggerRef
                {
                    References = new() { gameplayRef.onBurn },
                    UserId = User.GetInstanceID(),
                    AffectedEntityId = Target.GetInstanceID()
                });
            }
        });
    }
}