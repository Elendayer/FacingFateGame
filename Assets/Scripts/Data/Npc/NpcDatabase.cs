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
    public static Npc GetNpcById(string id)
    {
        if (!NpcLookup.TryGetValue(id, out var blueprint) || blueprint == null)
            return null;

        Npc npc = blueprint.Clone();

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
            name = "Wolf Gang",
            aiBias = AiBiasDatabase.GetBiasById("StupidFuck")

        });
    }
}


public class Npc
{
    public string id;
    public string name;
    public NpcAiBias aiBias;

    public Npc Clone()
    {
        return new Npc
        {
            id = this.id,
            name = this.name,
            aiBias = this.aiBias
        };
    }
}