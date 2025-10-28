using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class EntityOnMap : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;              // Units per second

    public Vector3Int currentCell = new Vector3Int(0,0,0);            // Current logical cell

    private Coroutine moveRoutine;
    private bool isDragging = false;
    private Vector3Int targetCell;

    private void Start()
    {
        MoveTo(currentCell);
        SetOccupied(true);     
    }

    public void Spawn(Vector3Int vector3Int)
    {
        refTilemap = TilemapUtilityScript.BaseTilemap;

        currentCell = vector3Int;
        transform.position = refTilemap.CellToWorld(currentCell);
        SetOccupied(false);
    }


    public void MoveTo(Vector3Int targetCell)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);

        List<Vector3Int> path = TilemapUtilityScript.FindPath(currentCell, targetCell)?.Path;

        if (path != null && path.Count > 0)
        {
            TilemapUtilityScript.SetTilesHighlight(path, TilemapUtilityScript.HighlightType.Path);
            moveRoutine = StartCoroutine(FollowPath(path));
        }
        else
        {
            Debug.LogWarning("No path found to " + targetCell);
        }
    }
    public void MoveTo(Vector3Int targetCell, float speed)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);

        List<Vector3Int> path = TilemapUtilityScript.FindPath(currentCell, targetCell)?.Path;

        if (path != null && path.Count > 0)
        {
            TilemapUtilityScript.SetTilesHighlight(path, refTilemap, TilemapUtilityScript.HighlightType.Path);
            moveRoutine = StartCoroutine(FollowPath(path, speed));
        }
        else
        {
            Debug.LogWarning("No path found to " + targetCell);
        }
    }

    public void MoveToViaPath(List<Vector3Int> path)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);

        if (path != null && path.Count > 0)
        {
            TilemapUtilityScript.SetTilesHighlight(path, TilemapUtilityScript.HighlightType.Path);
            moveRoutine = StartCoroutine(FollowPath(path));
        }
        else
        { 
            Debug.LogWarning("No path found to " + targetCell);
        }
    }

    public void TeleportTo(Vector3Int targetCell)
    {
        SetOccupied(false);
        Vector3 targetPos = refTilemap.GetCellCenterWorld(targetCell);
        transform.position = targetPos;
        currentCell = targetCell;
        SetOccupied(true);
    }

    private IEnumerator FollowPath(List<Vector3Int> path, float speed)
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
    void Update()
    {
        HandleMouseDrag();
    }

    #region Mouse Drag Targeting
    private void HandleMouseDrag()
    {
        // On mouse down, check if we clicked on this character
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int clickedCell = TilemapUtilityScript.BaseTilemap.WorldToCell(worldPos);
            Vector3 cellCenter = TilemapUtilityScript.BaseTilemap.GetCellCenterWorld(clickedCell);

            // Start drag if clicked near the character
            if (Vector3.Distance(cellCenter, transform.position) < 0.5f)
            {
                isDragging = true;
                Debug.Log("Started dragging character");
            }
        }

        // While dragging, update target cell
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetCell = TilemapUtilityScript.BaseTilemap.WorldToCell(worldPos);
        }

        // On release, move character to target
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            MoveTo(targetCell);
        }
    }
    #endregion
}
