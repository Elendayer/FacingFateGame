using System;
using System.Collections.Generic;
using UnityEngine;

namespace facingfate
{
    [CreateAssetMenu(menuName = "Combat UI/Status Icon Database", fileName = "StatusIconDatabase")]
    public class StatusIconDatabase : ScriptableObject
    {
        public List<StatusIconEntry> entries = new();

        public bool TryGet(string modifierName, out StatusIconEntry entry)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (string.Equals(entries[i].modifierName, modifierName, StringComparison.OrdinalIgnoreCase))
                {
                    entry = entries[i];
                    return true;
                }
            }

            entry = default;
            return false;
        }
    }

    [Serializable]
    public struct StatusIconEntry
    {
        public string modifierName;
        public string displayName;
        [TextArea(1, 8)] public string description;
        public Sprite icon;
    }
}
