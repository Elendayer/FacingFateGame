using UnityEngine;
using UnityEngine.EventSystems;

namespace facingfate
{

    [RequireComponent(typeof(CanvasGroup))]
    public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] public DraggableTargetType desiredDraggableTargetType;

        protected RectTransform rectTransform;
        protected CanvasGroup canvasGroup;
        protected Vector2 originalPosition;

        public LineRenderer lineRenderer;
        public int curveResolution = 6;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 2;
                lineRenderer.enabled = false;
                lineRenderer.useWorldSpace = true;
            }
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            originalPosition = rectTransform.anchoredPosition;
            canvasGroup.blocksRaycasts = false;

            if (lineRenderer != null)
                lineRenderer.enabled = true;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            Camera uiCamera = eventData.pressEventCamera != null ? eventData.pressEventCamera : FindObjectOfType<Canvas>()?.worldCamera;
            if (uiCamera == null) uiCamera = Camera.main;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform.parent as RectTransform,
                eventData.position,
                uiCamera,
                out var localPoint);
            rectTransform.anchoredPosition = localPoint;

            UpdateLine(rectTransform.position, uiCamera.ScreenToWorldPoint(eventData.position));
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = true;
            rectTransform.anchoredPosition = originalPosition;

            if (lineRenderer != null)
                lineRenderer.enabled = false;
        }

        protected void UpdateLine(Vector3 start, Vector3 end)
        {
            if (lineRenderer == null) return;

            Vector3 control = (start + end) / 2 + Vector3.up * 1f;

            lineRenderer.positionCount = curveResolution;
            for (int i = 0; i < curveResolution; i++)
            {
                float t = i / (float)(curveResolution - 1);
                Vector3 point = Mathf.Pow(1 - t, 2) * start +
                                2 * (1 - t) * t * control +
                                Mathf.Pow(t, 2) * end;
                lineRenderer.SetPosition(i, point);
            }
        }
    }
}