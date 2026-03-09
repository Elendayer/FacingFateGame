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
                SetText(nameText, "-"); SetText(hpText, "-"); SetText(staminaText, "-");
                SetFill(hpFill, 0f);
                statusBar?.Refresh();
                return;
            }

            SetText(nameText, GetEntityName(boundEntity));

            if (EntityStatReader.TryGetHealth(boundEntity, out float hpCur, out float hpMax))
            {
                SetText(hpText, hpMax > 0f ? $"{hpCur:0}/{hpMax:0}" : $"{hpCur:0}/??");
                SetFill(hpFill, hpMax > 0f ? Mathf.Clamp01(hpCur / hpMax) : 0f);
            }
            else { SetText(hpText, "-"); SetFill(hpFill, 0f); }

            if (EntityStatReader.TryGetStamina(boundEntity, out float stCur, out float stMax))
                SetText(staminaText, stMax > 0f ? $"{stCur:0}/{stMax:0}" : $"{stCur:0}/??");
            else
                SetText(staminaText, "-");

            statusBar?.Refresh();
        }

        private static void SetText(TMP_Text t, string s)
        {
            if (t == null) return;
            if (t.text != s) t.text = s;
        }

        private static void SetFill(Image img, float v)
        {
            if (img == null) return;
            if (!Mathf.Approximately(img.fillAmount, v)) img.fillAmount = v;
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
    }
}
