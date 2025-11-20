using UnityEngine;
using UnityEngine.Tilemaps;
using Utility;

[RequireComponent(typeof(Collider))]
public class DraggableCharacter : Draggable3D
{
    public EntityOnMap characterOnMap;
    public EntityScript characterEntity;

    private void Awake()
    {
        characterEntity = GetComponent<EntityScript>();
        characterOnMap = GetComponent<EntityOnMap>();

    }

    public override void OnMouseDown()
    {
        if(!TurnManager.Instance.CurrentTurnEntity == characterEntity)
        {
            isDragging = false;
            return;
        }
        base.OnMouseDown();
    }
    public override void HandleMove(PathData pathData)
    {
        Vector3Int dropCell = pathData.End;

        // Only move if valid
        if (dropCell == TilemapUtilityScript.InvalidPosition ||
            TilemapUtilityScript.CostInfoScript.costInfoDict[dropCell].isOccupied ||
            TilemapUtilityScript.CostInfoScript.costInfoDict[dropCell].isUnwalkable)
        {
            // Invalid drop → do nothing
            return;
        }

        // Move the character
        if (characterEntity.entityStats.CurrentStamina >= pathData.PathCost)
{        
            //CombatUtility.ApplyCost(characterEntity, characterOnMap.GetComponent<PlayerScript>().entityStats.CurrentStamina, pathData.PathCost);
            characterOnMap.MoveTo(dropCell);
        }

        // Optionally clear path highlight after move
        TilemapUtilityScript.ResetMaphightlight(TilemapUtilityScript.BaseTilemap);
    }

    public override void HandleHover(Vector3Int hoverCell)
    {
        if (hoverCell == TilemapUtilityScript.InvalidPosition ||
            TilemapUtilityScript.CostInfoScript.costInfoDict[hoverCell].isOccupied ||
            TilemapUtilityScript.CostInfoScript.costInfoDict[hoverCell].isUnwalkable)
        {
            // Invalid hover → clear highlight
            TilemapUtilityScript.ResetMaphightlight(TilemapUtilityScript.BaseTilemap);
            return;
        }

        // Highlight path from current position to hover cell
        Debug.Log(TilemapUtilityScript.FindPath(characterOnMap.currentCell, hoverCell).Path.Count);
        TilemapUtilityScript.SetTilesHighlight( TilemapUtilityScript.FindPath( characterOnMap.currentCell, hoverCell).Path, TilemapUtilityScript.HighlightType.Path);
    }
}
