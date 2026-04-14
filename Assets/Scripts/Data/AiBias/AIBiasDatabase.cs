using System;
using System.Collections.Generic;
using UnityEngine;

namespace facingfate
{
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

                cardReferenceBias = new Dictionary<GameplayRef, float>(),
                identityBias = new Dictionary<CardIdentity, float>(),

                RepositionCondition = RepositionCondition.surrounded
            });
            RegisterBias(new NpcAiBias()
            {
                id = "Balanced",

                cardReferenceBias = new Dictionary<GameplayRef, float>(),
                identityBias = new Dictionary<CardIdentity, float>(),

                RepositionCondition = RepositionCondition.lowHealth
            });
            RegisterBias(new NpcAiBias()
            {
                id = "Aggressive",
                cardReferenceBias = new Dictionary<GameplayRef, float>(),
                identityBias = new Dictionary<CardIdentity, float>()
            {
                { CardIdentity.Ranged, 1.5f}
            },

                RepositionCondition = RepositionCondition.preferRanged
            });

        }
    }
}