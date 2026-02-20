using System;
using System.Collections;
using System.Collections.Generic;
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

        public static IEnumerable<object> TryGetModifiers(Component entity)
        {
            if (entity == null) return null;

            // try field/property "entityModifiers"
            object listObj = ReflectionUtility.TryGetFieldOrProperty(entity, "entityModifiers")
                          ?? ReflectionUtility.TryGetFieldOrProperty(entity, "EntityModifiers");

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
