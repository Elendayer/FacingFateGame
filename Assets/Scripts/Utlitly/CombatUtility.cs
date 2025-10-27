using UnityEngine;

namespace Utility
{
    public static class CombatUtility
    {
        public static void ApplyDamage(EntityScript user, EntityScript target, int rawDamage, bool isAttack = false)
        {
            if (isAttack)
            {
                GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onAttack }, user.GetInstanceID(), target.GetInstanceID()));
            }

            // Pre-mitigation damage reduction (e.g., from abilities or effects)
            int damage = rawDamage;
            rawDamage = target.entityStats.DamageTakenReduction.ApplyFinalValue(rawDamage);

            // Step 1: Apply Armour
            if (target.entityStats.Armour.Value > 0)
            {
                damage = Mathf.Max(0, damage - (target.entityStats.IgnoreArmour.ApplyFinalValue(target.entityStats.Armour.Value)));
            }

            // Step 2: Apply to Block
            int block = target.entityStats.IgnoreBlock.ApplyFinalValue(target.entityStats.Block.Value);
            if (block > 0)
            {
                GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onBlocking }, user.GetInstanceID(), target.GetInstanceID()));

                int blockAbsorb = Mathf.Min(damage, block);
                target.entityStats.Block.AddModifier(new StatModifier(-blockAbsorb, ModifierScaling.Flat, name: "BaseValue"), ModifierMergeStrategy.Merge);
                damage -= blockAbsorb;
            }

            // Step 3: Apply to Health
            if (damage > 0)
            {
                if (isAttack)
                {
                    GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onDamage }, user.GetInstanceID(), target.GetInstanceID()));
                }

                target.entityStats.CurrentHealth.AddModifier(new StatModifier(-damage, ModifierScaling.Flat, name: "BaseValue"), ModifierMergeStrategy.Merge);
            }

            if(user.entityStats.Lifesteal.GetAllValues().Count > 0)
            {
                GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onLifesteal }, user.GetInstanceID(), target.GetInstanceID()));
                ApplyHealing(user, user, Mathf.CeilToInt(damage * (user.entityStats.Lifesteal.Value / 100f)));
            }        
        }

        public static void ApplyHealing(EntityScript user, EntityScript target, int healing)
        {
            GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onHeal }, user.GetInstanceID(), target.GetInstanceID()));

            target.entityStats.CurrentHealth.AddModifier(new StatModifier(
                Mathf.Min
                (
                    target.entityStats.CurrentHealth.Value + healing,
                    target.entityStats.MaxHealth.Value
            ),
                ModifierScaling.Flat, name: "BaseValue"), ModifierMergeStrategy.Merge);
        }

        public static void ApplyBuff(EntityScript user, EntityScript target, Stat targetStat, IStatModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onBuffed }, user.GetInstanceID(), target.GetInstanceID()));

            targetStat.AddModifier(mod, mergeStrategy);
        }
        public static void ApplyDebuff(EntityScript user, EntityScript target, Stat targetStat, IStatModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onDebuffed }, user.GetInstanceID(), target.GetInstanceID()));

            targetStat.AddModifier(mod, mergeStrategy);
        }
        public static void ApplyEntityModifier(EntityScript user, EntityScript target, IEntityModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            target.AddModifier(mod, mergeStrategy);
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