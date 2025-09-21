using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Utility;

public class DraggableCard : Draggable
{
    public CardScript cardScript; // Reference to the card logic
    private static readonly Vector3Int InvalidPosition = new Vector3Int(9999, 9999, 9999);

    private Vector3Int lastHighlightedTile = InvalidPosition;

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        cardScript = GetComponent<CardScript>();
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        HighlightCardEffectArea(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        TilemapUtilityScript.ResetMaphightlight(BaseTilemap);

        Vector3Int dropCell = TargetingUtility.GetValidDropTarget(eventData, cardScript);
        Debug.Log($"[DraggableCard] Drop position: {dropCell}");

        if (dropCell == InvalidPosition) return;

        List<EntityScript> targets = TargetingUtility.GetTargetsFromPosition(cardScript, dropCell,
            FindObjectsByType<EntityScript>(FindObjectsSortMode.None).ToList(),
            cardScript.cardData.Owner);

        Debug.Log($"[DraggableCard] Card {cardScript.cardData.cardName} targets before vetting: {targets.Count}");

        if (targets.Any())
        {
            Debug.Log($"[DraggableCard] Activating card {cardScript.cardData.cardName} on {string.Join(", ", targets.Select(t => t.name))}");
            cardScript.cardData.ActivateCard(targets, gameObject);
        }
    }

    private void HighlightCardEffectArea(PointerEventData eventData)
    {
        Vector3Int? currentTile = GetHoveredTile(eventData, BaseTilemap);
        if (currentTile == InvalidPosition || lastHighlightedTile == currentTile) return;

        List<Vector3Int> tilesToHighlight = currentTile.HasValue
            ? TargetingUtility.GetEffectAreaTiles(cardScript, currentTile.Value, cardScript.cardData.Owner)
            : new List<Vector3Int>();

        TilemapUtilityScript.ResetMaphightlight(BaseTilemap);
        TilemapUtilityScript.SetTilesHighlight(tilesToHighlight, BaseTilemap, TilemapUtilityScript.HighlightType.Target);

        lastHighlightedTile = currentTile ?? InvalidPosition;
    }

    private Vector3Int? GetHoveredTile(PointerEventData eventData, Tilemap tilemap)
    {
        foreach (GameObject hoveredObject in eventData.hovered)
        {
            if (hoveredObject.TryGetComponent(out DraggableTarget dt) &&
                dt.draggableTargetType == DraggableTargetType.CombatTile)
            {
                return tilemap.WorldToCell(hoveredObject.transform.position);
            }
        }
        return InvalidPosition;
    }
}
