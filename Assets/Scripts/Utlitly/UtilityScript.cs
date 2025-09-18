using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
    public static void ApplyDamage(EntityScript user, EntityScript target,  int rawDamage, bool isAttack = false, bool ignoreArmour = false, bool ignoreBlock = false)
    {
        if (isAttack)
        {
            GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onAttack }, user.GetInstanceID(), target.GetInstanceID()));
        }

        int damage = rawDamage;

        if (ignoreArmour == false)
        {
            if(target.Armour.Value > 0)
            {
                damage = Mathf.Max(0, damage - target.Armour.Value);
            }  
        }

        if (ignoreBlock == false)
        {
            int block = target.Block.Value;
            if (block > 0)
            {
                GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onBlockingRef }, user.GetInstanceID(), target.GetInstanceID()));

                int blockAbsorb = Mathf.Min(damage, block);
                target.Block.Value -= blockAbsorb;
                damage -= blockAbsorb;
            }
        }

        // Step 3: Apply to Health
        if (damage > 0)
        {
            GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onDamageRef }, user.GetInstanceID(), target.GetInstanceID()));

            target.CurrentHealth.Value -= damage;
        }
    }

    public static void ApplyHealing(EntityScript user, EntityScript target, int healing)
    {
        GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onHeal }, user.GetInstanceID(), target.GetInstanceID()));

        target.CurrentHealth.Value = Mathf.Min
            (
            target.CurrentHealth.Value + healing,
            target.MaxHealth.GetFinalValue()
            );
    }

    public static void ApplyBuff(EntityScript user, EntityScript target, Stat targetStat, IStatModifier mod, ModifierMergeStrategy mergeStrategy) 
    {
        foreach(gameplayRef gameplayRef in mod.To_TriggerGameplayRefs)
        {
            GameEvents.TriggerRefEvent(new TriggerRef( new() { gameplayRef }, user.GetInstanceID(), target.GetInstanceID()));
        }

        targetStat.AddModifier(mod, ModifierMergeStrategy.RefreshIncrease);
    }
}