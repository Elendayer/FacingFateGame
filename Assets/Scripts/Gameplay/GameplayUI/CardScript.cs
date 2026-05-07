using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace facingfate
{
    [Serializable]
    public class CardScript : MonoBehaviour
    {
        [SerializeField]

        public CardData cardData;
        public UnityEngine.UI.Image artworkRenderer;
        public GameObject cardBack;
        public GameObject cardFront;
        public TextMeshProUGUI nameText; // Optional: if you're using UI elements
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI range;
        public TextMeshProUGUI cost;

        public bool isLocked;
        public bool inPlay;

        private static readonly Regex TokenRegex = new Regex(@"\{([A-Za-z]+)(?:_(\d+))?\}", RegexOptions.Compiled);

        private static Dictionary<string, IStatResolver> _resolvers;

        public void SetupLock(bool b)
        {
            isLocked = b;

            if (isLocked)
            {
                GetComponent<DraggableCard>().enabled = false;
            }
            else
            {
                GetComponent<DraggableCard>().enabled = true;
            }
        }

        public void ApplyCardDataVisuals()
        {
            if (cardData != null)
            {
                cardData.cardDescriptionAction(cardData.Owner, cardData);
                artworkRenderer.sprite = cardData.cardArtwork;
                nameText.text = cardData.cardName;
                cost.text = $"{cardData.Cost}";
                descriptionText.text = cardData.cardDescription;

                range.text = GetRangeText(cardData);
            }

            StartCoroutine("DescriptionUpdate");
        }

        private IEnumerator DescriptionUpdate()
        {
            while (true)
            {
                // Stop if card data or owner is gone (e.g. entity destroyed on scene transition).
                if (cardData == null || cardData.Owner == null)
                    yield break;

                descriptionText.text = FormatCardDescription(cardData);
                range.text = FormatCardRange(cardData);

                yield return new WaitForSeconds(0.2f);
            }
        }
        private string FormatCardDescription(CardData d)
        {
            EnsureResolvers();

            EntityScript owner = d.Owner;

            return TokenRegex.Replace(d.cardDescription, match =>
            {
                string key = match.Groups[1].Value;

                if (!_resolvers.TryGetValue(key, out var resolver))
                    return match.Value;

                int baseValue = resolver.GetBaseValue(d);
                int currentValue = resolver.ResolveCurrent(d, owner);

                return ColorizeValue(currentValue, baseValue);
            });
        }


        private static string ColorizeValue(int value, int baseValue)
        {
            if (value > baseValue)
                return $"<color=#00FF00>{value}</color>"; // Green

            if (value < baseValue)
                return $"<color=#FF0000>{value}</color>"; // Red

            return $"<color=#000000>{value}</color>"; // Black
        }
        private string FormatCardRange(CardData d)
        {
            return GetRangeText(cardData);
        }

        public static string GetRangeText(CardData cardData)
        {
            var t = cardData.targetingData;

            List<string> parts = new();

            //  Affiliation if applicable
            if (t.CardTargetAffiliation != CardTargetAffiliation.Self)
            {
                if (t.CardTargetAffiliation == CardTargetAffiliation.Ally)
                {
                    parts.Add("Allies");
                }
                else if (t.CardTargetAffiliation == CardTargetAffiliation.Enemy)
                {
                    parts.Add("Enemies");
                }
                else
                {
                    parts.Add("All");
                }

                // if targeting uses Vision
                if (t.TargetingUsesVision)
                {
                    parts.Add("in Sight,");
                }
            }
            else
            {
                parts.Add("Self");
            }

            parts.Add($"{cardData.Range}m,");

            switch (t.cardTargetingMode)
            {
                case CardTargetingMode.Single:
                    parts.Add("Single");
                    break;

                case CardTargetingMode.Ring:
                    parts.Add($"Ring, {cardData.Radius}m by {cardData.Area}m");
                    break;
                case CardTargetingMode.RingSelf:
                    parts.Add($"Ring Self, {cardData.Radius}m by {cardData.Area}m");
                    break;
                case CardTargetingMode.Sphere:
                    parts.Add($"Radius, {cardData.Radius}m");
                    break;

                case CardTargetingMode.LineFree:
                    parts.Add($"Line Draw,{cardData.Area}m");
                    break;

                case CardTargetingMode.LineSelf:
                    parts.Add($"Line, {cardData.Area}m");
                    break;

                case CardTargetingMode.Cone:
                    parts.Add($"Cone");
                    break;

                case CardTargetingMode.Select:
                    parts.Add($"Select, {cardData.MaxTarget} Targets");
                    break;
                    case CardTargetingMode.SelectionUnique:
                        parts.Add($"Select Unique, {cardData.MaxTarget} Targets");
                    break;

                case CardTargetingMode.All:
                    parts.Add("All");
                    break;
            }

            // if effect uses Vision
            if (t.EffectUsesVision)
            {
                parts.Add("Blocked,");
            }
            else
            {
                parts.Add(",");
            }

            // Final join
            return string.Join("", parts);
        }

        public interface IStatResolver
        {
            int GetBaseValue(CardData d);
            int ResolveCurrent(CardData d, EntityScript owner);

            float GetBaseValueFloat(CardData d);
            float ResolveCurrentFloat(CardData d, EntityScript owner);
        }
        private abstract class SimpleStatResolver : IStatResolver
        {
            protected abstract int GetBase(CardData d);
            protected abstract int GetCurrent(CardData d);

            protected abstract float GetBaseFloat(CardData d);
            protected abstract float GetCurrentFloat(CardData d);

            public int GetBaseValue(CardData d)
            {
                return GetBase(d);
            }


            public int ResolveCurrent(CardData d, EntityScript owner)
            {
                return GetCurrent(d);
            }
            public float GetBaseValueFloat(CardData d)
            {
                return GetBase(d);
            }
            public float ResolveCurrentFloat(CardData d, EntityScript owner)
            {
                return GetCurrent(d);
            }
        }
        private static void EnsureResolvers()
        {
            if (_resolvers != null)
                return;

            _resolvers = new Dictionary<string, IStatResolver>(StringComparer.OrdinalIgnoreCase)
        {
            { "Power", new PowerResolver() },
            { "Damage", new DamageResolver() },
            { "Healing", new HealingResolver() },
            { "Duration", new DurationResolver() },
            { "Charges", new ChargesResolver() },
            { "Repeats", new RepeatsResolver() },
            { "Range", new RangeResolver() },
            { "Area", new AreaResolver() },
            { "Radius", new RadiusResolver() },
            { "MaxTarget", new MaxTargetResolver() }
        };
        }

        private class PowerResolver : SimpleStatResolver
        {
            protected override int GetBase(CardData d) => d.power_u;
            protected override int GetCurrent(CardData d) => d.Power;
            protected override float GetBaseFloat(CardData d) => d.power_u;
            protected override float GetCurrentFloat(CardData d) => d.Power;
        }

        private class DamageResolver : SimpleStatResolver
        {
            protected override int GetBase(CardData d) => d.damage_u;
            protected override int GetCurrent(CardData d) => d.Damage;
            protected override float GetBaseFloat(CardData d) => d.damage_u;
            protected override float GetCurrentFloat(CardData d) => d.Damage;
        }

        private class HealingResolver : SimpleStatResolver
        {
            protected override int GetBase(CardData d) => d.healing_u;
            protected override int GetCurrent(CardData d) => d.Healing;
            protected override float GetBaseFloat(CardData d) => d.healing_u;
            protected override float GetCurrentFloat(CardData d) => d.Healing;
        }

        private class DurationResolver : SimpleStatResolver
        {
            protected override int GetBase(CardData d) => d.duration_u;
            protected override int GetCurrent(CardData d) => d.Duration;
            protected override float GetBaseFloat(CardData d) => d.duration_u;
            protected override float GetCurrentFloat(CardData d) => d.Duration;
        }

        private class ChargesResolver : SimpleStatResolver
        {
            protected override int GetBase(CardData d) => d.charges_u;
            protected override int GetCurrent(CardData d) => d.Charges;
            protected override float GetBaseFloat(CardData d) => d.charges_u;
            protected override float GetCurrentFloat(CardData d) => d.Charges;
        }

        private class RepeatsResolver : SimpleStatResolver
        {
            protected override int GetBase(CardData d) => d.repeats_u;
            protected override int GetCurrent(CardData d) => d.Repeats;
            protected override float GetBaseFloat(CardData d) => d.repeats_u;
            protected override float GetCurrentFloat(CardData d) => d.Repeats;
        }

        private class RangeResolver : SimpleStatResolver
        {
            protected override int GetBase(CardData d) => (int)d.range_u;
            protected override int GetCurrent(CardData d) => (int)d.Range;
            protected override float GetBaseFloat(CardData d) => d.range_u;
            protected override float GetCurrentFloat(CardData d) => d.Range;
        }

        private class AreaResolver : SimpleStatResolver
        {
            protected override int GetBase(CardData d) => (int)d.area_u;
            protected override int GetCurrent(CardData d) => (int)d.Area;
            protected override float GetBaseFloat(CardData d) => d.area_u;
            protected override float GetCurrentFloat(CardData d) => d.Area;
        }

        private class RadiusResolver : SimpleStatResolver
        {
            protected override int GetBase(CardData d) => (int)d.radius_u;
            protected override int GetCurrent(CardData d) => (int)d.Radius;
            protected override float GetBaseFloat(CardData d) => d.radius_u;
            protected override float GetCurrentFloat(CardData d) => d.Radius;
        }

        private class MaxTargetResolver : SimpleStatResolver
        {
            protected override int GetBase(CardData d) => d.maxtarget_u;
            protected override int GetCurrent(CardData d) => d.MaxTarget;
            protected override float GetBaseFloat(CardData d) => d.maxtarget_u;
            protected override float GetCurrentFloat(CardData d) => d.MaxTarget;
        }


        public void ResetCard()
        {
            GetComponent<DraggableCard>().enabled = false;
        }

        internal void SetHidden()
        {
            cardBack.SetActive(true);
            cardFront.SetActive(false);
            GetComponent<DraggableCard>().enabled = false;
        }

        internal void SetRevealed()
        {
            cardBack.SetActive(false);
            cardFront.SetActive(true);
            GetComponent<DraggableCard>().enabled = true;
            ApplyCardDataVisuals();
            // Re-apply tutorial lock: DraggableCard.enabled = true above overrides any lock
            // set by ApplyLockToCard in HandManager.AddCard. Calling this here ensures the
            // lock is correct regardless of card draw timing relative to LockHandNextFrame.
            TutorialCombatManager.Instance?.ApplyLockToCard(gameObject);
        }
    }
}