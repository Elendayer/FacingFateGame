using UnityEngine;
using System.Collections.Generic;

namespace Utility
{
    public static class CombatUtility
    {
        public static void ApplyDamage(EntityScript user, EntityScript target, int rawDamage, bool isAttack = false)
        {
            if (isAttack)
            {
                GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onAttack }, user.GetInstanceID(), target.GetInstanceID()));
            }

            // Pre-mitigation damage reduction (e.g., from abilities or effects)
            int damage = rawDamage;
            rawDamage = target.entityStats.DamageReduction.ApplyFinalValue(rawDamage);

            // Step 1: Apply Armour
            if (target.entityStats.Armour.Value > 0)
            {
                damage = Mathf.Max(0, damage - (target.entityStats.IgnoreArmour.ApplyFinalValue(target.entityStats.Armour.Value)));
            }

            // Step 2: Apply to Block
            int block = target.entityStats.IgnoreBlock.ApplyFinalValue(target.entityStats.Block.Value);
            if (block > 0)
            {
                GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onBlocking }, user.GetInstanceID(), target.GetInstanceID()));

                int blockAbsorb = Mathf.Min(damage, block);
                target.entityStats.Block.AddModifier(new StatModifier(-blockAbsorb, ModifierScaling.Flat, name: "BaseValue"), ModifierMergeStrategy.Merge);
                damage -= blockAbsorb;
            }

            // Step 3: Apply to Health
            if (damage > 0)
            {
                if (isAttack)
                {
                    GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onDamage }, user.GetInstanceID(), target.GetInstanceID()));
                }

                target.entityStats.CurrentHealth.AddModifier(
                    new StatModifier(-damage, ModifierScaling.Flat, name: "BaseValue"),
                    ModifierMergeStrategy.Merge);

                GameEvents.TriggerRefEvent(new TriggerRef(
                    new() { gameplayRef.onHitLanded },
                    user.GetInstanceID(),
                    target.GetInstanceID()));
            }



            if (user.entityStats.Lifesteal.GetAllValues().Count > 0)
            {
                GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onLifesteal }, user.GetInstanceID(), target.GetInstanceID()));
                ApplyHealing(user, user, Mathf.CeilToInt(damage * (user.entityStats.Lifesteal.Value / 100f)));
            }        
        }

        public static void ApplyHealing(EntityScript user, EntityScript target, int healing)
        {
            GameEvents.TriggerRefEvent(new TriggerRef(new() { gameplayRef.onHeal }, user.GetInstanceID(), target.GetInstanceID()));

            target.entityStats.CurrentHealth.AddModifier(new StatModifier(Mathf.Min
                (
                target.entityStats.CurrentHealth.Value + healing,
                target.entityStats.MaxHealth.Value
                ), ModifierScaling.Flat, name: "BaseValue"), ModifierMergeStrategy.AddUnique);
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
        public static void ApplyEntityModifier(EntityScript user, EntityScript target, IEntityModifier mod, ModifierMergeStrategy mergeStrategy)
        {
            target.AddModifier(mod, mergeStrategy);
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

        // ---- Next-Hit STATUS (z.B. Stun) ---------------------------------------------

        // Einfacher Eintrag: rüste die nächsten 'charges' Treffer mit einem Status aus.
        // Bitte beim Aufruf gameplayRef.onStunned übergeben.
        public static void ApplyNextHitStatusWithCharges(
            EntityScript user,
            int duration,
            string effectName,
            gameplayRef statusRef,   // z.B. gameplayRef.onStunned
            int charges)
        {
            if (user == null || charges <= 0) return;

            duration = Mathf.Max(1, duration);

            for (int i = 0; i < charges; i++)
                ApplyNextHitStatus(user, duration, effectName, statusRef);
        }

        // Eine Charge „armen“: beim nächsten onHitLanded wird der Status auf das getroffene Ziel gelegt.
        private static void ApplyNextHitStatus(
            EntityScript user,
            int duration,
            string effectName,
            gameplayRef statusRef)    // z.B. gameplayRef.onStunned
        {
            if (user == null) return;

            bool consumed = false;

            var arm = new EntityModifier(
                statName: $"NextHit:{effectName}",
                baseValue: 0,
                to_Trigger_refs: new() { statusRef },   // rein dokumentarisch
                duration: 1,
                target: user.entityStats.CurrentHealth,
                triggerConditionRef: new TriggerRef
                {
            // WICHTIG: Wir hören auf den TREFFER des Users
            References = new() { gameplayRef.onHitLanded },
                    AffectedEntityId = user.GetInstanceID()
                },
                onRefEventAction: (mod, stat, _gRef) =>
                {
                    if (consumed) return;
                    consumed = true;

            // getroffene Einheit ermitteln (vom zuletzt ausgelösten Trigger)
            var hit = FindEntityById(mod.LastTriggerRef.AffectedEntityId);
                    if (hit == null) { mod.Duration = 0; return; }

            // Status-Modifier auf das Ziel: Dauer 'duration', triggert pro Rundenstart ein Event
            var status = new EntityModifier(
                        statName: effectName,                 // z.B. "Stun"
                        baseValue: 1,
                        to_Trigger_refs: new() { statusRef }, // z.B. onStunned
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

    }
}