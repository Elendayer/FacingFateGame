using System.Collections;
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
        public TextMeshPro staminaPreviewText;

        [Header("Entity Spacing")]
        [Tooltip("Minimum allowed distance to any other entity at the drop destination.")]
        [SerializeField] private float minEntityDistance = 0.7f;

        private NavMeshPathData _lastPathData;
        private bool _hasDragTarget;
        private bool _destinationBlocked; // true = too close to another entity
        private bool _isObstacleDisabledForTurn;

        private void Awake()
        {
            characterEntity = GetComponent<EntityScript>();
            characterOnMap = GetComponent<EntityOnMap>();
        }

        private void OnEnable()
        {
            GameEvents.OnTurnStart += OnTurnStart;
            GameEvents.OnTurnEnd += OnTurnEnd;
        }

        private void OnDisable()
        {
            GameEvents.OnTurnStart -= OnTurnStart;
            GameEvents.OnTurnEnd -= OnTurnEnd;
        }

        private void OnTurnStart()
        {
            // Check if this is the player's turn
            if (TurnManager.Instance?.CurrentTurnEntity != characterEntity) return;

            _isObstacleDisabledForTurn = true;
            if (characterOnMap != null)
                characterOnMap.DisableObstacleForDrag();
        }

        private void OnTurnEnd()
        {
            if (!_isObstacleDisabledForTurn) return;

            _isObstacleDisabledForTurn = false;
            if (characterOnMap != null)
                characterOnMap.EnableObstacleAfterDrag();
        }

        public override void OnMouseDown()
        {
            if (TimelineManager.isPaused) return;
            if (characterEntity.entityStats.IsRooted) return;

            // FIX: Guard against TurnManager having an empty / uninitialised TurnOrder.
            EntityScript currentTurn = TurnManager.Instance?.CurrentTurnEntity;
            if (currentTurn == null || currentTurn != characterEntity) return;

            // Tutorial: block movement if the current step has lockMovement enabled
            // (except when condition = MovedToTarget, which always needs movement).
            if (TutorialCombatManager.Instance != null && TutorialCombatManager.Instance.IsActive)
            {
                var tutStep = TutorialCombatManager.Instance.CurrentStep;
                bool isMovedToTarget = tutStep != null && tutStep.condition == CompletionCondition.MovedToTarget;
                bool shouldLock = tutStep == null || (tutStep.lockMovement && !isMovedToTarget);
                if (shouldLock) return;
            }

            entityStats = characterEntity.entityStats;

            // Ensure NavMeshObstacle is disabled while dragging
            if (characterOnMap != null)
                characterOnMap.DisableObstacleForDrag();

            base.OnMouseDown();
        }

        public override void OnMouseUp()
        {
            if (!isDragging) return;

            // Tutorial: restrict destination to movementTarget area only.
            if (_hasDragTarget && !_destinationBlocked
                && TutorialCombatManager.Instance != null && TutorialCombatManager.Instance.IsActive)
            {
                var tutStep = TutorialCombatManager.Instance.CurrentStep;
                if (tutStep != null
                    && tutStep.condition == CompletionCondition.MovedToTarget
                    && tutStep.movementTarget != null)
                {
                    float dist = Vector3.Distance(_lastPathData.End, tutStep.movementTarget.position);
                    if (dist > tutStep.movementThreshold)
                    {
                        _hasDragTarget = false; // destination too far from target — block move
                        Debug.Log("[DraggableCharacter] Tutorial: destination outside movementTarget area — blocked.");
                    }
                }
            }

            // Only move if we have a valid, unblocked destination.
            if (_hasDragTarget && !_destinationBlocked)
                MoveToPosition(_lastPathData);

            _hasDragTarget = false;
            _destinationBlocked = false;
            _lastPathData = null;

            staminaPreviewText.enabled = false;

            // Only re-enable NavMeshObstacle if the turn isn't controlling its state
            if (!_isObstacleDisabledForTurn && characterOnMap != null)
                characterOnMap.EnableObstacleAfterDrag();

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
            Vector3 pathEnd = (_lastPathData.CachedNavMeshPath != null && _lastPathData.CachedNavMeshPath.corners.Length > 0)
                ? _lastPathData.CachedNavMeshPath.corners[_lastPathData.CachedNavMeshPath.corners.Length - 1]
                : cursorPosition;

            _destinationBlocked = IsTooCloseToEntity(pathEnd);

            //Ensure preview is enabled while dragging
            staminaPreviewText.enabled = true;

            // ── Arrow preview ─────────────────────────────────────────────
            if (lineRenderer != null)
                UpdateArrow(currentPosition, cursorPosition);

            // ── Stamina text ──────────────────────────────────────────────
            if (staminaPreviewText != null)
            {
                Vector3 textpos = cursorPosition;
                textpos.y = 0.5f; // slightly above ground to avoid z-fighting

                staminaPreviewText.transform.position = textpos;

                if (_destinationBlocked)
                {
                    staminaPreviewText.text = "";
                }
                else
                {
                    staminaPreviewText.text = $"{previewPathCost}";
                }
            }
        }

        // ── Colour helpers ────────────────────────────────────────────────

        protected override void UpdateLineRendererColor(NavMeshPathData path)
        {
            base.UpdateLineRendererColor(path);
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

        private void MoveToPosition(NavMeshPathData pathData)
        {
            if (pathData == null || pathData.CachedNavMeshPath == null) return;

            float stamina = characterEntity.entityStats.CurrentStamina;
            if (stamina < pathData.PathCost) return;

            characterEntity.entityStats.CurrentStamina -= pathData.PathCost;

            if (characterOnMap != null)
                StartCoroutine(MoveWithStatsUpdate(pathData));
        }

        private IEnumerator MoveWithStatsUpdate(NavMeshPathData pathData)
        {
            yield return characterOnMap.StartMoveRoutineWithPath(pathData);

            // Update stats after movement completes
            characterEntity.entityStats.UpdateStats();
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