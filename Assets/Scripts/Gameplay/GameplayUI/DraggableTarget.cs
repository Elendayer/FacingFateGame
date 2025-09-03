using UnityEngine;

public class DraggableTarget : MonoBehaviour
{
    public DraggableTargetType draggableTargetType;
}

public enum DraggableTargetType
{
    Character,
    Tile,
    CardSlot,
}