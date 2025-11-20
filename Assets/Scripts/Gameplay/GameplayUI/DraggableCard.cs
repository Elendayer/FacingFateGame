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
        Debug.Log(TargetingUtility.GetHoveredTile(eventData));
        HighlightCardEffectArea(TargetingUtility.GetHoveredTile(eventData));
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        isDragging = false;
        TilemapUtilityScript.ResetMaphightlight(TilemapUtilityScript.BaseTilemap);

        Vector3Int dropCell = InvalidPosition;
        CardTargetType ctt = cardScript.cardData.targetingData.CardTargetType;

        List<Vector3Int> areaTiles = new();

        List<EntityScript> allEntities = FindObjectsByType<EntityScript>(0).ToList();
        List<EntityScript> targets = new();

        switch (ctt)
        {
            case CardTargetType.CombatTile:
                dropCell = TargetingUtility.GetHoveredTile(eventData) ?? InvalidPosition;
                areaTiles = TargetingUtility.GetEffectAreaTiles(cardScript, dropCell, cardScript.cardData.Owner, selectedTilesDuringDrag);
                targets = TargetingUtility.GetEntitiesFromTiles(areaTiles, allEntities);
                targets = TargetingUtility.GetValidTargets(cardScript, cardScript.cardData.Owner, targets);
                break;
            case CardTargetType.Entity:
                dropCell = TargetingUtility.GetHoveredTile(eventData) ?? InvalidPosition;
                areaTiles = TargetingUtility.GetEffectAreaTiles(cardScript, dropCell, cardScript.cardData.Owner, selectedTilesDuringDrag);
                targets = TargetingUtility.GetEntitiesFromTiles(areaTiles, allEntities);
                targets = TargetingUtility.GetValidTargets(cardScript, cardScript.cardData.Owner, targets);
                break;
            case CardTargetType.Ground:
                dropCell = TargetingUtility.GetHoveredTile(eventData) ?? InvalidPosition;
                break;
        }

        if (dropCell == InvalidPosition) return;

        if (ctt == CardTargetType.Entity || ctt == CardTargetType.CombatTile)
        {
            if (targets.Any())
            {
                Debug.Log($"[DraggableCard] Activating card {cardScript.cardData.cardName} on {string.Join(", ", targets.Select(t => t.name))}");
                cardScript.cardData.ActivateCard(targets, gameObject);
            }
        }
        else
        {
            Debug.Log($"[DraggableCard] Activating card {cardScript.cardData.cardName} on ground at {dropCell}");
            cardScript.cardData.ActivateCard(new List<Vector3Int> { dropCell }, gameObject);
        }
    }

    private void HighlightCardEffectArea(Vector3Int? hoveredTile)
    {
        if (hoveredTile == null || hoveredTile == InvalidPosition || hoveredTile == lastHighlightedTile) return;

        Vector3Int currentCell = cardScript.cardData.Owner.GetComponent<EntityOnMap>().currentCell;

        List<Vector3Int> validTiles = TilemapUtilityScript.GetTilesInRange(currentCell, cardScript.cardData.Range);
        if (!validTiles.Contains(hoveredTile.Value)) return;

        List<Vector3Int> effectTiles = TargetingUtility.GetEffectAreaTiles(cardScript, hoveredTile.Value, cardScript.cardData.Owner, selectedTilesDuringDrag);

        TilemapUtilityScript.ResetMaphightlight(TilemapUtilityScript.BaseTilemap);
        TilemapUtilityScript.SetTilesHighlight(validTiles, TilemapUtilityScript.HighlightType.Range);
        TilemapUtilityScript.SetTilesHighlight(selectedTilesDuringDrag, TilemapUtilityScript.HighlightType.Selected);

        if (cardScript.cardData.targetingData.cardSelectionType == CardTargetSelection.LineFree)
        {
            TilemapUtilityScript.SetTilesHighlight(effectTiles, TilemapUtilityScript.HighlightType.Line);
        }

        TilemapUtilityScript.SetTilesHighlight(effectTiles, TilemapUtilityScript.HighlightType.Target);

        lastHighlightedTile = hoveredTile;
    }
}
