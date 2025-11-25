using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utility
{
    public static class CombatUtility
    {
        public static void ApplyCost(CardData cardData, Stat resourceStat, int cost)
        {
            resourceStat.AddModifier(new StatModifier(resourceStat, -cost, ModifierScaling.Flat, name: "BaseValue"), ModifierMergeStrategy.Merge);
        }

        public static void ApplyDamage(CardData cardData, EntityScript target, int rawDamage = 0)
        {
            List<GameplayRef> refs = new() { GameplayRef.onHitLanded };
          
            int damage;
            if (cardData != null)
            {
                damage = cardData.Damage;
            }
            else
            {
                damage = rawDamage;
            }

            // 1) Pre-Mitigation
            damage = target.entityStats.DamageTakenModifier.ApplyFinalValue(damage);

            // 2) Armour
            if (target.entityStats.Armour.Value > 0)
            {
                int effectiveArmour = target.entityStats.IgnoreArmour.ApplyFinalValue(target.entityStats.Armour.Value);
                damage = Mathf.Max(0, damage - effectiveArmour);
            }

            // 3) Block
            int block = target.entityStats.IgnoreBlock.ApplyFinalValue(target.entityStats.Block);
            if (block > 0 && damage > 0)
            {
                refs.Add(GameplayRef.onBlocking);

                int blockAbsorb = Mathf.Min(damage, block);
                target.entityStats.Block -= blockAbsorb;
            }

            // 4) Health
            if (damage > 0)
            {
                refs.Add(GameplayRef.onDamageRecieved);

                target.entityStats.CurrentHealth -= damage;
            }

            if (cardData != null)
            {
                // 5) Lifesteal
                if (damage > 0 && cardData.Owner.entityStats.Lifesteal.GetAllValues().Count > 0)
                {
                    refs.Add(GameplayRef.onLifesteal);

                    int heal = Mathf.CeilToInt(damage * (cardData.Owner.entityStats.Lifesteal.Value / 100f));
                    ApplyHealing(cardData, cardData.Owner, heal);
                }
            }
            HandlePostCombatTrigger(refs, target, cardData, damage);
        }

        public static void ApplyHealing(CardData cardData, EntityScript target, int healing)
        {
            List<GameplayRef> refs = new();

            if (healing <= 0) return;

            refs.Add(GameplayRef.onHealRecieved);

            int missing = target.entityStats.MaxHealth.Value - target.entityStats.CurrentHealth;
            int effHeal = Mathf.Clamp(healing, 0, Mathf.Max(0, missing));

            if (effHeal > 0)
            {
                target.entityStats.CurrentHealth += effHeal;
            }
            HandlePostCombatTrigger( refs, target, cardData, effHeal);
        }

        public static void ApplyStatBuff(CardData cardData, EntityScript target, IStatModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            List<GameplayRef> refs = new();

            refs.Add(GameplayRef.onBuffRecieved);

            mod.Stat.AddModifier(mod, mergeStrategy);

            var dbg = target.GetComponent<StatusDebugView>();
            if (dbg != null)
            {
                var label = string.IsNullOrEmpty(mod.ModifierName) ? "BUFF" : mod.ModifierName;
                var eff = mod.Stat.GetModifierByName(mod.ModifierName) ?? mod;
                dbg.SyncDot(label, eff.BaseValue, eff.Duration);
            }
            HandlePostCombatTrigger( refs, target, cardData , mod.BaseValue);
        }

        public static void ApplyStatDebuff(CardData cardData, EntityScript target, Stat targetStat, IStatModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            List<GameplayRef> refs = new();
            refs.Add(GameplayRef.onDebuffRecieved);

            targetStat.AddModifier(mod, mergeStrategy);

            var dbg = target.GetComponent<StatusDebugView>();
            if (dbg != null)
            {
                var label = string.IsNullOrEmpty(mod.ModifierName) ? "DEBUFF" : mod.ModifierName;
                var eff = targetStat.GetModifierByName(mod.ModifierName) ?? mod;
                dbg.SyncDot(label, eff.BaseValue, eff.Duration);
            }
            HandlePostCombatTrigger( refs, target, cardData);
            GameEvents.TriggerRefEvent(new TriggerRef(refs, cardData.Owner, new() { target }, cardData));
        }

        public static void ApplyEntityModifier(CardData cardData, EntityScript target, EntityModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            Debug.Log($"Applying Modifier {mod.ModifierName} to {target.name}");
            List<GameplayRef> refs = new();
            refs.Add(GameplayRef.onModifierApplied);

            if (target == null || mod == null) return;

            target.AddModifier(mod, mergeStrategy);
            target.GetComponent<StatusDebugView>()?.Track(mod);

            Debug.Log($"Applied Modifier {mod.ModifierName} to {target.name}");
            HandlePostCombatTrigger( refs, target, cardData);
        }

        public static void SpawnEntity(CardData cardData, Vector3Int spawnPosition, string npcID, EntityAffiliation affiliation)
        {
            List<GameplayRef> refs = new();
            refs.Add(GameplayRef.onSummon);

            GameObject SpawnObj = GameObject.Instantiate(AssetManager.Instance.entityPrefab, parent: cardData.Owner.transform.parent);
            EntityOnMap entityOnMap = SpawnObj.GetComponent<EntityOnMap>();

            entityOnMap.Spawn(spawnPosition);

            NonPlayerScript spawnedEntity = SpawnObj.GetComponent<NonPlayerScript>();
            spawnedEntity.StartUp();

            NpcData npc = NpcDatabase.GetNpcById(npcID,spawnedEntity);
            spawnedEntity.name = npc.name;
            spawnedEntity.entityAffiliation = affiliation;

            HandlePostCombatTrigger( refs, spawnedEntity, cardData);
        }

        public static void HandlePreCombatTrigger(List<EntityScript> targets, CardData cardData)
        {
            if (cardData == null) return;
            List<GameplayRef> refs = new();
            refs.AddRange(cardData.cardIdentities.Select(c => (GameplayRef)Enum.Parse(typeof(GameplayRef), c.ToString())).ToList());

            GameplayRef classRef = (GameplayRef)Enum.Parse(typeof(GameplayRef), cardData. cardClass.ToString());
            refs.Add(classRef);

            GameplayRef typeRef = (GameplayRef)Enum.Parse(typeof(GameplayRef), cardData.cardType.ToString());
            refs.Add(typeRef);

            GameEvents.TriggerRefEvent(new TriggerRef(refs, cardData.Owner, targets, cardData, cardData.CardAiBias.ThroughputOverride(null, cardData, targets)));
        }
        public static void HandlePostCombatTrigger(List<GameplayRef> gameplayRefs, EntityScript target, CardData cardData, int throughput = 0)
        {
            if (cardData == null) return;

            List<GameplayRef> refs = gameplayRefs;

            GameEvents.TriggerRefEvent(new TriggerRef(refs, cardData.Owner, affectedEntities: new() { target }, cardData, throughput));
        }
    }
}