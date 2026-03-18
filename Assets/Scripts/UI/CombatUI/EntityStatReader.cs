using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace facingfate
{

    public static class EntityStatReader
    {
        public static float TryGetStat(Component entity, string[] keys, float fallback)
        {
            if (entity == null || keys == null || keys.Length == 0) return fallback;

            object stats = TryGetEntityStatsObject(entity);
            if (stats == null) return fallback;

            for (int i = 0; i < keys.Length; i++)
            {
                if (TryGetStatFromStatsObject(stats, keys[i], out float value))
                    return value;
            }

            return fallback;
        }

        public static bool TryGetHealth(Component entity, out float current, out float max)
        {
            current = -1f;
            max = -1f;

            object statsObj = TryGetEntityStatsObject(entity);
            if (statsObj == null) return false;

            current = TryReadFloat(statsObj,
                "CurrentHealth", "currentHealth", "HealthCurrent", "healthCurrent");

            // MaxHealth ist ein Stat-Objekt mit Value()-Methode → TryReadStatLikeFloat
            max = TryReadStatLikeFloat(statsObj,
                "MaxHealth", "maxHealth", "HealthMax", "healthMax", "MaxHP", "maxHP");

            if (max <= 0f)
            {
                float fb = TryGetStat(entity, new[] { "MaxHealth", "Health", "HP" }, -1f);
                if (fb > 0f) max = fb;
            }
            if (current < 0f)
            {
                float fb = TryGetStat(entity, new[] { "CurrentHealth", "Health", "HP" }, -1f);
                if (fb >= 0f) current = fb;
            }

            return current >= 0f;
        }

        public static bool TryGetStamina(Component entity, out float current, out float max)
        {
            current = -1f;
            max = -1f;

            object statsObj = TryGetEntityStatsObject(entity);
            if (statsObj == null) return false;

            current = TryReadFloat(statsObj,
                "CurrentStamina", "currentStamina", "StaminaCurrent", "staminaCurrent");

            max = TryReadStatLikeFloat(statsObj,
                "MaxStamina", "maxStamina", "StaminaMax", "staminaMax", "MaxEnergy", "maxEnergy");

            if (max <= 0f)
            {
                float fb = TryGetStat(entity, new[] { "MaxStamina", "Stamina", "AP" }, -1f);
                if (fb > 0f) max = fb;
            }
            if (current < 0f)
            {
                float fb = TryGetStat(entity, new[] { "CurrentStamina", "Stamina", "AP" }, -1f);
                if (fb >= 0f) current = fb;
            }

            return current >= 0f;
        }

        public static IEnumerable<object> TryGetModifiers(Component entity)
        {
            if (entity == null) return null;

            // try field/property "entityModifiers"
            object listObj = ReflectionUtility.TryGetFieldOrProperty(entity, "entityModifiers")
                          ?? ReflectionUtility.TryGetFieldOrProperty(entity, "EntityModifiers");

            Debug.Log($"[TryGetModifiers] listObj={listObj?.GetType().Name ?? "NULL"}");

            if (listObj is IEnumerable enumerable)
            {
                List<object> res = new List<object>();
                foreach (object o in enumerable)
                {
                    if (o != null) res.Add(o);
                }
                return res;
            }

            return null;
        }

        public static string TryGetModifierName(object modifier)
        {
            if (modifier == null) return null;
            object v = ReflectionUtility.TryGetFieldOrProperty(modifier, "ModifierName")
                     ?? ReflectionUtility.TryGetFieldOrProperty(modifier, "modifierName")
                     ?? ReflectionUtility.TryGetFieldOrProperty(modifier, "Name");
            return v != null ? v.ToString() : null;
        }

        public static float TryGetModifierFloat(object modifier, string member, float fallback)
        {
            if (modifier == null) return fallback;
            object v = ReflectionUtility.TryGetFieldOrProperty(modifier, member)
                     ?? ReflectionUtility.TryGetFieldOrProperty(modifier, char.ToLowerInvariant(member[0]) + member.Substring(1));
            return TryConvertFloat(v, fallback);
        }

        public static bool TryGetModifierBool(object modifier, string member, bool fallback)
        {
            if (modifier == null) return fallback;
            object v = ReflectionUtility.TryGetFieldOrProperty(modifier, member)
                     ?? ReflectionUtility.TryGetFieldOrProperty(modifier, char.ToLowerInvariant(member[0]) + member.Substring(1));
            return TryConvertBool(v, fallback);
        }

        private static object TryGetEntityStatsObject(Component entity)
        {
            // common patterns: entity.stats, entity.entityStats, entity.EntityStats
            object stats = ReflectionUtility.TryGetFieldOrProperty(entity, "entityStats")
                       ?? ReflectionUtility.TryGetFieldOrProperty(entity, "EntityStats")
                       ?? ReflectionUtility.TryGetFieldOrProperty(entity, "stats")
                       ?? ReflectionUtility.TryGetFieldOrProperty(entity, "Stats");

            if (stats != null) return stats;

            // fallback: if there is a component named "EntityStats"
            Type entityStatsType = ReflectionUtility.FindTypeByName("EntityStats");
            if (entityStatsType != null)
            {
                var comp = entity.GetComponent(entityStatsType);
                if (comp != null) return comp;
            }

            return null;
        }

        private static bool TryGetStatFromStatsObject(object stats, string key, out float value)
        {
            value = 0f;
            if (stats == null || string.IsNullOrWhiteSpace(key)) return false;

            Type t = stats.GetType();

            // method: GetStat(string)
            MethodInfo getStat = t.GetMethod("GetStat", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (getStat != null)
            {
                var parameters = getStat.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                {
                    object statObj = getStat.Invoke(stats, new object[] { key });
                    if (TryReadStatValue(statObj, out value)) return true;
                }
            }

            // field/property holding list/dict of stats
            object container = ReflectionUtility.TryGetFieldOrProperty(stats, "stats")
                            ?? ReflectionUtility.TryGetFieldOrProperty(stats, "Stats");
            if (container is IEnumerable enumerable)
            {
                foreach (object s in enumerable)
                {
                    if (s == null) continue;

                    object nameObj = ReflectionUtility.TryGetFieldOrProperty(s, "Name")
                                  ?? ReflectionUtility.TryGetFieldOrProperty(s, "StatName")
                                  ?? ReflectionUtility.TryGetFieldOrProperty(s, "statName");

                    if (nameObj == null) continue;

                    if (string.Equals(nameObj.ToString(), key, StringComparison.OrdinalIgnoreCase))
                    {
                        if (TryReadStatValue(s, out value)) return true;
                    }
                }
            }

            return false;
        }

        private static float TryReadFloat(object obj, params string[] members)
        {
            if (obj == null || members == null) return -1f;

            for (int i = 0; i < members.Length; i++)
            {
                object v = ReflectionUtility.TryGetFieldOrProperty(obj, members[i]);
                float f = TryConvertFloat(v, -1f);
                if (f >= 0f) return f;
            }

            return -1f;
        }

        private static float TryReadStatLikeFloat(object obj, params string[] members)
        {
            if (obj == null || members == null) return -1f;

            for (int i = 0; i < members.Length; i++)
            {
                object v = ReflectionUtility.TryGetFieldOrProperty(obj, members[i]);
                if (v == null) continue;

                // Direkter float
                float direct = TryConvertFloat(v, -1f);
                if (direct >= 0f) return direct;

                // Stat-Objekt mit Value()-Methode (z.B. EntityStats.MaxHealth.Value())
                Type vType = v.GetType();
                MethodInfo valueMethod = vType.GetMethod("Value",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (valueMethod != null && valueMethod.GetParameters().All(p => p.IsOptional))
                {
                    // Optionale Parameter müssen explizit mit Type.Missing übergeben werden
                    var paramInfos = valueMethod.GetParameters();
                    object[] args = new object[paramInfos.Length];
                    for (int j = 0; j < paramInfos.Length; j++)
                        args[j] = Type.Missing;

                    object result = valueMethod.Invoke(v, args);
                    float fromMethod = TryConvertFloat(result, -1f);
                    if (fromMethod >= 0f) return fromMethod;
                }

                // Stat-Objekt mit Value-Property
                if (TryReadStatValue(v, out float fromProp)) return fromProp;
            }

            return -1f;
        }

        private static bool TryReadStatValue(object statObj, out float value)
        {
            value = 0f;
            if (statObj == null) return false;

            object v = ReflectionUtility.TryGetFieldOrProperty(statObj, "Value")
                     ?? ReflectionUtility.TryGetFieldOrProperty(statObj, "CurrentValue")
                     ?? ReflectionUtility.TryGetFieldOrProperty(statObj, "ModifiedValue")
                     ?? ReflectionUtility.TryGetFieldOrProperty(statObj, "BaseValue");

            if (v == null) return false;

            value = TryConvertFloat(v, 0f);
            return true;
        }

        private static float TryConvertFloat(object v, float fallback)
        {
            if (v == null) return fallback;

            try
            {
                if (v is float f) return f;
                if (v is int i) return i;
                if (v is double d) return (float)d;
                if (float.TryParse(v.ToString(), out float parsed)) return parsed;
            }
            catch { }

            return fallback;
        }

        private static bool TryConvertBool(object v, bool fallback)
        {
            if (v == null) return fallback;

            try
            {
                if (v is bool b) return b;
                if (bool.TryParse(v.ToString(), out bool parsed)) return parsed;
            }
            catch { }

            return fallback;
        }
    }
}
