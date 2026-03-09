using facingfate;
using TMPro;
using UnityEngine;
using Utility;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DraggableCharacter : Draggable3D
{
    public EntityOnMap characterOnMap;
    public EntityScript characterEntity;

    [Header("Movement Cost Preview")]
    public int previewPathCost;                      
    public TextMeshProUGUI staminaPreviewText;

    private void Awake()
    {
        characterEntity = GetComponent<EntityScript>();
        characterOnMap = GetComponent<EntityOnMap>();

    }

    public override void OnMouseDown()
    {
        if (TimelineManager.isPaused == true) { return; }
        if (characterEntity.entityStats.IsRooted) { return; }

        if (TurnManager.Instance.CurrentTurnEntity == characterEntity)
        {        
            base.OnMouseDown();
        }
    }

    public override void OnMouseUp()
    {
        moveCostModifer = characterEntity.entityStats.MovementCostModifier;
        base.OnMouseUp();
        
    }
    public override void HandleMove(PathData pathData)
    {
        Vector3Int dropCell = pathData.End;

        // Validate target cell
        int dropKey = TilemapUtilityScript.PositionToKey(dropCell);
        if (dropCell == TilemapUtilityScript.InvalidPosition ||
            !TilemapUtilityScript.CostInfoScript.tileInfoDict.ContainsKey(dropKey) ||
            TilemapUtilityScript.CostInfoScript.tileInfoDict[dropKey].isOccupied ||
            TilemapUtilityScript.CostInfoScript.tileInfoDict[dropKey].isUnwalkable)
        {
            return;
        }

        // Only move if enough stamina
        if (characterEntity.entityStats.CurrentStamina >= pathData.PathCost)
        {
            bool moveComplete = false;

            // Enqueue movement via global action queue
            ActionQueueUtility.EnqueueMovement(characterOnMap, pathData, () => moveComplete = true);
        }
        // Clear movement previews and reset tile highlights
        ClearMovementPreview();
        TilemapUtilityScript.ResetMaphightlight(TilemapUtilityScript.BaseTilemap);
    }

    public override void HandleHover(Vector3Int hoverCell)
    {
        // Ungültig / blockiert? -> Anzeige löschen
        int hoverKey = TilemapUtilityScript.PositionToKey(hoverCell);
        if (hoverCell == TilemapUtilityScript.InvalidPosition ||
            !TilemapUtilityScript.CostInfoScript.tileInfoDict.ContainsKey(hoverKey) ||
            TilemapUtilityScript.CostInfoScript.tileInfoDict[hoverKey].isOccupied ||
            TilemapUtilityScript.CostInfoScript.tileInfoDict[hoverKey].isUnwalkable)
        {
            ClearMovementPreview();
            return;
        }

        // Highlight path from current position to hover cell
        TilemapUtilityScript.SetTilesHighlight( MovementUtility.FindPath( characterOnMap.currentCell, hoverCell).Path, TilemapUtilityScript.HighlightType.Path);
    }

    private void ClearMovementPreview()
    {
        previewPathCost = 0;
        if (staminaPreviewText != null) staminaPreviewText.text = string.Empty;
    }


}
