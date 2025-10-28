using System.Collections.Generic;
using UnityEngine;
using Utility;

public static class CardDatabase
{
    private static Dictionary<int, CardData> cardLookup = new Dictionary<int, CardData>();

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
    public static CardData GetCardById(int id, EntityScript owner)
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
        SpearmanCards.RegisterAll();
        AssassinCards.RegisterAll();
        MysticCards.RegisterAll();
        PhysicianCards.RegisterAll();
        NeutralCards.RegisterAll();
    }
}