using UnityEngine;
using UnityEngine.InputSystem;

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
        protected Vector3 currentPosition;

        public Stat moveCostModifer;

        protected virtual void Update()
        {
            // Check for drag start with InputSystem
            if (Mouse.current?.leftButton.wasPressedThisFrame == true && !isDragging)
            {
                OnMouseDown();
            }

            // Check for drag end with InputSystem
            if (Mouse.current?.leftButton.wasReleasedThisFrame == true && isDragging)
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
        }

        protected virtual void OnDragUpdate()
        {
            // Base implementation: raycast and update arrow
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 cursorPosition = hit.point;

                if (lineRenderer != null)
                    UpdateArrow(currentPosition, cursorPosition);
            }
        }

        protected void UpdateArrow(Vector3 start, Vector3 end)
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