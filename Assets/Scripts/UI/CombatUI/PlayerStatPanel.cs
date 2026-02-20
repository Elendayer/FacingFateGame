using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace facingfate
{
    public class PlayerStatsPanel : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private Image hpFill;
        [SerializeField] private TMP_Text staminaText;
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
                if (nameText != null) nameText.text = "-";
                if (hpText != null) hpText.text = "-";
                if (staminaText != null) staminaText.text = "-";
                if (hpFill != null) hpFill.fillAmount = 0f;
                statusBar?.Refresh();
                return;
            }

            if (nameText != null) nameText.text = boundEntity.gameObject.name;

            // Best-effort stat read (robust against unknown stat model)
            float hpCur = EntityStatReader.TryGetStat(boundEntity, new[] { "HP", "Health", "health" }, -1f);
            float hpMax = EntityStatReader.TryGetStat(boundEntity, new[] { "MaxHP", "MaxHealth", "healthMax" }, -1f);
            float stamina = EntityStatReader.TryGetStat(boundEntity, new[] { "Stamina", "Energy", "AP" }, -1f);

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

            if (staminaText != null)
            {
                if (stamina >= 0f) staminaText.text = stamina.ToString("0");
                else staminaText.text = "-";
            }

            statusBar?.Refresh();
        }
    }
}
