using TMPro;
using UnityEngine;

namespace facingfate
{
    /// <summary>
    /// A single row in the detailed stats panel.
    /// Prefab layout: one TMP_Text for the label (left), one for the value (right).
    /// A TooltipTrigger is added automatically at runtime if not already present.
    /// </summary>
    public class StatRowEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text valueText;

        private TooltipTrigger tooltip;

        /// <summary>Full setup — call once after Instantiate.</summary>
        public void Setup(string label, string value, string tooltipHeader, string tooltipBody)
        {
            if (labelText != null) labelText.text = label;
            if (valueText != null) valueText.text = value;

            tooltip = GetComponent<TooltipTrigger>()
                   ?? gameObject.AddComponent<TooltipTrigger>();
            tooltip.Set(tooltipHeader, tooltipBody);
        }

        /// <summary>Update just the value text each refresh cycle.</summary>
        public void UpdateValue(string value)
        {
            if (valueText != null) valueText.text = value;
        }
    }
}
