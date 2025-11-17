using UnityEngine;
using UnityEngine.Tilemaps;
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
        if(TurnManager.Instance.CurrentTurnEntity != characterEntity)
        {
            isDragging = false;
            return;
        }
        base.OnMouseDown();
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

        if (characterEntity.entityStats.CurrentStamina.Value >= pathData.PathCost)
        {
            characterOnMap.MoveTo(dropCell);
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

        // Pfad von der aktuellen Zelle zur Hover-Zelle berechnen
        PathData pathData = TilemapUtilityScript.FindPath(characterOnMap.currentCell, hoverCell);

        if (pathData == null || pathData.Path == null || pathData.Path.Count == 0)
        {
            ClearMovementPreview();
            return;
        }

        // Gesamt-Stamina-Kosten im Inspector sichtbar machen
        previewPathCost = pathData.PathCost;

        // UI-Text aktualisieren, falls verlinkt
        if (staminaPreviewText != null)
        {
            staminaPreviewText.text = previewPathCost.ToString();
            bool canAfford = characterEntity.entityStats.CurrentStamina.Value >= previewPathCost;
            staminaPreviewText.color = canAfford ? Color.white : Color.red;
        }
    }

    private void ClearMovementPreview()
    {
        previewPathCost = 0;
        if (staminaPreviewText != null) staminaPreviewText.text = string.Empty;
    }


}
