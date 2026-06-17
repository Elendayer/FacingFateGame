using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace facingfate
{
    public class PlayerStatsPanel : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private Slider hpSlider;

        [SerializeField] private TMP_Text staminaText;
        [SerializeField] private Slider staminaSlider;
        [SerializeField] private StatusEffectBarUI statusBar;

        private EntityScript boundEntity;

        [Header("Animation")]
        [SerializeField] private float sliderAnimDuration = 0.4f;
        [SerializeField] private Ease sliderEase = Ease.OutQuart;

        public void Bind(EntityScript entity)
        {
            // Unsubscribe from old entity
            if (boundEntity != null && boundEntity.entityStats != null)
                boundEntity.entityStats.OnStatsChanged -= Refresh;

            boundEntity = entity;

            // Subscribe to new entity for immediate refresh on any stat change
            if (boundEntity != null && boundEntity.entityStats != null)
                boundEntity.entityStats.OnStatsChanged += Refresh;

            if (statusBar != null) statusBar.Bind(entity);
            else Debug.Log("[PlayerStatPanel] statusBar ist NULL!");
        }

        private void OnDisable()
        {
            if (boundEntity != null && boundEntity.entityStats != null)
                boundEntity.entityStats.OnStatsChanged -= Refresh;
        }

        public void Refresh()
        {
            //Debug.Log($"[PlayerStatPanel] Refresh – boundEntity={boundEntity?.gameObject.name ?? "NULL"}, statusBar={statusBar?.name ?? "NULL"}");
            if (boundEntity == null)
            {
                SetText(nameText, "-");
                SetText(hpText, "-");
                SetText(staminaText, "-");
                SetSlider(hpSlider, 0f, 1f);
                SetSlider(staminaSlider, 0f, 1f);
                statusBar?.Refresh();
                return;
            }

            SetText(nameText, boundEntity.name);

            int hpMax = (int)boundEntity.entityStats.MaxHealth;
            int hpCur = (int)boundEntity.entityStats.CurrentHealth;

            int stMax = (int)boundEntity.entityStats.MaxStamina;
            int stCur = (int)boundEntity.entityStats.CurrentStamina;

            SetText(hpText, hpMax > 0f ? $"{hpCur:0}/{hpMax:0}" + " HP" : $"{hpCur:0}/??");
            SetSlider(hpSlider, hpCur, hpMax);

            SetText(staminaText, stMax > 0f ? $"{stCur:0}/{stMax:0}" + " Stamina" : $"{stCur:0}/??");
            SetSlider(staminaSlider, stCur, stMax);

            statusBar?.Refresh();
        }

        private static void SetText(TMP_Text t, string s)
        {
            if (t == null) return;
            if (t.text != s) t.text = s;
        }
        private static void SetSlider(Slider s, float current, float max)
        {
            if (s == null) return;
            s.minValue = 0f;
            s.maxValue = max > 0f ? max : 1f;

            s.DOKill();
            s.DOValue(current >= 0f ? current : 0f, 0.4f)
                .SetEase(Ease.OutQuart)
                .SetUpdate(true);
        }
    }
}
