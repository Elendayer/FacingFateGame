using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utility;

public class EntityOnMap : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector3Int currentCell = new Vector3Int(0,0,0);            // Current logical cell
    public float defaultMovementSpeed = 3f;                                      // Movement speed in units per second

    private Coroutine moveRoutine;
    private bool isDragging = false;

    private void Start()
    {
        TeleportTo(currentCell);
        SetOccupied(true);     
    }

    public void Spawn(Vector3Int vector3Int)
    {
        currentCell = vector3Int;
        transform.position = TilemapUtilityScript.BaseTilemap.CellToWorld(currentCell);
        SetOccupied(false);
    }


    public void MoveTo(PathData pathData, EntityScript entityScript = null, float moveSpeed = 3f)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        Stat moveCostModifier = entityScript.entityStats.MovementCostModifier;

        if (pathData != null && pathData.Path.Count > 0)
        {
            TilemapUtilityScript.SetTilesHighlight(pathData.Path, TilemapUtilityScript.HighlightType.Path);
            
            moveRoutine = StartCoroutine(FollowPath(pathData.Path, moveSpeed));

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
        SetOccupied(false);
        Vector3 targetPos = TilemapUtilityScript.BaseTilemap.GetCellCenterWorld(targetCell);
        transform.position = targetPos;
        currentCell = targetCell;
        SetOccupied(true);
    }

    public Coroutine StartMove(List<Vector3Int> path)
    {
        return StartCoroutine(FollowPath(path, defaultMovementSpeed));
    }

    private IEnumerator FollowPath(List<Vector3Int> path, float speed = 3f)
    {
        SetOccupied(false);

        foreach (var cell in path)
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
        SetOccupied(true);

        TilemapUtilityScript.ResetMaphightlight(path);
    }

    public void SetOccupied( bool b)
    {
        GetCostInfo().isOccupied = b;
    }

    public CostInfo GetCostInfo()
    {
        TilemapUtilityScript.BaseTilemap.GetComponent<CostInfoScript>().costInfoDict.TryGetValue(currentCell, out CostInfo costInfo);
        return costInfo;
    }
}
