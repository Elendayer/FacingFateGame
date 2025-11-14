using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
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

        //Clear Selection
        selectedTilesDuringDrag.Clear();
    }
    private void Update()
    {
        if (isDragging)
        {
            // This runs every frame while dragging
            if (Input.GetMouseButtonDown(1)) // Only while left click is held
            {
                int maxTargets = 0;

                switch (cardScript.cardData.targetingData.cardSelectionType)
                {
                    case CardTargetSelection.Select:
                        {
                            maxTargets = cardScript.cardData.Area;
                            break;
                        }
                    case CardTargetSelection.LineFree:
                        {
                            maxTargets = 2;
                            break;
                        }
                }
                if (selectedTilesDuringDrag.Contains(lastHighlightedTile.Value))
                {
                    selectedTilesDuringDrag.Remove(lastHighlightedTile.Value);
                }
                else
                {
                    if (selectedTilesDuringDrag.Count <= maxTargets)
                    {
                        selectedTilesDuringDrag.Add(lastHighlightedTile.Value);
                    }
                }
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

        Vector3Int dropCell = TilemapUtilityScript.InvalidPosition;
        CardTargetType ctt = cardScript.cardData.targetingData.CardTargetType;

        List<EntityScript> targets = new();

        switch (ctt)
        {
            case CardTargetType.CombatTile:
                dropCell = TargetingUtility.GetValidTileDrop(eventData, cardScript);
                targets = TargetingUtility.GetEntitiesFromPosition(cardScript, dropCell, FindObjectsByType<EntityScript>(FindObjectsSortMode.None).ToList(), cardScript.cardData.Owner, selectedTilesDuringDrag);
                ; break;
            case CardTargetType.Entity:
                dropCell = TargetingUtility.GetValidEntityDrop(eventData, cardScript);
                targets = TargetingUtility.GetEntitiesFromPosition(cardScript, dropCell, FindObjectsByType<EntityScript>(FindObjectsSortMode.None).ToList(), cardScript.cardData.Owner, selectedTilesDuringDrag);
                break;
            case CardTargetType.Ground:
                dropCell = TargetingUtility.GetValidGroundDrop(eventData, cardScript);
                break;
        }

        if (dropCell == InvalidPosition) return;

        Debug.Log($"[DraggableCard] Card {cardScript.cardData.cardName} targets before vetting: {targets.Count}");

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
            cardScript.cardData.ActivateCard(new List<Vector3Int>() { dropCell }, gameObject);
        }
    }
    private void HighlightCardEffectArea(Vector3Int? Tile)
    {
        Vector3Int? currentHoveredTile = Tile;
        if (currentHoveredTile == InvalidPosition || lastHighlightedTile == currentHoveredTile) { return; }

        List<Vector3Int> areaTiles = currentHoveredTile.HasValue
            ? TargetingUtility.GetEffectAreaTiles(cardScript, currentHoveredTile.Value, cardScript.cardData.Owner)
            : new List<Vector3Int>();

        Vector3Int currentPositionTile = cardScript.cardData.Owner.GetComponent<EntityOnMap>().currentCell;
        List<Vector3Int> validTiles = TilemapUtilityScript.GetTilesInRange(currentPositionTile,cardScript.cardData.Range);

        if (validTiles.Contains(currentHoveredTile.Value) == false) { return; }

        TilemapUtilityScript.ResetMaphightlight(TilemapUtilityScript.BaseTilemap);
        TilemapUtilityScript.SetTilesHighlight(validTiles, TilemapUtilityScript.HighlightType.Range);
        TilemapUtilityScript.SetTilesHighlight(selectedTilesDuringDrag, TilemapUtilityScript.HighlightType.Selected);

        if(cardScript.cardData.targetingData.cardSelectionType == CardTargetSelection.LineFree)
        {
            List<Vector3Int> lineTiles = TargetingUtility.GetEffectAreaTiles(cardScript, currentHoveredTile.Value, cardScript.cardData.Owner, selectedTilesDuringDrag);
            TilemapUtilityScript.SetTilesHighlight(lineTiles, TilemapUtilityScript.HighlightType.Line);
        }

        TilemapUtilityScript.SetTilesHighlight(areaTiles, TilemapUtilityScript.HighlightType.Target);

        lastHighlightedTile = currentHoveredTile ?? InvalidPosition;
    }
}
