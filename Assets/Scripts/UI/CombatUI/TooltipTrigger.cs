using UnityEngine;
using UnityEngine.EventSystems;

namespace facingfate
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [TextArea(1, 8)]
        [SerializeField] private string header;

        [TextArea(1, 12)]
        [SerializeField] private string body;

        public void Set(string newHeader, string newBody)
        {
            header = newHeader;
            body = newBody;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (TooltipSystem.Instance == null) return;
            if (string.IsNullOrWhiteSpace(body)) return;

            TooltipSystem.Instance.Show(header, body);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipSystem.Instance == null) return;
            TooltipSystem.Instance.Hide();
        }

        private void OnDisable()
        {
            // Hide tooltip if this trigger is destroyed/disabled while hovered.
            // Unity does not fire OnPointerExit when a GameObject is destroyed mid-hover.
            TooltipSystem.Instance?.Hide();
        }
    }
}
