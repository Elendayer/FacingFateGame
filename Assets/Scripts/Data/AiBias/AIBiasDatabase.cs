using System;
using System.Collections.Generic;
using UnityEngine;

public class AiBiasDatabase : MonoBehaviour
{

    private static Dictionary<string, NpcAiBias> AILookup = new Dictionary<string, NpcAiBias>();

    public static void RegisterBias(NpcAiBias bias)
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
    public static NpcAiBias GetBiasById(string id)
    {
        if (!AILookup.TryGetValue(id, out var blueprint) || blueprint == null)
            return null;

        NpcAiBias AiBias = blueprint.Clone();

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
        RegisterBias(new NpcAiBias()
        {
            id = "StupidFuck",

            intentionBias = new Dictionary<Intention, int>(),
            refBias = new Dictionary<GameplayRef, int>(),
            identityBias = new Dictionary<CardIdentity, int>(),

            RepositionCondition = RepositionCondition.surrounded
        });
        RegisterBias(new NpcAiBias()
        {
            id = "Balanced",

            intentionBias = new Dictionary<Intention, int>(),
            refBias = new Dictionary<GameplayRef, int>(),
            identityBias = new Dictionary<CardIdentity, int>(),

            RepositionCondition = RepositionCondition.lowHealth
        });
        RegisterBias(new NpcAiBias()
        {
            id = "Aggressive",
            intentionBias = new Dictionary<Intention, int>(),
            refBias = new Dictionary<GameplayRef, int>(),
            identityBias = new Dictionary<CardIdentity, int>()
            {
                { CardIdentity.Ranged, 50}
            },

            RepositionCondition = RepositionCondition.preferRanged
        });

    }
}