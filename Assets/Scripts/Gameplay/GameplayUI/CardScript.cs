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
                cardData.CardDescription(cardData.Owner, cardData);
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

            return $"<color=#FFFFFF>{value}</color>"; // White
        }
        private string FormatCardRange(CardData d)
        {
            return GetRangeText(cardData);
        }

        public static string GetRangeText(CardData cardData)
        {
            var t = cardData.targetingData;

            List<string> parts = new();

            switch (t.cardTargetingMode)
            {
                case CardTargetingMode.Single:
                    parts.Add("Single Target");
                    break;

                case CardTargetingMode.Ring:
                    parts.Add($"Ring, {cardData.Radius} by {cardData.Area}");
                    break;

                case CardTargetingMode.Radius:
                    parts.Add($"Radius, {cardData.Radius}");
                    break;

                case CardTargetingMode.LineFree:
                    parts.Add($"Line Free, with maximum length {cardData.Area}");
                    break;

                case CardTargetingMode.LineSelf:
                    parts.Add($"Line from Self, {cardData.Range}");
                    break;

                case CardTargetingMode.Cone:
                    parts.Add($"Cone from Self, {cardData.Range} by {cardData.Area}");
                    break;

                case CardTargetingMode.Select:
                    parts.Add($"Select, {cardData.MaxTarget} targets");
                    break;

                case CardTargetingMode.All:
                    parts.Add("All");
                    break;
            }

            // if effect uses Vision
            if (t.EffectUsesVision)
            {
                parts.Add("Blocked by Obstacles,");
            }
            else
            {
                parts.Add(",");
            }

            //  Affiliation if applicable
            if (t.CardTargetAffiliation != CardTargetAffiliation.Self)
            {
                if (t.CardTargetAffiliation == CardTargetAffiliation.Ally)
                {
                    parts.Add("Targeting Allies");
                }
                else if (t.CardTargetAffiliation == CardTargetAffiliation.Enemy)
                {
                    parts.Add("Targeting Enemies");
                }
                else
                {
                    parts.Add("Targeting Everything");
                }

                // if targeting uses Vision
                if (t.TargetingUsesVision)
                {
                    parts.Add("in Sight,");
                }
            }
            else
            {
                parts.Add("Targets Self");
            }

            // Range (only if mode uses range)
            if (t.cardTargetingMode is not CardTargetingMode.All)
            {
                parts.Add($"within {cardData.Range} Tiles");
            }

            // Final join
            return string.Join(" ", parts);
        }

        public interface IStatResolver
        {
            int GetBaseValue(CardData d);
            int ResolveCurrent(CardData d, EntityScript owner);
        }
        private abstract class SimpleStatResolver : IStatResolver
        {
            protected abstract int GetBase(CardData d);
            protected abstract int GetCurrent(CardData d);

            public int GetBaseValue(CardData d)
            {
                return GetBase(d);
            }

            public int ResolveCurrent(CardData d, EntityScript owner)
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
        }

        private class DamageResolver : SimpleStatResolver
        {
            protected override int GetBase(CardData d) => d.damage_u;
            protected override int GetCurrent(CardData d) => d.Damage;
        }

        private class HealingResolver : SimpleStatResolver
        {
            protected override int GetBase(CardData d) => d.healing_u;
            protected override int GetCurrent(CardData d) => d.Healing;
        }

        private class DurationResolver : SimpleStatResolver
        {
            protected override int GetBase(CardData d) => d.duration_u;
            protected override int GetCurrent(CardData d) => d.Duration;
        }

        private class ChargesResolver : SimpleStatResolver
        {
            protected override int GetBase(CardData d) => d.charges_u;
            protected override int GetCurrent(CardData d) => d.Charges;
        }

        private class RepeatsResolver : SimpleStatResolver
        {
            protected override int GetBase(CardData d) => d.repeats_u;
            protected override int GetCurrent(CardData d) => d.Repeats;
        }

        private class RangeResolver : SimpleStatResolver
        {
            protected override int GetBase(CardData d) => d.range_u;
            protected override int GetCurrent(CardData d) => d.Range;
        }

        private class AreaResolver : SimpleStatResolver
        {
            protected override int GetBase(CardData d) => d.area_u;
            protected override int GetCurrent(CardData d) => d.Area;
        }

        private class RadiusResolver : SimpleStatResolver
        {
            protected override int GetBase(CardData d) => d.radius_u;
            protected override int GetCurrent(CardData d) => d.Radius;
        }

        private class MaxTargetResolver : SimpleStatResolver
        {
            protected override int GetBase(CardData d) => d.maxtarget_u;
            protected override int GetCurrent(CardData d) => d.MaxTarget;
        }


        public void ResetCard()
        {
            GetComponent<DraggableCard>().enabled = false;
        }

        internal void SetHidden()
        {
            cardBack.SetActive(true);
            GetComponent<DraggableCard>().enabled = false;
        }

        internal void SetRevealed()
        {
            cardBack.SetActive(false);
            GetComponent<DraggableCard>().enabled = true;
            ApplyCardDataVisuals();
        }
    }
}