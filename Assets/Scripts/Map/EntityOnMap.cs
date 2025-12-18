using System.Collections;
using UnityEngine;
using Utility;

public class EntityOnMap : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector3Int currentCell = new Vector3Int(0,0,0);            // Current logical cell
    public float defaultMovementSpeed = 3f;                                      // Movement speed in units per second

    private Coroutine moveRoutine;
    private bool isDragging = false;

    private CostInfoScript costInfoScript;

    public void Startup()
    {
        costInfoScript = FindAnyObjectByType<CostInfoScript>();

        TeleportTo(currentCell);
        SetOccupied(currentCell, true);     
    }

    public void Spawn(Vector3Int desiredCell)
    {
        currentCell = vector3Int;
        transform.position = TilemapUtilityScript.BaseTilemap.CellToWorld(currentCell);
        SetOccupied(vector3Int, false);
    }


    public void MoveTo(PathData pathData, EntityScript entityScript = null, float moveSpeed = 3f)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        Stat moveCostModifier = entityScript.entityStats.MovementCostModifier;

        if (pathData != null && pathData.Path.Count > 0)
        {
            TilemapUtilityScript.SetTilesHighlight(pathData.Path, TilemapUtilityScript.HighlightType.Path);
            
            moveRoutine = StartCoroutine(FollowPath(pathData, moveSpeed));

            if (entityScript != null)
            {
                entityScript.entityStats.CurrentStamina -= pathData.PathCost;
            }
        }
        else
        {
            Debug.LogWarning("No path found");
        }
    }
    public void TeleportTo(Vector3Int targetCell)
    {
        SetOccupied(currentCell, false);
        Vector3 targetPos = TilemapUtilityScript.BaseTilemap.GetCellCenterWorld(targetCell);
        transform.position = targetPos;
        currentCell = targetCell;
        SetOccupied(currentCell, true);
    }

    public Coroutine StartMove(PathData pathData)
    {
        return StartCoroutine(FollowPath(pathData, defaultMovementSpeed));
    }

    private IEnumerator FollowPath(PathData pathData, float speed = 3f)
    {
        SetOccupied(pathData.Start,false);

        foreach (var cell in pathData.Path)
        {
            Vector3 targetPos = TilemapUtilityScript.BaseTilemap.GetCellCenterWorld(cell);

            // Smooth movement toward target cell
            while (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                yield return null;
            }

            // Snap exactly to cell center
            transform.position = targetPos;

            // Update logical cell
            currentCell = cell;
        }

        moveRoutine = null;
        SetOccupied(pathData.End, true);

        TilemapUtilityScript.ResetMaphightlight(pathData.Path);
    }

    public void SetOccupied(Vector3Int pos, bool b)
    {
        costInfoScript.costInfoDict[pos].isOccupied = b;
    }
}
