using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace facingfate
{
    public class Stat
    {
        public EntityScript owner;

        public float Value(EntityScript entityScript = null, CardData cardData = null) => GetFinalValue(entityScript, cardData);

        public readonly List<IStatModifier> statModifiers = new();

        public void AddModifier(IStatModifier modifier, ModifierMergeStrategy strategy = ModifierMergeStrategy.Override)
        {
            var existing = statModifiers.FirstOrDefault(m => m.ModifierName == modifier.ModifierName);

            switch (strategy)
            {
                case ModifierMergeStrategy.AddUnique:
                    statModifiers.Add(modifier);
                    break;

                case ModifierMergeStrategy.Override:
                    if (existing != null) statModifiers.Remove(existing);
                    statModifiers.Add(modifier);
                    break;

                case ModifierMergeStrategy.Merge:
                    if (existing is StatModifier existingMod && modifier is StatModifier newMod)
                    {
                        existingMod.BaseValue += newMod.BaseValue;
                    }
                    else
                    {
                        statModifiers.Add(modifier);
                    }
                    break;

                case ModifierMergeStrategy.RefreshDurationAndMerge:
                    if (existing is StatModifier existingRefresh && modifier is StatModifier newRefresh)
                    {
                        existingRefresh.BaseValue += newRefresh.BaseValue;
                        existingRefresh.Duration = Math.Max(existingRefresh.GetRemainingDuration(), newRefresh.GetRemainingDuration());
                    }
                    else
                    {
                        statModifiers.Add(modifier);
                    }
                    break;

                case ModifierMergeStrategy.RefreshDurationAndOverride:
                    if (existing is StatModifier existingRefreshDuration && modifier is StatModifier newRefreshDuration)
                    {
                        existingRefreshDuration.BaseValue = Mathf.Max(existingRefreshDuration.BaseValue, newRefreshDuration.BaseValue);
                        existingRefreshDuration.Duration = Math.Max(existingRefreshDuration.GetRemainingDuration(), newRefreshDuration.GetRemainingDuration());
                    }
                    else
                    {
                        statModifiers.Add(modifier);
                    }
                    break;
            }
            modifier.Init();
        }

        public void RemoveModifier(IStatModifier modifier)
        {
            modifier.OnRemove();
        }

        private float GetFinalValue(EntityScript entityScript = null, CardData cardData = null)
        {
            var entity = entityScript ?? owner;
            float baseValue = 0;

            foreach (var mod in statModifiers.Where(m => !m.IsExpired))
            {
                var statMod = mod as StatModifier;
                if (statMod != null && statMod.EvaluateCondition(entity, cardData))
                {
                    baseValue += statMod.BaseValue;
                }
            }

            return baseValue;
        }

        public List<float> GetAllMultiplierValues(EntityScript entityScript = null, CardData cardData = null)
        {
            var entity = entityScript ?? owner;
            return statModifiers
                .Where(m => !m.IsExpired && m is StatModifier statMod && statMod.EvaluateCondition(entity, cardData))
                .Cast<StatModifier>()
                .Select(m => m.BaseValue)
                .ToList();
        }

        public bool HasReference(GameplayRef reference)
            => statModifiers.Any(m => m.To_TriggerGameplayRefs.Contains(reference) && !m.IsExpired);

        public IStatModifier GetModifierByName(string name)
            => statModifiers.FirstOrDefault(m => m.ModifierName == name && !m.IsExpired);

        public void AddOrReplaceModifier(IStatModifier modifier)
        {
            var existing = statModifiers.FirstOrDefault(m => m.ModifierName == modifier.ModifierName);
            if (existing != null) statModifiers.Remove(existing);
            statModifiers.Add(modifier);
        }

        public void Tick()
        {
            foreach (IStatModifier mod in statModifiers)
            {
                if (mod != null)
                {
                    mod.Tick();
                }
            }
        }

        public void UpdateStat()
        {
            foreach (IStatModifier mod in statModifiers)
            {
                mod.UpdateStatModifier();
            }
        }
    }


    // -------------------- Enums --------------------

    public enum ModifierMergeStrategy
    {
        AddUnique,
        Override,
        Merge,
        RefreshDurationAndMerge,
        RefreshDurationAndOverride
    }

    public enum ModifierScaling
    {
        Flat,
        Percent,
        Multiplier
    }

    public enum StatAspect
    {
        Power,
        Damage,
        Healing,
        Cost,
        Duration,
        Repeats,
        Range
    }
}