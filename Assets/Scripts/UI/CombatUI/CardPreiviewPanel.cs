using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace facingfate
{
    public class CardPreviewPanel : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public static CardPreviewPanel Instance { get; private set; }

        [SerializeField] private GameObject previewRoot;
        [SerializeField] private CardScript previewCardScript;

        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.15f;
        [SerializeField] private float scaleDuration = 0.15f;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            Instance = this;
            canvasGroup = previewRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = previewRoot.AddComponent<CanvasGroup>();

            canvasGroup.blocksRaycasts = false;
            previewRoot.SetActive(false);
        }

        public void Show(CardScript source)
        {
            if (source?.cardData == null) return;

            previewRoot.SetActive(true);
            previewCardScript.cardData = source.cardData;
            previewCardScript.ApplyCardDataVisuals();

            if (previewCardScript.cardBack != null)
                previewCardScript.cardBack.SetActive(false);

            canvasGroup.DOKill();
            previewRoot.transform.DOKill();
            canvasGroup.alpha = 0f;
            previewRoot.transform.localScale = Vector3.one * 0.85f;
            canvasGroup.DOFade(1f, fadeDuration).SetUpdate(true);
            previewRoot.transform.DOScale(1f, scaleDuration)
                .SetEase(Ease.OutBack).SetUpdate(true);

            // Raycasts aktivieren damit Drag funktioniert
            canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            previewCardScript.StopAllCoroutines();
            canvasGroup.blocksRaycasts = false;

            canvasGroup.DOKill();
            previewRoot.transform.DOKill();
            canvasGroup.DOFade(0f, fadeDuration)
                .SetUpdate(true)
                .OnComplete(() => previewRoot.SetActive(false));
        }

        // ── Drag von Preview an ausgewählte Karte weiterleiten ──

        public void OnBeginDrag(PointerEventData eventData)
        {
            GameObject selected = HandManager.Instance?.GetSelectedCard();
            if (selected == null) return;

            ExecuteEvents.Execute<IBeginDragHandler>(
                selected, eventData, ExecuteEvents.beginDragHandler);
        }

        public void OnDrag(PointerEventData eventData)
        {
            GameObject selected = HandManager.Instance?.GetSelectedCard();
            if (selected == null) return;

            ExecuteEvents.Execute<IDragHandler>(
                selected, eventData, ExecuteEvents.dragHandler);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            GameObject selected = HandManager.Instance?.GetSelectedCard();
            if (selected == null) return;

            ExecuteEvents.Execute<IEndDragHandler>(
                selected, eventData, ExecuteEvents.endDragHandler);
        }
    }
}