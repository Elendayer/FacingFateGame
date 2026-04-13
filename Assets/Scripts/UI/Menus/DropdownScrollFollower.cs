using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace dungeonduell
{
    public class DropdownScrollFollower : MonoBehaviour
    {
        private ScrollRect scrollRect;

        void Awake()
        {
            scrollRect = GetComponentInParent<ScrollRect>();
        }

        void Update()
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected == null || scrollRect == null) return;

            if (selected.transform.IsChildOf(transform))
            {
                // automatisch in den sichtbaren Bereich scrollen
                RectTransform selectedRect = selected.GetComponent<RectTransform>();
                if (selectedRect != null)
                {
                    Canvas.ForceUpdateCanvases(); // wichtig bei dynamischen Listen
                    scrollRect.content.GetComponent<VerticalLayoutGroup>()?.CalculateLayoutInputVertical();
                    scrollRect.verticalNormalizedPosition = Mathf.Clamp01(GetScrollPosition(selectedRect));
                }
            }
        }

        private float GetScrollPosition(RectTransform target)
        {
            float contentHeight = scrollRect.content.rect.height;
            float viewportHeight = scrollRect.viewport.rect.height;
            float elementPos = Mathf.Abs(target.anchoredPosition.y);
            return (elementPos - viewportHeight / 2) / (contentHeight - viewportHeight);
        }
    }
}
