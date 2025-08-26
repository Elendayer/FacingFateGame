using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableCard : Draggable
{
    public CardScript cardScript; // Reference to the card logic

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        // Custom logic for dropping on card slots
        foreach (GameObject result in eventData.hovered)
        {
            DraggableTarget target = result.gameObject.GetComponent<DraggableTarget>();
            if (target != null && target.draggableTargetType == desiredDraggableTargetType)
            {
                break;
            }
        }
    }
}