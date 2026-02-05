using System;
using UnityEngine;
using Utility;

namespace facingfate
{
    [RequireComponent(typeof(Collider))]
    public class Draggable3D : MonoBehaviour
    {
        [Header("Arrow / Line Preview")]
        public LineRenderer lineRenderer;
        public int curveResolution = 6;
        public float arrowHeightOffset = 1f;

        protected bool isDragging = false;
        protected Vector3 startPosition;
        protected Vector3Int lastHoveredTile = TilemapUtilityScript.InvalidPosition;

        public Stat moveCostModifer;

        public virtual void OnMouseDown()
        {
            isDragging = true;

            startPosition = transform.position;

            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
                lineRenderer.positionCount = curveResolution;
            }
        }

        public virtual void OnMouseUp()
        {
            if (!isDragging) return;
            isDragging = false;

            if (lineRenderer != null)
                lineRenderer.enabled = false;

            // Trigger drag-end event
            Vector3Int dropCell = lastHoveredTile;
            PathData pathData = MovementUtility.FindPath(
                Vector3Int.RoundToInt(startPosition),
                dropCell,
                movementCostModifier: moveCostModifer
            );

            Debug.Log($"[Draggable3D] Drag ended on tile: {dropCell}");
            HandleMove(pathData);

            // Reset tile highlights
            TilemapUtilityScript.ResetMaphightlight(TilemapUtilityScript.BaseTilemap);
        }



        void Update()
        {
            if (!isDragging) return;

            // Raycast to world
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 cursorPosition = hit.point;

                // Update arrow preview
                if (lineRenderer != null)
                    UpdateArrow(startPosition, cursorPosition);

                // Update hovered tile
                Vector3Int hoveredTile = TargetingUtility.GetHoveredTile(ray);

                if (hoveredTile != TilemapUtilityScript.InvalidPosition &&
                    hoveredTile != lastHoveredTile)
                {
                    lastHoveredTile = hoveredTile;

                    // Trigger drag-update event
                    HandleHover(hoveredTile);

                    // Highlight the hovered tile
                    TilemapUtilityScript.ResetMaphightlight(TilemapUtilityScript.BaseTilemap);
                    if (hoveredTile != TilemapUtilityScript.InvalidPosition)
                    {
                        Vector3Int vector3Int = Vector3Int.RoundToInt(startPosition);
                        TilemapUtilityScript.SetTilesHighlight(
                            MovementUtility.FindPath(vector3Int, hoveredTile).Path,
                            TilemapUtilityScript.HighlightType.Path
                        );
                    }
                }
            }
        }
        public virtual void HandleMove(PathData pathData)
        {

        }
        public virtual void HandleHover(Vector3Int hoveredTile)
        {

        }

        public void UpdateArrow(Vector3 start, Vector3 end)
        {
            Vector3 control = (start + end) / 2 + Vector3.up * arrowHeightOffset;

            lineRenderer.positionCount = curveResolution;
            for (int i = 0; i < curveResolution; i++)
            {
                float t = i / (float)(curveResolution - 1);
                Vector3 point = Mathf.Pow(1 - t, 2) * start +
                                2 * (1 - t) * t * control +
                                Mathf.Pow(t, 2) * end;
                lineRenderer.SetPosition(i, point);
            }
        }
    }
}
