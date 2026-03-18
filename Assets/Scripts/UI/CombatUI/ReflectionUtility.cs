using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace facingfate
{

public static class ReflectionUtility
    {
        public static Type FindTypeByName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return null;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type[] types;
                try { types = assemblies[i].GetTypes(); }
                catch { continue; }

                for (int t = 0; t < types.Length; t++)
                {
                    if (types[t].Name == typeName) return types[t];
                }
            }

            return null;
        }

        public static object TryGetStaticFieldOrProperty(Type type, string name)
        {
            if (type == null || string.IsNullOrWhiteSpace(name)) return null;

            try
            {
                var f = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (f != null) return f.GetValue(null);

                var p = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (p != null) return p.GetValue(null);
            }
            catch { }

            return null;
        }

        public static Transform TryGetTransformFieldOrProperty(object obj, string name)
        {
            object v = TryGetFieldOrProperty(obj, name);
            return v as Transform;
        }

        public static object TryGetFieldOrProperty(object obj, string name)
        {
            if (obj == null || string.IsNullOrWhiteSpace(name)) return null;

            try
            {
                Type type = obj.GetType();

                while (type != null)
                {
                    var f = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    if (f != null) return f.GetValue(obj);

                    var p = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    if (p != null) return p.GetValue(obj);

                    type = type.BaseType;
                }
            }
            catch { }

            return null;
        }

        // Safe: liest TurnOrder + CurrentTurnIndex ohne CurrentTurnEntity-Getter zu triggern
        public static Component TryGetCurrentTurnEntity(MonoBehaviour turnManager)
        {
            if (turnManager == null) return null;

            try
            {
                object turnOrderObj = TryGetFieldOrProperty(turnManager, "TurnOrder");
                object idxObj = TryGetFieldOrProperty(turnManager, "CurrentTurnIndex");

                if (turnOrderObj is IList list && list.Count > 0)
                {
                    int idx = 0;
                    if (idxObj is int i) idx = i;
                    else int.TryParse(idxObj?.ToString(), out idx);

                    if (idx < 0 || idx >= list.Count) idx = 0;
                    return list[idx] as Component;
                }
            }
            catch { }

            // Fallbacks (alles safe-catched)
            try
            {
                object direct = TryGetFieldOrProperty(turnManager, "currentTurnEntity")
                             ?? TryGetFieldOrProperty(turnManager, "CurrentTurnEntity");
                return direct as Component;
            }
            catch { }

            return null;
        }
    }
}
