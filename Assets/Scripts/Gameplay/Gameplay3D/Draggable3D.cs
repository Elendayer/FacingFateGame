using UnityEngine;
using UnityEngine.AI;

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

        protected EntityStats entityStats;

        protected virtual void Update()
        {
            if (InputManager.Instance.IsLeftMouseButtonPressed && !isDragging)
            {
                if (InputManager.Instance.TryRaycastFromMouse(out RaycastHit hit))
                {
                    hit.collider.TryGetComponent(out Draggable3D draggable);
                    if (draggable == this)
                        OnMouseDown();
                }
            }

            if (InputManager.Instance.IsLeftMouseButtonReleased && isDragging)
            {
                OnMouseUp();
                return;
            }

            if (!isDragging) return;

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

            // NOTE: No manual obstacle toggling here.
            // EntityOnMap handles its own obstacle/agent state internally.
        }

        protected virtual void OnDragUpdate()
        {
            if (InputManager.Instance.TryRaycastFromMouse(out RaycastHit hit))
            {
                if (lineRenderer != null)
                    UpdateArrow(currentPosition, hit.point);
            }
        }

        protected void UpdateArrow(Vector3 start, Vector3 end)
        {
            // Entities are already NavMeshObstacles while idle, so the
            // NavMesh already has holes around them. No manual enabling needed.
            NavMeshPathData path = MovementUtility.FindPath(start, end, entityStats);

            if (path?.CachedNavMeshPath != null && path.CachedNavMeshPath.corners.Length > 0)
            {
                lineRenderer.positionCount = path.CachedNavMeshPath.corners.Length;
                for (int i = 0; i < path.CachedNavMeshPath.corners.Length; i++)
                    lineRenderer.SetPosition(i, path.CachedNavMeshPath.corners[i]);
            }

            UpdateLineRendererColor(path);
        }

        protected virtual void UpdateLineRendererColor(NavMeshPathData path)
        {
            if (path != null)
                currentPathCost = path.PathCost;
            ApplyLineRendererColor();
        }

        protected virtual void ApplyLineRendererColor()
        {
            if (lineRenderer != null && lineRenderer.material != null)
                lineRenderer.material.color = affordablePathColor;
        }
    }
}