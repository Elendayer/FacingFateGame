using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace facingfate
{

    [RequireComponent(typeof(Collider))]
    public class DraggableCharacter : Draggable3D
    {
        public EntityOnMap characterOnMap;
        public EntityScript characterEntity;

        [Header("Movement Cost Preview")]
        public int previewPathCost;
        public TextMeshProUGUI staminaPreviewText;

        private Vector3 dragEndPosition;

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
            if (dragEndPosition != Vector3.zero && dragEndPosition != currentPosition)
            {
                MoveToPosition(dragEndPosition);
            }

            dragEndPosition = Vector3.zero;

            // Now end the drag
            base.OnMouseUp();
        }

        protected override void OnDragUpdate()
        {
            // Raycast from camera to world
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 cursorPosition = hit.point;
                dragEndPosition = cursorPosition;

                // Calculate path and cost for preview
                PathData pathData = MovementUtility.FindPath(currentPosition, cursorPosition, false, true, moveCostModifer);
                previewPathCost = pathData.PathCost;

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

        private void MoveToPosition(Vector3 targetPosition)
        {
            // Validate path exists and has sufficient stamina
            PathData pathData = MovementUtility.FindPath(currentPosition, targetPosition, false, true, moveCostModifer);

            Debug.Log($"[DraggableCharacter] MoveToPosition called. From: {currentPosition}, To: {targetPosition}");
            Debug.Log($"[DraggableCharacter] Path valid: {pathData.Path != null && pathData.Path.Count > 0}, Cost: {pathData.PathCost}");

            if (pathData.Path == null || pathData.Path.Count == 0)
            {
                Debug.Log($"[DraggableCharacter] No valid path found");
                return;
            }

            float currentStamina = characterEntity.entityStats.CurrentStamina;
            Debug.Log($"[DraggableCharacter] Current stamina: {currentStamina}, Path cost: {pathData.PathCost}");

            if (currentStamina < pathData.PathCost)
            {
                Debug.Log($"[DraggableCharacter] Insufficient stamina");
                return;
            }

            // Deduct stamina cost first
            characterEntity.entityStats.CurrentStamina -= pathData.PathCost;
            Debug.Log($"[DraggableCharacter] Stamina deducted. New stamina: {characterEntity.entityStats.CurrentStamina}");

            // Execute movement via wrapper coroutine
            Debug.Log($"[DraggableCharacter] Starting move routine to: {targetPosition}");
            if (characterOnMap != null)
            {
                StartCoroutine(ExecuteMovement(targetPosition));
            }
        }

        private IEnumerator ExecuteMovement(Vector3 targetPosition)
        {
            Debug.Log($"[DraggableCharacter] ExecuteMovement coroutine started");
            yield return characterOnMap.StartMoveRoutine(targetPosition);
            Debug.Log($"[DraggableCharacter] Movement completed");
        }
    }
}