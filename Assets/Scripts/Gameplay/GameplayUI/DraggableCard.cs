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

        private void Update()
        {
            if (!isDragging) return;

            // Track cursor position and update highlighted position
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            Vector3 cursorPosition = TargetingUtility.GetHoveredNavMesh(ray);

            // Update VFX position to follow cursor only if position is valid
            if (dragVFX != null && cursorPosition != Vector3.zero && cursorPosition != InvalidPosition)
            {
                if (HasValidTargetsAtPosition(cursorPosition, cardScript.cardData.targetingData.cardTargetingMode))
                {
                    dragVFX.transform.position = cursorPosition;
                }
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

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
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

            // Only play card if valid targets were selected during drag
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

            Vector3 aimWorldPos = selectedPositionsDuringDrag[selectedPositionsDuringDrag.Count - 1];
            TargetingModeData targetingModeData = TargetingUtility.GetAffected(cardScript, aimWorldPos, cardScript.cardData.Owner, cardScript.cardData.targetingData.EffectUsesVision, selectedPositionsDuringDrag, true);

            cardScript.cardData.ActivateCardEffect(targetingModeData, gameObject);
            HandManager.Instance?.SelectCard(null);
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
            // Get affected entities at this position based on targeting mode
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

            // For All targeting mode, any navmesh position is valid
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

            dragVFX = vfx.obj;
        }

        private string GetVFXNameForTargetingMode()
        {
            return cardScript.cardData.targetingData.cardTargetingMode switch
            {
                CardTargetingMode.Select => "vfx_targeting_sphere",
                CardTargetingMode.Radius => "vfx_targeting_sphere",
                CardTargetingMode.Single => "vfx_targeting_single",
                CardTargetingMode.LineFree => "vfx_targeting_line",
                _ => "vfx_targeting_default"
            };
        }
    }
}
