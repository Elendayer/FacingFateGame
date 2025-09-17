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
    Poison,
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
    [Header("Meta")]
    public int cardID = 0;
    public string cardName = string.Empty;
    public EntityScript Owner = null;

    [Header("Visuals")]
    public Sprite cardArtwork;
    public string cardDescription;

    [Header("Typing")]
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

    [Header("Card Target")]
    public TargetArea targetingData;

    [Header("Card Effect Target")]
    public List<CardType> EffectTargetTypes;
    public CardScript targetCard;

    public CardData Clone()
    {
        // Hilfsfunktion: Stat flach kopieren (nur Base-Value, keine Modifiers)
        Stat CloneStat(Stat s) => new Stat { Value = s != null ? s.Value : 0 };

        return new CardData
        {
            // Identity & Meta
            cardID = cardID,
            cardName = cardName,
            Owner = Owner,
            cardArtwork = cardArtwork,
            cardDescription = cardDescription,

            // Typisierung
            cardType = cardType,
            cardClass = cardClass,
            cardElement = cardElement != null ? new List<CardElement>(cardElement) : new List<CardElement>(),

            // Werte (u = base, s = eigene Stat-Container pro Instanz)
            cost_u = cost_u,
            cost_s = CloneStat(cost_s),
            power_u = power_u,
            power_s = CloneStat(power_s),
            duration_u = duration_u,
            duration_s = CloneStat(duration_s),
            repeats_u = repeats_u,
            repeats_s = CloneStat(repeats_s),

            // Targeting-Flags (keine Ziel-Referenzen übernehmen)
            targetingData = targetingData,

            // Delegates (zeigen auf dieselben Methoden – gewünscht)
            SetCardDescription = SetCardDescription,
            CardEffect = CardEffect,

            // AI
            Intention = Intention,
            triggerCondition = triggerCondition,
            priority = priority,
            cooldown = cooldown
        };
    }

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

    public Action<EntityScript, CardData> SetCardDescription =
        (user, data) => Debug.Log($"Not defined Description of {data.cardName}");

    public Action<EntityScript, EntityScript, CardData> CardEffect =
        (user, target, data) => Debug.Log($"Not defined Effect used by {user} at {target} by Card {data.cardName}");

    [Header("AI")]
    public Intention Intention = Intention.None;
    public gameplayRef triggerCondition;
    public int priority = 0;
    public int cooldown = 1;

    public void ActivateCard(List<EntityScript> targetEntity, GameObject obj)
    {
        foreach (EntityScript target in targetEntity)
            CardEffect?.Invoke(Owner, target, this);
        HandManager.Instance.DiscardCard(obj);
    }
}
public enum CardTargetType
{
    Entity,
    CombatTile,
}
public enum CardTargetAffiliation
{
    All,
    Self,
    Ally,
    AllyNeutral,
    Enemy,
    EnemyNeutral,
    AllyEnemy,
}
public enum CardTargetArea
{
    Single,
    Radius,
    Ring,
    LineFree,
    LineSelf,
    All
}
[System.Serializable]
public class TargetArea
{
    public CardTargetType CardTargetType;
    public CardTargetAffiliation CardTargetAffiliation;
    public CardTargetArea areaType = CardTargetArea.Single;
    public int range = 1; 
    public int area = 1; 
}