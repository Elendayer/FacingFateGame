using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace facingfate
{

    public class StatusEffectBarUI : MonoBehaviour
    {
        [SerializeField] private Transform iconContainer;
        [SerializeField] private StatusEffectIconUI iconPrefab;
        [SerializeField] private int maxIcons = 12;

        private EntityScript boundEntity;
        private readonly List<StatusEffectIconUI> spawned = new();
        private readonly Dictionary<string, int> _maxDurations = new();

        public void Bind(EntityScript entity)
        {
            boundEntity = entity;
        }

        public void Refresh()
        {
            //Debug.Log($"[StatusBar] auf {gameObject.name} – boundEntity={boundEntity?.gameObject.name ?? "NULL"}");
            Clear();

            if (boundEntity == null || iconContainer == null || iconPrefab == null)
            {
                return;
            }

            var modifiers = boundEntity.GetActiveModifiers();
            if (modifiers == null || modifiers.Count == 0) return;

            int shown = 0;
            foreach (var mod in modifiers)
            {
                if (shown >= maxIcons) break;

                string modName = mod.ModifierName;
                if (string.IsNullOrWhiteSpace(modName)) continue;

                int duration = mod.Duration;
                int charges = mod.Charges;
                int baseValue = (int)mod.BaseValue;

                if (duration > 0)
                {
                    if (!_maxDurations.TryGetValue(modName, out int stored) || duration > stored)
                        _maxDurations[modName] = duration;
                }
                int maxDuration = _maxDurations.TryGetValue(modName, out int maxStored) ? maxStored : duration;

                bool isExpired = mod.IsExpired;
                if (isExpired) continue;

                var ui = Instantiate(iconPrefab, iconContainer);

                // Resolve icon + display text; description always comes from the instance
                EffectDatabase.TryGetUIInfo(modName, out _, out Sprite icon);
                string desc = mod.Description;

                ui.SetIcon(icon);
                ui.SetCounters(duration, baseValue, maxDuration);
                ui.SetTooltip(modName, BuildTooltip(modName, desc, duration, charges, baseValue));

                spawned.Add(ui);
                shown++;
            }
        }

        private string BuildTooltip(string title, string desc, float duration, float charges, float baseValue)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (!string.IsNullOrWhiteSpace(desc))
            {
                sb.AppendLine(desc.Trim());
                sb.AppendLine();
            }

            string stats = "";

            if (duration >= 0f && duration < 9999f)
                stats += $"Duration: {duration:0}";

            if (baseValue > 0f)
                stats += (stats.Length > 0 ? "  |  " : "") + $"Damage: {baseValue:0}";

            if (charges >= 0f && charges < 9999f)
                stats += (stats.Length > 0 ? "  |  " : "") + $"Stacks: {charges:0}";

            if (stats.Length > 0)
                sb.AppendLine(stats);

            return sb.ToString().Trim();
        }

        private void Clear()
        {
            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] != null) Destroy(spawned[i].gameObject);
            }
            spawned.Clear();

            if (iconContainer == null) return;
            for (int i = iconContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(iconContainer.GetChild(i).gameObject);
            }
        }
    }
}
