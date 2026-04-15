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

        private Component boundEntity;
        private readonly List<StatusEffectIconUI> spawned = new();

        public void Bind(Component entity)
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

            var modifiers = EntityStatReader.TryGetModifiers(boundEntity);
            if (modifiers == null) return;

            int shown = 0;
            foreach (object mod in modifiers)
            {
                if (shown >= maxIcons) break;

                string modName = EntityStatReader.TryGetModifierName(mod);
                if (string.IsNullOrWhiteSpace(modName)) continue;

                int duration = EntityStatReader.TryGetModifierInt(mod, "Duration", -1);
                int charges = EntityStatReader.TryGetModifierInt(mod, "Charges", -1);
                int baseValue = EntityStatReader.TryGetModifierInt(mod, "BaseValue", -1);

                bool isExpired = EntityStatReader.TryGetModifierBool(mod, "IsExpired", false);
                if (isExpired) continue;

                var ui = Instantiate(iconPrefab, iconContainer);

                // Resolve icon + display text
                EffectDatabase.TryGetUIInfo(modName, out string desc, out Sprite icon);

                ui.SetIcon(icon);
                ui.SetCounters(duration, baseValue);
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
