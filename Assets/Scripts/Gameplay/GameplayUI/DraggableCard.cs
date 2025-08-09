using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;

    public LineRenderer lineRenderer;

    void Awake()
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }
    }

    public int curveResolution = 3; // Number of points in the curve

    public void OnDrag(PointerEventData eventData)
    {
        if (lineRenderer != null)
        {
            Vector3 start = rectTransform.position;
            Vector3 end = eventData.pointerCurrentRaycast.worldPosition;

            Vector3 control = (start + end) / 2 + Vector3.up * 1f; // Adjust for curvature

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


    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        if (lineRenderer != null) lineRenderer.enabled = false;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        transform.parent.TryGetComponent<CardSlot>(out CardSlot OriginSlot);

        foreach (RaycastResult result in results)
        {
            DraggableTarget target = result.gameObject.GetComponent<DraggableTarget>();

            if (target != null)
            {
                DropCard(result, target);
            }

            // No valid slot found, reset position
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    private void DropCard(RaycastResult result, DraggableTarget target)
    {
        switch (target.GetType().Name)
        {
            case "CardSlot":

                CardSlot slot = result.gameObject.GetComponent<CardSlot>();
                CardScript cs = GetComponent<CardScript>();
                switch (cs.cardData.cardType)
                {
                    case CardType.Ability:
                        if (slot.cardScript != null)
                        {
                            slot.PlayCardOntoSlot(cs);
                        }
                        break;
                    default:
                        if (slot.cardScript == null)
                        {
                            slot.AttachCardToSlot(cs.gameObject);
                        }
                        break;
                }
                break;
        
            case "CardSlotDivide":

                CardSlotDivide slotDivide = result.gameObject.GetComponent<CardSlotDivide>();
                int i = slotDivide.divideIndex;

                if (SlotManager.Instance.slots[slotDivide.divideIndex].cardScript == null)
                {
                    SlotManager.Instance.slots[i].AttachCardToSlot(this.gameObject);
                    Debug.Log("Divide UP");
                }
                else if (SlotManager.Instance.slots[slotDivide.divideIndex + 1].cardScript == null)
                {
                    SlotManager.Instance.slots[i + 1].AttachCardToSlot(this.gameObject);
                    Debug.Log("Divide Down");
                }
                else
                {
                    Debug.Log("Both Filled");
                }
                break;
        }
    }
}