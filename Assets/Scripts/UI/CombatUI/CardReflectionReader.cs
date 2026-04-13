using System;
using System.Reflection;
using UnityEngine;

namespace facingfate
{
    public static class CardReflectionReader
    {
        public static string GetCardLabel(GameObject cardGO)
        {
            if (cardGO == null) return "-";

            // Try CardScript -> CardData -> Name/Cost
            var cardScript = GetComponentByTypeName(cardGO, "CardScript");
            if (cardScript != null)
            {
                object cardData = ReflectionUtility.TryGetFieldOrProperty(cardScript, "cardData")
                               ?? ReflectionUtility.TryGetFieldOrProperty(cardScript, "CardData");

                string name = TryReadString(cardData, new[] { "CardName", "cardName", "Name", "title" });
                float cost = TryReadFloat(cardData, new[] { "Cost", "cost", "StaminaCost", "staminaCost" }, -1f);

                if (!string.IsNullOrWhiteSpace(name) && cost >= 0f)
                    return $"{name} (Cost {cost:0})";

                if (!string.IsNullOrWhiteSpace(name))
                    return name;
            }

            return cardGO.name;
        }

        public static string GetCardTooltip(GameObject cardGO)
        {
            if (cardGO == null) return "";

            var cardScript = GetComponentByTypeName(cardGO, "CardScript");
            if (cardScript == null) return "";

            object cardData = ReflectionUtility.TryGetFieldOrProperty(cardScript, "cardData")
                           ?? ReflectionUtility.TryGetFieldOrProperty(cardScript, "CardData");

            if (cardData == null) return "";

            string name = TryReadString(cardData, new[] { "CardName", "cardName", "Name", "title" });
            string desc = TryReadString(cardData, new[] { "Description", "description", "RulesText", "rulesText" });

            float cost = TryReadFloat(cardData, new[] { "Cost", "cost", "StaminaCost", "staminaCost" }, -1f);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (!string.IsNullOrWhiteSpace(name)) sb.AppendLine(name);
            if (cost >= 0f) sb.AppendLine($"Cost: {cost:0}");
            if (!string.IsNullOrWhiteSpace(desc))
            {
                sb.AppendLine();
                sb.AppendLine(desc.Trim());
            }

            return sb.ToString().Trim();
        }

        private static Component GetComponentByTypeName(GameObject go, string typeName)
        {
            if (go == null) return null;

            var t = ReflectionUtility.FindTypeByName(typeName);
            if (t == null) return null;

            return go.GetComponent(t);
        }

        private static string TryReadString(object obj, string[] keys)
        {
            if (obj == null) return null;

            for (int i = 0; i < keys.Length; i++)
            {
                object v = ReflectionUtility.TryGetFieldOrProperty(obj, keys[i]);
                if (v != null)
                {
                    string s = v.ToString();
                    if (!string.IsNullOrWhiteSpace(s)) return s;
                }
            }

            return null;
        }

        private static float TryReadFloat(object obj, string[] keys, float fallback)
        {
            if (obj == null) return fallback;

            for (int i = 0; i < keys.Length; i++)
            {
                object v = ReflectionUtility.TryGetFieldOrProperty(obj, keys[i]);
                if (v == null) continue;

                if (v is float f) return f;
                if (v is int ii) return ii;
                if (float.TryParse(v.ToString(), out float parsed)) return parsed;
            }

            return fallback;
        }
    }
}
