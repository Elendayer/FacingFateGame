using UnityEngine;
using System.Collections.Generic;

namespace Utility
{
    public static class CombatUtility
    {
        public static void ApplyCost(EntityScript user, Stat resourceStat, int cost)
        {
            resourceStat.AddModifier(new StatModifier(-cost, ModifierScaling.Flat, name: "BaseValue"), ModifierMergeStrategy.Merge);
            GameEvents.TriggerRefEvent(new TriggerRef(new() { }, user.GetInstanceID()));
        }
        public static void ApplyDamage(EntityScript user, EntityScript target, int rawDamage, bool isAttack = false)
        {
            if (isAttack)
            {
                GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onAttack }, user.GetInstanceID(), target.GetInstanceID()));
            }

            // Damage-Basis
            int damage = rawDamage;

            // 1) Pre-Mitigation (Resistenzen o.ä.)
            damage = target.entityStats.DamageReduction.ApplyFinalValue(damage);

            // 2) Armour
            if (target.entityStats.Armour.Value > 0)
            {
                int effectiveArmour = target.entityStats.IgnoreArmour.ApplyFinalValue(target.entityStats.Armour.Value);
                damage = Mathf.Max(0, damage - effectiveArmour);
            }

            // 3) Block
            int block = target.entityStats.IgnoreBlock.ApplyFinalValue(target.entityStats.Block.Value);
            if (block > 0 && damage > 0)
            {
                GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onBlocking }, user.GetInstanceID(), target.GetInstanceID()));
                int blockAbsorb = Mathf.Min(damage, block);
                target.entityStats.Block.AddModifier(new StatModifier(-blockAbsorb, ModifierScaling.Flat, name: "BaseValue"), ModifierMergeStrategy.Merge);
                damage -= blockAbsorb;
            }

            // 4) Health
            if (damage > 0)
            {
                if (isAttack)
                {
                    GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onDamage }, user.GetInstanceID(), target.GetInstanceID()));
                }

                target.entityStats.CurrentHealth.AddModifier(
                    new StatModifier(-damage, ModifierScaling.Flat, name: "BaseValue"),
                    ModifierMergeStrategy.Merge);

                GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onHitLanded }, user.GetInstanceID(), target.GetInstanceID()));
            }

            // 5) Lifesteal
            if (damage > 0 && user.entityStats.Lifesteal.GetAllValues().Count > 0)
            {
                GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onLifesteal }, user.GetInstanceID(), target.GetInstanceID()));
                int heal = Mathf.CeilToInt(damage * (user.entityStats.Lifesteal.Value / 100f));
                ApplyHealing(user, user, heal);
            }
        }


        public static void ApplyHealing(EntityScript user, EntityScript target, int healing)
        {
            if (healing <= 0) return;

            GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onHeal }, user.GetInstanceID(), target.GetInstanceID()));

            int missing = target.entityStats.MaxHealth.Value - target.entityStats.CurrentHealth.Value;
            int effHeal = Mathf.Clamp(healing, 0, Mathf.Max(0, missing));

            if (effHeal > 0)
            {
                target.entityStats.CurrentHealth.AddModifier(
                    new StatModifier(+effHeal, ModifierScaling.Flat, name: "BaseValue"),
                    ModifierMergeStrategy.Merge);
            }
        }


        public static void ApplyBuff(EntityScript user, EntityScript target, Stat targetStat, IStatModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onBuffed }, user.GetInstanceID(), target.GetInstanceID()));

            targetStat.AddModifier(mod, mergeStrategy);

            var dbg = target.GetComponent<StatusDebugView>();
            if (dbg != null)
            {
                var label = string.IsNullOrEmpty(mod.ModifierName) ? "BUFF" : mod.ModifierName;
                var eff = targetStat.GetModifierByName(mod.ModifierName) ?? mod;
                dbg.SyncDot(label, eff.BaseValue, eff.Duration);
            }
        }
        public static void ApplyDebuff(EntityScript user, EntityScript target, Stat targetStat, IStatModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onDebuffed }, user.GetInstanceID(), target.GetInstanceID()));

            targetStat.AddModifier(mod, mergeStrategy);

            var dbg = target.GetComponent<StatusDebugView>();
            if (dbg != null)
            {
                var label = string.IsNullOrEmpty(mod.ModifierName) ? "DEBUFF" : mod.ModifierName;
                var eff = targetStat.GetModifierByName(mod.ModifierName) ?? mod;
                dbg.SyncDot(label, eff.BaseValue, eff.Duration);
            }
        }

        public static void ApplyEntityModifier(
            EntityScript user,
            EntityScript target,
            EntityModifier mod,
            ModifierMergeStrategy mergeStrategy)
        {
            if (target == null || mod == null) return;

            //Debug
            if (mod.StatModifier == null) mod.StatModifier = new StatModifier(mod.BaseValue, ModifierScaling.Flat, duration: mod.Duration, on_triggerConditionRef: mod.TriggerConditionRef, target: mod.TargetStat, name: mod.ModifierName);
            mod.AddListener();
            target.AddModifier(mod, mergeStrategy);
            target.GetComponent<StatusDebugView>()?.Track(mod);
        }

        public static void SpawnEntity(EntityScript user, Vector3Int spawnPosition, string npcID, EntityAffiliation affiliation )
        {
            GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onSummon }, user.GetInstanceID(), -1));

            GameObject SpawnObj = GameObject.Instantiate(AssetManager.Instance.entityPrefab, parent: user.transform.parent);
            EntityOnMap entityOnMap = SpawnObj.GetComponent<EntityOnMap>();

            entityOnMap.Spawn(spawnPosition);

            NonPlayerScript spawnedEntity = SpawnObj.GetComponent<NonPlayerScript>();
            spawnedEntity.StartUp();

            Npc npc =  NpcDatabase.GetNpcById(npcID);
            
            spawnedEntity.name = npc.name;
            spawnedEntity.entityAffiliation = affiliation;
            spawnedEntity.npcAIBias = npc.aiBias;
        }
    }
}