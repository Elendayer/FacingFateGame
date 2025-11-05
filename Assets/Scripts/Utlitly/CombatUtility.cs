using UnityEngine;
using System.Collections.Generic;

namespace Utility
{
    public static class CombatUtility
    {
        public static void ApplyCost(CardData cardData, Stat resourceStat, int cost)
        {
            resourceStat.AddModifier(new StatModifier(-cost, ModifierScaling.Flat, name: "BaseValue"), ModifierMergeStrategy.Merge);
            GameEvents.TriggerRefEvent(new TriggerRef(new() { }, cardData.Owner.GetInstanceID(), cardData: cardData));
        }

        public static void ApplyDamage(CardData cardData, EntityScript target, int rawDamage, bool isAttack = false)
        {
            if (isAttack)
            {
                GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onAttack }, cardData.Owner.GetInstanceID(), target.GetInstanceID(), cardData));
            }

            int damage = rawDamage;

            // 1) Pre-Mitigation
            damage = target.entityStats.DamageTakenReduction.ApplyFinalValue(damage);

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
                GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onBlocking }, cardData.Owner.GetInstanceID(), target.GetInstanceID(), cardData));
                int blockAbsorb = Mathf.Min(damage, block);
                target.entityStats.Block.AddModifier(new StatModifier(-blockAbsorb, ModifierScaling.Flat, name: "BaseValue"), ModifierMergeStrategy.Merge);
                damage -= blockAbsorb;
            }

            // 4) Health
            if (damage > 0)
            {
                if (isAttack)
                {
                    GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onDamage }, cardData.Owner.GetInstanceID(), target.GetInstanceID(), cardData));
                }

                target.entityStats.CurrentHealth.AddModifier(
                    new StatModifier(-damage, ModifierScaling.Flat, name: "BaseValue"),
                    ModifierMergeStrategy.Merge);

                GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onHitLanded }, cardData.Owner.GetInstanceID(), target.GetInstanceID(), cardData));
            }

            // 5) Lifesteal
            if (damage > 0 && cardData.Owner.entityStats.Lifesteal.GetAllValues().Count > 0)
            {
                GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onLifesteal }, cardData.Owner.GetInstanceID(), target.GetInstanceID(), cardData));
                int heal = Mathf.CeilToInt(damage * (cardData.Owner.entityStats.Lifesteal.Value / 100f));
                ApplyHealing(cardData, cardData.Owner, heal);
            }
        }

        public static void ApplyHealing(CardData cardData, EntityScript target, int healing)
        {
            if (healing <= 0) return;

            GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onHeal }, cardData.Owner.GetInstanceID(), target.GetInstanceID(), cardData));

            int missing = target.entityStats.MaxHealth.Value - target.entityStats.CurrentHealth.Value;
            int effHeal = Mathf.Clamp(healing, 0, Mathf.Max(0, missing));

            if (effHeal > 0)
            {
                target.entityStats.CurrentHealth.AddModifier(
                    new StatModifier(+effHeal, ModifierScaling.Flat, name: "BaseValue"),
                    ModifierMergeStrategy.Merge);
            }
        }

        public static void ApplyBuff(CardData cardData, EntityScript target, Stat targetStat, IStatModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onBuffed }, cardData.Owner.GetInstanceID(), target.GetInstanceID(), cardData));

            targetStat.AddModifier(mod, mergeStrategy);

            var dbg = target.GetComponent<StatusDebugView>();
            if (dbg != null)
            {
                var label = string.IsNullOrEmpty(mod.ModifierName) ? "BUFF" : mod.ModifierName;
                var eff = targetStat.GetModifierByName(mod.ModifierName) ?? mod;
                dbg.SyncDot(label, eff.BaseValue, eff.Duration);
            }
        }

        public static void ApplyDebuff(CardData cardData, EntityScript target, Stat targetStat, IStatModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onDebuffed }, cardData.Owner.GetInstanceID(), target.GetInstanceID(), cardData));

            targetStat.AddModifier(mod, mergeStrategy);

            var dbg = target.GetComponent<StatusDebugView>();
            if (dbg != null)
            {
                var label = string.IsNullOrEmpty(mod.ModifierName) ? "DEBUFF" : mod.ModifierName;
                var eff = targetStat.GetModifierByName(mod.ModifierName) ?? mod;
                dbg.SyncDot(label, eff.BaseValue, eff.Duration);
            }
        }

        public static void ApplyEntityModifier(CardData cardData, EntityScript target, EntityModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            if (target == null || mod == null) return;

            if (mod.StatModifier == null)
                mod.StatModifier = new StatModifier(mod.BaseValue, ModifierScaling.Flat, duration: mod.Duration, on_triggerConditionRef: mod.TriggerConditionRef, name: mod.ModifierName);

            mod.AddListener();
            target.AddModifier(mod, mergeStrategy);
            target.GetComponent<StatusDebugView>()?.Track(mod);
        }

        public static void SpawnEntity(CardData cardData, Vector3Int spawnPosition, string npcID, EntityAffiliation affiliation)
        {
            GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onSummon }, cardData.Owner.GetInstanceID(), -1, cardData));

            GameObject SpawnObj = GameObject.Instantiate(AssetManager.Instance.entityPrefab, parent: cardData.Owner.transform.parent);
            EntityOnMap entityOnMap = SpawnObj.GetComponent<EntityOnMap>();

            entityOnMap.Spawn(spawnPosition);

            NonPlayerScript spawnedEntity = SpawnObj.GetComponent<NonPlayerScript>();
            spawnedEntity.StartUp();

            Npc npc = NpcDatabase.GetNpcById(npcID);
            spawnedEntity.name = npc.name;
            spawnedEntity.entityAffiliation = affiliation;
            spawnedEntity.npcAIBias = npc.aiBias;
        }
    }
}
