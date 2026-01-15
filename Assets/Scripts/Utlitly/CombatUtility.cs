using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using facingfate;

namespace Utility
{
    public static class CombatUtility
    {
        public static void ApplyCost(CardData cardData, Stat resourceStat, int cost)
        {
            resourceStat.AddModifier(new StatModifier("BaseValue", resourceStat, -cost, ModifierScaling.Flat), ModifierMergeStrategy.Merge);
        }

        // Direct Damage that bypasses armour and block
        public static void ApplyEffectDamage(int rawDamage, EntityScript target, GameplayRef dotType)
        {
            List<GameplayRef> refs = new() { dotType, GameplayRef.onDamageRecieved };

            target.entityStats.CurrentHealth -= rawDamage;

            HandlePostCombatTrigger(refs, target, target, null, rawDamage);
        }

        // Standard Damage Application
        public static void ApplyDamage(CardData cardData, EntityScript target, int rawDamage = 0)
        {
            List<GameplayRef> refs = new() { GameplayRef.onHitLanded };

            int damage;
            if (cardData != null)
            {
                Debug.Log($"Card Data Damage: {cardData.Damage}");
                damage = cardData.Damage;
            }
            else
            {
                Debug.Log($"Raw Damage: {rawDamage}");
                damage = rawDamage;
            }

            Debug.Log($"Applying {damage} damage to {target.name}");

            // 1) Pre-Mitigation
            damage = target.entityStats.DamageTakenModifier.ApplyFinalValue(damage);

            // 2) Armour
            if (target.entityStats.Armour.Value() > 0)
            {
                int effectiveArmour = target.entityStats.IgnoreArmour.ApplyFinalValue(target.entityStats.Armour.Value());
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

                    int heal = Mathf.CeilToInt(damage * (cardData.Owner.entityStats.Lifesteal.Value() / 100f));
                    ApplyHealing(cardData, cardData.Owner, heal);
                }
                HandlePostCombatTrigger(refs, cardData.Owner, target, cardData, damage); return;
            }
            HandlePostCombatTrigger(refs, null, target, null, damage); return;
        }

        public static void ApplyHealing(CardData cardData, EntityScript target, int healing)
        {
            if (healing <= 0) return;

            List<GameplayRef> refs = new();

            int missing = target.entityStats.MaxHealth.Value() - target.entityStats.CurrentHealth;
            int effHeal = Mathf.Clamp(healing, 0, Mathf.Max(0, missing));

            if (effHeal > 0)
            {
                refs.Add(GameplayRef.onHealRecieved);

                target.entityStats.CurrentHealth += effHeal;
            }
            if (cardData != null)
            {
                HandlePostCombatTrigger(refs, cardData.Owner, target, cardData, effHeal);

            }
            else
            {
                HandlePostCombatTrigger(refs, target, target, null, effHeal);
            }
        }

        public static void ApplyStatBuff(CardData cardData, EntityScript target, IStatModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            List<GameplayRef> refs = new();

            refs.Add(GameplayRef.onBuffRecieved);

            mod.Stat.AddModifier(mod, mergeStrategy);

            Debug.Log($"Applied Buff {mod.ModifierName} to {target.name}");
            HandlePostCombatTrigger(refs, cardData.Owner, target, cardData, mod.BaseValue);
        }

        public static void ApplyStatDebuff(CardData cardData, EntityScript target, IStatModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            List<GameplayRef> refs = new();
            refs.Add(GameplayRef.onDebuffRecieved);

            mod.Stat.AddModifier(mod, mergeStrategy);

            Debug.Log($"Applied Debuff {mod.ModifierName} to {target.name}");
            HandlePostCombatTrigger(refs, cardData.Owner, target, cardData);        }

        public static void ApplyEntityModifier(CardData cardData, EntityScript target, EntityModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            List<GameplayRef> refs = new();
            refs.Add(GameplayRef.onModifierApplied);

            if (target == null || mod == null) return;

            if ( mod.Owner == null)
            {
                mod.Owner = target;
            }
            target.AddModifier(mod, mergeStrategy);
            target.GetComponent<StatusDebugView>()?.Track(mod);

            Debug.Log($"Applied Modifier {mod.ModifierName} to {target.name}");
            HandlePostCombatTrigger(refs, cardData.Owner, target, cardData);
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

            NpcData npc = NpcDatabase.GetNpcById(npcID, spawnedEntity);
            spawnedEntity.name = npc.name;
            spawnedEntity.entityAffiliation = affiliation;

            HandlePostCombatTrigger(refs, cardData.Owner, spawnedEntity, cardData);
        }
        public static void SpawnGroundEffect(CardData cardData, Vector3Int spawnPosition, GroundEffectDataBase groundEffectData)
        {
            List<GameplayRef> refs = new();
            GameObject SpawnObj = GameObject.Instantiate(AssetManager.Instance.groundEffectPrefab, parent: cardData.Owner.transform.parent);
            GroundEffectScript groundEffectScript = SpawnObj.GetComponent<GroundEffectScript>();
            SpawnObj.transform.position = TilemapUtilityScript.BaseTilemap.CellToWorld(spawnPosition);
            groundEffectScript.EffectData = groundEffectData;
            HandlePostCombatTrigger(refs, cardData.Owner, null, cardData);
        }
        public static void HandlePreCombatTrigger(List<EntityScript> targets, CardData cardData)
        {
            if (cardData == null) return;
            List<GameplayRef> refs = new() { GameplayRef.onCardPlayed };
            refs.AddRange(cardData.cardIdentities.Select(c => (GameplayRef)Enum.Parse(typeof(GameplayRef), c.ToString())).ToList());

            GameplayRef classRef = (GameplayRef)Enum.Parse(typeof(GameplayRef), cardData.cardClass.ToString());
            refs.Add(classRef);

            GameplayRef typeRef = (GameplayRef)Enum.Parse(typeof(GameplayRef), cardData.cardType.ToString());
            refs.Add(typeRef);

            GameEvents.TriggerRefEvent(new ToSendTriggerReference(refs, cardData.Owner, targets, cardData, cardData.CardAiBias.ThroughputOverride(null, cardData, targets)));
        }
        public static void HandlePostCombatTrigger(List<GameplayRef> gameplayRefs, EntityScript user, EntityScript target, CardData cardData = null, int throughput = 0)
        {
            List<GameplayRef> refs = new() { GameplayRef.onCardEffectEnd};
            refs.AddRange(gameplayRefs);

            if (cardData != null)
            {
                GameEvents.TriggerRefEvent(new ToSendTriggerReference(refs, user, new() { target }, cardData, throughput));
            }
            else
            {
                GameEvents.TriggerRefEvent(new ToSendTriggerReference(refs, user, new() { target }, null, throughput));
            }
        }
    }
}