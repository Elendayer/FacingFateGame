using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using static UnityEngine.EventSystems.EventTrigger;

public class DraggableCard : Draggable
{
    public CardScript cardScript; // Reference to the card logic

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        cardScript = GetComponent<CardScript>();

        if (!IsValidDrop(eventData, out List<GameObject> hoveredObjects))
            return;

        List<EntityScript> targets = GetTargets(eventData, hoveredObjects);
        if (targets.Any())
        {
            cardScript.cardData.ActivateCard(targets,gameObject);
        }
    }

    private bool IsValidDrop(PointerEventData eventData, out List<GameObject> hoveredObjects)
    {
        hoveredObjects = eventData.hovered;
        bool validDrop = false;

        switch (cardScript.cardData.targetingData.CardTargetType)
        {
            case CardTargetType.Self:
                desiredDraggableTargetType = DraggableTargetType.Character;
                validDrop = hoveredObjects.Contains(cardScript.cardData.Owner.gameObject);
                break;

            case CardTargetType.Allies:
                desiredDraggableTargetType = DraggableTargetType.Character;
                validDrop = hoveredObjects.Any(go =>
                    go.TryGetComponent(out EntityScript entity) &&
                    IsSameType(entity, cardScript.cardData.Owner));
                break;

            case CardTargetType.Enemy:
                desiredDraggableTargetType = DraggableTargetType.Character;
                validDrop = hoveredObjects.Any(go =>
                    go.TryGetComponent(out EntityScript entity) &&
                  !  IsSameType(entity, cardScript.cardData.Owner));
                break;

            case CardTargetType.All:
                desiredDraggableTargetType = DraggableTargetType.Character;
                validDrop = hoveredObjects.Any(go =>
                    go.TryGetComponent(out EntityScript entity));
                break;

            case CardTargetType.Tile:
                desiredDraggableTargetType = DraggableTargetType.Tile;
                validDrop = hoveredObjects.Any(go =>
                    go.TryGetComponent(out DraggableTarget target) &&
                    target.draggableTargetType == DraggableTargetType.Tile);
                break;
        }

        return validDrop;
    }

    private bool IsSameType(EntityScript a, EntityScript b)
    {
        Debug.Log($"{a.name} and {b.name}    {a.GetType() == b.GetType()}");
        return a.GetType() == b.GetType();
    }

    private List<EntityScript> GetTargets(PointerEventData eventData, List<GameObject> hoveredObjects)
    {
        List<EntityScript> targets = new();
        Tilemap tilemap = cardScript.cardData.Owner.GetComponentInParent<Tilemap>();

        switch (cardScript.cardData.targetingData.areaType)
        {
            case CardTargetArea.Single:
                targets = hoveredObjects
                    .Select(go => go.GetComponent<EntityScript>())
                    .Where(entity => entity != null)
                    .Take(1)
                    .ToList();
                break;

            case CardTargetArea.Self:
                targets.Add(cardScript.cardData.Owner);
                break;

            case CardTargetArea.Line:
                targets = GetEntitiesFromTiles(
                    TileMapUtilityScript.GetTilesInLine(
                        tilemap.WorldToCell(cardScript.cardData.Owner.transform.position),
                        cardScript.cardData.targetingData.range, 0, tilemap));
                break;

            case CardTargetArea.All:
                targets = FindObjectsByType<EntityScript>(FindObjectsSortMode.None).ToList();
                break;

            case CardTargetArea.Sphere:
                targets = GetEntitiesFromTiles(
                    TileMapUtilityScript.GetTilesInRadius(
                        tilemap.WorldToCell(cardScript.cardData.Owner.transform.position),
                        cardScript.cardData.targetingData.range, tilemap));
                break;

            case CardTargetArea.Ring:
                targets = GetEntitiesFromTiles(
                    TileMapUtilityScript.GetTilesInRing(
                        tilemap.WorldToCell(cardScript.cardData.Owner.transform.position),
                        cardScript.cardData.targetingData.range, tilemap));
                break;
        }

        return VetTargets(targets);
    }

    private List<EntityScript> GetEntitiesFromTiles(List<Vector3Int> tiles)
    {
        Tilemap tilemap = cardScript.cardData.Owner.GetComponentInParent<Tilemap>();
        var allEntities = FindObjectsByType<EntityScript>(FindObjectsSortMode.None).ToList();
        return TileMapUtilityScript.GetEntitiesOnTiles(tiles, allEntities, tilemap);
    }

    private List<EntityScript> VetTargets(List<EntityScript> preTargets)
    {
        return preTargets.Where(entity =>
            cardScript.cardData.targetingData.CardTargetType switch
            {
                CardTargetType.Allies => entity.GetType() == cardScript.cardData.Owner.GetType(),
                CardTargetType.Enemy => entity.GetType() != cardScript.cardData.Owner.GetType(),
                _ => true
            }).ToList();
    }
}
