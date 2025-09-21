using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utility;

public class EntityOnMap : MonoBehaviour
{
    [Header("References")]
    public Tilemap refTilemap;            // Overlay tilemap where character appears
    public  BasemapHexTile DefaultTile;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;              // Units per second

    public Vector3Int currentCell = new Vector3Int(0,0,0);            // Current logical cell

    private Coroutine moveRoutine;
    private bool isDragging = false;
    private Vector3Int targetCell;

    private void Start()
    {
        transform.position = refTilemap.CellToWorld(currentCell);
        SetOccupied(true);     
    }
    /// <summary>
    /// Move the character to the target cell coordinate using A* pathfinding.
    /// </summary>
    public void MoveTo(Vector3Int targetCell)
    {
        SetOccupied(false);
        if (moveRoutine != null) StopCoroutine(moveRoutine);

        List<Vector3Int> path = TilemapUtilityScript.FindPath(currentCell, targetCell);

        if (path != null && path.Count > 0)
        {
            TilemapUtilityScript.SetTilesHighlight(path, refTilemap, TilemapUtilityScript.HighlightType.Path);
            moveRoutine = StartCoroutine(FollowPath(path));
        }
        else
            Debug.LogWarning("No path found to " + targetCell);
    }

    private IEnumerator FollowPath(List<Vector3Int> path)
    {
        foreach (var cell in path)
        {
            Vector3 targetPos = refTilemap.GetCellCenterWorld(cell);

            // Smooth movement toward target cell
            while (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }

            // Snap exactly to cell center
            transform.position = targetPos;

            // Update logical cell
            currentCell = cell;
        }

        moveRoutine = null;
        SetOccupied(true);

        TilemapUtilityScript.ResetMaphightlight(path,refTilemap);
    }

    public void SetOccupied( bool b)
    {
        GetCostInfo().isOccupied = b;
    }

    public CostInfo GetCostInfo()
    {
        refTilemap.GetComponent<CostInfoScript>().costInfoDict.TryGetValue(currentCell, out CostInfo costInfo);
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
            Vector3Int clickedCell = refTilemap.WorldToCell(worldPos);
            Vector3 cellCenter = refTilemap.GetCellCenterWorld(clickedCell);

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
            targetCell = refTilemap.WorldToCell(worldPos);
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
