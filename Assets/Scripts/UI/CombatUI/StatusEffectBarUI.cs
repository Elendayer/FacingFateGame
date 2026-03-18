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
            Debug.Log($"[StatusBar] auf {gameObject.name} – boundEntity={boundEntity?.gameObject.name ?? "NULL"}");
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

                float duration = EntityStatReader.TryGetModifierFloat(mod, "Duration", -1f);
                float charges = EntityStatReader.TryGetModifierFloat(mod, "Charges", -1f);
                float baseValue = EntityStatReader.TryGetModifierFloat(mod, "BaseValue", -1f);

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

            if (duration >= 0f) sb.AppendLine($"Duration: {duration:0}");
            if (charges >= 0f) sb.AppendLine($"Stacks: {charges:0}");
            if (baseValue >= 0f) sb.AppendLine($"Value: {baseValue:0}");

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
