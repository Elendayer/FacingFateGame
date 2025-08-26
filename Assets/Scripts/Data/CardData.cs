using System;
using System.Collections.Generic;
using UnityEngine;

public enum CardType
{
    Skill,
    Item,
    Ability,
    Technique,
    Spell,
    Blessing,
    Curse
}
public enum CardElement
{
    Non,
    Physical,
    Fire,
    Ice,
    Air,
    Earth,
    Shadow,
    Light,
    Blood,
    Arcane,
    Soul,
    Divine,
    Occult
}
public enum CardClass
{
    Knight,
    Rogue,
    Wizard,
    Cleric,
    Paladin,
    Warlock,
    Ranger,
    Druid,
    Barbarian,
    Alchemist,
    Monster,
}

[System.Serializable]
public class CardData
{
    public int cardID = 0;
    public string cardName = string.Empty;
    public EntityScript Owner;

    public Sprite cardArtwork;
    public string cardDescription;

    public CardType cardType;
    public CardClass cardClass;
    public List<CardElement> cardElement;

    [Header("Cost")]
    public int cost_u = 0;
    public Stat cost_s = new();
    public int Cost => cost_u + cost_s.GetFinalValue() + GetValues(StatAspect.Cost);

    [Header("Power")]
    public int power_u = 0;
    public Stat power_s = new();
    public int Power => power_u + power_s.GetFinalValue() + GetValues(StatAspect.Power);

    [Header("Duration")]
    public int duration_u = 0;
    public Stat duration_s = new();
    public int Duration => duration_u + duration_s.GetFinalValue() + GetValues(StatAspect.Duration);

    [Header("Repeats")]
    public int repeats_u = 0;
    public Stat repeats_s = new();
    public int Repeats => repeats_u + repeats_s.GetFinalValue() + GetValues(StatAspect.Repeats);

    [Header("Targets")]
    public CardType targetCardType;
    public CardScript targetCard;
    public bool targetSelf = false;

    int GetValues(StatAspect aspect)
    {
        int value = 0;

        value =
            Owner.GetStatValue(cardType, aspect)
            + Owner.GetStatValue(cardClass, aspect);

        foreach (CardElement ce in cardElement)
        {
            value += Owner.GetStatValue(ce, aspect);
        }

        return value;
    }
    public CardTargetType TargetType;

    public Action<EntityScript, CardData> SetCardDescription =
        (user, data) => Debug.Log($"Not defined Description of {data.cardName}");

    public Action<EntityScript, EntityScript, CardData> CardEffect =
        (user, target, data) => Debug.Log($"Not defined Effect used by {user} at {target} by Card {data.cardName}");

    [Header("AI")]
    public Intention Intention = Intention.None;
    public gameplayReference triggerCondition;
    public int priority = 0;
    public int cooldown = 1;

    public void ActivateCard(List<EntityScript> targetEntity)
    {
        foreach (EntityScript target in targetEntity)
            CardEffect?.Invoke(Owner, target, this);
    }
}
public enum CardTargetType
{
    Player,
    Enemy,
    Self
}