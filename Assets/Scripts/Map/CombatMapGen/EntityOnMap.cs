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
        currentCell = desiredCell;
        transform.position = TilemapUtilityScript.BaseTilemap.CellToWorld(currentCell);
        SetOccupied(desiredCell, false);
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

    public IEnumerator StartMoveRoutine(PathData pathData)
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        moveRoutine = StartCoroutine(FollowPath(pathData, defaultMovementSpeed));
        yield return moveRoutine;
    }

    private IEnumerator FollowPath(PathData pathData, float speed)
    {
        SetOccupied(currentCell, false);

        foreach (var cell in pathData.Path)
        {
            Vector3 targetPos = TilemapUtilityScript.BaseTilemap.GetCellCenterWorld(cell);

            while ((transform.position - targetPos).sqrMagnitude > 0.0025f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    speed * Time.deltaTime
                );
                yield return null;
            }

            transform.position = targetPos;
            currentCell = cell;
        }

        moveRoutine = null;
        SetOccupied(currentCell, true);
        TilemapUtilityScript.ResetMaphightlight(pathData.Path);
    }


    public void SetOccupied(Vector3Int pos, bool b)
    {
        costInfoScript.costInfoDict[pos].isOccupied = b;
    }
}
