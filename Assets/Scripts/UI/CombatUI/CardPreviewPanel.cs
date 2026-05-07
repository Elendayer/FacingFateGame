using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.VFX;
using DG.Tweening;

namespace facingfate
{
    public class CardPreviewPanel : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public static CardPreviewPanel Instance { get; private set; }

        [SerializeField] private GameObject previewRoot;
        [SerializeField] private CardScript previewCardScript;

        [Header("Hover Animation")]
        [SerializeField] private float fadeDuration  = 0.15f;
        [SerializeField] private float scaleDuration = 0.15f;

        [Header("Toggle Button")]
        [SerializeField] private TMP_Text toggleButtonText;

        private CanvasGroup canvasGroup;
        private bool        isPreviewEnabled = true;

        private GameObject rangeIndicatorVFX;
        private VisualEffect rangeIndicatorEffect;

        // ── Lifecycle ──────────────────────────────────────────────────────────

        private void Awake()
        {
            Instance = this;

            canvasGroup = previewRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = previewRoot.AddComponent<CanvasGroup>();

            canvasGroup.blocksRaycasts = false;
            previewRoot.SetActive(false);
        }

        // ── Toggle ─────────────────────────────────────────────────────────────

        /// <summary>Called by the toggle button's OnClick.</summary>
        public void TogglePreview()
        {
            isPreviewEnabled = !isPreviewEnabled;

            if (!isPreviewEnabled)
                ForceHide();

            if (toggleButtonText != null)
            {
                toggleButtonText.text = isPreviewEnabled ? "◄" : "►";
                toggleButtonText.transform.DOKill();
                toggleButtonText.transform
                    .DOPunchScale(Vector3.one * 0.35f, 0.3f, vibrato: 1, elasticity: 0.5f)
                    .SetUpdate(true);
            }
        }

        // ── Show / Hide ────────────────────────────────────────────────────────

        public void Show(CardScript source)
        {
            // Honour the toggle — don't show while preview is disabled
            if (!isPreviewEnabled) return;
            if (source?.cardData == null) return;

            // Only reset alpha/scale when the panel was actually hidden.
            // Skipping the reset when already visible prevents the flicker
            // caused by jumping back to alpha 0 on every card hover.
            bool wasHidden = !previewRoot.activeSelf || canvasGroup.alpha < 0.01f;

            previewRoot.SetActive(true);
            previewCardScript.cardData = source.cardData;
            previewCardScript.ApplyCardDataVisuals();

            if (previewCardScript.cardBack != null)
                previewCardScript.cardBack.SetActive(false);

            canvasGroup.DOKill();
            previewRoot.transform.DOKill();

            if (wasHidden)
            {
                canvasGroup.alpha = 0f;
                previewRoot.transform.localScale = Vector3.one * 0.85f;
            }

            canvasGroup.DOFade(1f, fadeDuration).SetUpdate(true);
            previewRoot.transform.DOScale(1f, scaleDuration)
                .SetEase(Ease.OutBack).SetUpdate(true);

            // Only block raycasts during active drag so tooltip triggers behind the panel
            // remain reachable when the player is just hovering or has a card selected.
            canvasGroup.blocksRaycasts = DraggableCard.ActiveDraggingCard != null;

            // Display range indicator
            ShowRangeIndicator(source.cardData);
        }

        public void Hide()
        {
            // If preview is enabled and a card is selected, keep showing it
            if (isPreviewEnabled)
            {
                GameObject selected = HandManager.Instance?.GetSelectedCard();
                if (selected != null)
                {
                    CardScript selectedCard = selected.GetComponent<CardScript>();
                    if (selectedCard?.cardData != null)
                    {
                        Show(selectedCard);
                        return;
                    }
                }
            }

            ForceHide();
        }

        /// <summary>Always hides the panel, regardless of selected card or enabled state.</summary>
        private void ForceHide()
        {
            previewCardScript.StopAllCoroutines();
            canvasGroup.blocksRaycasts = false;

            canvasGroup.DOKill();
            previewRoot.transform.DOKill();
            canvasGroup.DOFade(0f, fadeDuration)
                .SetUpdate(true)
                .OnComplete(() => previewRoot.SetActive(false));

            // Hide range indicator
            HideRangeIndicator();
        }

        // ── Pointer: cancel targeting on click ────────────────────────────────

        public void OnPointerClick(PointerEventData eventData)
        {
            DraggableCard.ActiveDraggingCard?.CancelDrag();
        }

        // ── Drag forwarding to selected hand card ─────────────────────────────

        public void OnBeginDrag(PointerEventData eventData)
        {
            GameObject selected = HandManager.Instance?.GetSelectedCard();
            if (selected == null) return;
            ExecuteEvents.Execute<IBeginDragHandler>(selected, eventData, ExecuteEvents.beginDragHandler);
        }

        public void OnDrag(PointerEventData eventData)
        {
            GameObject selected = HandManager.Instance?.GetSelectedCard();
            if (selected == null) return;
            ExecuteEvents.Execute<IDragHandler>(selected, eventData, ExecuteEvents.dragHandler);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            GameObject selected = HandManager.Instance?.GetSelectedCard();
            if (selected == null) return;
            ExecuteEvents.Execute<IEndDragHandler>(selected, eventData, ExecuteEvents.endDragHandler);
        }

        // ── Range Indicator ────────────────────────────────────────────────────

        private void ShowRangeIndicator(CardData cardData)
        {
            // Hide any existing range indicator
            HideRangeIndicator();

            // Get the player entity
            PlayerScript player = Object.FindObjectOfType<PlayerScript>();
            if (player == null) return;

            // Get the range indicator prefab from AssetManager
            AssetManager assetManager = AssetManager.Instance;
            if (assetManager == null || assetManager.rangeIndicator == null) return;

            // Clone the range indicator
            rangeIndicatorVFX = Instantiate(assetManager.rangeIndicator.gameObject);
            rangeIndicatorEffect = rangeIndicatorVFX.GetComponent<VisualEffect>();

            if (rangeIndicatorEffect == null) return;

            // Set the position to the player's position
            rangeIndicatorVFX.transform.position = player.transform.position;

            // Set the Radius property to the card's range
            if (rangeIndicatorEffect.HasFloat("Radius"))
            {
                rangeIndicatorEffect.SetFloat("Radius", cardData.Range);
            }

            // Set the Start property to the player's position
            if (rangeIndicatorEffect.HasVector3("Start"))
            {
                rangeIndicatorEffect.SetVector3("Start", player.transform.position);
            }
        }

        private void HideRangeIndicator()
        {
            if (rangeIndicatorVFX != null)
            {
                Destroy(rangeIndicatorVFX);
                rangeIndicatorVFX = null;
                rangeIndicatorEffect = null;
            }
        }
    }
}
