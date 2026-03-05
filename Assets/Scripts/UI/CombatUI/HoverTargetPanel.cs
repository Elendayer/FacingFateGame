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
        [SerializeField] private Image hpFill;
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
                if (hpFill != null) hpFill.fillAmount = 0f;
                statusBar?.Refresh();
                return;
            }

            if (titleText != null) titleText.text = boundEntity.gameObject.name;

            // Robustere Keys: falls deine Stat-Namen variieren
            float hpCur = EntityStatReader.TryGetStat(boundEntity, new[] { "HP", "Health", "health" }, -1f);
            float hpMax = EntityStatReader.TryGetStat(boundEntity, new[] { "MaxHP", "MaxHealth", "HPMax", "healthMax" }, -1f);

            string hpString;
            if (hpCur >= 0f && hpMax > 0f)
                hpString = $"{hpCur:0}/{hpMax:0}";
            else if (hpCur >= 0f)
                hpString = $"{hpCur:0}/??";
            else
                hpString = "-";

            if (hpText != null && hpText.text != hpString)
                hpText.text = hpString;

            if (hpFill != null)
            {
                float fill = (hpCur >= 0f && hpMax > 0f) ? Mathf.Clamp01(hpCur / hpMax) : 0f;
                if (!Mathf.Approximately(hpFill.fillAmount, fill))
                    hpFill.fillAmount = fill;
            }

            statusBar?.Refresh();
        }
    }
}
