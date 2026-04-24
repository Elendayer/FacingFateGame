using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using facingfate;


    public static class CombatUtility
    {
        // Damage that bypasses mitigation, armour, and block. Used for DOT effects and other similar cases.
        // Does not trigger onHitLanded or any related triggers, but does trigger onDamageRecieved and related triggers.
        public static void ApplyEffectDamage(float rawDamage, EntityScript target, GameplayRef dotType, VFXData vfxData)
        {
            List<GameplayRef> refs = new() { dotType, GameplayRef.onDamageRecieved };

            target.entityStats.CurrentHealth -= rawDamage;
            DamageNumberSpawner.Instance?.SpawnDamage(target, Mathf.RoundToInt(rawDamage), DamageNumberSpawner.NumberType.Dot);

            HandleOnDamageVFX(vfxData, target);

            HandlePostCombatTrigger(refs, target, target, null, (int)rawDamage);
        }

        // Standard Damage Application
        public static void ApplyDamage(CardData cardData, EntityScript target, VFXData vfxData, float rawDamage = 0)
        {
            List<GameplayRef> refs = new() { GameplayRef.onHitLanded };

            float damage;

            if (cardData != null)
            {
                damage = cardData.Damage;
            }
            else
            {
                damage = rawDamage;
            }

            Debug.Log($"Applying {damage} damage to {target.name}");

            // 1) Pre-Mitigation
            damage = target.entityStats.ApplyStatModifiers(damage, target.entityStats.DamageTakenModifier_Flat, target.entityStats.DamageTakenModifier_Increase, target.entityStats.DamageTakenModifier_Multiplier);

            // 2) Armour
            if (target.entityStats.CurrentArmour > 0)
            {
                float effectiveArmour = target.entityStats.CurrentArmour * cardData.Owner.entityStats.IgnoreArmour.Value();
                damage = Mathf.Max(0, damage - effectiveArmour);
            }

            // 3) Block
            float block = target.entityStats.CurrentBlock;
            if (block > 0 && damage > 0)
            {
                refs.Add(GameplayRef.onBlocking);

                float blockAbsorb = Mathf.Min(damage, block);
                target.entityStats.CurrentBlock -= blockAbsorb;
            }

            // 4) Health
            if (damage > 0)
            {
                refs.Add(GameplayRef.onDamageRecieved);

                target.entityStats.CurrentHealth -= damage;
                DamageNumberSpawner.Instance?.SpawnDamage(target, Mathf.RoundToInt(damage), DamageNumberSpawner.NumberType.Damage);
            }

            if (cardData != null)
            {
                // 5) Lifesteal
                if (damage > 0 && cardData.Owner.entityStats.Lifesteal.Value() > 0)
                {
                    refs.Add(GameplayRef.onLifesteal);

                    float heal = damage * (cardData.Owner.entityStats.Lifesteal.Value());
                    ApplyHealing(cardData, cardData.Owner, heal);
                }
                HandlePostCombatTrigger(refs, cardData.Owner, target, cardData, (int)damage); return;
            }

            HandleOnDamageVFX(vfxData, target);

            HandlePostCombatTrigger(refs, null, target, null, (int)damage); return;
        }

        public static void ApplyHealing(CardData cardData, EntityScript target, float healing)
        {
            if (healing <= 0) return;

            List<GameplayRef> refs = new();

            float missing = target.entityStats.MaxHealth - target.entityStats.CurrentHealth;
            float effHeal = Mathf.Clamp(healing, 0, Mathf.Max(0, missing));

            if (effHeal > 0)
            {
                refs.Add(GameplayRef.onHealRecieved);

                target.entityStats.CurrentHealth += effHeal;
                DamageNumberSpawner.Instance?.SpawnDamage(target, Mathf.RoundToInt(effHeal), DamageNumberSpawner.NumberType.Heal);
            }
            if (cardData != null)
            {
                HandlePostCombatTrigger(refs, cardData.Owner, target, cardData, (int)effHeal);

            }
            else
            {
                HandlePostCombatTrigger(refs, target, target, null, (int)effHeal);
            }
        }

        public static void ApplyStatBuff(CardData cardData, EntityScript target, IStatModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            List<GameplayRef> refs = new();

            refs.Add(GameplayRef.onBuffRecieved);

            mod.Stat.AddModifier(mod, mergeStrategy);

            Debug.Log($"Applied Buff {mod.ModifierName} to {target.name}");
            HandlePostCombatTrigger(refs, cardData.Owner, target, cardData, (int)mod.BaseValue);
        }

        public static void ApplyStatDebuff(CardData cardData, EntityScript target, IStatModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            List<GameplayRef> refs = new() { GameplayRef.onDebuffRecieved };

            mod.Stat.AddModifier(mod, mergeStrategy);

            Debug.Log($"Applied Debuff {mod.ModifierName} to {target.name}");
            HandlePostCombatTrigger(refs, cardData.Owner, target, cardData);        }

        public static void ApplyEntityModifier(CardData cardData, EntityScript EffectOwner, EntityModifier mod, ModifierMergeStrategy mergeStrategy, int valueOverride = 0)
        {
            List<GameplayRef> refs = new() { GameplayRef.onModifierApplied };

            if (EffectOwner == null || mod == null) return;

            if ( mod.Owner == null)
            {
                mod.Owner = EffectOwner;
            }

            // If the modifier has a reference trigger but no entity to check, set it to check the effect owner by default. This allows for more concise modifier definitions in cases where the modifier is meant to trigger off of the entity it's applied to.
            if (mod.OnRef_Trigger.CheckEntity == null)
            {
                mod.OnRef_Trigger = new RelevantTriggerCheck()
                {
                    OnTriggerReference = mod.OnRef_Trigger.OnTriggerReference,
                    CheckType = mod.OnRef_Trigger.CheckType,
                    CheckEntity = EffectOwner,
                    CardData = cardData
                };
            }

            // Value override is used for cases where the modifier value is determined at application rather than beforehand, such as with scaling modifiers. If the valueOverride is 0, it will use the default value from the modifier data.
            if (valueOverride != 0)
            {
                mod.BaseValue = valueOverride;
            }

            EffectOwner.AddModifier(mod, mergeStrategy);
            DamageNumberSpawner.Instance?.SpawnDamage(EffectOwner, 0, DamageNumberSpawner.NumberType.Modifier);

            Debug.Log($"Applied Modifier {mod.ModifierName} to {EffectOwner.name}");
            HandlePostCombatTrigger(refs, cardData.Owner, EffectOwner, cardData);
        }

    #region Spawning
    public static NonPlayerScript SpawnEntity(Vector3 spawnPosition, string npcID, EntityAffiliation affiliation, bool hasTurn = true)
    {
        GameObject spawnObj = GameObject.Instantiate(AssetManager.Instance.entityPrefab, position: spawnPosition, rotation: Quaternion.identity);
        spawnObj.GetComponent<EntityOnMap>().TeleportTo(spawnPosition);

        NonPlayerScript entity = spawnObj.GetComponent<NonPlayerScript>();
        entity.NpcID = npcID;       // must be set before StartUp — NonPlayerScript.StartUp() reads NpcID to load from DB
        entity.entityAffiliation = affiliation;
        entity.StartUp();

        if (hasTurn)
        {
            TurnManager.Instance.AddTurn(entity);
        }

        return entity;
    }

        public static void SpawnEntity(CardData cardData, Vector3 spawnPosition, string npcID, EntityAffiliation affiliation, bool hasTurn = true)
        {
            List<GameplayRef> refs = new();
            refs.Add(GameplayRef.onSummon);

        GameObject spawnObj = GameObject.Instantiate(AssetManager.Instance.entityPrefab, position: spawnPosition, rotation: Quaternion.identity);
        EntityOnMap entityOnMap = spawnObj.GetComponent<EntityOnMap>();

            entityOnMap.TeleportTo(spawnPosition);

            NonPlayerScript spawnedEntity = spawnObj.GetComponent<NonPlayerScript>();
            spawnedEntity.NpcID             = npcID;      // must be set before StartUp
            spawnedEntity.entityAffiliation = affiliation;
            spawnedEntity.StartUp();

            if (hasTurn == true)
            {
                TurnManager.Instance.AddTurn(spawnedEntity);
            }

            HandlePostCombatTrigger(refs, cardData.Owner, spawnedEntity, cardData);
        }

        public static void SpawnGroundEffect(CardData cardData, Vector3 spawnPosition, GroundEffectDataBase groundEffectData)
        {
            List<GameplayRef> refs = new();
            GameObject SpawnObj = GameObject.Instantiate(AssetManager.Instance.groundEffectPrefab, parent: cardData.Owner.transform.parent);
            GroundEffectScript groundEffectScript = SpawnObj.GetComponent<GroundEffectScript>();
            SpawnObj.transform.position =spawnPosition;
            groundEffectScript.EffectData = groundEffectData;
            HandlePostCombatTrigger(refs, cardData.Owner, null, cardData);
        }
        #endregion



        // Handles VFX instantiation for damage application.
        private static void HandleOnDamageVFX(VFXData vfxData, EntityScript target)
        {
            if (vfxData == null) return;
            if (vfxData.attachToMesh)
            {
                vfxData.mesh = target.EntityModel.mesh;
                AssetManager.Instance.CreateVFXAttachedToEntityMesh(vfxData, target); return;
            }
            else
            {
                AssetManager.Instance.CreateVFXAttachedToGameObjects(vfxData, new List<EntityScript>() { target });
            }
        }


        #region Post Combat Trigger Handlers

        // Handles triggering any relevant pre-combat triggers when a card is played. This includes onCardPlayed, as well as any triggers related to the card's class, type, or identities.
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
       
        // Handles triggering any relevant post-combat triggers after damage, healing, buffs, debuffs, or entity modifiers have been applied.
        public static void HandlePostCombatTrigger(List<GameplayRef> gameplayRefs, EntityScript user, EntityScript target, CardData cardData = null, int throughput = 0)
        {
            List<GameplayRef> refs = new() {};
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
        #endregion
    }
