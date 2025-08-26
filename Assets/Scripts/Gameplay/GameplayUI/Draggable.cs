using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    public DraggableTargetType desiredDraggableTargetType;

    protected RectTransform rectTransform;
    protected CanvasGroup canvasGroup;
    protected Vector2 originalPosition;

    public LineRenderer lineRenderer;
    public int curveResolution = 6; // Number of points in the curve


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

        if (lineRenderer != null) lineRenderer.enabled = true;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (lineRenderer != null)
        {
            Vector3 start = rectTransform.position;
            Vector3 end = eventData.pointerCurrentRaycast.worldPosition;

            Vector3 control = (start + end) / 2 + Vector3.up * 1f;

            lineRenderer.positionCount = curveResolution;
            for (int i = 0; i < curveResolution; i++)
            {
                float t = i / (float)(curveResolution - 1);
                Vector3 curvedPoint = Mathf.Pow(1 - t, 2) * start +
                                      2 * (1 - t) * t * control +
                                      Mathf.Pow(t, 2) * end;

                lineRenderer.SetPosition(i, curvedPoint);
            }
        }
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        if (lineRenderer != null) lineRenderer.enabled = false;

        // Reset to original position by default
        rectTransform.anchoredPosition = originalPosition;
    }
}
