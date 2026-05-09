using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using DG.Tweening;

namespace facingfate
{
    [RequireComponent(typeof(Camera))]
    public class FreeCameraController : MonoBehaviour
    {
        // ── Look angle ─────────────────────────────────────────────────────────
        [Header("Look Angle")]
        [Tooltip("Degrees the camera tilts down from the horizon. 0 = side-on, 90 = straight down.")]
        [SerializeField] private float pitchAngle = 50f;
        [Tooltip("Initial horizontal rotation in degrees.")]
        [SerializeField] private float initialYaw = 0f;
        [Tooltip("Fixed distance from focus point. Adjust to frame the map correctly.")]
        [SerializeField] private float cameraDistance = 20f;

        // ── Auto Pan ───────────────────────────────────────────────────────────
        [Header("Auto Pan")]
        [Tooltip("Minimum pan duration in seconds (used for very short distances).")]
        [SerializeField] private float minPanDuration = 0.3f;
        [Tooltip("Maximum pan duration in seconds (used for long distances).")]
        [SerializeField] private float maxPanDuration = 1.2f;
        [Tooltip("Reference speed (units/sec) used to derive pan duration from distance.")]
        [SerializeField] private float autoFocusSpeed = 20f;
        [Tooltip("DOTween ease curve for auto-pan and spacebar snap.")]
        [SerializeField] private Ease panEase = Ease.InOutSine;

        // ── Manual Pan ─────────────────────────────────────────────────────────
        [Header("Pan")]
        [Tooltip("Arrow key pan speed in world units per second.")]
        [SerializeField] private float arrowPanSpeed = 15f;
        [Tooltip("SmoothDamp time for manual pan feel.")]
        [SerializeField] private float panSmoothTime = 0.08f;

        // ── Edge Scroll ────────────────────────────────────────────────────────
        [Header("Edge Scroll")]
        [Tooltip("Disable during editor testing to prevent unintended panning.")]
        [SerializeField] private bool edgeScrollEnabled = true;
        [Tooltip("Dead zone in pixels from screen edge that triggers edge scroll.")]
        [SerializeField] private float edgeScrollMargin = 30f;
        [Tooltip("Edge scroll speed in world units per second.")]
        [SerializeField] private float edgeScrollSpeed = 12f;

        // ── Scroll Zoom ────────────────────────────────────────────────────────
        [Header("Scroll Zoom")]
        [Tooltip("World-units of zoom per scroll notch.")]
        [SerializeField] private float scrollZoomSpeed = 50f;
        [Tooltip("Minimum camera distance — prevents clipping into mesh.")]
        [SerializeField] private float minCameraDistance = 8f;
        [Tooltip("Maximum camera distance.")]
        [SerializeField] private float maxCameraDistance = 40f;

        // ── Middle-Mouse Drag ──────────────────────────────────────────────────
        [Header("Middle-Mouse Drag")]
        [Tooltip("Drag sensitivity multiplier. 1 = natural 1:1 map grab.")]
        [SerializeField] private float dragSensitivity = 1f;

        // ── Bounds ─────────────────────────────────────────────────────────────
        [Header("Bounds")]
        [Tooltip("NavMesh sample radius for bounds clamping. Must exceed max camera movement per frame. 50 is safe for most maps.")]
        [SerializeField] private float navMeshClampRadius = 50f;

        // ── State ──────────────────────────────────────────────────────────────
        private enum CameraState { AutoPanning, PlayerControl, NpcFollowing }
        private CameraState state = CameraState.PlayerControl;

        private Camera cam;
        private float yaw;

        private Vector3 focusPoint;
        private Vector3 targetFocusPoint;
        private Vector3 panVelocity;

        private EntityScript currentEntity;
        private bool isPlayerTurn;
        private bool combatActive;

        private Tweener autoPanTween;

        private bool isDragging;
        private Vector3 dragLastWorldPos;
        private float dragPlaneY;

        // ── Lifecycle ──────────────────────────────────────────────────────────

        private void Awake()
        {
            cam = GetComponent<Camera>();
            yaw = initialYaw;

            // Start focused on whatever world position is below the camera
            Vector3 origin = transform.position + transform.forward * cameraDistance;
            focusPoint = targetFocusPoint = origin;

            ApplyCameraTransform();
        }

        private void OnEnable()
        {
            GameEvents.OnTurnEntityChanged += OnTurnEntityChanged;
            GameEvents.OnCombatStart += OnCombatStart;
            GameEvents.OnCombatEnd += OnCombatEnd;
        }

        private void OnDisable()
        {
            GameEvents.OnTurnEntityChanged -= OnTurnEntityChanged;
            GameEvents.OnCombatStart -= OnCombatStart;
            GameEvents.OnCombatEnd -= OnCombatEnd;
            autoPanTween?.Kill();
        }

        private void Update()
        {
            if (!combatActive) return;

            // Player input cancels auto-pan
            if (state == CameraState.AutoPanning && HasPlayerInput())
            {
                autoPanTween?.Kill();
                autoPanTween = null;
                state = CameraState.PlayerControl;
            }

            // NPC follow: track entity each frame
            if (state == CameraState.NpcFollowing && currentEntity != null)
                targetFocusPoint = ClampFocusPoint(currentEntity.transform.position);

            // Player input only accepted during PlayerControl (NPC follow blocks it)
            if (state != CameraState.NpcFollowing)
            {
                HandleKeyboardPan();
                HandleEdgeScroll();
                HandleDragPan();
                HandleScrollZoom();
            }

            HandleSpacebarSnap();
            SmoothApply();
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Smoothly pans to a world position using the auto-pan tween.
        /// Interrupts any in-progress auto-pan.
        /// </summary>
        public void FocusOn(Vector3 worldPosition)
        {
            StartAutoPan(worldPosition);
        }

        // ── Event handlers ─────────────────────────────────────────────────────

        private void OnCombatStart()
        {
            combatActive = true;
        }

        private void OnCombatEnd(bool playerWon)
        {
            // Tutorial wave transitions fire CombatEnd between waves — keep camera active.
            if (TutorialCombatManager.Instance != null && TutorialCombatManager.Instance.IsActive)
                return;

            combatActive = false;
            autoPanTween?.Kill();
            autoPanTween = null;
            state = CameraState.PlayerControl;
        }

        private void OnTurnEntityChanged(EntityScript entity)
        {
            currentEntity = entity;
            isPlayerTurn = entity.GetComponent<PlayerScript>() != null;
            StartAutoPan(entity.transform.position);
        }

        // ── Auto pan ───────────────────────────────────────────────────────────

        private void StartAutoPan(Vector3 worldTarget)
        {
            autoPanTween?.Kill();
            panVelocity = Vector3.zero;

            Vector3 target = ClampFocusPoint(worldTarget);
            float dist = Vector3.Distance(focusPoint, target);
            float duration = Mathf.Clamp(dist / autoFocusSpeed, minPanDuration, maxPanDuration);

            state = CameraState.AutoPanning;

            autoPanTween = DOTween.To(
                () => focusPoint,
                v =>
                {
                    focusPoint = v;
                    targetFocusPoint = v;
                },
                target,
                duration
            ).SetEase(panEase).OnComplete(() =>
            {
                autoPanTween = null;
                state = isPlayerTurn ? CameraState.PlayerControl : CameraState.NpcFollowing;
            });
        }

        // ── Input handlers ─────────────────────────────────────────────────────

        /// <summary>Returns true if the player is giving any camera movement input this frame.</summary>
        private bool HasPlayerInput()
        {
            Keyboard kb = Keyboard.current;
            Mouse mouse = Mouse.current;

            if (kb != null &&
                (kb.upArrowKey.isPressed || kb.downArrowKey.isPressed ||
                 kb.leftArrowKey.isPressed || kb.rightArrowKey.isPressed))
                return true;

            if (mouse != null)
            {
                if (mouse.middleButton.isPressed) return true;

                if (edgeScrollEnabled)
                {
                    Vector2 pos = mouse.position.ReadValue();
                    if (pos.x >= 0 && pos.x <= Screen.width && pos.y >= 0 && pos.y <= Screen.height)
                    {
                        if (pos.x < edgeScrollMargin || pos.x > Screen.width  - edgeScrollMargin ||
                            pos.y < edgeScrollMargin || pos.y > Screen.height - edgeScrollMargin)
                            return true;
                    }
                }
            }

            return false;
        }

        private void HandleKeyboardPan()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            Vector2 input = Vector2.zero;
            if (kb.upArrowKey.isPressed)    input.y += 1f;
            if (kb.downArrowKey.isPressed)  input.y -= 1f;
            if (kb.leftArrowKey.isPressed)  input.x -= 1f;
            if (kb.rightArrowKey.isPressed) input.x += 1f;
            if (input == Vector2.zero) return;

            ApplyPanDelta(input.normalized, arrowPanSpeed);
        }

        private void HandleEdgeScroll()
        {
            if (!edgeScrollEnabled) return;
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 pos = mouse.position.ReadValue();

            // Mouse outside window — stop scrolling
            if (pos.x < 0 || pos.x > Screen.width || pos.y < 0 || pos.y > Screen.height) return;

            Vector2 dir = Vector2.zero;

            if (pos.x < edgeScrollMargin)                  dir.x -= 1f;
            if (pos.x > Screen.width  - edgeScrollMargin)  dir.x += 1f;
            if (pos.y < edgeScrollMargin)                  dir.y -= 1f;
            if (pos.y > Screen.height - edgeScrollMargin)  dir.y += 1f;
            if (dir == Vector2.zero) return;

            ApplyPanDelta(dir.normalized, edgeScrollSpeed);
        }

        private void HandleDragPan()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.middleButton.wasPressedThisFrame)
            {
                isDragging = true;
                dragPlaneY = focusPoint.y;
                dragLastWorldPos = MouseOnPlane(dragPlaneY);
            }

            if (mouse.middleButton.wasReleasedThisFrame)
                isDragging = false;

            if (!isDragging) return;

            Vector3 current = MouseOnPlane(dragPlaneY);
            Vector3 delta = (dragLastWorldPos - current) * dragSensitivity; // natural grab feel
            dragLastWorldPos = current;
            targetFocusPoint = ClampFocusPoint(targetFocusPoint + delta);
        }

        private void HandleScrollZoom()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Approximately(scroll, 0f)) return;

            cameraDistance = Mathf.Clamp(
                cameraDistance - scroll * scrollZoomSpeed * Time.deltaTime,
                minCameraDistance,
                maxCameraDistance);
        }

        private void HandleSpacebarSnap()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null || !kb.spaceKey.wasPressedThisFrame) return;
            if (currentEntity == null) return;
            StartAutoPan(currentEntity.transform.position);
        }

        // ── Smooth apply ───────────────────────────────────────────────────────

        private void SmoothApply()
        {
            // During AutoPanning, DOTween drives focusPoint directly — skip SmoothDamp.
            if (state != CameraState.AutoPanning)
            {
                focusPoint = Vector3.SmoothDamp(
                    focusPoint, targetFocusPoint, ref panVelocity, panSmoothTime);
            }
            ApplyCameraTransform();
        }

        private void ApplyCameraTransform()
        {
            transform.rotation = Quaternion.Euler(pitchAngle, yaw, 0f);
            transform.position = focusPoint - transform.forward * cameraDistance;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void ApplyPanDelta(Vector2 dir, float speed)
        {
            Vector3 forward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            Vector3 right   = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;
            Vector3 move    = (forward * dir.y + right * dir.x) * speed * Time.deltaTime;
            targetFocusPoint = ClampFocusPoint(targetFocusPoint + move);
        }

        /// <summary>
        /// Clamps a candidate focus point to the nearest valid NavMesh position.
        /// Large radius ensures any shape map works — camera slides along real borders.
        /// Returns current focusPoint as fallback if NavMesh is missing entirely.
        /// </summary>
        private Vector3 ClampFocusPoint(Vector3 candidate)
        {
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshClampRadius, NavMesh.AllAreas))
                return hit.position;
            return focusPoint;
        }

        /// <summary>Projects the mouse ray onto a horizontal plane at height y.</summary>
        private Vector3 MouseOnPlane(float y)
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return focusPoint;

            Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
            Plane plane = new Plane(Vector3.up, new Vector3(0f, y, 0f));
            if (plane.Raycast(ray, out float dist))
                return ray.GetPoint(dist);

            return focusPoint;
        }
    }
}
