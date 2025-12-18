using UnityEngine;
using Utility;
using TMPro;

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
        if (characterEntity.entityStats.IsRooted) { return; }

        if (TurnManager.Instance.CurrentTurnEntity == characterEntity)
        {        
            base.OnMouseDown();
            startPosition = characterOnMap.currentCell;
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

        if (dropCell == TilemapUtilityScript.InvalidPosition ||
            TilemapUtilityScript.CostInfoScript.costInfoDict[dropCell].isOccupied ||
            TilemapUtilityScript.CostInfoScript.costInfoDict[dropCell].isUnwalkable)
        {
            return;
        }

        // Move the character
        if (characterEntity.entityStats.CurrentStamina >= pathData.PathCost)
{        
            characterOnMap.MoveTo(pathData, characterEntity);
        }

        ClearMovementPreview();
        TilemapUtilityScript.ResetMaphightlight(TilemapUtilityScript.BaseTilemap);
    }


    public override void HandleHover(Vector3Int hoverCell)
    {
        // Ungültig / blockiert? -> Anzeige löschen
        if (hoverCell == TilemapUtilityScript.InvalidPosition ||
            TilemapUtilityScript.CostInfoScript.costInfoDict[hoverCell].isOccupied ||
            TilemapUtilityScript.CostInfoScript.costInfoDict[hoverCell].isUnwalkable)
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
