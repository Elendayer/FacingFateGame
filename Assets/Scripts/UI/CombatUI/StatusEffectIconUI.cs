using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace facingfate
{

    public class StatusEffectIconUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image durationFillImage;
        [SerializeField] private TMP_Text stacksText;
        [SerializeField] private TooltipTrigger tooltip;

        public void SetIcon(Sprite sprite)
        {
            if (iconImage != null) iconImage.sprite = sprite;
            if (iconImage != null) iconImage.enabled = (sprite != null);
        }

        public void SetCounters(float duration, float stacks, float maxDuration = -1f)
        {
            if (durationFillImage != null)
            {
                if (duration >= 0f && maxDuration > 0f)
                    durationFillImage.fillAmount = Mathf.Clamp01(duration / maxDuration);
                else
                    durationFillImage.fillAmount = 1f;
            }

            if (stacksText != null)
                stacksText.text = stacks >= 0f ? stacks.ToString("0") : "";
        }

        public void SetTooltip(string header, string body)
        {
            if (tooltip == null) return;
            tooltip.Set(header, body);
            tooltip.enabled = !string.IsNullOrWhiteSpace(body);
        }
    }
}
