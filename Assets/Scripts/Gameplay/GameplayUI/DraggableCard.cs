using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.VFX;
using UnityEngine.InputSystem;

namespace facingfate
{
    public class DraggableCard : DraggableUI, IPointerClickHandler
    {
        public CardScript cardScript;
        private static readonly Vector3 InvalidPosition = new Vector3(9999, 9999, 9999);

        private readonly List<Vector3> selectedPositionsDuringDrag = new();
        private bool isDragging = false;
        private bool wasDragged = false;
        private GameObject dragVFX = null;
        private VisualEffect dragVFXEffect = null;

        public override void OnBeginDrag(PointerEventData eventData)
        {
            wasDragged = true;
            base.OnBeginDrag(eventData);
            cardScript = GetComponent<CardScript>();
            isDragging = true;
            selectedPositionsDuringDrag.Clear();

            // Create VFX based on targeting type
            CreateDragVFX();
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);

            if (!isDragging) return;

            // Track cursor position using floor collider raycast for stable targeting
            Ray ray = Camera.main.ScreenPointToRay(eventData.position);
            Vector3 cursorPosition = GetFloorHitPosition(ray);

            // Always update VFX position to follow cursor for smooth movement
            if (dragVFX != null && cursorPosition != Vector3.zero && cursorPosition != InvalidPosition)
            {
                dragVFXEffect.SetVector3("End", cursorPosition);
                dragVFX.transform.position = cursorPosition;
            }

            if (Mouse.current?.rightButton.wasPressedThisFrame == true)
            {
                int maxTargets = cardScript.cardData.targetingData.cardTargetingMode switch
                {
                    CardTargetingMode.Select => cardScript.cardData.MaxTarget,
                    CardTargetingMode.LineFree => 2,
                    _ => 1
                };

                // Only add position if it has valid targets
                if (cursorPosition != Vector3.zero && cursorPosition != InvalidPosition)
                {
                    if (HasValidTargetsAtPosition(cursorPosition, cardScript.cardData.targetingData.cardTargetingMode))
                    {
                        if (selectedPositionsDuringDrag.Contains(cursorPosition))
                        {
                            selectedPositionsDuringDrag.Remove(cursorPosition);
                        }
                        else if (selectedPositionsDuringDrag.Count < maxTargets)
                        {
                            selectedPositionsDuringDrag.Add(cursorPosition);
                        }
                    }
                }
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            base.OnEndDrag(eventData);

            // Clean up drag VFX
            if (dragVFX != null)
            {
                Destroy(dragVFX);
                dragVFX = null;
            }

            // Determine if this card uses multi-selection or single-cast targeting
            bool isMultiSelectMode = IsMultiSelectionTargetingMode(cardScript.cardData.targetingData.cardTargetingMode);

            Vector3 aimWorldPos;
            List<Vector3> targetPositions;

            if (isMultiSelectMode)
            {
                // Multi-selection mode: require explicit right-click selections
                if (selectedPositionsDuringDrag.Count == 0)
                {
                    HandManager.Instance?.SelectCard(null);
                    return;
                }

                // Validate that at least one target position has valid targets
                if (!HasValidTargetsAtPositions(selectedPositionsDuringDrag))
                {
                    HandManager.Instance?.SelectCard(null);
                    return;
                }

                aimWorldPos = selectedPositionsDuringDrag[selectedPositionsDuringDrag.Count - 1];
                targetPositions = selectedPositionsDuringDrag;
            }
            else
            {
                // Single-cast mode: use the drag end position
                Ray ray = Camera.main.ScreenPointToRay(eventData.position);
                aimWorldPos = GetFloorHitPosition(ray);

                if (aimWorldPos == InvalidPosition || aimWorldPos == Vector3.zero)
                {
                    HandManager.Instance?.SelectCard(null);
                    return;
                }

                // Validate that this position has valid targets
                if (!HasValidTargetsAtPosition(aimWorldPos, cardScript.cardData.targetingData.cardTargetingMode))
                {
                    HandManager.Instance?.SelectCard(null);
                    return;
                }

                targetPositions = new List<Vector3> { aimWorldPos };
            }

            TargetingModeData targetingModeData = TargetingUtility.GetAffected(cardScript, aimWorldPos, cardScript.cardData.Owner, cardScript.cardData.targetingData.EffectUsesVision, targetPositions, true);

            cardScript.cardData.ActivateCardEffect(targetingModeData, gameObject);
            HandManager.Instance?.SelectCard(null);
        }

        private bool IsMultiSelectionTargetingMode(CardTargetingMode mode)
        {
            return mode == CardTargetingMode.Select || mode == CardTargetingMode.LineFree;
        }

        private bool HasValidTargetsAtPositions(List<Vector3> positions)
        {
            if (positions == null || positions.Count == 0) return false;
            if (cardScript?.cardData == null) return false;

            CardTargetingMode targetingMode = cardScript.cardData.targetingData.cardTargetingMode;

            foreach (var position in positions)
            {
                if (HasValidTargetsAtPosition(position, targetingMode))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasValidTargetsAtPosition(Vector3 position, CardTargetingMode targetingMode)
        {
            // Ground-type cards can be cast at any valid position without requiring entities
            if (cardScript?.cardData?.targetingData?.CardTargetType == CardTargetType.Ground)
            {
                return position != Vector3.zero && position != InvalidPosition;
            }

            // Entity-type cards require at least one valid target entity at the position
            TargetingModeData targetingModeData = TargetingUtility.GetAffected(
                cardScript,
                position,
                cardScript.cardData.Owner,
                cardScript.cardData.targetingData.EffectUsesVision,
                new List<Vector3> { position },
                false
            );

            // Check if there are any valid targets at this position
            if (targetingModeData?.targetedEntities != null && targetingModeData.targetedEntities.Count > 0)
            {
                return true;
            }

            // For All targeting mode, any valid position is valid
            if (targetingMode == CardTargetingMode.All && position != Vector3.zero)
            {
                return true;
            }

            return false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (wasDragged) { wasDragged = false; return; }
            if (eventData.button != PointerEventData.InputButton.Left) return;
            HandManager.Instance?.SelectCard(gameObject);
        }

        private void CreateDragVFX()
        {
            if (cardScript == null) return;

            string vfxName = GetVFXNameForTargetingMode();
            if (string.IsNullOrEmpty(vfxName)) return;

            (GameObject obj, VisualEffect effect) vfx = AssetManager.Instance.CreateVFX(vfxName);
            if (vfx.obj == null) return;

            vfx.effect.SetFloat("Range", cardScript.cardData.Range);
            vfx.effect.SetFloat("Width", cardScript.cardData.Radius);
            vfx.effect.SetVector3("Start", cardScript.cardData.Owner.transform.position);
            vfx.effect.SetVector3("End", cardScript.cardData.Owner.transform.position);

            dragVFX = vfx.obj;
            dragVFXEffect = vfx.effect;
        }

        private string GetVFXNameForTargetingMode()
        {
            return cardScript.cardData.targetingData.cardTargetingMode switch
            {
                CardTargetingMode.Select => "vfx_targeting_sphere",
                CardTargetingMode.Radius => "vfx_targeting_sphere",
                CardTargetingMode.Single => "vfx_targeting_single",
                CardTargetingMode.LineFree => "vfx_targeting_line",
                CardTargetingMode.LineSelf => "vfx_targeting_line",
                _ => "vfx_targeting_default"
            };
        }

        private Vector3 GetFloorHitPosition(Ray ray)
        {
            int floorLayer = LayerMask.GetMask("Floor");
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, floorLayer))
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green);
                return hit.point;
            }
            return InvalidPosition;
        }
    }
}
