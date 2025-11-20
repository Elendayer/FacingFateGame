using System;
using System.Collections.Generic;
using System.Linq;
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
    public int Cost => Owner.entityStats.PowerModifier.ApplyFinalValue(damage_s.ApplyFinalValue(cost_u));

    [Header("Power")]
    public int power_u = 0;
    public Stat power_s = new();
    public int Power =>  Owner.entityStats.PowerModifier.ApplyFinalValue( damage_s.ApplyFinalValue(power_u));

    [Header("Damage")]
    public int damage_u = 0;
    public Stat damage_s = new();
    public int Damage => Owner.entityStats.DamageOutModifier.ApplyFinalValue( damage_s.ApplyFinalValue(damage_u));

    [Header("Healing")]
    public int healing_u = 0;
    public Stat healing_s = new();
    public int Healing => Owner.entityStats.HealingOutModifier.ApplyFinalValue( healing_s.ApplyFinalValue(healing_u));

    [Header("Duration")]
    public int duration_u = 0;
    public Stat duration_s = new();
    public int Duration => Owner.entityStats.HealingOutModifier.ApplyFinalValue(duration_s.ApplyFinalValue(duration_u));

    [Header("Repeats")]
    public int repeats_u = 0;
    public Stat repeats_s = new();
    public int Repeats => repeats_u + repeats_s.Value;

    [Header("Range & Area")]
    public int range_u = 1;
    public Stat range_s = new();
    public int Range => Owner.entityStats.RangeModifier.ApplyFinalValue(range_s.ApplyFinalValue(range_u));
    public int area_u = 1;

    public Stat area_s = new();
    public int Area => Owner.entityStats.AreaModifier.ApplyFinalValue(area_s.ApplyFinalValue(area_u));

    [Header("Charges")]
    public int charges_u = 0;
    public int Charges => charges_u;

    [Header("Card Target")]
    public TargetingData targetingData = new();

    [Header("Card Effect Target")]
    public List<GameplayRef> GameplayReferences { get; internal set; } = new();

    public List<CardType> EffectTargetTypes = new();
    public CardScript TargetCard;


    public Action<EntityScript, CardData> CardDescription =
        (user, data) => Debug.Log($"Not defined Description of {data.cardName}");

    public Action<EntityScript, EntityScript, CardData> CardEffect =
        (user, target, data) => Debug.Log($"Not defined Effect used by {user} at {target} by Card {data.cardName}");

    public Action<EntityScript, Vector3Int, CardData> CardEffectGround =
        (user, target, data) => Debug.Log($"Not defined Ground Effect used by {user} at {target} by Card {data.cardName}");

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
            range_u = range_u,
            area_u = area_u,

            charges_u = charges_u,

            cost_s = new Stat(),
            power_s = new Stat(),
            damage_s = new Stat(),
            healing_s = new Stat(), 

            duration_s = new Stat(),
            repeats_s = new Stat(),
            range_s = new Stat(),
            area_s = new Stat(),



            // Targeting-Flags (keine Ziel-Referenzen übernehmen)
            targetingData = targetingData,

            // Delegates (zeigen auf dieselben Methoden – gewünscht)
            CardDescription = CardDescription,
            CardEffect = CardEffect,
            CardEffectGround = CardEffectGround,

            // AI
            CardAiBias = CardAiBias,
        };
    }

    public void ActivateCard(List<EntityScript> targetEntity, GameObject obj)
    {
        foreach (EntityScript target in targetEntity)
        {
            CardEffect?.Invoke(Owner, target, this);
        }
        HandManager.Instance.DiscardCard(obj);
    }
    public void ActivateCard(List<Vector3Int> targetCell, GameObject obj)
    {
        foreach(Vector3Int target in targetCell)
        {        
            CardEffectGround?.Invoke(Owner, target, this);
        }
        HandManager.Instance.DiscardCard(obj);
    }
}

public enum CardTargetType
{
    Entity,
    CombatTile,
    Ground,
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
    public CardTargetSelection cardSelectionType;
}

public class CardAiBias
{
    // Unique ID for lookup
    public Func<EntityScript, bool> triggerConditionTargets = (entity) => true;
    public Func<EntityScript, bool> triggerConditionUser = (entity) => true;

    // Override for Friendly Fire Avoidance
    public CardTargetAffiliation AffiliationBiasOverride = CardTargetAffiliation.None;

    // Additional Throughput
    public int DamageOverride = 0;
    public int HealingOverride = 0;
    public int PowerOverride = 0;

    // Divider for Throughput to scale down high values
    public float DamageBiasMultipier = 1;
    public float HealingBiasMultipier = 1;
    public float PowerBiasMultipier = 1;

    public int cooldown = 1;


    public int ThroughputOverride(NpcAiBias aiBias, CardData cardData, List<EntityScript> target)
    {
        float DamageRef = cardData.Damage;
        float HealingRef = cardData.Healing;
        float PowerRef = cardData.Power;

        List<EntityScript> ValidTargets = new();
        List<EntityScript> InValidTargets = new();

        if (!triggerConditionUser(cardData.Owner))
        {
            Debug.Log("user condition was false");
            return 0;
        }

        float total = 0;

        total += (DamageRef + DamageOverride) * Math.Max(1, DamageBiasMultipier);
        total += (HealingRef + HealingOverride) * Math.Max(1, HealingBiasMultipier);
        total += (PowerRef + PowerOverride) * Math.Max(1, PowerBiasMultipier);

        total *= 100;

        // Apply GameplayRef Biases
        foreach (var reference in aiBias.cardReferenceBias)
        {
            if (cardData.GameplayReferences.Contains(reference.Key))
            {
                total = total * Math.Max(1, reference.Value);
            }
        }

        foreach (var identity in aiBias.identityBias)
        {
            if (cardData.cardIdentities.Contains(identity.Key))
            {
                total = total * Math.Max(1, identity.Value);
            }
        }

        // Apply Affiliation Biases
        foreach (var t in target)
        {
            if (!triggerConditionTargets(t)) { continue; }

            switch (AffiliationBiasOverride)
            {
                case CardTargetAffiliation.Self:
                    if (t == cardData.Owner )
                    {
                        ValidTargets.Add(t);
                    }
                    else
                    {
                        InValidTargets.Add(t);
                    }
                    break;
                    case CardTargetAffiliation.Ally:
                    if (t.entityAffiliation == cardData.Owner.entityAffiliation && t != cardData.Owner)
                    {
                        ValidTargets.Add(t);
                    }
                    else
                    {
                        InValidTargets.Add(t);
                    }
                    break;
                    case CardTargetAffiliation.Enemy:
                    if (t.entityAffiliation != cardData.Owner.entityAffiliation)
                    {
                        ValidTargets.Add(t);
                    }
                    else
                    {
                        InValidTargets.Add(t);
                    }
                    break;
                    default:
                    ValidTargets.Add(t);
                    break;
            }
        }

        float validScore = 0;
        foreach (var t in ValidTargets)
        {
            foreach(var a in aiBias.targetReferenceBias)
            {
                if (t.HasReference(a.Key).found)
                {
                    validScore = total * Math.Max(1, a.Value);
                    break;
                }
            }
            validScore += total;
        }

        float inValidScore = (total/2)*InValidTargets.Count;
        total = validScore - inValidScore;
        return (int)total;
    }
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
