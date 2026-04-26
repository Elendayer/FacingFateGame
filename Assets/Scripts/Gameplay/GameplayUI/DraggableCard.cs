using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.VFX;
using UnityEngine.InputSystem;
using Unity.Mathematics;

namespace facingfate
{
    public class DraggableCard : DraggableUI, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>Set while a card drag/targeting is in progress. Used by CardPreviewPanel to cancel.</summary>
        public static DraggableCard ActiveDraggingCard { get; private set; }

        public CardScript cardScript;
        private static readonly Vector3 InvalidPosition = new Vector3(9999, 9999, 9999);

        [Header("Wwise UI SFX")]
        [Tooltip("Optional, empty = silent")] [SerializeField] private AK.Wwise.Event hoverSfx;

        // Shared across all card instances — prevents rapid-fire when hovering over multiple cards
        private static float _lastHoverSfxTime = -1f;
        private const float HoverSfxCooldown = 0.1f;

        /// <summary>Stores selected targets during drag — either positions (Ground type) or entities (Entity type)</summary>
        private struct TargetSelection
        {
            public Vector3 position;
            public EntityScript entity;

            public TargetSelection(Vector3 pos) { position = pos; entity = null; }
            public TargetSelection(EntityScript ent) { entity = ent; position = ent != null ? ent.transform.position : Vector3.zero; }

            public bool IsEntity => entity != null;
            public Vector3 GetPosition() => entity != null ? entity.transform.position : position;
        }

        private readonly List<TargetSelection> selectedTargetsDuringDrag = new();
        private bool isDragging = false;
        private bool wasDragged = false;
        private bool isCancelled = false;
        private bool wasSelectedBeforeDrag = false;
        private GameObject dragVFX = null;
        private VisualEffect dragVFXEffect = null;
        private bool wasRightMouseDown = false;
        private List<EntityScript> lastHighlightedEntities = new();
        public void OnPointerClick(PointerEventData eventData)
        {
            if (wasDragged) { wasDragged = false; return; }
            if (eventData.button != PointerEventData.InputButton.Left) return;
            HandManager.Instance?.SelectCard(gameObject);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Time.unscaledTime - _lastHoverSfxTime >= HoverSfxCooldown)
            {
                WwiseAudioHelper.PlayGlobal(hoverSfx, gameObject);
                _lastHoverSfxTime = Time.unscaledTime;
            }

            // If dragging, don't highlight on hover since we're already showing targeted entities
            if (isDragging) return;

            HandManager.Instance?.HoverCard(gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Clear any hover highlights
            foreach (var entity in lastHighlightedEntities)
            {
                if (entity?.EntityVisual != null)
                {
                    entity.EntityVisual.ClearHighlight();
                }
            }
            lastHighlightedEntities.Clear();
            HandManager.Instance?.HoverCard(null);
        }

        private void Update()
        {
            // Check for right-click during drag, independent of mouse movement
            if (!isDragging) return;

            bool isRightMouseDown = Mouse.current?.rightButton.isPressed == true;
            bool rightClickPressed = isRightMouseDown && !wasRightMouseDown;
            wasRightMouseDown = isRightMouseDown;

            if (rightClickPressed)
            {
                // Get current mouse position from Input System
                Vector2 mousePos = Mouse.current?.position.ReadValue() ?? Vector2.zero;
                PointerEventData eventData = new PointerEventData(EventSystem.current)
                {
                    position = mousePos
                };
                HandleRightClickDuringDrag(eventData);
            }
        }

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
            selectedTargetsDuringDrag.Clear();
            wasRightMouseDown = false;

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
            
            dragVFX.transform.position = cursorPosition;
            if (dragVFXEffect.HasVector3("End"))
            {
                dragVFXEffect.SetVector3("End", cursorPosition);
            }

            CardData cardData = cardScript.cardData;

            // Visualize targeting effect preview during drag
            if (cursorPosition != Vector3.zero && cursorPosition != InvalidPosition && cardData != null)
            {
                List<Vector3> previewPositions = selectedTargetsDuringDrag.Count > 0 
                    ? selectedTargetsDuringDrag.Select(t => t.GetPosition()).ToList() 
                    : new List<Vector3> { cursorPosition };

                TargetingModeData previewData = TargetingUtility.GetAffected(cardScript, cursorPosition, cardData.Owner, cardData.targetingData.EffectUsesVision, previewPositions, false);

                // Update entity highlights based on affected entities
                UpdateEntityHighlights(previewData);
            }
        }

        private void UpdateEntityHighlights(TargetingModeData targetingData)
        {
            // Clear previous highlights
            foreach (var entity in lastHighlightedEntities)
            {
                if (entity?.EntityVisual != null)
                {
                    entity.EntityVisual.ClearHighlight();
                }
            }
            lastHighlightedEntities.Clear();

            // Highlight new affected entities
            if (targetingData?.targetedEntities != null && cardScript?.cardData != null)
            {
                // Determine if this card uses selection-based targeting
                bool isSelectionMode = IsSelectionTargetingMode(cardScript.cardData.targetingData.cardTargetingMode);

                foreach (var entity in targetingData.targetedEntities)
                {
                    if (entity?.EntityVisual != null)
                    {
                        // Use HighlightSelection for selection-based targeting modes
                        if (isSelectionMode)
                        {
                            entity.EntityVisual.HighlightSelection();
                        }
                        else
                        {
                            // Use HighlightInAreaOfEffect for area-based targeting modes
                            entity.EntityVisual.HighlightInAreaOfEffect();
                        }
                        lastHighlightedEntities.Add(entity);
                    }
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
            wasRightMouseDown  = false;

            if (dragVFX != null) { Destroy(dragVFX); dragVFX = null; }
            if (lineRenderer != null) lineRenderer.enabled = false;

            // Clear entity highlights
            foreach (var entity in lastHighlightedEntities)
            {
                if (entity?.EntityVisual != null)
                {
                    entity.EntityVisual.ClearHighlight();
                }
            }
            lastHighlightedEntities.Clear();

            canvasGroup.blocksRaycasts = true;

            // Deselect only if we highlighted the card ourselves; otherwise leave it selected
            if (!wasSelectedBeforeDrag)
                HandManager.Instance?.SelectCard(null);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            isDragging         = false;
            ActiveDraggingCard = null;
            wasRightMouseDown  = false;

            if (dragVFX != null) { Destroy(dragVFX); dragVFX = null; }

            // Clear entity highlights
            foreach (var entity in lastHighlightedEntities)
            {
                if (entity?.EntityVisual != null)
                {
                    entity.EntityVisual.ClearHighlight();
                }
            }
            lastHighlightedEntities.Clear();

            // Cancelled — cleanup was done in CancelDrag(), just reset the flag
            if (isCancelled)
            {
                isCancelled                = false;
                canvasGroup.blocksRaycasts = true;
                return;
            }

            base.OnEndDrag(eventData); // restores blocksRaycasts + anchoredPosition

            // Determine if this card uses multi-selection or single-cast targeting
            bool isMultiSelectMode = IsSelectionTargetingMode(cardScript.cardData.targetingData.cardTargetingMode);

            Vector3 aimWorldPos;
            List<Vector3> targetPositions;

            if (isMultiSelectMode)
            {
                // Multi-selection mode: require explicit right-click selections
                if (selectedTargetsDuringDrag.Count == 0)
                {
                    HandManager.Instance?.SelectCard(null);
                    return;
                }

                // Validate that at least one target has valid targets
                List<Vector3> selectedPositions = selectedTargetsDuringDrag.Select(t => t.GetPosition()).ToList();
                if (!HasValidTargetsAtPositions(selectedPositions))
                {
                    HandManager.Instance?.SelectCard(null);
                    return;
                }

                aimWorldPos = selectedTargetsDuringDrag[selectedTargetsDuringDrag.Count - 1].GetPosition();
                targetPositions = selectedPositions;
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

        private bool IsSelectionTargetingMode(CardTargetingMode mode)
        {
            return mode == CardTargetingMode.Select || mode == CardTargetingMode.SelectionUnique || mode == CardTargetingMode.LineFree;
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





        private void CreateDragVFX()
        {
            if (cardScript == null) return;

            string vfxName = GetVFXNameForTargetingMode(); 

            VFXData vfxData = new VFXData(vfxName)
            {
                vfxName = vfxName,
                attachToMesh = false,
                activationCount = 1,
                range = cardScript.cardData.Range,
                area = cardScript.cardData.Area,
                radius = cardScript.cardData.Radius,
                start = cardScript.cardData.Owner.transform.position,
                end = Vector3.zero
            };

            if (string.IsNullOrEmpty(vfxName)) return;

            (GameObject obj, VisualEffect effect) vfx = AssetManager.Instance.CreateVFX(vfxName, vfxData);
            if (vfx.obj == null) return;
   
            dragVFX = vfx.obj;
            dragVFXEffect = vfx.effect;
        }

        private string GetVFXNameForTargetingMode()
        {
            return cardScript.cardData.targetingData.cardTargetingMode switch
            {
                CardTargetingMode.Select => "vfx_targeting_single",
                CardTargetingMode.Radius => "vfx_targeting_sphere",
                CardTargetingMode.Ring => "vfx_targeting_ring",
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

        private bool TryGetEntityHitByRay(Ray ray, out EntityScript entity)
        {
            Debug.Log("Performing entity raycast for targeting...");
            entity = null;
            int entityLayer = LayerMask.GetMask("Entity");

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, entityLayer))
            {
                Debug.DrawLine(ray.origin, hit.point, Color.red, 0.5f);
                Debug.Log($"Entity raycast hit: {hit.collider.name} at {hit.point}");

                EntityScript hitEntity = hit.collider.GetComponent<EntityScript>();
                if (hitEntity != null)
                {
                    entity = hitEntity;
                    return true;
                }
            }
            else
            {
                Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.yellow, 0.5f);
                Debug.Log("Entity raycast: No hit detected");
            }

            return false;
        }

        private void HandleRightClickDuringDrag(PointerEventData eventData)
        {
            Debug.Log($"HandleRightClickDuringDrag: Right-click at {eventData.position} while dragging");
            if (!isDragging || cardScript?.cardData == null) return;

            Ray ray = Camera.main.ScreenPointToRay(eventData.position);
            Vector3 cursorPosition = GetFloorHitPosition(ray);

            bool isSelect = IsSelectionTargetingMode(cardScript.cardData.targetingData.cardTargetingMode);
            bool isSelectionUnique = cardScript.cardData.targetingData.cardTargetingMode == CardTargetingMode.SelectionUnique;

            if (isSelect)
            {
                // Multi-select: right-click adds / removes a target
                int maxTargets = cardScript.cardData.targetingData.cardTargetingMode == CardTargetingMode.Select ? cardScript.cardData.MaxTarget : (isSelectionUnique ? cardScript.cardData.MaxTarget : 2);

                if (cursorPosition != Vector3.zero && cursorPosition != InvalidPosition)
                {
                    CardTargetType targetType = cardScript.cardData.targetingData.CardTargetType;

                    if (targetType == CardTargetType.Ground)
                    {
                        // Ground targeting: check if ray hit the floor
                        if (HasValidTargetsAtPosition(cursorPosition, cardScript.cardData.targetingData.cardTargetingMode))
                        {
                            TargetSelection groundTarget = new TargetSelection(cursorPosition);

                            // Check if this position is already selected
                            bool isDuplicate = selectedTargetsDuringDrag.Any(t => !t.IsEntity && Vector3.Distance(t.position, cursorPosition) < 0.1f);

                            if (isDuplicate)
                            {
                                if (isSelectionUnique)
                                {
                                    // SelectionUnique: deselect the duplicate position
                                    selectedTargetsDuringDrag.RemoveAll(t => !t.IsEntity && Vector3.Distance(t.position, cursorPosition) < 0.1f);
                                    Debug.Log($"SelectionUnique: Deselected ground position at {cursorPosition}");
                                }
                                else
                                {
                                    // Select mode: deselect the position (toggle off)
                                    selectedTargetsDuringDrag.RemoveAll(t => !t.IsEntity && Vector3.Distance(t.position, cursorPosition) < 0.1f);
                                    Debug.Log($"Select: Deselected ground target at {cursorPosition}");
                                }
                            }
                            else
                            {
                                // Position is not selected yet
                                if (selectedTargetsDuringDrag.Count >= maxTargets)
                                {
                                    // At max capacity: remove first (oldest) selection
                                    selectedTargetsDuringDrag.RemoveAt(0);
                                    Debug.Log($"Max targets reached: removed first selected target");
                                }

                                selectedTargetsDuringDrag.Add(groundTarget);
                                Debug.Log($"Selected ground target at {cursorPosition}");
                            }
                        }
                    }
                    else if (targetType == CardTargetType.Entity)
                    {
                        // Entity targeting: check if ray hit an entity
                        if (TryGetEntityHitByRay(ray, out EntityScript hitEntity) && hitEntity != null)
                        {
                            // Validate the entity before allowing selection using TargetingUtility
                            if (!TargetingUtility.IsDraggableTargetValid(cardScript.cardData, hitEntity, cardScript.cardData.Owner))
                            {
                                Debug.Log($"Entity {hitEntity.name} is not a valid target");
                                return;
                            }

                            // Verify the entity is a valid target according to targeting rules
                            TargetingModeData targetingData = TargetingUtility.GetAffected(
                                cardScript,
                                hitEntity.transform.position,
                                cardScript.cardData.Owner,
                                cardScript.cardData.targetingData.EffectUsesVision,
                                new List<Vector3> { hitEntity.transform.position },
                                true
                            );

                            if (targetingData?.targetedEntities != null && targetingData.targetedEntities.Contains(hitEntity))
                            {
                                TargetSelection entityTarget = new TargetSelection(hitEntity);

                                // Check if this exact entity is already selected
                                int existingIndex = selectedTargetsDuringDrag.FindIndex(t => t.IsEntity && t.entity == hitEntity);

                                if (existingIndex >= 0)
                                {
                                    // Entity is already selected
                                    if (isSelectionUnique)
                                    {
                                        // SelectionUnique: deselect the entity
                                        selectedTargetsDuringDrag.RemoveAt(existingIndex);
                                        Debug.Log($"SelectionUnique: Deselected entity {hitEntity.name}");
                                    }
                                    else
                                    {
                                        // Select mode: deselect the entity (toggle off)
                                        selectedTargetsDuringDrag.RemoveAt(existingIndex);
                                        Debug.Log($"Select: Deselected entity target: {hitEntity.name}");
                                    }
                                }
                                else
                                {
                                    // Entity is not selected yet
                                    if (selectedTargetsDuringDrag.Count >= maxTargets)
                                    {
                                        // At max capacity: remove first (oldest) selection
                                        selectedTargetsDuringDrag.RemoveAt(0);
                                        Debug.Log($"Max targets reached: removed first selected target");
                                    }

                                    // Add new selection
                                    selectedTargetsDuringDrag.Add(entityTarget);
                                    Debug.Log($"Selected entity target: {hitEntity.name}");
                                }
                            }
                            else
                            {
                                Debug.Log($"Entity {hitEntity.name} is not in the targeting area or is not a valid target");
                            }
                        }
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
}
