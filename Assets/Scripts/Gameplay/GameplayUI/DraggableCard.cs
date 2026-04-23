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
        /// <summary>Set while a card drag/targeting is in progress. Used by CardPreviewPanel to cancel.</summary>
        public static DraggableCard ActiveDraggingCard { get; private set; }

        public CardScript cardScript;
        private static readonly Vector3 InvalidPosition = new Vector3(9999, 9999, 9999);

        private readonly List<Vector3> selectedPositionsDuringDrag = new();
        private bool isDragging = false;
        private bool wasDragged = false;
        private bool isCancelled = false;
        private bool wasSelectedBeforeDrag = false;
        private GameObject dragVFX = null;
        private VisualEffect dragVFXEffect = null;

        public override void OnBeginDrag(PointerEventData eventData)
        {
            wasDragged = true;

            cardScript = GetComponent<CardScript>();

            // Check if the player has enough stamina to play this card
            if (cardScript?.cardData?.Owner != null && cardScript.cardData.Owner.entityStats.CurrentStamina < cardScript.cardData.Cost)
            {
                Debug.LogWarning($"Insufficient stamina to play {cardScript.cardData.cardName}. Required: {cardScript.cardData.Cost}, Available: {cardScript.cardData.Owner.entityStats.CurrentStamina}");
                wasDragged = false;
                return;
            }

            base.OnBeginDrag(eventData);

            // Suppress base LineRenderer — DraggableCard uses VFX for targeting
            if (lineRenderer != null)
                lineRenderer.enabled = false;

            isDragging  = true;
            isCancelled = false;
            selectedPositionsDuringDrag.Clear();

            // Highlight the card in-hand during targeting (slight upward offset via selectedOffsetY)
            wasSelectedBeforeDrag = HandManager.Instance?.GetSelectedCard() == gameObject;
            if (!wasSelectedBeforeDrag)
                HandManager.Instance?.SelectCard(gameObject);

            ActiveDraggingCard = this;

            CreateDragVFX();
        }

        public override void OnDrag(PointerEventData eventData)
        {
            // Do NOT call base.OnDrag — card stays in the hand, only VFX follows cursor
            if (!isDragging) return;

            // Escape cancels targeting
            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                CancelDrag();
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(eventData.position);
            Vector3 cursorPosition = GetFloorHitPosition(ray);

            if (dragVFX != null && cursorPosition != Vector3.zero && cursorPosition != InvalidPosition)
            {
                dragVFXEffect.SetVector3("End", cursorPosition);
                dragVFX.transform.position = cursorPosition;
            }

            // Visualize targeting effect preview during drag
            if (cursorPosition != Vector3.zero && cursorPosition != InvalidPosition && cardScript?.cardData != null)
            {
                List<Vector3> previewPositions = selectedPositionsDuringDrag.Count > 0 ? selectedPositionsDuringDrag : new List<Vector3> { cursorPosition };
                TargetingModeData previewData = TargetingUtility.GetAffected(cardScript, cursorPosition, cardScript.cardData.Owner, cardScript.cardData.targetingData.EffectUsesVision, previewPositions, false);
            }

            bool isMultiSelect = IsMultiSelectionTargetingMode(cardScript.cardData.targetingData.cardTargetingMode);

            if (Mouse.current?.rightButton.wasPressedThisFrame == true)
            {
                if (isMultiSelect)
                {
                    // Multi-select: right-click adds / removes a target position
                    int maxTargets = cardScript.cardData.targetingData.cardTargetingMode == CardTargetingMode.Select
                        ? cardScript.cardData.MaxTarget : 2;

                    if (cursorPosition != Vector3.zero && cursorPosition != InvalidPosition)
                    {
                        if (HasValidTargetsAtPosition(cursorPosition, cardScript.cardData.targetingData.cardTargetingMode))
                        {
                            if (selectedPositionsDuringDrag.Contains(cursorPosition))
                                selectedPositionsDuringDrag.Remove(cursorPosition);
                            else if (selectedPositionsDuringDrag.Count < maxTargets)
                                selectedPositionsDuringDrag.Add(cursorPosition);
                        }
                    }
                }
                else
                {
                    // All other modes: right-click cancels targeting
                    CancelDrag();
                }
            }
        }

        /// <summary>
        /// Cancels targeting and returns the card to its highlighted-but-idle state in the hand.
        /// Triggered by Escape, right-click (non-multi-select), or clicking the CardPreviewPanel.
        /// </summary>
        public void CancelDrag()
        {
            if (!isDragging) return;

            isCancelled        = true;
            isDragging         = false;
            ActiveDraggingCard = null;

            if (dragVFX != null) { Destroy(dragVFX); dragVFX = null; }
            if (lineRenderer != null) lineRenderer.enabled = false;

            canvasGroup.blocksRaycasts = true;

            // Deselect only if we highlighted the card ourselves; otherwise leave it selected
            if (!wasSelectedBeforeDrag)
                HandManager.Instance?.SelectCard(null);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            isDragging         = false;
            ActiveDraggingCard = null;

            if (dragVFX != null) { Destroy(dragVFX); dragVFX = null; }

            // Cancelled — cleanup was done in CancelDrag(), just reset the flag
            if (isCancelled)
            {
                isCancelled                = false;
                canvasGroup.blocksRaycasts = true;
                return;
            }

            base.OnEndDrag(eventData); // restores blocksRaycasts + anchoredPosition

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
            // Check if target is within range of the caster
            if (cardScript?.cardData?.Owner != null)
            {
                float distanceToCaster = Vector3.Distance(cardScript.cardData.Owner.transform.position, position);
                if (distanceToCaster > cardScript.cardData.Range)
                {
                    return false;
                }
            }

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
