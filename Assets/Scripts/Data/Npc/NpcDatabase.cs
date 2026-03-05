using System.Collections.Generic;
using UnityEngine;

namespace facingfate
{
    public class NpcDatabase : MonoBehaviour
    {
        private static Dictionary<string, NpcData> NpcLookup = new Dictionary<string, NpcData>();

        public static void RegisterNpc(NpcData npc)
        {
            if (!NpcLookup.ContainsKey(npc.id))
            {
                NpcLookup[npc.id] = npc;
                //Debug.Log($"Registered card: {card.cardName} (ID: {card.cardID})");
            }
            else
            {
                Debug.LogWarning($"Duplicate card ID detected: {npc.id}");
            }
        }
        public static NpcData GetNpcById(string id, EntityScript entity)
        {
            if (!NpcLookup.TryGetValue(id, out var blueprint) || blueprint == null)
                return null;

            NpcData npc = blueprint.Clone(entity);

            return npc;
        }

        public static List<NpcData> GetAllNpcs()
        {
            return new List<NpcData>(NpcLookup.Values);
        }

        public static void RegisterAll()
        {
            RegisterNpcs();
        }

        private static void RegisterNpcs()
        {
            RegisterNpc(new NpcData()
            {
                id = "Spear_Test_Npc",
                name = "Spear",
                aiBias = AiBiasDatabase.GetBiasById("StupidFuck"),

                cardIds = new List<int>()
            {
                140201
            }

            });

            RegisterNpc(new NpcData()
            {
                id = "Mystic_Test_Npc",
                name = "Mystic",
                aiBias = AiBiasDatabase.GetBiasById("StupidFuck"),

                cardIds = new List<int>()
            {
                110108,
            }

            });
            RegisterNpc(new NpcData()
            {
                id = "Assassin_Test_Npc",
                name = "Assassin",
                aiBias = AiBiasDatabase.GetBiasById("StupidFuck"),
                cardIds = new List<int>()
            {
                110105
            }
            });

            RegisterNpc(new NpcData()
            {
                id = "Physician_Test_Npc",
                name = "Physician",
                aiBias = AiBiasDatabase.GetBiasById("StupidFuck"),
                cardIds = new List<int>()
            {
                110104,
            }
            });
        }
    }


    public class NpcData
    {
        public string id;
        public string name;
        public NpcAiBias aiBias = new();

        public List<int> cardIds = new();

        public NpcData Clone(EntityScript entity)
        {
            return new NpcData
            {
                id = this.id,
                name = $"{this.name}_{entity.GetInstanceID()}",
                aiBias = this.aiBias,
                cardIds = this.cardIds
            };
        }
    }
}