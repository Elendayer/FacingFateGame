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

            float hpCur = EntityStatReader.TryGetStat(boundEntity, new[] { "HP", "Health", "health" }, -1f);
            float hpMax = EntityStatReader.TryGetStat(boundEntity, new[] { "MaxHP", "MaxHealth", "healthMax" }, -1f);

            if (hpText != null)
            {
                if (hpCur >= 0f && hpMax > 0f) hpText.text = $"{hpCur:0}/{hpMax:0}";
                else if (hpCur >= 0f) hpText.text = $"{hpCur:0}";
                else hpText.text = "-";
            }

            if (hpFill != null)
            {
                if (hpCur >= 0f && hpMax > 0f) hpFill.fillAmount = Mathf.Clamp01(hpCur / hpMax);
                else hpFill.fillAmount = 0f;
            }

            statusBar?.Refresh();
        }
    }
}
