using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        Vector3Int spawnCell = GetNearestWalkableCell(currentCell);
        TeleportTo(spawnCell);   
        //MoveTo(spawnCell);
    }

    public void Spawn(Vector3Int desiredCell)
    {

        Vector3Int spawnCell = GetNearestWalkableCell(desiredCell);
        TeleportTo(spawnCell);
    }


    public void MoveTo(Vector3Int targetCell, bool isTeleport = false)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);

        List<Vector3Int> path = TilemapUtilityScript.FindPath(currentCell, targetCell)?.Path;

        if (path != null && path.Count > 0)
        {
            TilemapUtilityScript.SetTilesHighlight(path, TilemapUtilityScript.HighlightType.Path);
            moveRoutine = StartCoroutine(FollowPath(path, moveSpeed));
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
            TilemapUtilityScript.SetTilesHighlight(path, TilemapUtilityScript.HighlightType.Path);
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
            moveRoutine = StartCoroutine(FollowPath(path, moveSpeed));
        }
        else
        { 
            Debug.LogWarning("No path found to " + targetCell);
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

    private Vector3Int GetNearestWalkableCell(Vector3Int desiredCell)
    {
        var costInfoScript = TilemapUtilityScript.BaseTilemap.GetComponent<CostInfoScript>();
        if (costInfoScript == null || costInfoScript.costInfoDict == null || costInfoScript.costInfoDict.Count == 0)
        {
            Debug.LogWarning("EntityOnMap: No CostInfoScript / empty costInfoDict found. Using desiredCell as spawn.");
            return desiredCell;
        }

        var dict = costInfoScript.costInfoDict;

        if (dict.TryGetValue(desiredCell, out var info) &&
            !info.isUnwalkable &&
            !info.isOccupied)
        {
            return desiredCell;
        }

        bool found = false;
        Vector3Int bestCell = desiredCell;
        float bestDistSq = float.MaxValue;

        foreach (var kvp in dict)
        {
            var cell = kvp.Key;
            var ci = kvp.Value;

            // Nur wirklich begehbare und nicht besetzte Tiles
            if (ci.isUnwalkable || ci.isOccupied) continue;

            float dx = cell.x - desiredCell.x;
            float dy = cell.y - desiredCell.y;
            float distSq = dx * dx + dy * dy;

            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                bestCell = cell;
                found = true;
            }
        }

        if (!found)
        {
            Debug.LogWarning($"EntityOnMap: No walkable cell found near {desiredCell}, staying in place.");
            return desiredCell;
        }

        return bestCell;
    }


    public Coroutine StartMove(List<Vector3Int> path)
    {
        Coroutine coroutine = StartCoroutine(FollowPath(path, moveSpeed));
        return coroutine;
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
        var info = GetCostInfo();
        if (info != null)
        {
            info.isOccupied = b;
        }
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
