using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace facingfate
{

    public class HoverTargetPanel : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private Slider hpSlider;
        [SerializeField] private StatusEffectBarUI statusBar;

        private Component boundEntity;

        public void Bind(Component entity)
        {
            boundEntity = entity;
            if (statusBar != null) statusBar.Bind(entity);
        }

        public void Refresh()
        {
            if (boundEntity == null)
            {
                if (titleText != null) titleText.text = "Hover Target";
                if (hpText != null) hpText.text = "-";
                if (hpSlider != null) SetSlider(hpSlider, 0f, 1f);
                statusBar?.Refresh();
                return;
            }

            if (titleText != null) titleText.text = GetEntityName(boundEntity);

            EntityStatReader.TryGetHealth(boundEntity, out int hpCur, out int hpMax);

            string hpString;
            if (hpCur >= 0f && hpMax > 0f)
                hpString = $"{hpCur:0}/{hpMax:0}";
            else if (hpCur >= 0f)
                hpString = $"{hpCur:0}/??";
            else
                hpString = "-";

            if (hpText != null && hpText.text != hpString)
                hpText.text = hpString;

            if (hpSlider != null)
            {
                SetSlider(hpSlider, hpCur, hpMax);
            }

            statusBar?.Refresh();
        }
        private static string GetEntityName(Component entity)
        {
            // Versucht zuerst NpcData.name über NonPlayerScript zu lesen
            object npcData = ReflectionUtility.TryGetFieldOrProperty(entity, "npcData")
                          ?? ReflectionUtility.TryGetFieldOrProperty(entity, "NpcData");

            if (npcData != null)
            {
                object nameObj = ReflectionUtility.TryGetFieldOrProperty(npcData, "name");
                if (nameObj != null && !string.IsNullOrWhiteSpace(nameObj.ToString()))
                    return nameObj.ToString();
            }

            // Fallback: GameObject-Name
            return entity.gameObject.name;
        }

        private static void SetSlider(Slider s, float current, float max)
        {
            if (s == null) return;
            s.minValue = 0f;
            s.maxValue = max > 0f ? max : 1f;
            s.value = current >= 0f ? current : 0f;
        }
    }
}
