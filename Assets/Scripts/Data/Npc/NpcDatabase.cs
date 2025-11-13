using System.Collections.Generic;
using UnityEngine;

public class NpcDatabase : MonoBehaviour
{
    private static Dictionary<string, Npc> NpcLookup = new Dictionary<string, Npc>();

    public static void RegisterNpc(Npc npc)
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
    public static Npc GetNpcById(string id, EntityScript entity)
    {
        if (!NpcLookup.TryGetValue(id, out var blueprint) || blueprint == null)
            return null;

        Npc npc = blueprint.Clone(entity);

        return npc;
    }

    public static List<Npc> GetAllNpcs()
    {
        return new List<Npc>(NpcLookup.Values);
    }

    public static void RegisterAll()
    {
        RegisterNpcs();
    }

    private static void RegisterNpcs()
    {
        RegisterNpc(new Npc()
        {
            id = "0001",
            name = "Enemy_Wolfgang",
            aiBias = AiBiasDatabase.GetBiasById("StupidFuck"),

            cardIds = new List<int>()
            {
                110202
            }

        });
        RegisterNpc(new Npc()
        {
            id = "0002",
            name = "Enemy_Boss_Jens",
            aiBias = AiBiasDatabase.GetBiasById("Aggressive"),
            cardIds = new List<int>()
            {
                100111,
                100111,
                100111
            }
            });

        RegisterNpc(new Npc()
        {
            id = "0003",
            name = "Ally_Sophia",
            aiBias = AiBiasDatabase.GetBiasById("Balanced"),
            cardIds = new List<int>()
            {
                100108,
                110104
            }
        });
    }
}


public class Npc
{
    public string id;
    public string name;
    public NpcAiBias aiBias = new();

    public List<int> cardIds = new();

    public Npc Clone(EntityScript entity)
    {
        return new Npc
        {
            id = this.id,
            name = $"{this.name}_{entity.GetInstanceID()}",
            aiBias = this.aiBias,
            cardIds = this.cardIds
        };
    }
}