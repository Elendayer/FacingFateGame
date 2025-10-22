using UnityEngine;
using System.Collections.Generic;

namespace Utility
{
    public static class CombatUtility
    {
        public static void ApplyDamage(EntityScript user, EntityScript target, int rawDamage, bool isAttack = false)
        {
            if (isAttack)
                GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onAttack }, user.GetInstanceID(), target.GetInstanceID()));

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
                    GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onDamage }, user.GetInstanceID(), target.GetInstanceID()));

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

            VenomUtility.TryConsumeAndApplyOnHit(user, target);
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
            GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onBuffed }, user.GetInstanceID(), target.GetInstanceID()));

            targetStat.AddModifier(mod, mergeStrategy);
        }
        public static void ApplyDebuff(EntityScript user, EntityScript target, Stat targetStat, IStatModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onDebuffed }, user.GetInstanceID(), target.GetInstanceID()));

            targetStat.AddModifier(mod, mergeStrategy);
        }
        // CombatUtility.cs

        public static void ApplyEntityModifier(
            EntityScript user,
            EntityScript target,
            EntityModifier mod,
            ModifierMergeStrategy mergeStrategy)
        {
            if (target == null || mod == null) return;

            // Falls dein Mod Events nutzt
            mod.AddListener();

            // Deine bestehende Pipeline
            target.AddModifier(mod, mergeStrategy);

            // Optional: Inspector-Debug (nur wenn du StatusDebugView nutzt)
            target.GetComponent<StatusDebugView>()?.Track(mod);
        }

        // Wrapper für Altaufrufe mit dem Interface
        public static void ApplyEntityModifier(
            EntityScript user,
            EntityScript target,
            IEntityModifier mod,
            ModifierMergeStrategy mergeStrategy)
        {
            if (target == null || mod == null) return;
            if (mod is EntityModifier em)
                ApplyEntityModifier(user, target, em, mergeStrategy);
            else
                Debug.LogWarning($"[CombatUtility] Unsupported IEntityModifier '{mod.GetType().Name}'. Use ApplyBuff(...) for Stat modifiers.");
        }




        public static EntityScript FindEntityById(int instanceId)
        {
            var all = Object.FindObjectsByType<EntityScript>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].GetInstanceID() == instanceId)
                    return all[i];
            return null;
        }

        // Einmaliger "Next Hit" → trägt DoT am getroffenen Ziel auf
        public static void ApplyNextHitDot(EntityScript user, int tick, int duration, string effectName, gameplayRef tickRef)
        {
            if (user == null) return;

            bool consumed = false;
            var arm = new EntityModifier(
                statName: $"NextHit:{effectName}",
                baseValue: tick,
                to_Trigger_refs: new() { tickRef },        // nur dokumentarisch
                duration: 1,
                target: user.entityStats.CurrentHealth,
                triggerConditionRef: new TriggerRef
                {
                    References = new() { gameplayRef.onHitLanded },
                    AffectedEntityId = user.GetInstanceID()
                },
                onRefEventAction: (mod, stat, gRef) =>
                {
                    if (consumed) return;
                    consumed = true;

            // NEU: Ziel kommt aus dem gemerkten Trigger
            var hit = FindEntityById(mod.LastTriggerRef.AffectedEntityId);
                    if (hit == null) return;

                    int dur = duration > 0 ? duration : 6;
                    int val = Mathf.Max(1, mod.BaseValue);

                    var dot = new EntityModifier(
                        statName: effectName,                     // "Poison"/"Burn"/"Bleed"
                        baseValue: val,
                        to_Trigger_refs: new() { tickRef },
                        duration: dur,
                        target: hit.entityStats.CurrentHealth,
                        triggerConditionRef: new TriggerRef
                        {
                            References = new() { gameplayRef.onTurnStart },
                            AffectedEntityId = hit.GetInstanceID()
                        },
                        onRefEventAction: (m2, s2, g2) =>
                        {
                            GameEvents.TriggerRefEvent(new TriggerRef
                            {
                                References = new() { tickRef },
                                UserId = user.GetInstanceID(),
                                AffectedEntityId = hit.GetInstanceID()
                            });
                            ApplyDamage(user, hit, m2.BaseValue);
                        });

            // wie bei Needlestorm gewünscht: Merge
            ApplyEntityModifier(user, hit, dot, ModifierMergeStrategy.Merge);

            // sich selbst „verbrauchen“
            mod.Duration = 0;
                });

            // mehrere Next-Hit-Mods dürfen koexistieren
            ApplyEntityModifier(user, user, arm, ModifierMergeStrategy.Merge);
        }


        public static void ApplyNextHitStatusWithCharges(
            EntityScript user,
            int duration,
            string effectName,
            gameplayRef statusRef,  
            int charges)
        {
            if (user == null || charges <= 0) return;

            duration = Mathf.Max(1, duration);

            for (int i = 0; i < charges; i++)
                ApplyNextHitStatus(user, duration, effectName, statusRef);
        }

        private static void ApplyNextHitStatus(
            EntityScript user,
            int duration,
            string effectName,
            gameplayRef statusRef)    
        {
            if (user == null) return;

            bool consumed = false;

            var arm = new EntityModifier(
                statName: $"NextHit:{effectName}",
                baseValue: 0,
                to_Trigger_refs: new() { statusRef },   
                duration: 1,
                target: user.entityStats.CurrentHealth,
                triggerConditionRef: new TriggerRef
                {

            References = new() { gameplayRef.onHitLanded },
                    AffectedEntityId = user.GetInstanceID()
                },
                onRefEventAction: (mod, stat, _gRef) =>
                {
                    if (consumed) return;
                    consumed = true;

            var hit = FindEntityById(mod.LastTriggerRef.AffectedEntityId);
                    if (hit == null) { mod.Duration = 0; return; }

            var status = new EntityModifier(
                        statName: effectName,               
                        baseValue: 1,
                        to_Trigger_refs: new() { statusRef }, 
                        duration: duration,
                        target: hit.entityStats.CurrentHealth,
                        triggerConditionRef: new TriggerRef
                        {
                            References = new() { gameplayRef.onTurnStart },
                            AffectedEntityId = hit.GetInstanceID()
                        },
                        onRefEventAction: (m2, s2, _g2) =>
                        {
                    // hier nur das Status-Event feuern; Auswirkung (z.B. Skip Turn) macht dein Turn/AI-System
                    GameEvents.TriggerRefEvent(new TriggerRef
                            {
                                References = new() { statusRef },
                                UserId = user.GetInstanceID(),
                                AffectedEntityId = hit.GetInstanceID()
                            });
                        });

            // Status anwenden (kein Verlängern nötig) – einfache Zusammenführung genügt
            ApplyEntityModifier(user, hit, status, ModifierMergeStrategy.Merge);

            // diese „Arming“-Charge verbrauchen
            mod.Duration = 0;
                });

            // Den Arming-Mod auf den User legen
            ApplyEntityModifier(user, user, arm, ModifierMergeStrategy.Merge);
        }

        public static EntityModifier CreateDot(
            EntityScript user,
            EntityScript target,
            string effectName,
            int tickValue,
            int duration,
            gameplayRef procRef)
        {
            return new EntityModifier(
                statName: effectName,
                baseValue: tickValue,
                to_Trigger_refs: new() { procRef },
                duration: duration,
                target: target.entityStats.CurrentHealth,
                triggerConditionRef: new TriggerRef
                {
                    References = new() { gameplayRef.onTurnStart },
                    AffectedEntityId = target.GetInstanceID()
                },
                onRefEventAction: (mod, stat, ev) =>
                {
            // optionales „procced“-Event
            GameEvents.TriggerRefEvent(new TriggerRef
                    {
                        References = new() { procRef },
                        UserId = user.GetInstanceID(),
                        AffectedEntityId = target.GetInstanceID()
                    });

            // Tick-Schaden
            CombatUtility.ApplyDamage(user, target, mod.BaseValue);
                }
            );
        }

        public static void ApplyDot(
            EntityScript user,
            EntityScript target,
            string effectName,
            int tickValue,
            int duration,
            gameplayRef procRef,
            ModifierMergeStrategy merge,
            bool immediateTick = false)
        {
            var mod = CreateDot(user, target, effectName, tickValue, duration, procRef);

            // deine bestehende Pipeline; hier drin optional StatusDebugView.Track(mod)
            ApplyEntityModifier(user, target, mod, merge);

            if (immediateTick)
            {
                ApplyDamage(user, target, tickValue);
                GameEvents.TriggerRefEvent(new TriggerRef
                {
                    References = new() { procRef },
                    UserId = user.GetInstanceID(),
                    AffectedEntityId = target.GetInstanceID()
                });
            }
        }

    }
}