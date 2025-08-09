using System.Collections.Generic;
using UnityEngine;

public static class UtilityScript
{
    public static void ZeroTransform(Transform t)
    {
        t.position = Vector3.zero;
        t.rotation = Quaternion.identity;
    }
    public static void ZeroLocalTransform(Transform t)
    {
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
    }
    public static void ZeroLocalRectTransform(RectTransform t)
    {
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
    }
    public static void Discard(CardScript cs)
    {
        cs.gameObject.transform.SetParent(DeckManager.Instance.discardParent);

        cs.SetHidden();  // Hide discarded card
        cs.ResetCard();
    }
    public static void Discard(GameObject gameObject)
    {
        CardScript cs = gameObject.GetComponent<CardScript>();

        cs.gameObject.transform.SetParent(DeckManager.Instance.discardParent);

        cs.SetHidden();  // Hide discarded card
        cs.ResetCard();
    }
}
public static class CombatUtils
{
    public static void ApplyDamage(EntityManager target, int rawDamage)
    {
        // Step 1: Reduce by Armor
        int reducedDamage = Mathf.Max(0, rawDamage - target.Armour.Value);

        // Step 2: Reduce Block first
        int block = target.Block.Value;
        if (block > 0)
        {
            int blockAbsorb = Mathf.Min(reducedDamage, block);
            target.Block.Value -= blockAbsorb;
            reducedDamage -= blockAbsorb;
        }

        // Step 3: Apply to Health
        if (reducedDamage > 0)
        {
            target.CurrentHealth.Value -= reducedDamage;
        }
    }

    public static void ApplyHealing(EntityManager target, int healing)
    {
        target.CurrentHealth.Value = Mathf.Min(
            target.CurrentHealth.Value + healing,
            target.MaxHealth.GetFinalValue()
        );
    }
}