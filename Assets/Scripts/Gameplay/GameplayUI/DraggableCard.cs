using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class DraggableCard : Draggable
{
    public CardScript cardScript; // Reference to the card logic

    private static readonly Vector3Int InvalidPosition = new Vector3Int(9999, 9999, 9999);

    private Vector3Int lastHighlightedTile = new(); // Store the last highlighted tiles

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

        TileMapUtilityScript.ResetMaphightlight(BaseTilemap);

        Vector3Int pos = GetValidDropTarget(eventData);
        Debug.Log($"Drop position: {pos}");

        if (pos == InvalidPosition)
        {
            return; // Invalid drop target
        }

        List<EntityScript> targets = GetTargetEntities(pos);
        Debug.Log($"Card {cardScript.cardData.cardName} dropped on {pos} with {targets.Count} targets.");

        targets = VetTargetsEntities(targets);
        Debug.Log($"Card {cardScript.cardData.cardName} has {targets.Count} valid targets after vetting.");

        if (targets.Any())
        {
            Debug.Log($"Activating card {cardScript.cardData.cardName} on targets: {string.Join(", ", targets.Select(t => t.name))}");

            cardScript.cardData.ActivateCard(targets, gameObject);
        }
    }

    private Vector3Int GetValidDropTarget(PointerEventData eventData)
    {
        foreach (GameObject hoveredObject in eventData.hovered)
        {
            if (hoveredObject.TryGetComponent(out DraggableTarget dt))
            {
                Debug.Log($"Hovered over {hoveredObject.name} with DraggableTarget of type {dt.draggableTargetType}");
                switch (cardScript.cardData.targetingData.CardTargetType)
                {
                    case CardTargetType.Entity:
                        if (dt.draggableTargetType == DraggableTargetType.CombatCharacter && hoveredObject.TryGetComponent(out EntityScript entitiy))
                        {
                            return entitiy.GetComponent<EntityOnMap>().currentCell;
                        }
                        break;

                    case CardTargetType.CombatTile:
                        if (dt.draggableTargetType == DraggableTargetType.CombatTile)
                            return GetTilePosition(hoveredObject);
                        break;
                }
            }
        }
        return InvalidPosition; // Invalid position
    }
    private bool IsSameType(EntityScript a, EntityScript b)
    {
        Debug.Log($"{a.name} and {b.name}    {a.GetType() == b.GetType()}");
        return a.GetType() == b.GetType();
    }
    private List<EntityScript> GetTargetEntities(Vector3Int pos)
    {
        List<EntityScript> targets = new();

        List<Vector3Int> positions = new();
        Tilemap tilemap = cardScript.cardData.Owner.GetComponentInParent<Tilemap>();

        switch (cardScript.cardData.targetingData.areaType)
        {
            case CardTargetArea.Single:
                targets = GetEntitiesFromTiles(new List<Vector3Int> { pos });
                break;

            case CardTargetArea.All:
                targets = FindObjectsByType<EntityScript>(FindObjectsSortMode.None).ToList();
                break;

            case CardTargetArea.Line:
                positions = TileMapUtilityScript.GetTilesInLine(pos, cardScript.cardData.targetingData.range, 0);
                targets = GetEntitiesFromTiles(positions); 
                break;

            case CardTargetArea.Radius:
                positions = TileMapUtilityScript.GetTilesInRadius(pos, cardScript.cardData.targetingData.range);
                targets = GetEntitiesFromTiles(positions);
                break;

            case CardTargetArea.Ring:
                positions = TileMapUtilityScript.GetTilesInRing(pos, cardScript.cardData.targetingData.range);
                targets = GetEntitiesFromTiles(positions);
                break;
        }

        Debug.Log($"Found {targets.Count} potential targets before vetting.");
        return targets;
    }
    private List<EntityScript> GetEntitiesFromTiles(IEnumerable<Vector3Int> tiles)
    {
        List<EntityScript> allEntities = FindObjectsByType<EntityScript>(FindObjectsSortMode.None).ToList();

        return allEntities.Where(entity => tiles.Contains(entity.GetComponent<EntityOnMap>().currentCell)).ToList();
    }
    private List<EntityScript> VetTargetsEntities(List<EntityScript> preTargets)
    {
        // Safeguard: ensure we have the necessary data
        var cardData = cardScript?.cardData;
        var targeting = cardData?.targetingData;
        var owner = cardData?.Owner;

        if (targeting == null || owner == null)
        {
            Debug.LogWarning("Missing targeting data or owner when vetting targets.");
            return new List<EntityScript>();
        }

        return preTargets.Where(target =>
        {
            switch (targeting.CardTargetType)
            {
                case CardTargetType.Entity:
                    return targeting.CardTargetAffiliation switch
                    {
                        CardTargetAffiliation.Ally => IsSameType(target, owner),
                        CardTargetAffiliation.Enemy => !IsSameType(target, owner),
                        CardTargetAffiliation.Self => target == owner,
                        CardTargetAffiliation.All => true,
                        _ => true
                    };

                case CardTargetType.CombatTile:
                    return targeting.CardTargetAffiliation switch
                    {
                        CardTargetAffiliation.Ally => IsSameType(target, owner),
                        CardTargetAffiliation.Enemy => !IsSameType(target, owner),
                        CardTargetAffiliation.Self => target == owner,
                        CardTargetAffiliation.All => true,
                        _ => false
                    };

                default:
                    return false;
            }
        }).ToList();
    }

    private void HighlightCardEffectArea(PointerEventData eventData)
    {
        // Get the currently hovered tile
        Vector3Int? currentTarget = GetHoveredTile(eventData, BaseTilemap);

        // If the highlighted area isnt valid, do nothing
        if (currentTarget == InvalidPosition)
        {
            return;
        }
        // If the highlighted area hasn't changed, do nothing
        if (lastHighlightedTile == currentTarget)
        {
            return;
        }

        //Debug.Log($"Current hovered tile: {currentTarget}");

        // Determine the new tiles to highlight
        List<Vector3Int> tilesToHighlight = currentTarget.HasValue
            ? GetEffectAreaTiles(currentTarget.Value, BaseTilemap)
            : new List<Vector3Int>();

        // Clear the previous highlights
        TileMapUtilityScript.ResetMaphightlight(BaseTilemap);

        // Highlight the new tiles
       TileMapUtilityScript.SetTilesHighlight(tilesToHighlight, BaseTilemap, TileMapUtilityScript.HighlightType.Target);

        // Update the last highlighted tiles
        if (currentTarget.HasValue)
        {
            //Debug.Log($"Highlighting tiles around {currentTarget.Value}");
            lastHighlightedTile = (Vector3Int)currentTarget;
        }
        else         
            lastHighlightedTile = InvalidPosition;
    }

    private Vector3Int GetTilePosition(GameObject hoveredObject)
    {
        if (BaseTilemap != null)
        {
            Vector3 worldPosition = hoveredObject.transform.position;
            return BaseTilemap.WorldToCell(worldPosition);
        }
        return InvalidPosition;
    }
    private Vector3Int? GetHoveredTile(PointerEventData eventData, Tilemap tilemap)
    {
        foreach (GameObject hoveredObject in eventData.hovered)
        {
            if (hoveredObject.TryGetComponent(out DraggableTarget dt) &&
                dt.draggableTargetType == DraggableTargetType.CombatTile)
            {
                Vector3 worldPosition = hoveredObject.transform.position;
                Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
                return cellPosition;
            }
        }
        return InvalidPosition; // No valid tile found
    }

    private List<Vector3Int> GetEffectAreaTiles(Vector3Int centerTile, Tilemap tilemap)
    {
        // Determine the area of effect based on the card's targeting data
        switch (cardScript.cardData.targetingData.areaType)
        {
            case CardTargetArea.Radius:
                return TileMapUtilityScript.GetTilesInRadius(centerTile, cardScript.cardData.targetingData.range);

            case CardTargetArea.Line:
                return TileMapUtilityScript.GetTilesInLine(centerTile, cardScript.cardData.targetingData.range, 0);

            case CardTargetArea.Ring:
                return TileMapUtilityScript.GetTilesInRing(centerTile, cardScript.cardData.targetingData.range);

            default:
                return new List<Vector3Int> { centerTile };
        }
    }
}