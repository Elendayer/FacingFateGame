using System;
using System.Collections;
using UnityEngine;
using Utility;

public class EntityOnMap : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector3Int currentCell = new Vector3Int(0,0,0);      // Current logical cell
    public float defaultMovementSpeed = 3f;                     // Movement speed in units per second

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
    public void TeleportTo(Vector3Int targetCell)
    {
        SetOccupied(currentCell, false);
        Vector3 targetPos = TilemapUtilityScript.BaseTilemap.GetCellCenterWorld(targetCell);
        transform.position = targetPos;
        currentCell = targetCell;
        SetOccupied(currentCell, true);
    }

    public IEnumerator StartJumpRoutine(Vector3Int targetTile)
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        moveRoutine = StartCoroutine(JumpTo(targetTile));
        yield return moveRoutine;
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


    [SerializeField] private float jumpDuration = 0.35f;
    [SerializeField] private float jumpHeight = 1.5f;

    private IEnumerator JumpTo(Vector3Int targetTile)
    {
        Vector3 startWorldPos = transform.position;
        Vector3 endWorldPos = TilemapUtilityScript.BaseTilemap.CellToWorld(targetTile);

        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / jumpDuration);

            // Horizontal interpolation
            Vector3 position = Vector3.Lerp(startWorldPos, endWorldPos, t);

            // Vertical arc (parabolic)
            float heightOffset = jumpHeight * 4f * t * (1f - t);
            position.y += heightOffset;

            transform.position = position;
            yield return null;
        }

        transform.position = endWorldPos;
        currentCell = targetTile;

        SetOccupied(currentCell, true);
    }
    public void SetOccupied(Vector3Int pos, bool b)
    {
        costInfoScript.costInfoDict[pos].isOccupied = b;
    }
}
