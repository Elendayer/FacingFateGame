using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace facingfate
{

    public class HoverTargetPanel : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private Slider hpSlider;
        [SerializeField] private TMP_Text stamText;
        [SerializeField] private Slider stamSlider;
        [SerializeField] private StatusEffectBarUI statusBar;

        private EntityScript boundEntity;

        [SerializeField] private float fadeDuration = 0.2f;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
        }

        public void Bind(EntityScript entity)
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
                if (stamText != null) stamText.text = "-";
                if (stamSlider != null) SetSlider(stamSlider, 0f, 1f);
                statusBar?.Refresh();

                if (canvasGroup != null)
                {
                    canvasGroup.DOKill();
                    canvasGroup.DOFade(0f, fadeDuration).SetUpdate(true);
                }
                return;
            }

            if (titleText != null) titleText.text = GetEntityName(boundEntity);

            var stats = boundEntity.entityStats;
            int hpCur = (int)stats.CurrentHealth;
            int hpMax = (int)stats.MaxHealth;

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

            int stamCur = (int)stats.CurrentStamina;
            int stamMax = (int)stats.MaxStamina;

            string stamString;
            if (stamCur >= 0f && stamMax > 0f)
                stamString = $"{stamCur:0}/{stamMax:0}";
            else if (stamCur >= 0f)
                stamString = $"{stamCur:0}/??";
            else
                stamString = "-";

            if (stamText != null && stamText.text != stamString)
                stamText.text = stamString;

            if (stamSlider != null)
            {
                SetSlider(stamSlider, stamCur, stamMax);
            }

            statusBar?.Refresh();
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.DOFade(1f, fadeDuration).SetUpdate(true);
            }
        }
        private static string GetEntityName(EntityScript entity)
        {
            // Try to get NPC name first
            var npcScript = entity.GetComponent<NonPlayerScript>();
            if (npcScript != null && npcScript.npcData != null && !string.IsNullOrWhiteSpace(npcScript.npcData.name))
                return npcScript.npcData.name;

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
