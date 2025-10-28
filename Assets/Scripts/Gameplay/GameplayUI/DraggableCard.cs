using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Utility;

public class DraggableCard : DraggableUI
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
        TilemapUtilityScript.ResetMaphightlight(TilemapUtilityScript.BaseTilemap);

        Vector3Int dropCell = TilemapUtilityScript.InvalidPosition;
        CardTargetType ctt = cardScript.cardData.targetingData.CardTargetType;

        List<EntityScript> targets = new();

        switch (ctt)
        {
            case CardTargetType.CombatTile:
                dropCell = TargetingUtility.GetValidTileDrop(eventData, cardScript);
                targets = TargetingUtility.GetEntitiesFromPosition(cardScript, dropCell, FindObjectsByType<EntityScript>(FindObjectsSortMode.None).ToList(), cardScript.cardData.Owner);

                ; break;
            case CardTargetType.Entity:
                dropCell = TargetingUtility.GetValidEntityDrop(eventData, cardScript);
                targets = TargetingUtility.GetEntitiesFromPosition(cardScript, dropCell, FindObjectsByType<EntityScript>(FindObjectsSortMode.None).ToList(), cardScript.cardData.Owner);
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
    private void HighlightCardEffectArea(PointerEventData eventData)
    {
        Vector3Int? currentTile = TargetingUtility.GetHoveredTile(eventData);
        if (currentTile == InvalidPosition || lastHighlightedTile == currentTile) return;

        List<Vector3Int> tilesToHighlight = currentTile.HasValue
            ? TargetingUtility.GetEffectAreaTiles(cardScript, currentTile.Value, cardScript.cardData.Owner)
            : new List<Vector3Int>();

        TilemapUtilityScript.ResetMaphightlight(TilemapUtilityScript.BaseTilemap);
        TilemapUtilityScript.SetTilesHighlight(tilesToHighlight, TilemapUtilityScript.HighlightType.Target);

        lastHighlightedTile = currentTile ?? InvalidPosition;
    }

}
