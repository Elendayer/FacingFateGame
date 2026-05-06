using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace dungeonduell
{
    public class DropdownScrollFollower : MonoBehaviour
    {
        private ScrollRect scrollRect;
        [SerializeField] private float scrollEdgePadding = 50f; // Padding from edges before auto-scroll triggers

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
                // Handle mouse wheel scroll
                float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scrollDelta) > 0.01f)
                {
                    float newScrollPosition = scrollRect.verticalNormalizedPosition - scrollDelta;
                    scrollRect.verticalNormalizedPosition = Mathf.Clamp01(newScrollPosition);
                }

                // automatisch in den sichtbaren Bereich scrollen
                RectTransform selectedRect = selected.GetComponent<RectTransform>();
                if (selectedRect != null)
                {
                    Canvas.ForceUpdateCanvases(); // wichtig bei dynamischen Listen
                    scrollRect.content.GetComponent<VerticalLayoutGroup>()?.CalculateLayoutInputVertical();

                    // Only auto-scroll if the selected item is significantly outside the viewport
                    if (IsSelectedItemOutsideViewport(selectedRect))
                    {
                        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(GetScrollPosition(selectedRect));
                    }
                }
            }
        }

        private bool IsSelectedItemOutsideViewport(RectTransform selectedItem)
        {
            if (scrollRect.viewport == null) return false;

            // Get the world bounds of the selected item and viewport
            Bounds itemBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scrollRect.content, selectedItem);
            Bounds viewportBounds = new Bounds(Vector3.zero, new Vector3(scrollRect.viewport.rect.width, scrollRect.viewport.rect.height, 0));

            // Check if item is outside viewport with padding consideration
            float itemTop = -itemBounds.min.y;
            float itemBottom = -itemBounds.max.y;
            float viewportTop = scrollEdgePadding;
            float viewportBottom = scrollRect.viewport.rect.height - scrollEdgePadding;

            // If item is completely outside viewport (ignoring padding), then auto-scroll
            return itemTop > viewportBottom || itemBottom < viewportTop;
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


