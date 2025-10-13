using System;
using System.Collections.Generic;
using UnityEngine;


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
    public List<CardIdentity> cardIdentities = new();

    [Header("Cost")]
    public int cost_u = 0;
    public Stat cost_s = new();
    public int Cost => cost_u + cost_s.Value + GetValues(StatAspect.Cost);

    [Header("Power")]
    public int power_u = 0;
    public Stat power_s = new();
    public int Power =>  power_u + power_s.Value + GetValues(StatAspect.Power);

    [Header("Damage")]
    public int damage_u = 0;
    public Stat damage_s = new();
    public int Damage => Owner.entityStats.DamageIncrease.ApplyFinalValue( damage_s.ApplyFinalValue(damage_u + GetValues(StatAspect.Damage)));

    [Header("Healing")]
    public int healing_u = 0;
    public Stat healing_s = new();
    public int Healing => Owner.entityStats.HealingIncrease.ApplyFinalValue( healing_s.ApplyFinalValue(healing_u) + GetValues(StatAspect.Healing));

    [Header("Duration")]
    public int duration_u = 0;
    public Stat duration_s = new();
    public int Duration => duration_u + duration_s.Value + GetValues(StatAspect.Duration);

    [Header("Repeats")]
    public int repeats_u = 0;
    public Stat repeats_s = new();
    public int Repeats => repeats_u + repeats_s.Value + GetValues(StatAspect.Repeats);


    [Header("Card Target")]
    public TargetingData targetingData = new();

    [Header("Card Effect Target")]
    public List<gameplayRef> GameplayReferences { get; internal set; } = new();

    public List<CardType> EffectTargetTypes = new();
    public CardScript TargetCard;

    int GetValues(StatAspect aspect)
    {
        int value = 0;

        value =
            Owner.entityStats.GetStatValue(cardType, aspect)
            + Owner.entityStats.GetStatValue(cardClass, aspect);

        foreach (CardIdentity ce in cardIdentities)
        {
            value += Owner.entityStats.GetStatValue(ce, aspect);
        }

        return value;
    }

    public Action<EntityScript, CardData> CardDescription =
        (user, data) => Debug.Log($"Not defined Description of {data.cardName}");

    public Action<EntityScript, EntityScript, CardData> CardEffect =
        (user, target, data) => Debug.Log($"Not defined Effect used by {user} at {target} by Card {data.cardName}");

    [Header("AI")]
    public CardAiBias CardAiBias = new();

    public CardData Clone()
    {
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
            cardIdentities = cardIdentities != null ? new List<CardIdentity>(cardIdentities) : new List<CardIdentity>(),

            // Werte (u = base, s = eigene Stat-Container pro Instanz)
            cost_u = cost_u,
            power_u = power_u,
            damage_u = damage_u,
            healing_u = healing_u,
            duration_u = duration_u,
            repeats_u = repeats_u,

            cost_s = new Stat(),
            power_s = new Stat(),
            damage_s = new Stat(),
            healing_s = new Stat(),

            duration_s = new Stat(),
            repeats_s = new Stat(),

            // Targeting-Flags (keine Ziel-Referenzen übernehmen)
            targetingData = targetingData,

            // Delegates (zeigen auf dieselben Methoden – gewünscht)
            CardDescription = CardDescription,
            CardEffect = CardEffect,

            // AI
            CardAiBias = CardAiBias,
        };
    }

    public void ActivateCard(List<EntityScript> targetEntity, GameObject obj)
    {
        GenerateTriggerFromCardData();

        foreach (EntityScript target in targetEntity)
        {
            CardEffect?.Invoke(Owner, target, this);
        }
        HandManager.Instance.DiscardCard(obj);
    }
    private void GenerateTriggerFromCardData()
    {
        // Lokaler Helfer: versucht einen gameplayRef anhand eines Namens zu feuern
        void TryTrigger(string name, int userId)
        {
            if (string.IsNullOrEmpty(name)) return;

            if (Enum.TryParse<gameplayRef>(name, out var gref))
            {
                GameEvents.TriggerRefEvent(new TriggerRef
                {
                    References = new() { gref },
                    UserId = userId
                });
            }
            else
            {
                Debug.Log($"[CardData] gameplayRef '{name}' not found. Skipping trigger.");
            }
        }

        // UserId vom Kartenbesitzer (falls vorhanden)
        int uid = Owner != null ? Owner.GetInstanceID() : 0;

        // Klasse, Typ und Identities probieren
        TryTrigger(cardClass.ToString(), uid);
        TryTrigger(cardType.ToString(), uid);

        if (cardIdentities != null)
        {
            foreach (var id in cardIdentities)
                TryTrigger(id.ToString(), uid);
        }
    }
}
public enum CardTargetType
{
    Entity,
    CombatTile,
}
public enum CardTargetAffiliation
{
    None,
    All,
    Self,
    Ally,
    AllyNeutral,
    Enemy,
    EnemyNeutral,
    AllyEnemy,
}
public enum CardTargetSelection
{
    Single,
    Radius,
    Ring,
    LineFree,
    LineSelf,
    Cone,
    Select,
    All
}

[System.Serializable]
public class TargetingData
{
    public CardTargetType CardTargetType;
    public CardTargetAffiliation CardTargetAffiliation;
    public CardTargetSelection SelectionType = CardTargetSelection.Single;
    public int range = 1; 
    public int area = 1; 
}

public class CardAiBias
{
    // General Intention of the Card
    public Intention Intention = Intention.None;

    // Unique ID for lookup
    public gameplayRef triggerCondition = gameplayRef.None;

    // Override for Friendly Fire Avoidance
    public CardTargetAffiliation AffiliationBiasOverride = CardTargetAffiliation.None;

    // Additional Throughput
    public int throughputBase = 0;
    // Divider for Throughput to scale down high values
    public int throughputScale = 1;
    // Additional Throughput per gameplayRef
    public Dictionary<gameplayRef, int> throughputBias = new();

    public int cooldown = 1;


    public int ThroughputOverride(List<EntityScript> target)
    {
        int OverrideValue = throughputScale;
        foreach (KeyValuePair<gameplayRef, int> gRef in throughputBias)
        {
            foreach (EntityScript t in target)
            {
                if (t.HasReference(gRef.Key))
                {
                    OverrideValue += gRef.Value;
                }
            }
        }
        return OverrideValue;
    }
}


public enum Intention
{
    None,
    Damage,
    Block,
    Heal,
    Buff,
    Debuff,
    BuffDebuff,
    Summon,
    Other,
}

// If Updated needs to update GameplayReference as well
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
public enum CardIdentity
{
    None,
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
    Occult,
    Ranged,
    Melee
}
// If Updated needs to update GameplayReference as well
public enum CardClass
{

    Spearman,
    Assassin,
    Mystic,
    Physician,
    Neutral,
    
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
