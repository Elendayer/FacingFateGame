using UnityEngine;
using UnityEngine.EventSystems;

namespace facingfate
{
    /// <summary>
    /// Attach to any UI panel to make it draggable by mouse.
    /// Drag anywhere on the panel to reposition it.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class DraggablePanel : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        private RectTransform _rect;
        private Canvas _canvas;
        private Vector2 _dragOffset;

        private void Awake()
        {
            _rect   = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );
            _dragOffset = _rect.anchoredPosition - localPoint;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint)) return;

            _rect.anchoredPosition = localPoint + _dragOffset;
        }
    }
}
