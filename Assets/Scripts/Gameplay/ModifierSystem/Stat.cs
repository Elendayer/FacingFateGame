using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace facingfate
{
    public class Stat
    {
        public EntityScript owner;

        public int Value(EntityScript entityScript = null, CardData cardData = null) => GetFinalValue(entityScript, cardData);

    public readonly List<IStatModifier> statModifiers = new();
    public void AddModifier(IStatModifier modifier, ModifierMergeStrategy strategy = ModifierMergeStrategy.Override)
    {
        //Debug.Log($"[Stat] Adding modifier: {modifier.ModifierName} with strategy: {strategy} for {modifier.BaseValue}");
        var existing = statModifiers.FirstOrDefault(m => m.ModifierName == modifier.ModifierName);

        switch (strategy)
        {
            //Debug.Log($"[Stat] Adding modifier: {modifier.ModifierName} with strategy: {strategy} for {modifier.BaseValue}");
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

        public int GetFlatValue()
        {
            int baseValue = 0;
            foreach (var mod in statModifiers.Where(m => !m.IsExpired))
            {
                if (mod is StatModifier statMod && statMod.ModifierScaling == ModifierScaling.Flat)
                {
                    baseValue += statMod.BaseValue;
                }
            }
            return baseValue;
        }
        public int GetPercentValue()
        {
            int percentValue = 0;
            foreach (var mod in statModifiers.Where(m => !m.IsExpired))
            {
                if (mod is StatModifier statMod && statMod.ModifierScaling == ModifierScaling.Percent)
                {
                    percentValue += statMod.BaseValue;
                }
            }
            return percentValue;
        }
        public List<int> GetMultiplierValues()
        {
            List<int> multipliers = new();
            foreach (var mod in statModifiers.Where(m => !m.IsExpired))
            {
                if (mod is StatModifier statMod && statMod.ModifierScaling == ModifierScaling.Multiplier)
                {
                    multipliers.Add(statMod.BaseValue);
                }
            }
            return multipliers;

        }


    private int GetFinalValue(EntityScript entityScript = null, CardData cardData = null)
    {
        int BaseValue = 0;
        int percent = 0;
        List<int> multipliers = new();

        foreach (var mod in statModifiers.Where(m => !m.IsExpired))
        {
            // Check for conditional modifier and its condition
            if (mod.Condition(entityScript, cardData))
            {
                if (mod.ModifierName == "MeleeRangeIncrease")
                {
                    Debug.Log($"[Stat] Applying modifier: {mod.ModifierName} with value: {mod.BaseValue}");
                    Debug.Log(mod.Condition(entityScript, cardData));
                }

                // Check for conditional modifier and its condition
                if (mod.Condition(entityScript, cardData))
                {
                    // Apply modifier based on its scaling type
                    if (mod is StatModifier statMod)
                    {
                        switch (statMod.ModifierScaling)
                        {
                            case ModifierScaling.Flat:
                                BaseValue += statMod.BaseValue;
                                break;
                            case ModifierScaling.Percent:
                                percent += statMod.BaseValue;
                                break;
                            case ModifierScaling.Multiplier:
                                multipliers.Add(statMod.BaseValue);
                                break;
                        }
                    }
                }
            }

        BaseValue = BaseValue * (100 + percent) / 100;
        float floatBaseValue = (float)BaseValue;

        foreach (var mult in multipliers)
        {
            float pM = mult;
            float pMulti = pM / 100f;

            floatBaseValue = floatBaseValue * pMulti;
        }

        BaseValue = Mathf.RoundToInt(floatBaseValue);

        return BaseValue;
    }

    public int ApplyFinalValue(int value, EntityScript entityScript = null, CardData cardData = null)
    {
        int baseValue = value;
        int percent = 0;
        List<int> multipliers = new();

        foreach (IStatModifier mod in statModifiers.Where(m => !m.IsExpired))
        {
            int baseValue = value;
            int percent = 0;
            List<int> multipliers = new();

            foreach (IStatModifier mod in statModifiers.Where(m => !m.IsExpired))
            {
                if (mod.ModifierName == "MeleeRangeIncrease")
                {
                    Debug.Log($"[Stat] Applying modifier: {mod.ModifierName} with value: {mod.BaseValue}");
                    Debug.Log(mod.Condition(entityScript, cardData));
                }

                // Check for conditional modifier and its condition
                if (mod.Condition(entityScript, cardData))
                {
                    // Apply modifier based on its scaling type
                    if (mod is StatModifier statMod)
                    {
                        switch (statMod.ModifierScaling)
                        {
                            case ModifierScaling.Flat:
                                baseValue += statMod.BaseValue;
                                break;
                            case ModifierScaling.Percent:
                                percent += statMod.BaseValue;
                                break;
                            case ModifierScaling.Multiplier:
                                multipliers.Add(statMod.BaseValue);
                                break;
                        }
                    }
                }
            }

            baseValue = baseValue * (100 + percent) / 100;

            foreach (var mult in multipliers)
            {
                baseValue = (baseValue * mult) / 100;
            }

            return baseValue;
        }

        baseValue = baseValue * (100 + percent) / 100;

        float floatBaseValue = (float)baseValue;

        foreach (var mult in multipliers)
        {
            floatBaseValue = (floatBaseValue * mult) / 100;
        }

        baseValue = Mathf.RoundToInt(floatBaseValue);

        return baseValue;
    }

        public IStatModifier GetModifierByName(string name)
            => statModifiers.FirstOrDefault(m => m.ModifierName == name && !m.IsExpired);

        public void AddOrReplaceModifier(IStatModifier modifier)
        {
            var existing = statModifiers.FirstOrDefault(m => m.ModifierName == modifier.ModifierName);
            if (existing != null) statModifiers.Remove(existing);
            statModifiers.Add(modifier);
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