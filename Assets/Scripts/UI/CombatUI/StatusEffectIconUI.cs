using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace facingfate
{

    public class StatusEffectIconUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text durationText;
        [SerializeField] private TMP_Text stacksText;
        [SerializeField] private TooltipTrigger tooltip;

        public void SetIcon(Sprite sprite)
        {
            if (iconImage != null) iconImage.sprite = sprite;
            if (iconImage != null) iconImage.enabled = (sprite != null);
        }

        public void SetCounters(float duration, float stacks)
        {
            if (durationText != null)
                durationText.text = duration >= 0f ? duration.ToString("0") : "";

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
