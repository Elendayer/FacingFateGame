using System;
using System.Collections.Generic;
using UnityEngine;

public class AiBiasDatabase : MonoBehaviour
{

    private static Dictionary<string, NpcAiBias> AILookup = new Dictionary<string, NpcAiBias>();

    public static void RegisterCard(NpcAiBias bias)
    {
        if (!AILookup.ContainsKey(bias.id))
        {
            AILookup[bias.id] = bias;
            //Debug.Log($"Registered card: {card.cardName} (ID: {card.cardID})");
        }
        else
        {
            Debug.LogWarning($"Duplicate card ID detected: {bias.id}");
        }
    }
    public static NpcAiBias GetCardById(string id)
    {
        if (!AILookup.TryGetValue( id, out var AiBias) || AiBias == null)
            return null;

        return AiBias;
    }

    public static List<NpcAiBias> GetAllBias()
    {
        return new List<NpcAiBias>(AILookup.Values);
    }

    public static void RegisterAll()
    {
        RegisterAIBias();
    }

    private static void RegisterAIBias()
    {
        throw new NotImplementedException();
    }
}
