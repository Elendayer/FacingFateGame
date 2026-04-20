using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace facingfate
{
    /// <summary>
    /// Smooth free-roam camera for combat scenes.
    ///
    /// Controls
    ///   Pan          WASD / Arrow keys (hold Shift for fast)
    ///   Pan (drag)   Middle-mouse drag
    ///   Edge scroll  Mouse near screen border
    ///   Zoom         Scroll wheel
    ///   Rotate       Q / E
    ///   Re-centre    Home key  (snaps back to worldCenter)
    ///
    /// Bounds
    ///   Movement is clamped to the NavMesh surface so the camera never
    ///   looks into empty space.  A max-radius clamp around worldCenter
    ///   provides a hard outer limit.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class FreeCameraController : MonoBehaviour
    {
        // ── Look angle ─────────────────────────────────────────────────────────
        [Header("Look Angle")]
        [Tooltip("Degrees the camera tilts down from the horizon. 0 = side-on, 90 = straight down.")]
        [SerializeField] private float pitchAngle  = 50f;
        [SerializeField] private float initialYaw  =  0f;

        // ── Pan ────────────────────────────────────────────────────────────────
        [Header("Pan")]
        [SerializeField] private float panSpeed        = 15f;
        [SerializeField] private float shiftMultiplier =  2.5f;
        [SerializeField] private float panSmoothTime   =  0.08f;

        [Header("Edge Scroll")]
        [SerializeField] private bool  edgeScrollEnabled = true;
        [SerializeField] private float edgeScrollMargin  = 30f;   // screen-pixels
        [SerializeField] private float edgeScrollSpeed   = 12f;

        // ── Zoom ───────────────────────────────────────────────────────────────
        [Header("Zoom")]
        [SerializeField] private float minZoom        =  5f;
        [SerializeField] private float maxZoom        = 30f;
        [SerializeField] private float zoomSpeed      =  4f;
        [SerializeField] private float zoomSmoothTime =  0.12f;

        // ── Rotation ───────────────────────────────────────────────────────────
        [Header("Rotation")]
        [SerializeField] private bool  rotationEnabled = true;
        [SerializeField] private float rotationSpeed   = 90f;   // degrees/sec

        // ── Bounds ─────────────────────────────────────────────────────────────
        [Header("Bounds")]
        [Tooltip("Drag any scene object here as the map centre. Defaults to world origin.")]
        [SerializeField] private Transform worldCenter;
        [SerializeField] private float     maxDistanceFromCenter = 40f;
        [Tooltip("How far from the candidate point to search for the nearest NavMesh surface.")]
        [SerializeField] private float     navMeshSampleRadius   =  3f;

        // ── Middle-mouse drag ──────────────────────────────────────────────────
        [Header("Middle-Mouse Drag")]
        [SerializeField] private bool dragPanEnabled = true;

        // ── Runtime state ──────────────────────────────────────────────────────
        private Camera  cam;
        private float   yaw;

        private Vector3 focusPoint;        // actual ground pivot (smoothly follows target)
        private Vector3 targetFocusPoint;  // desired ground pivot (set by input, always clamped)
        private Vector3 panVelocity;

        private float   currentZoom;
        private float   targetZoom;
        private float   zoomVelocity;

        private bool    isDragging;
        private Vector3 dragLastWorldPos;
        private float   dragPlaneY;

        // ── Lifecycle ──────────────────────────────────────────────────────────

        private void Awake()
        {
            cam = GetComponent<Camera>();
            yaw = initialYaw;

            float midZoom = Mathf.Lerp(minZoom, maxZoom, 0.35f);
            currentZoom = targetZoom = midZoom;

            Vector3 origin = worldCenter != null ? worldCenter.position : Vector3.zero;
            focusPoint = targetFocusPoint = SnapToNavMesh(origin, 200f) ?? origin;

            ApplyCameraTransform();
        }

        private void Update()
        {
            HandleRotation();
            HandleKeyboardPan();
            HandleEdgeScroll();
            HandleDragPan();
            HandleZoom();
            HandleRecenter();
            SmoothApply();
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Smoothly moves the camera focus to a world position.</summary>
        public void FocusOn(Vector3 worldPosition, bool instant = false)
        {
            targetFocusPoint = ClampFocusPoint(worldPosition);
            if (instant)
            {
                focusPoint  = targetFocusPoint;
                panVelocity = Vector3.zero;
                ApplyCameraTransform();
            }
        }

        // ── Input handlers ─────────────────────────────────────────────────────

        private void HandleRotation()
        {
            if (!rotationEnabled) return;
            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            float dir = 0f;
            if (kb.qKey.isPressed) dir -= 1f;
            if (kb.eKey.isPressed) dir += 1f;
            if (dir == 0f) return;

            yaw  = (yaw + dir * rotationSpeed * Time.deltaTime) % 360f;
        }

        private void HandleKeyboardPan()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            Vector2 input = Vector2.zero;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    input.y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  input.y -= 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  input.x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;
            if (input == Vector2.zero) return;

            float speed = panSpeed
                          * (kb.leftShiftKey.isPressed ? shiftMultiplier : 1f)
                          * ZoomFraction();

            ApplyPanDelta(input.normalized, speed);
        }

        private void HandleEdgeScroll()
        {
            if (!edgeScrollEnabled) return;
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 pos = mouse.position.ReadValue();
            Vector2 dir = Vector2.zero;

            if (pos.x < edgeScrollMargin)                  dir.x -= 1f;
            if (pos.x > Screen.width  - edgeScrollMargin)  dir.x += 1f;
            if (pos.y < edgeScrollMargin)                  dir.y -= 1f;
            if (pos.y > Screen.height - edgeScrollMargin)  dir.y += 1f;
            if (dir == Vector2.zero) return;

            ApplyPanDelta(dir.normalized, edgeScrollSpeed * ZoomFraction());
        }

        private void HandleDragPan()
        {
            if (!dragPanEnabled) return;
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.middleButton.wasPressedThisFrame)
            {
                isDragging       = true;
                dragPlaneY       = focusPoint.y;
                dragLastWorldPos = MouseOnPlane(dragPlaneY);
            }

            if (mouse.middleButton.wasReleasedThisFrame)
                isDragging = false;

            if (!isDragging) return;

            Vector3 current = MouseOnPlane(dragPlaneY);
            Vector3 delta   = dragLastWorldPos - current;   // inverted for natural grab feel
            dragLastWorldPos = current;

            targetFocusPoint = ClampFocusPoint(targetFocusPoint + delta);
        }

        private void HandleZoom()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) < 0.01f) return;

            targetZoom = Mathf.Clamp(targetZoom - scroll * zoomSpeed * 0.1f, minZoom, maxZoom);
        }

        private void HandleRecenter()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;
            if (!kb.homeKey.wasPressedThisFrame) return;

            Vector3 origin = worldCenter != null ? worldCenter.position : Vector3.zero;
            targetFocusPoint = ClampFocusPoint(origin);
        }

        // ── Smooth apply ───────────────────────────────────────────────────────

        private void SmoothApply()
        {
            focusPoint = Vector3.SmoothDamp(
                focusPoint, targetFocusPoint, ref panVelocity, panSmoothTime);

            currentZoom = Mathf.SmoothDamp(
                currentZoom, targetZoom, ref zoomVelocity, zoomSmoothTime);

            ApplyCameraTransform();
        }

        private void ApplyCameraTransform()
        {
            transform.rotation = Quaternion.Euler(pitchAngle, yaw, 0f);
            transform.position = focusPoint - transform.forward * currentZoom;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>Translates a 2D direction (relative to current yaw) into a world-space pan.</summary>
        private void ApplyPanDelta(Vector2 dir, float speed)
        {
            Vector3 forward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            Vector3 right   = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;
            Vector3 move    = (forward * dir.y + right * dir.x) * speed * Time.deltaTime;
            targetFocusPoint = ClampFocusPoint(targetFocusPoint + move);
        }

        /// <summary>
        /// Applies two clamps to a candidate focus point:
        ///   1. Snaps to the nearest NavMesh surface (prevents looking into the void).
        ///   2. Hard radius limit around worldCenter.
        /// </summary>
        private Vector3 ClampFocusPoint(Vector3 candidate)
        {
            // 1 — NavMesh surface clamp
            Vector3? snapped = SnapToNavMesh(candidate, navMeshSampleRadius);
            if (snapped.HasValue)
                candidate = snapped.Value;
            // If NavMesh sample fails entirely (e.g., no NavMesh baked), fall through.

            // 2 — Radius clamp from world centre
            Vector3 center = worldCenter != null ? worldCenter.position : Vector3.zero;
            Vector3 offset = candidate - center;
            if (offset.magnitude > maxDistanceFromCenter)
                candidate = center + offset.normalized * maxDistanceFromCenter;

            return candidate;
        }

        /// <summary>Returns the nearest NavMesh position within the given search radius, or null.</summary>
        private static Vector3? SnapToNavMesh(Vector3 point, float radius)
        {
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, radius, NavMesh.AllAreas))
                return hit.position;
            return null;
        }

        /// <summary>Projects the mouse ray onto a horizontal plane at height y.</summary>
        private Vector3 MouseOnPlane(float y)
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return focusPoint;

            Ray   ray   = cam.ScreenPointToRay(mouse.position.ReadValue());
            Plane plane = new Plane(Vector3.up, new Vector3(0f, y, 0f));
            if (plane.Raycast(ray, out float dist))
                return ray.GetPoint(dist);

            return focusPoint;
        }

        /// <summary>
        /// Pan speed fraction based on current zoom.
        /// Zoomed in → slower panning (map feels consistent at any distance).
        /// </summary>
        private float ZoomFraction() => currentZoom / maxZoom;
    }
}
