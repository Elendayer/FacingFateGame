using System.Collections.Generic;
using UnityEngine;
namespace facingfate
{
    public static class CardDatabase
    {
        private static Dictionary<string, CardData> cardLookup = new Dictionary<string, CardData>();

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
        public static CardData GetCardById(string id, EntityScript owner)
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
            // Register cards from all classes
            SpearmanCards.RegisterAll();
            AssassinCards.RegisterAll();
            MysticCards.RegisterAll();
            PhysicianCards.RegisterAll();
            NeutralCards.RegisterAll();

            // Register Effects
            EffectDatabase.RegisterAll();

            // Register completed
        }
    }
}