using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Utility
{
    public static class CombatUtility
    {
        public static void ApplyCost(CardData cardData, Stat resourceStat, int cost)
        {
            resourceStat.AddModifier(new StatModifier(resourceStat, -cost, ModifierScaling.Flat, name: "BaseValue"), ModifierMergeStrategy.Merge);
        }

        public static void ApplyDamage(CardData cardData, EntityScript target, int rawDamage, bool isAttack = false)
        {
            List<GameplayRef> refs = new();

            if (isAttack)
            {
                refs.Add(GameplayRef.onAttack);
            }

            int damage = rawDamage;

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
                refs.Add(GameplayRef.onDamage);

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
            HandleTrigger(refs, target, cardData);
        }

        public static void ApplyHealing(CardData cardData, EntityScript target, int healing)
        {
            List<GameplayRef> refs = new();

            if (healing <= 0) return;

            refs.Add(GameplayRef.onHeal);

            int missing = target.entityStats.MaxHealth.Value - target.entityStats.CurrentHealth;
            int effHeal = Mathf.Clamp(healing, 0, Mathf.Max(0, missing));

            if (effHeal > 0)
            {
                target.entityStats.CurrentHealth += effHeal;
            }
            HandleTrigger( refs, target, cardData);
        }

        public static void ApplyBuff(CardData cardData, EntityScript target, IStatModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            List<GameplayRef> refs = new();

            refs.Add(GameplayRef.onBuffed);

            mod.Stat.AddModifier(mod, mergeStrategy);

            var dbg = target.GetComponent<StatusDebugView>();
            if (dbg != null)
            {
                var label = string.IsNullOrEmpty(mod.ModifierName) ? "BUFF" : mod.ModifierName;
                var eff = mod.Stat.GetModifierByName(mod.ModifierName) ?? mod;
                dbg.SyncDot(label, eff.BaseValue, eff.Duration);
            }
            HandleTrigger( refs, target, cardData);
        }

        public static void ApplyDebuff(CardData cardData, EntityScript target, Stat targetStat, IStatModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            List<GameplayRef> refs = new();
            refs.Add(GameplayRef.onDebuffed);

            targetStat.AddModifier(mod, mergeStrategy);

            var dbg = target.GetComponent<StatusDebugView>();
            if (dbg != null)
            {
                var label = string.IsNullOrEmpty(mod.ModifierName) ? "DEBUFF" : mod.ModifierName;
                var eff = targetStat.GetModifierByName(mod.ModifierName) ?? mod;
                dbg.SyncDot(label, eff.BaseValue, eff.Duration);
            }
            HandleTrigger( refs, target, cardData);
            GameEvents.TriggerRefEvent(new TriggerRef(refs, cardData.Owner, target, cardData));
        }

        public static void ApplyEntityModifier(CardData cardData, EntityScript target, EntityModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            List<GameplayRef> refs = new();

            if (target == null || mod == null) return;

            target.AddModifier(mod, mergeStrategy);
            target.GetComponent<StatusDebugView>()?.Track(mod);

            HandleTrigger( refs, target, cardData);
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

            HandleTrigger( refs, spawnedEntity, cardData);
        }

        public static void HandleTrigger( List<GameplayRef> gameplayRefs, EntityScript target, CardData cardData)
        {
            if (cardData == null) return;

            List<GameplayRef> refs = gameplayRefs;

            refs.AddRange(cardData.cardIdentities.Select(c => (GameplayRef)Enum.Parse(typeof(GameplayRef), c.ToString())).ToList());
            GameplayRef classRef = (GameplayRef)Enum.Parse(typeof(GameplayRef), cardData. cardClass.ToString());
            GameplayRef typeRef = (GameplayRef)Enum.Parse(typeof(GameplayRef), cardData.cardType.ToString());

            refs.Add(classRef);
            refs.Add(typeRef);

            GameEvents.TriggerRefEvent(new TriggerRef(refs, cardData.Owner, target, cardData));
        }
    }
}