using System.Collections.Generic;
using UnityEngine;

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
    }

    private static void RegisterKnightCards()
    {
        RegisterCard(new CardData()
        {
            cardID = 100001,
            cardName = "Strike",
            cardType = CardType.Technique,
            cardClass = CardClass.Knight,
            cardElement = new() { CardElement.Physical },

            cost_u = 1,
            power_u = 10,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                areaType = CardTargetArea.Single,
                range = 1,
                area = 1,
            },

            SetCardDescription = (User, data) =>
            {
                data.cardDescription = $"Deal {data.Power} Damage";
            },

            CardEffect = (User, target, data) =>
            {
                target.CurrentHealth.Value -= data.Power;
            }
        });

        RegisterCard(new CardData()
        {
            cardID = 100002,
            cardName = "Thrust",
            cardType = CardType.Technique,
            cardClass = CardClass.Knight,
            cardElement = new() { CardElement.Physical },

            cost_u = 2,
            power_u = 6,

            targetingData = new()
            { 
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.Enemy,
                areaType = CardTargetArea.LineSelf,
                range = 3,
                area = 1,
            },

            SetCardDescription = (User, data) =>
            {
                data.cardDescription = $"Deal {data.Power} Damage in a {data.targetingData.range} Tile Line";
            },

            CardEffect = (User, target, data) =>
            {
                target.CurrentHealth.Value -= data.Power;
            }
        });

        RegisterCard(new CardData()
        {
            cardID = 100003,
            cardName = "Defend",
            cardType = CardType.Skill,
            cardClass = CardClass.Knight,
            cardElement = new() { CardElement.Physical },

            cost_u = 1,
            power_u = 5,
            duration_u = 2,


            SetCardDescription = (user, data) =>
            {
                data.cardDescription = $"Gain {data.Power} Block for {data.Duration} turns.";
            },

            CardEffect = (user, target, data) =>
            {
                user.Block.AddModifier(new StatModifier( data.Power,StatScaling.Flat, gameplayRef.onBlockingRef, data.Duration));
            }
        });

        RegisterCard(new CardData()
        {
            cardID = 100004,
            cardName = "Empowering Scream",
            cardType = CardType.Ability,
            cardClass = CardClass.Knight,
            cardElement = new() { CardElement.Physical },

            cost_u = 3,
            power_u = 3,

            EffectTargetTypes = new() { CardType.Technique },


            SetCardDescription = (user, data) =>
            {
                data.cardDescription = $"Increase the power of a Technique by {data.Power}";
            },

            CardEffect = (user, target, data) =>
            {
                data.targetCard.cardData.power_s.AddModifier(new StatModifier(data.Power, StatScaling.Flat, gameplayRef.onBuffedRef, name: "Empower"), ModifierMergeStrategy.RefreshIncrease);
            }
        });

        RegisterCard(new CardData()
        {
            cardID = 100005,
            cardName = "Bash",
            cardType = CardType.Technique,
            cardClass = CardClass.Knight,
            cardElement = new() { CardElement.Physical },

            cost_u = 2,
            duration_u = 1,


            SetCardDescription = (User, data) =>
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
            cardElement = new() { CardElement.Light },

            cost_u = 2,
            power_u = 1,
            duration_u = 1,

            targetingData = new()
            {
                CardTargetType = CardTargetType.Entity,
                CardTargetAffiliation = CardTargetAffiliation.Self,
                areaType = CardTargetArea.Single,
                range = 0
            },

            SetCardDescription = (User, data) =>
            {
                int heal = data.Power * data.Duration;
                data.cardDescription = $"Restore {heal} Health. Increase your Maximum Health by {heal}";
            },

            CardEffect = (User, target, data) =>
            {
                User.MaxHealth.AddModifier(new StatModifier(data.Power, StatScaling.Flat, gameplayRef.onBuffedRef, name: "Valiant Blessing"), ModifierMergeStrategy.Increase);
                CombatUtils.ApplyHealing(target, data.Power);
            }

        });
        RegisterCard(new CardData()
        {
            cardID = 100007,
            cardName = "Fire Bomb",
            cardType = CardType.Spell,
            cardClass = CardClass.Knight,
            cardElement = new() { CardElement.Fire },

            cost_u = 2,
            power_u = 3,
            duration_u = 6,

            targetingData = new()
            {
                CardTargetType = CardTargetType.CombatTile,
                CardTargetAffiliation = CardTargetAffiliation.All,
                areaType = CardTargetArea.Radius,
                range = 3,
                area = 2,
            },


            SetCardDescription = (User, data) =>
            {
                data.cardDescription = $"Ignite dealing {data.Power} for {data.Duration} turns";
            },

            CardEffect = (User, target, data) =>
            {
                // Create a FunctionModifier that applies damage over time on each turn start
                var burnModifier = new FunctionModifier(
                    statName: "burn",
                    baseValue: data.Power,
                    statScaling: StatScaling.Flat,
                    toTrigger_ref: gameplayRef.onBurningRef,
                    duration: data.Duration,
                    target: target.CurrentHealth,
                    triggerConditionRef: new TriggerRef() { Reference = gameplayRef.onTurnStart, TargetId = target.GetInstanceID()},
                    onRefEventAction: (modifier, stat, toTrigger_Reference) =>
                    {
                        CombatUtils.ApplyDamage(target, modifier.BaseValue);
                    });

                target.CurrentHealth.AddModifier(burnModifier, ModifierMergeStrategy.RefreshIncrease);
            }
        });
    }
    private static void RegisterEnemyCards()
    {
        RegisterCard(new CardData()
        {
            cardID = 120101,
            cardName = "Bite",
            cardElement = new() { CardElement.Physical },
            cardClass = CardClass.Monster,

            power_u = 5,


            CardEffect = (User, target, data) =>
            {
                target.CurrentHealth.Value -= data.Power + User.GetStatValue(EntityAttributeEnum.Strength);
            }
        });
        RegisterCard(new CardData()
        {
            cardID = 120102,
            cardName = "Claw",
            cardElement = new() { CardElement.Physical },
            cardClass = CardClass.Monster,

            power_u = 0,


            CardEffect = (User, target, data) =>
            {
                target.CurrentHealth.Value -= data.Power + User.GetStatValue(EntityAttributeEnum.Strength);
                }
        });
    }
}