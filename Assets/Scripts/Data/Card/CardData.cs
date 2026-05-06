using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace facingfate
{
    [System.Serializable]
    public class CardData
    {
        [Header("Meta")]
        public string cardID = "MissingID";
        public string cardName = string.Empty;
        public EntityScript Owner = null;

        [Header("Audio")]
        // Wwise event to post when this card's effect fires. Leave empty = silent.
        // Example: "Play_CardEffect"
        public string playSfxEvent;

        // Wwise switches to set before posting playSfxEvent.
        // Each entry: group = Switch Group name in Wwise, value = Switch name in Wwise.
        // Example: new WwiseSwitchEntry { group = "CardSoundIdentity",   value = "Physical" }
        //          new WwiseSwitchEntry { group = "CardSoundDamageType", value = "Melee"    }
        // Leave list empty to post without any switch changes.
        public List<WwiseSwitchEntry> soundSwitches = new();

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
        public Stat cost_s_Flat = new();
        public Stat cost_s_Increase = new();
        public Stat cost_s_Multiplier = new();
        public int Cost
        {
            get
            {
                float baseValue = Resolve(costFunc, cost_u);

                float flatBonus = Owner.entityStats.CardCostModifier_Flat.Value(Owner, cardData: this) + cost_s_Flat.Value(Owner, this);

                float increaseBonus = cost_s_Increase.Value (Owner, this) + Owner.entityStats.CardCostModifier_Increase.Value();

                float result = (baseValue + flatBonus) * (1f + (increaseBonus / 100f));

                List<float> oMult = Owner.entityStats.CardCostModifier_Multiplier.GetAllMultiplierValues(Owner, this);
                List <float> cMult = cost_s_Multiplier.GetAllMultiplierValues(Owner, this);

                List<float> multipliers = cMult.Concat(oMult).ToList();

                foreach (var mult in multipliers)
                {
                    result *= mult;
                }

                return result < 0 ? 0 : Mathf.RoundToInt(result);
            }
        }

        [Header("Power")]
        public Func<CardData, int> powerFunc;
        public int power_u = 0;
        public Stat power_s_Flat = new();
        public Stat power_s_Increase = new();
        public Stat power_s_Multiplier = new();
        public int Power
        {
            get
            {
                float baseValue = Resolve(powerFunc, power_u);

                float flatBonus = Owner.entityStats.PowerModifier_Flat.Value(Owner, cardData: this) + power_s_Flat.Value( Owner, this);

                float increaseBonus = power_s_Increase.Value(Owner, this) + Owner.entityStats.PowerModifier_Increase.Value();

                float result = (baseValue + flatBonus) * (1f + (increaseBonus / 100f));

                List<float> oMult = Owner.entityStats.PowerModifier_Multiplier.GetAllMultiplierValues(Owner, this);
                List<float> cMult = power_s_Multiplier.GetAllMultiplierValues(Owner, this);

                List<float> multipliers = cMult.Concat(oMult).ToList();

                foreach (var mult in multipliers)
                {
                    result *= mult;
                }

                return result < 0 ? 0 : Mathf.RoundToInt(result);
            }
        }

        [Header("Damage")]
        public Func<CardData, int> damageFunc;
        public int damage_u = 0;
        public Stat damage_s_Flat = new();
        public Stat damage_s_Increase = new();
        public Stat damage_s_Multiplier = new();
        public int Damage
        {
            get
            {
                float baseValue = Resolve(damageFunc, damage_u);

                float flatBonus = Owner.entityStats.DamageOutModifier_Flat.Value(Owner, cardData: this) + damage_s_Flat.Value(Owner, this);

                float increaseBonus = damage_s_Increase.Value(Owner, this) + Owner.entityStats.DamageOutModifier_Increase.Value();

                float result = (baseValue + flatBonus) * (1f + (increaseBonus / 100f));

                List<float> oMult = Owner.entityStats.DamageOutModifier_Multiplier.GetAllMultiplierValues(Owner, this);
                List<float> cMult = damage_s_Multiplier.GetAllMultiplierValues(Owner, this);

                List<float> multipliers = cMult.Concat(oMult).ToList();

                foreach (var mult in multipliers)
                {
                    result *= mult;
                }

                return result < 0 ? 0 : Mathf.RoundToInt(result);
            }
        }

        [Header("Healing")]
        public Func<CardData, int> healingFunc;
        public int healing_u = 0;
        public Stat healing_s_Flat = new();
        public Stat healing_s_Increase = new();
        public Stat healing_s_Multiplier = new();
        public int Healing
        {
            get
            {
                float baseValue = Resolve(healingFunc, healing_u);

                float flatBonus = Owner.entityStats.HealingOutModifier_Flat.Value(Owner, cardData: this) + healing_s_Flat.Value(Owner, this);

                float increaseBonus = healing_s_Increase.Value(Owner, this) + Owner.entityStats.HealingOutModifier_Increase.Value();

                float result = (baseValue + flatBonus) * (1f + (increaseBonus / 100f));

                List<float> oMult = Owner.entityStats.HealingOutModifier_Multiplier.GetAllMultiplierValues(Owner, this);
                List<float> cMult = healing_s_Multiplier.GetAllMultiplierValues(Owner, this);

                List<float> multipliers = cMult.Concat(oMult).ToList();

                foreach (var mult in multipliers)
                {
                    result *= mult;
                }

                return result < 0 ? 0 : Mathf.RoundToInt(result);
            }
        }

        [Header("Secondary Damage")]
        public Func<CardData, int> secondaryDamageFunc;
        public int secondaryDamage_u = 0;
        public int SecondaryDamage
        {
            get
            {
                float baseValue = Resolve(secondaryDamageFunc, secondaryDamage_u);

                float flatBonus = Owner.entityStats.DamageOutModifier_Flat.Value(Owner, cardData: this) + damage_s_Flat.Value(Owner, this);

                float increaseBonus = damage_s_Increase.Value(Owner, this) + Owner.entityStats.DamageOutModifier_Increase.Value();

                float result = (baseValue + flatBonus) * (1f + (increaseBonus / 100f));

                List<float> oMult = Owner.entityStats.DamageOutModifier_Multiplier.GetAllMultiplierValues(Owner, this);
                List<float> cMult = damage_s_Multiplier.GetAllMultiplierValues(Owner, this);

                List<float> multipliers = cMult.Concat(oMult).ToList();

                foreach (var mult in multipliers)
                {
                    result *= mult;
                }

                return result < 0 ? 0 : Mathf.RoundToInt(result);
            }
        }

        [Header("Secondary Healing")]
        public Func<CardData, int> secondaryHealingFunc;
        public int secondaryHealing_u = 0;
        public int SecondaryHealing
        {
            get
            {
                float baseValue = Resolve(secondaryHealingFunc, secondaryHealing_u);

                float flatBonus = Owner.entityStats.HealingOutModifier_Flat.Value(Owner, cardData: this) + healing_s_Flat.Value(Owner, this);

                float increaseBonus = healing_s_Increase.Value(Owner, this) + Owner.entityStats.HealingOutModifier_Increase.Value();

                float result = (baseValue + flatBonus) * (1f + (increaseBonus / 100f));

                List<float> oMult = Owner.entityStats.HealingOutModifier_Multiplier.GetAllMultiplierValues(Owner, this);
                List<float> cMult = healing_s_Multiplier.GetAllMultiplierValues(Owner, this);

                List<float> multipliers = cMult.Concat(oMult).ToList();

                foreach (var mult in multipliers)
                {
                    result *= mult;
                }

                return result < 0 ? 0 : Mathf.RoundToInt(result);
            }
        }

        [Header("Duration")]
        public Func<CardData, int> durationFunc;
        public int duration_u = 99999;
        public Stat duration_s_Flat = new();
        public Stat duration_s_Increase = new();
        public Stat duration_s_Multiplier = new();
        public int Duration
        {
            get
            {
                float baseValue = Resolve(durationFunc, duration_u);

                float flatBonus = Owner.entityStats.DurationModifier_Flat.Value(Owner, cardData: this) + duration_s_Flat.Value(Owner, this);

                float increaseBonus = duration_s_Increase.Value(Owner, this) + Owner.entityStats.DurationModifier_Increase.Value();

                float result = (baseValue + flatBonus) * (1f + (increaseBonus / 100f));

                List<float> oMult = Owner.entityStats.DurationModifier_Multiplier.GetAllMultiplierValues(Owner, this);
                List<float> cMult = duration_s_Multiplier.GetAllMultiplierValues(Owner, this);

                List<float> multipliers = cMult.Concat(oMult).ToList();

                foreach (var mult in multipliers)
                {
                    result *= mult;
                }

                return result < 0 ? 0 : Mathf.RoundToInt(result);
            }
        }

        [Header("Repeats")]
        public Func<CardData, int> repeatsFunc;
        public int repeats_u = 1;
        public Stat additionalRepeats = new();
        public int Repeats
        {
            get
            {
                float baseValue = Resolve(repeatsFunc, repeats_u);
                float flatBonus = additionalRepeats.Value(Owner, this);

                int result = Mathf.RoundToInt(baseValue + flatBonus);


                return result < 0 ? 0 : result;
            }
        }

        [Header("Area of Effect")]
        public Func<CardData, float> rangeFunc;
        public float range_u = 1f;
        public Stat range_s_Flat = new();
        public Stat range_s_Increase = new();
        public Stat range_s_Multiplier = new();
        public float Range
        {
            get
            {
                float baseValue = Resolve(rangeFunc, range_u);

                float flatBonus = Owner.entityStats.RangeModifier_Flat.Value(Owner, cardData: this) + range_s_Flat.Value(Owner, this);

                float increaseBonus = range_s_Increase.Value(Owner, this) + Owner.entityStats.RangeModifier_Increase.Value();

                float result = (baseValue + flatBonus) * (1f + (increaseBonus / 100f));

                List<float> oMult = Owner.entityStats.RangeModifier_Multiplier.GetAllMultiplierValues(Owner, this);
                List<float> cMult = range_s_Multiplier.GetAllMultiplierValues(Owner, this);

                List<float> multipliers = cMult.Concat(oMult).ToList();

                foreach (var mult in multipliers)
                {
                    result *= mult;
                }

                return result < 0 ? 0 : result;
            }
        }

        public Func<CardData, float> areaFunc;
        public float area_u = 1f;
        public Stat area_s_Flat = new();
        public Stat area_s_Increase = new();
        public Stat area_s_Multiplier = new();
        public float Area
        {
            get
            {
                float baseValue = Resolve(areaFunc, area_u);

                float flatBonus = Owner.entityStats.AreaModifier_Flat.Value(Owner, cardData: this) + area_s_Flat.Value(Owner, this);

                float increaseBonus = area_s_Increase.Value(Owner, this) + Owner.entityStats.AreaModifier_Increase.Value();

                float result = (baseValue + flatBonus) * (1f + (increaseBonus / 100f));

                List<float> oMult = Owner.entityStats.AreaModifier_Multiplier.GetAllMultiplierValues(Owner, this);
                List<float> cMult = area_s_Multiplier.GetAllMultiplierValues(Owner, this);

                List<float> multipliers = cMult.Concat(oMult).ToList();

                foreach (var mult in multipliers)
                {
                    result *= mult;
                }

                return result < 0 ? 0 : result;
            }
        }

        public Func<CardData, float> radiusFunc;
        public float radius_u = 1f;
        public Stat radius_s_Flat = new();
        public Stat radius_s_Increase = new();
        public Stat radius_s_Multiplier = new();
        public float Radius
        {
            get
            {
                float baseValue = Resolve(radiusFunc, radius_u);

                float flatBonus = Owner.entityStats.RadiusModifier_Flat.Value(Owner, cardData: this) + radius_s_Flat.Value(Owner, this);

                float increaseBonus = radius_s_Increase.Value(Owner, this) + Owner.entityStats.RadiusModifier_Increase.Value();

                float result = (baseValue + flatBonus) * (1f + (increaseBonus / 100f));

                List<float> oMult = Owner.entityStats.RadiusModifier_Multiplier.GetAllMultiplierValues(Owner, this);
                List<float> cMult = radius_s_Multiplier.GetAllMultiplierValues(Owner, this);

                List<float> multipliers = cMult.Concat(oMult).ToList();

                foreach (var mult in multipliers)
                {
                    result *= mult;
                }

                return result < 0 ? 0 : result;
            }
        }

        public Func<CardData, int> maxTargetFunc;
        public int maxtarget_u = 0;
        public Stat additionalMaxTargets = new();
        public Stat maxTarget_s_Increase = new();
        public Stat maxTarget_s_Multiplier = new();
        public int MaxTarget
        {
            get
            {
                float baseValue = Resolve(maxTargetFunc, maxtarget_u);
                float flatBonus = Owner.entityStats.AdditonalMaxTargets.Value(Owner, cardData: this) + additionalMaxTargets.Value(Owner, this);

                int result = Mathf.RoundToInt(baseValue + flatBonus);

                return result < 0 ? 0 : result;
            }
        }


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


        public Action<EntityScript, CardData> cardDescriptionAction =
            (user, data) =>
            {
                Debug.Log($"Not defined Description of {data.cardName}");
            };

        [Header("Card Action Sequence")]
        public List<CardAction> cardActionSequence = new();


        [Header("AI")]
        public CardAiBias CardAiBias = new();

        private int Resolve(Func<CardData, int> func, int fallback)
        {
            return func != null ? func(this) : fallback;
        }

        private float Resolve(Func<CardData, float> func, float fallback)
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
                playSfxEvent  = playSfxEvent,
                soundSwitches = soundSwitches != null ? new List<WwiseSwitchEntry>(soundSwitches) : new(),

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
                secondaryDamage_u = secondaryDamage_u,
                secondaryDamageFunc = secondaryDamageFunc,
                secondaryHealing_u = secondaryHealing_u,
                secondaryHealingFunc = secondaryHealingFunc,

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

                // Card Stats - Cost
                cost_s_Flat = new Stat(),
                cost_s_Increase = new Stat(),
                cost_s_Multiplier = new Stat(),

                // Card Stats - Power
                power_s_Flat = new Stat(),
                power_s_Increase = new Stat(),
                power_s_Multiplier = new Stat(),

                // Card Stats - Damage
                damage_s_Flat = new Stat(),
                damage_s_Increase = new Stat(),
                damage_s_Multiplier = new Stat(),

                // Card Stats - Healing
                healing_s_Flat = new Stat(),
                healing_s_Increase = new Stat(),
                healing_s_Multiplier = new Stat(),

                // Card Stats - Duration
                duration_s_Flat = new Stat(),
                duration_s_Increase = new Stat(),
                duration_s_Multiplier = new Stat(),

                // Card Stats - Repeats
                additionalRepeats = new Stat(),

                // Card Stats - Range
                range_s_Flat = new Stat(),
                range_s_Increase = new Stat(),
                range_s_Multiplier = new Stat(),

                // Card Stats - Area
                area_s_Flat = new Stat(),
                area_s_Increase = new Stat(),
                area_s_Multiplier = new Stat(),

                // Card Stats - Radius
                radius_s_Flat = new Stat(),
                radius_s_Increase = new Stat(),
                radius_s_Multiplier = new Stat(),

                // Card Stats - MaxTarget
                additionalMaxTargets = new Stat(),


                // Targeting-Flags (keine Ziel-Referenzen übernehmen)
                targetingData = targetingData,

                // Delegates (zeigen auf dieselben Methoden – gewünscht)
                cardDescriptionAction = cardDescriptionAction,

                // Card Action Sequence
                cardActionSequence = cardActionSequence != null ? new List<CardAction>(cardActionSequence) : new List<CardAction>(),

                // AI
                CardAiBias = CardAiBias,
            };
        }
        public void ActivateCardEffect(TargetingModeData targetingModeData, GameObject cardObj)
        {
            CardData cardData = cardObj.GetComponent<CardScript>().cardData;

            // Check if the owner has enough stamina to pay the card cost
            if (cardData.Owner.entityStats.CurrentStamina < cardData.Cost)
            {
                Debug.LogWarning($"Not enough stamina to play {cardData.cardName}. Required: {cardData.Cost}, Available: {cardData.Owner.entityStats.CurrentStamina}");
                return;
            }

            // Deduct the card cost from the owner's stamina
            cardData.Owner.entityStats.CurrentStamina -= cardData.Cost;
            HandUI.RefreshHandLocks(cardData.Owner);

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
        public int DamageOverrideValue = 0;
        public int HealingOverrideValue = 0;
        public int PowerOverrideValue = 0;
        public int ConditionalOverrideValue = 0;

        // Divider for Throughput to scale down high values
        public float DamageOverrideMultipier = 1;
        public float HealingOverrideMultipier = 1;
        public float PowerOverrideMultipier = 1;
        public float ConditionalOverrideMultipier = 1;


        // Condition
        public bool? TargetStaticBool;
        public Func<EntityScript, CardData, bool> TargetDynamicConditionFunc;

        private bool TargetCondition(EntityScript entityScript = null, CardData cardData = null) => TargetDynamicConditionFunc != null
            ? TargetDynamicConditionFunc.Invoke(entityScript, cardData)
            : TargetStaticBool ?? false;

        // Cooldown in turns before this bias can be applied again (for dynamic biases that change after use)
        public int cooldown = 1;

        public int ThroughputOverride(NpcAiBias aiBias, CardData cardData, List<EntityScript> targets)
        {
            // Basic guard checks
            if (cardData == null || cardData.Owner == null || targets == null || targets.Count == 0)
                return 0;

            // Check if the user condition is met
            if (!triggerConditionUser(cardData.Owner))
            {
                return 0;
            }

            // If owner has a taunt target, ensure it's included in the targets list
            var taunt = cardData.Owner.entityStats?.tauntTarget;
            if (taunt != null && !targets.Contains(taunt))
            {
                return 0;
            }

            // Compute a base throughput for the card and apply card-level biases
            float baseTotal = ComputeCardThroughput(cardData);
            baseTotal = ApplyCardBiases(baseTotal, aiBias, cardData);

            float validScore = 0f;
            float invalidScore = 0f;

            foreach (var t in targets)
            {
                if (t == null) continue;
                if (!triggerConditionTargets(t)) continue;

                // Check affiliation validity and apply biases or penalties accordingly
                bool isValid = IsValidAffiliationTarget(t, cardData.Owner, AffiliationBiasOverride);

                if (isValid)
                {
                    validScore += ApplyTargetBiases(baseTotal, aiBias, t);
                }
                else
                {
                    // apply a smaller penalty per invalid target
                    invalidScore += baseTotal * 0.5f;
                }

                // Apply conditional overrides per target (additive, scaled by multiplier)
                if (TargetCondition(t, cardData))
                {
                    validScore += ConditionalOverrideValue * ConditionalOverrideMultipier;
                }
            }

            float finalScore = validScore - invalidScore;
            return Mathf.RoundToInt(finalScore);
        }
        private float ComputeCardThroughput(CardData card)
        {
            float total = 0;

            total += (card.Damage + DamageOverrideValue) *  DamageOverrideMultipier;
            total += (card.Healing + HealingOverrideValue) *  HealingOverrideMultipier;
            total += (card.Power + PowerOverrideValue) *  PowerOverrideMultipier;

            return total;
        }
        private float ApplyCardBiases(float total, NpcAiBias bias, CardData card)
        {
            if (bias == null) return total;

            // Apply bias values as direct multipliers (e.g. 1.5 -> 50% increase)
            foreach (var reference in bias.cardReferenceBias)
            {
                if (reference.Value == 0) continue;
                if (card.GameplayReferences != null && card.GameplayReferences.Contains(reference.Key))
                {
                    total *= reference.Value;
                }
            }

            foreach (var identity in bias.identityBias)
            {
                if (identity.Value == 0) continue;
                if (card.cardIdentities != null && card.cardIdentities.Contains(identity.Key))
                {
                    total *= identity.Value;
                }
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
            if (bias.targetReferenceBias == null || bias.targetReferenceBias.Count == 0) return total;

            // Combine target-level biases into a multiplier (direct multiplier values)
            float multiplier = 1f;
            foreach (var tb in bias.targetReferenceBias)
            {
                if (tb.Value == 0) continue;
                var has = target.HasReference(tb.Key);
                if (has.found)
                {
                    multiplier *= tb.Value;
                }
            }

            return total * multiplier;
        }
    }
    [System.Serializable]
    public class CardTargetingData
    {
        public bool TargetingUsesVision = false;
        public bool EffectUsesVision = false;
        public CardTargetType CardTargetType;
        public CardTargetAffiliation CardTargetAffiliation;
        public CardTargetingMode cardTargetingMode;
    }

    public enum CardTargetType
    {
        Entity,
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
        Sphere,
        Ring,
        RingSelf,
        LineFree,
        LineSelf,
        Cone,
        Select,
        SelectionUnique,
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
    [System.Serializable]
    public struct WwiseSwitchEntry
    {
        public string group; // Switch Group name in Wwise, e.g. "CardSoundIdentity"
        public string value; // Switch name in Wwise,       e.g. "Physical"
    }

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