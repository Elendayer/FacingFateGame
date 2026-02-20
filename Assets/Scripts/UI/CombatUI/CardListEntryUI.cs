using UnityEngine;
using TMPro;

namespace facingfate
{
    public class CardListEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TooltipTrigger tooltip;

        public void SetText(string text)
        {
            if (labelText != null) labelText.text = text;
        }

        public void SetTooltip(string body)
        {
            if (tooltip == null) return;

            if (string.IsNullOrWhiteSpace(body))
            {
                tooltip.enabled = false;
                return;
            }

            tooltip.enabled = true;
            tooltip.Set("Card", body);
        }
    }
}
