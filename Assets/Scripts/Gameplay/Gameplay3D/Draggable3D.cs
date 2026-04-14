using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace facingfate
{
    [RequireComponent(typeof(Collider))]
    public class Draggable3D : MonoBehaviour
    {
        [Header("Arrow / Line Preview")]
        public LineRenderer lineRenderer;
        public int curveResolution = 6;
        public float arrowHeightOffset = 1f;

        [Header("Path Cost Colors")]
        public Color affordablePathColor = Color.green;
        public Color unaffordablePathColor = Color.red;

        protected bool isDragging = false;
        protected Vector3 currentPosition;
        protected int currentPathCost = 0;

        public float moveCostModifer;

        protected virtual void Update()
        {
            // Check for drag start via InputManager
            if (InputManager.Instance.IsLeftMouseButtonPressed && !isDragging)
            {
                if (InputManager.Instance.TryRaycastFromMouse(out RaycastHit hit))
                {
                    hit.collider.TryGetComponent(out Draggable3D draggable);
                    if (draggable == this)
                    {
                        OnMouseDown();
                    }
                }
            }

            // Check for drag end via InputManager
            if (InputManager.Instance.IsLeftMouseButtonReleased && isDragging)
            {
                OnMouseUp();
                return;
            }

            if (!isDragging) return;

            // Call virtual method for drag-specific updates
            OnDragUpdate();
        }

        public virtual void OnMouseDown()
        {
            isDragging = true;
            currentPosition = transform.position;

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

            // Disable obstacle carving when drag ends
            DisableAgentObstacles();
        }

        protected virtual void DisableAgentObstacles()
        {
            // Start coroutine to disable carving and re-enable agents with proper timing
            StartCoroutine(DisableAgentObstaclesRoutine());
        }

        protected virtual IEnumerator DisableAgentObstaclesRoutine()
        {
            // Find all entities and disable their obstacle carving
            EntityOnMap[] allEntities = FindObjectsByType<EntityOnMap>(FindObjectsSortMode.None);

            foreach (EntityOnMap entity in allEntities)
            {
                if (entity.gameObject == gameObject) continue;

                entity.DisableObstacleCarving();
            }

            // Yield a frame to allow the navmesh to rebuild
            yield return null;

            // Re-enable agents after navmesh has rebuilt
            foreach (EntityOnMap entity in allEntities)
            {
                if (entity.gameObject == gameObject) continue;

                StartCoroutine(entity.ReenableAgentAfterCarvingDisabled());
            }
        }

        protected virtual void OnDragUpdate()
        {
            // Base implementation: raycast and update arrow
            if (InputManager.Instance.TryRaycastFromMouse(out RaycastHit hit))
            {
                Vector3 cursorPosition = hit.point;

                if (lineRenderer != null)
                    UpdateArrow(currentPosition, cursorPosition);
            }
        }

        protected void UpdateArrow(Vector3 start, Vector3 end)
        {
            // Ensure all agents on the map have NavMeshObstacles with carving enabled
            EnsureAgentObstacles();

            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path);

            if (path.status == NavMeshPathStatus.PathComplete || path.status == NavMeshPathStatus.PathPartial)
            {
                lineRenderer.positionCount = path.corners.Length;
                for (int i = 0; i < path.corners.Length; i++)
                {
                    lineRenderer.SetPosition(i, path.corners[i]);
                }
            }
            else
            {
                // Fallback: show direct line if path cannot be calculated
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, start);
                lineRenderer.SetPosition(1, end);
            }

            // Update line renderer color based on path cost vs stamina
            UpdateLineRendererColor(start, end);
        }

        protected virtual void EnsureAgentObstacles()
        {
            // Find all entities on the map and enable their obstacle carving for pathfinding preview
            EntityOnMap[] allEntities = FindObjectsByType<EntityOnMap>(FindObjectsSortMode.None);

            foreach (EntityOnMap entity in allEntities)
            {
                // Skip self
                if (entity.gameObject == gameObject) continue;

                // Use EntityOnMap's cached navMeshAgent and navMeshObstacle
                entity.EnableObstacleCarving();
            }
        }

        protected virtual void UpdateLineRendererColor(Vector3 start, Vector3 end)
        {
            PathData pathData = MovementUtility.FindPath(start, end, false, true, moveCostModifer);
            currentPathCost = pathData.PathCost;

            ApplyLineRendererColor();
        }

        protected virtual void ApplyLineRendererColor()
        {
            if (lineRenderer != null && lineRenderer.material != null)
            {
                lineRenderer.material.color = affordablePathColor;
            }
        }
    }
}