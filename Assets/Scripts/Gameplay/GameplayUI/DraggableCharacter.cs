using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class DraggableCharacter : Draggable
{
    public CharacterOnMap character;
    public Tilemap baseTilemap;

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        // Convert mouse position to tile cell
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int targetCell = baseTilemap.WorldToCell(worldPos);

        targetCell = new Vector3Int(targetCell.x, targetCell.y, 0); // Ensure z=0 for 2D tilemaps

        Debug.Log($"{worldPos} to cell {targetCell}");

        if (character != null && baseTilemap.GetTile(targetCell) != null)
        {
            character.MoveTo(targetCell);
        }
    }
}