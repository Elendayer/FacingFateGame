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

        private PathData _lastPathData;
        private bool _hasDragTarget;

        private void Awake()
        {
            characterEntity = GetComponent<EntityScript>();
            characterOnMap = GetComponent<EntityOnMap>();
        }

        public override void OnMouseDown()
        {
            // Character-specific validation before starting drag
            if (TimelineManager.isPaused) return;
            if (characterEntity.entityStats.IsRooted) return;
            if (TurnManager.Instance.CurrentTurnEntity != characterEntity) return;

            // Cache the movement cost modifier for this drag session
            moveCostModifer = characterEntity.entityStats.MovementCostModifier;

            // Validation passed, start drag
            base.OnMouseDown();
        }

        public override void OnMouseUp()
        {
            if (!isDragging) return;

            // Execute movement BEFORE calling base.OnMouseUp() which disables drag
            if (_hasDragTarget)
            {
                MoveToPosition(_lastPathData);
            }

            _hasDragTarget = false;
            _lastPathData = null;

            // Now end the drag
            base.OnMouseUp();
        }

        protected override void OnDragUpdate()
        {
            // Raycast from camera to world
            if (InputManager.Instance.TryRaycastFromMouse(out RaycastHit hit))
            {
                Vector3 cursorPosition = hit.point;

                // Calculate path once and cache it to avoid redundant FindPath calls
                _lastPathData = MovementUtility.FindPath(currentPosition, cursorPosition, false, true, moveCostModifer);
                previewPathCost = _lastPathData.PathCost;
                currentPathCost = _lastPathData.PathCost;
                _hasDragTarget = true;

                // Update arrow preview
                if (lineRenderer != null)
                    UpdateArrow(currentPosition, cursorPosition);

                // Update stamina preview text
                if (staminaPreviewText != null)
                {
                    float currentStamina = characterEntity.entityStats.CurrentStamina;
                    if (currentStamina >= previewPathCost)
                    {
                        staminaPreviewText.text = $"{currentStamina:F0} → {currentStamina - previewPathCost:F0}";
                        staminaPreviewText.color = Color.white;
                    }
                    else
                    {
                        staminaPreviewText.text = $"{currentStamina:F0} (Insufficient)";
                        staminaPreviewText.color = Color.red;
                    }
                }
            }
        }

        protected override void UpdateLineRendererColor(Vector3 start, Vector3 end)
        {
            // currentPathCost is already set from the cached PathData; skip the redundant FindPath call
            ApplyLineRendererColor();
        }

        protected override void ApplyLineRendererColor()
        {
            // Override to check character stamina and set line color accordingly
            if (characterEntity != null && lineRenderer != null && lineRenderer.material != null)
            {
                float currentStamina = characterEntity.entityStats.CurrentStamina;
                Color lineColor = currentStamina >= currentPathCost ? affordablePathColor : unaffordablePathColor;
                lineRenderer.material.color = lineColor;
            }
            else
            {
                base.ApplyLineRendererColor();
            }
        }

        private void MoveToPosition(PathData pathData)
        {
            if (pathData?.Path == null || pathData.Path.Count == 0) return;

            float currentStamina = characterEntity.entityStats.CurrentStamina;
            if (currentStamina < pathData.PathCost) return;

            // Deduct stamina cost first
            characterEntity.entityStats.CurrentStamina -= pathData.PathCost;

            // Execute movement using the cached path
            if (characterOnMap != null)
            {
                StartCoroutine(characterOnMap.StartMoveRoutineWithPath(pathData));
            }
        }
    }
}