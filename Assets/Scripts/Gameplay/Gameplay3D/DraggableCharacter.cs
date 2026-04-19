using TMPro;
using UnityEngine;

namespace facingfate
{
    public class DraggableCharacter : Draggable3D
    {
        public EntityOnMap characterOnMap;
        public EntityScript characterEntity;

        [Header("Movement Cost Preview")]
        [SerializeField] private int previewPathCost;
        public TextMeshProUGUI staminaPreviewText;

        [Header("Entity Spacing")]
        [Tooltip("Minimum allowed distance to any other entity at the drop destination.")]
        [SerializeField] private float minEntityDistance = 1.2f;

        private PathData _lastPathData;
        private bool _hasDragTarget;
        private bool _destinationBlocked; // true = too close to another entity

        private void Awake()
        {
            characterEntity = GetComponent<EntityScript>();
            characterOnMap = GetComponent<EntityOnMap>();
        }

        public override void OnMouseDown()
        {
            if (TimelineManager.isPaused) return;
            if (characterEntity.entityStats.IsRooted) return;

            // FIX: Guard against TurnManager having an empty / uninitialised TurnOrder.
            EntityScript currentTurn = TurnManager.Instance?.CurrentTurnEntity;
            if (currentTurn == null || currentTurn != characterEntity) return;

            entityStats = characterEntity.entityStats;
            base.OnMouseDown();
        }

        public override void OnMouseUp()
        {
            if (!isDragging) return;

            // Only move if we have a valid, unblocked destination.
            if (_hasDragTarget && !_destinationBlocked)
                MoveToPosition(_lastPathData);

            _hasDragTarget = false;
            _destinationBlocked = false;
            _lastPathData = null;

            base.OnMouseUp();
        }

        protected override void OnDragUpdate()
        {
            if (!InputManager.Instance.TryRaycastFromMouse(out RaycastHit hit)) return;

            Vector3 cursorPosition = hit.point;

            // ── Path calculation ──────────────────────────────────────────
            _lastPathData = MovementUtility.FindPath(currentPosition, cursorPosition, characterEntity.entityStats, false, true);
            previewPathCost = _lastPathData.PathCost;
            currentPathCost = _lastPathData.PathCost;
            _hasDragTarget = true;

            // ── Minimum-distance check ────────────────────────────────────
            // Use the actual end of the found path (may differ from cursor
            // if walkClose stopped the path early near an obstacle).
            Vector3 pathEnd = (_lastPathData.Path != null && _lastPathData.Path.Count > 0)
                ? _lastPathData.Path[_lastPathData.Path.Count - 1]
                : cursorPosition;

            _destinationBlocked = IsTooCloseToEntity(pathEnd);

            // ── Arrow preview ─────────────────────────────────────────────
            if (lineRenderer != null)
                UpdateArrow(currentPosition, cursorPosition);

            // ── Stamina text ──────────────────────────────────────────────
            if (staminaPreviewText != null)
            {
                float stamina = characterEntity.entityStats.CurrentStamina;

                if (_destinationBlocked)
                {
                    staminaPreviewText.text = "Too close!";
                    staminaPreviewText.color = Color.red;
                }
                else if (stamina >= previewPathCost)
                {
                    staminaPreviewText.text = $"{stamina:F0} → {stamina - previewPathCost:F0}";
                    staminaPreviewText.color = Color.white;
                }
                else
                {
                    staminaPreviewText.text = $"{stamina:F0} (Insufficient)";
                    staminaPreviewText.color = Color.red;
                }
            }
        }

        // ── Colour helpers ────────────────────────────────────────────────

        protected override void UpdateLineRendererColor(Vector3 start, Vector3 end)
        {
            ApplyLineRendererColor();
        }

        protected override void ApplyLineRendererColor()
        {
            if (characterEntity == null || lineRenderer == null || lineRenderer.material == null)
            {
                base.ApplyLineRendererColor();
                return;
            }

            if (_destinationBlocked)
            {
                lineRenderer.material.color = unaffordablePathColor; // zeigt rot an
                return;
            }

            float stamina = characterEntity.entityStats.CurrentStamina;
            Color lineColor = stamina >= currentPathCost ? affordablePathColor : unaffordablePathColor;
            lineRenderer.material.color = lineColor;
        }

        // ── Movement execution ────────────────────────────────────────────

        private void MoveToPosition(PathData pathData)
        {
            if (pathData?.Path == null || pathData.Path.Count == 0) return;

            float stamina = characterEntity.entityStats.CurrentStamina;
            if (stamina < pathData.PathCost) return;

            characterEntity.entityStats.CurrentStamina -= pathData.PathCost;

            if (characterOnMap != null)
                StartCoroutine(characterOnMap.StartMoveRoutineWithPath(pathData));
        }

        // ── Proximity helper ──────────────────────────────────────────────

        /// <summary>Returns true if <paramref name="position"/> is within
        /// <see cref="minEntityDistance"/> of any entity other than this one.</summary>
        private bool IsTooCloseToEntity(Vector3 position)
        {
            EntityOnMap[] all = FindObjectsByType<EntityOnMap>(FindObjectsSortMode.None);
            foreach (EntityOnMap other in all)
            {
                if (other == characterOnMap) continue;
                if (Vector3.Distance(position, other.transform.position) < minEntityDistance)
                    return true;
            }
            return false;
        }
    }
}