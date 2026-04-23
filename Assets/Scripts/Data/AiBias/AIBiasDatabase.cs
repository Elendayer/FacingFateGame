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
                id = "Balanced",
                cardReferenceBias = new Dictionary<GameplayRef, float>(),
                identityBias = new Dictionary<CardIdentity, float>(),
            });

            RegisterBias(new NpcAiBias()
            {
                id = "Balanced_Surrounded",

                cardReferenceBias = new Dictionary<GameplayRef, float>(),
                identityBias = new Dictionary<CardIdentity, float>(),

                RepositionCondition = RepositionCondition.surrounded
            });

            RegisterBias(new NpcAiBias()
            {
                id = "Balanced_LowHealth",

                cardReferenceBias = new Dictionary<GameplayRef, float>(),
                identityBias = new Dictionary<CardIdentity, float>(),

                RepositionCondition = RepositionCondition.lowHealth
            });

            RegisterBias(new NpcAiBias()
            {
                id = "Aggressive_Ranged",
                cardReferenceBias = new Dictionary<GameplayRef, float>(),
                identityBias = new Dictionary<CardIdentity, float>()
                {
                    { CardIdentity.Ranged, 1.5f}
                },

                RepositionCondition = RepositionCondition.preferRanged
            });

            RegisterBias(new NpcAiBias()
            {
                id = "Aggressive_Melee",
                cardReferenceBias = new Dictionary<GameplayRef, float>(),
                identityBias = new Dictionary<CardIdentity, float>()
                {
                    { CardIdentity.Melee, 1.5f}
                },
            });

            RegisterBias(new NpcAiBias()
            {
                id = "Defensive",
                cardReferenceBias = new Dictionary<GameplayRef, float>(),
                identityBias = new Dictionary<CardIdentity, float>(),

                RepositionCondition = RepositionCondition.lowHealth
            });

            RegisterBias(new NpcAiBias()
            {
                id = "Supportive",
                cardReferenceBias = new Dictionary<GameplayRef, float>(),
                identityBias = new Dictionary<CardIdentity, float>()
                {
                    { CardIdentity.Healing, 1.5f },
                    { CardIdentity.Buff, 1.5f },
                    { CardIdentity.Debuff, 1.5f }
                }
            });
        }
    }
}