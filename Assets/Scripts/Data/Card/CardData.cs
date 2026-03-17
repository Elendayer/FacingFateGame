using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using static facingfate.GameEvents;

namespace facingfate
{
    [System.Serializable]
    public class CardData
    {
        [Header("Meta")]
        public string cardID = "MissingID";
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
        public Func<CardData, int> costFunc;
        public int cost_u = 0;
        public Stat cost_s = new();
        public int Cost =>
            Owner.entityStats.CardCostModifier.ApplyFinalValue(
                cost_s.ApplyFinalValue(Resolve(costFunc, cost_u), Owner, this),
                Owner,
                this
            );

        [Header("Power")]
        public Func<CardData, int> powerFunc;
        public int power_u = 0;
        public Stat power_s = new();
        public int Power =>
            Owner.entityStats.PowerModifier.ApplyFinalValue(
                power_s.ApplyFinalValue(Resolve(powerFunc, power_u), Owner, this),
                Owner,
                this
            );

        [Header("Damage")]
        public Func<CardData, int> damageFunc;
        public int damage_u = 0;
        public Stat damage_s = new();
        public int Damage =>
            Owner.entityStats.DamageOutModifier.ApplyFinalValue(
                damage_s.ApplyFinalValue(Resolve(damageFunc, damage_u), Owner, this),
                Owner,
                this
            );

        [Header("Healing")]
        public Func<CardData, int> healingFunc;
        public int healing_u = 0;
        public Stat healing_s = new();
        public int Healing =>
            Owner.entityStats.HealingOutModifier.ApplyFinalValue(
                healing_s.ApplyFinalValue(Resolve(healingFunc, healing_u), Owner, this),
                Owner,
                this
            );

        [Header("Duration")]
        public Func<CardData, int> durationFunc;
        public int duration_u = 99999;
        public Stat duration_s = new();
        public int Duration =>
            Owner.entityStats.DurationModifier.ApplyFinalValue(
                duration_s.ApplyFinalValue(Resolve(durationFunc, duration_u), Owner, this),
                Owner,
                this
            );

        [Header("Repeats")]
        public Func<CardData, int> repeatsFunc;
        public int repeats_u = 1;
        public Stat repeats_s = new();
        public int Repeats =>
            Resolve(repeatsFunc, repeats_u) + repeats_s.Value(Owner, this);

        [Header("Area of Effect")]
        public Func<CardData, int> rangeFunc;
        public int range_u = 1;
        public Stat range_s = new();
        public int Range =>
            Owner.entityStats.RangeModifier.ApplyFinalValue(
                range_s.ApplyFinalValue(Resolve(rangeFunc, range_u), Owner, this),
                Owner,
                this
            );

        public Func<CardData, int> areaFunc;
        public int area_u = 1;
        public Stat area_s = new();
        public int Area =>
            Owner.entityStats.AreaModifier.ApplyFinalValue(
                area_s.ApplyFinalValue(Resolve(areaFunc, area_u), Owner, this),
                Owner,
                this
            );

        public Func<CardData, int> radiusFunc;
        public int radius_u = 1;
        public Stat radius_s = new();
        public int Radius =>
            Owner.entityStats.RadiusModifier.ApplyFinalValue(
                radius_s.ApplyFinalValue(Resolve(radiusFunc, radius_u), Owner, this),
                Owner,
                this
            );

        public Func<CardData, int> maxTargetFunc;
        public int maxtarget_u = 0;
        public Stat maxTarget_s = new();
        public int MaxTarget =>
            Owner.entityStats.MaxTargetModifier.ApplyFinalValue(
                maxTarget_s.ApplyFinalValue(Resolve(maxTargetFunc, maxtarget_u), Owner, this),
                Owner,
                this
            );

        [Header("Charges")]
        public Func<CardData, int> chargesFunc;
        public int charges_u = 0;
        public int Charges => Resolve(chargesFunc, charges_u);

        [Header("StatusEffects")]
        public bool isFrozen = false;


        [Header("Card Target")]
        public CardTargetingData targetingData = new();

        [Header("Card Effect Target")]
        public List<GameplayRef> GameplayReferences { get; internal set; } = new();

        public List<CardType> EffectTargetTypes = new();
        public CardScript TargetCard;


        public Action<EntityScript, CardData> CardDescription =
            (user, data) => 
            { 
                Debug.Log($"Not defined Description of {data.cardName}");
            };

        public Action<EntityScript, EntityScript, CardData> CardEffect =
            (user, target, data) =>
            {
                Debug.Log($"Not defined Effect used by {user} at {target} by Card {data.cardName}");
            };

        public Action<EntityScript, Vector3Int, CardData> CardEffectGround =
            (user, target, data) => 
            {
                // Debug.Log($"Not defined Ground Effect used by {user} at {target} by Card {data.cardName}");
            };
        public Action<TargetingModeData> CardVfx =
            (targetData) =>
            {
                AssetManager.Instance.CreateVFXAttachedToGameObjects("LightningStrike", targetData.targetedEntities);           
            };

        [Header("AI")]
        public CardAiBias CardAiBias = new();

        private int Resolve(Func<CardData, int> func, int fallback)
        {
            return func != null ? func(this) : fallback;
        }

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
                costFunc = costFunc,
                power_u = power_u,
                powerFunc = powerFunc,
                damage_u = damage_u,
                damageFunc = damageFunc,
                healing_u = healing_u,
                healingFunc = healingFunc,

                duration_u = duration_u,
                durationFunc = durationFunc,
                repeats_u = repeats_u,
                repeatsFunc = repeatsFunc,
                range_u = range_u,
                rangeFunc = rangeFunc,
                area_u = area_u,
                areaFunc = areaFunc,
                radius_u = radius_u,
                radiusFunc = radiusFunc,
                maxtarget_u = maxtarget_u,
                maxTargetFunc = maxTargetFunc,

                charges_u = charges_u,
                chargesFunc = chargesFunc,

                cost_s = new Stat(),
                power_s = new Stat(),
                damage_s = new Stat(),
                healing_s = new Stat(),

                duration_s = new Stat(),
                repeats_s = new Stat(),
                range_s = new Stat(),
                area_s = new Stat(),
                radius_s = new Stat(),
                maxTarget_s = new Stat(),


                // Targeting-Flags (keine Ziel-Referenzen übernehmen)
                targetingData = targetingData,

                // Delegates (zeigen auf dieselben Methoden – gewünscht)
                CardDescription = CardDescription,
                CardEffect = CardEffect,
                CardEffectGround = CardEffectGround,
                CardVfx = CardVfx,

                // AI
                CardAiBias = CardAiBias,
            };
        }
        public void ActivateCardEffect(TargetingModeData targetingModeData, GameObject cardObj)
        {
            CardData cardData = cardObj.GetComponent<CardScript>().cardData;

            // Enqueue the card execution in the action queue
            ActionQueueUtility.EnqueueCardExecution(cardData.Owner, cardData, targetingModeData, cardObj);
        }
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

        public int ThroughputOverride(NpcAiBias aiBias, CardData cardData, List<EntityScript> targets)
        {
            if (!triggerConditionUser(cardData.Owner))
            {
                Debug.Log("user condition was false");
                return 0;
            }

            if (cardData.Owner.entityStats.tauntTarget != null)
            {
                if (!targets.Contains(cardData.Owner.entityStats.tauntTarget))
                {
                    Debug.Log("Taunted Target wasn't targeted");
                    return 0;
                }
            }

            float baseTotal = ComputeBaseTotal(cardData);
            baseTotal = ApplyCardBiases(baseTotal, aiBias, cardData);

            float validScore = 0f;
            float invalidScore = 0f;

            foreach (var t in targets)
            {
                if (!triggerConditionTargets(t)) { continue; }
                ;
                bool isValid = IsValidAffiliationTarget(t, cardData.Owner, AffiliationBiasOverride);

                if (isValid)
                {
                    validScore += ApplyTargetBiases(baseTotal, aiBias, t);
                }
                else
                {
                    invalidScore += baseTotal * 0.5f;  // penalty
                }
            }

            float finalScore = validScore - invalidScore;
            return (int)finalScore;
        }
        private float ComputeBaseTotal(CardData card)
        {
            float total = 0;

            total += (card.Damage + DamageOverride) * Math.Max(1, DamageBiasMultipier);
            total += (card.Healing + HealingOverride) * Math.Max(1, HealingBiasMultipier);
            total += (card.Power + PowerOverride) * Math.Max(1, PowerBiasMultipier);

            return total * 100f;
        }
        private float ApplyCardBiases(float total, NpcAiBias bias, CardData card)
        {
            if (bias == null) return total;

            foreach (var reference in bias.cardReferenceBias)
            {
                if (card.GameplayReferences.Contains(reference.Key))
                    total *= Math.Max(1, reference.Value);
            }

            foreach (var identity in bias.identityBias)
            {
                if (card.cardIdentities.Contains(identity.Key))
                    total *= Math.Max(1, identity.Value);
            }

            return total;
        }
        private bool IsValidAffiliationTarget(EntityScript target, EntityScript owner, CardTargetAffiliation mode)
        {
            return mode switch
            {
                CardTargetAffiliation.Self => target == owner,
                CardTargetAffiliation.Ally => target.entityAffiliation == owner.entityAffiliation && target != owner,
                CardTargetAffiliation.Enemy => target.entityAffiliation != owner.entityAffiliation,
                _ => true
            };
        }
        private float ApplyTargetBiases(float total, NpcAiBias bias, EntityScript target)
        {
            if (bias == null) return total;
            if (bias.targetReferenceBias.Count == 0) return total;

            float score = 0;

            foreach (var tb in bias.targetReferenceBias)
            {
                if (target.HasReference(tb.Key).found)
                {
                    score += total * Math.Max(1, tb.Value);
                }
                else
                {
                    score += total;
                }
            }
            return score;
        }
    }
    [System.Serializable]
    public class CardTargetingData
    {
        public bool TargetingUsesVision;
        public bool EffectUsesVision;
        public CardTargetType CardTargetType;
        public CardTargetAffiliation CardTargetAffiliation;
        public CardTargetingMode cardTargetingMode;
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
    public enum CardTargetingMode
    {
        Single,
        Radius,
        Ring,
        LineFree,
        LineSelf,
        Cone,
        Select,
        All,
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

        // Elements
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

        //Attack Types
        Ranged,
        Melee,
        Magic,
        Summon,
        Healing,
        Buff,
        Debuff,

        //Alchemy 
        Alchemical,
        Potion,
        Brew,
        Tonic,
        Venom,

        Mechanical,
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
}