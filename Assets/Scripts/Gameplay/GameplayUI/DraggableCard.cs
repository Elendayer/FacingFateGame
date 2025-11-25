using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility;

public class DraggableCard : DraggableUI
{
    public CardScript cardScript; // Reference to the card logic
    private static readonly Vector3Int InvalidPosition = new Vector3Int(9999, 9999, 9999);

    private Vector3Int? lastHighlightedTile = InvalidPosition;
    private readonly List<Vector3Int> selectedTilesDuringDrag = new();
    private bool isDragging = false;

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        cardScript = GetComponent<CardScript>();
        isDragging = true;
        selectedTilesDuringDrag.Clear();
    }

    private void Update()
    {
        if (!isDragging || lastHighlightedTile == InvalidPosition) return;

        if (Input.GetMouseButtonDown(1)) // Right-click to select/deselect tiles
        {
            int maxTargets = cardScript.cardData.targetingData.cardSelectionType switch
            {
                CardTargetSelection.Select => cardScript.cardData.Area,
                CardTargetSelection.LineFree => 2,
                _ => 1
            };

            if (selectedTilesDuringDrag.Contains(lastHighlightedTile.Value))
            {
                selectedTilesDuringDrag.Remove(lastHighlightedTile.Value);
            }
            else if (selectedTilesDuringDrag.Count < maxTargets)
            {
                selectedTilesDuringDrag.Add(lastHighlightedTile.Value);
            }
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        HighlightCardEffectArea(TargetingUtility.GetHoveredTile(eventData));
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        isDragging = false;
        TilemapUtilityScript.ResetMaphightlight(TilemapUtilityScript.BaseTilemap);

        Vector3Int dropCell = InvalidPosition;

        TargetingModeData targetingModeData;

        List<EntityScript> allEntities = FindObjectsByType<EntityScript>(0).ToList();

        dropCell = TargetingUtility.GetHoveredTile(eventData) ?? InvalidPosition;
        targetingModeData = TargetingUtility.GetAffected(cardScript, dropCell, cardScript.cardData.Owner, selectedTilesDuringDrag, true);

        Debug.Log($"[DraggableCard] Activating card {cardScript.cardData.cardName} on {string.Join(", ", targetingModeData.targetedEntities.Select(t => t.name))}");
        cardScript.cardData.ActivateCardEffect(targetingModeData, gameObject);

    }

    private void HighlightCardEffectArea(Vector3Int? hoveredTile)
    {
        if (hoveredTile == null || hoveredTile == InvalidPosition || hoveredTile == lastHighlightedTile) return;

        Vector3Int currentCell = cardScript.cardData.Owner.GetComponent<EntityOnMap>().currentCell;

        List<Vector3Int> validTiles = TilemapUtilityScript.GetTilesInRange(currentCell, cardScript.cardData.Range);
        if (!validTiles.Contains(hoveredTile.Value)) return;

        TargetingModeData tmd = TargetingUtility.GetAffected(cardScript, hoveredTile.Value, cardScript.cardData.Owner, selectedTilesDuringDrag, false);

        TilemapUtilityScript.ResetMaphightlight(TilemapUtilityScript.BaseTilemap);
        TilemapUtilityScript.SetTilesHighlight(validTiles, TilemapUtilityScript.HighlightType.Range);
        TilemapUtilityScript.SetTilesHighlight(selectedTilesDuringDrag, TilemapUtilityScript.HighlightType.Selected);

        if (cardScript.cardData.targetingData.cardSelectionType == CardTargetSelection.LineFree)
        {
            TilemapUtilityScript.SetTilesHighlight(tmd.targetedTiles, TilemapUtilityScript.HighlightType.Line);
        }

        TilemapUtilityScript.SetTilesHighlight(tmd.targetedTiles, TilemapUtilityScript.HighlightType.Target);

        lastHighlightedTile = hoveredTile;
    }
}
