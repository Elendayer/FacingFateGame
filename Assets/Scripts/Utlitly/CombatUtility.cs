using UnityEngine;

namespace Utility
{
    public static class CombatUtility
    {
        public static void ApplyDamage(EntityScript user, EntityScript target, int rawDamage, bool isAttack = false, bool ignoreArmour = false, bool ignoreBlock = false)
        {
            if (isAttack)
            {
                GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onAttack }, user.GetInstanceID(), target.GetInstanceID()));
            }

            int damage = rawDamage;

            if (ignoreArmour == false)
            {
                if (target.Armour.Value > 0)
                {
                    damage = Mathf.Max(0, damage - target.Armour.Value);
                }
            }

            if (ignoreBlock == false)
            {
                int block = target.Block.Value;
                if (block > 0)
                {
                    GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onBlocking }, user.GetInstanceID(), target.GetInstanceID()));

                    int blockAbsorb = Mathf.Min(damage, block);
                    target.Block.Value -= blockAbsorb;
                    damage -= blockAbsorb;
                }
            }

            // Step 3: Apply to Health
            if (damage > 0)
            {
                if (isAttack)
                {
                    GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onDamage }, user.GetInstanceID(), target.GetInstanceID()));
                }

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
            GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onBuffed }, user.GetInstanceID(), target.GetInstanceID()));

            targetStat.AddModifier(mod, ModifierMergeStrategy.RefreshIncrease);
        }
        public static void ApplyDebuff(EntityScript user, EntityScript target, Stat targetStat, IStatModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onDebuffed }, user.GetInstanceID(), target.GetInstanceID()));

            targetStat.AddModifier(mod, ModifierMergeStrategy.RefreshIncrease);
        }
        public static void ApplyModifier(EntityScript user, EntityScript target, Stat targetStat, IStatModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            targetStat.AddModifier(mod, ModifierMergeStrategy.RefreshIncrease);
        }
    }
}