using UnityEngine;
using DG.Tweening;

namespace facingfate
{
    public class CardPreviewPanel : MonoBehaviour
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
        }

        public void Hide()
        {
            previewCardScript.StopAllCoroutines();

            canvasGroup.DOKill();
            previewRoot.transform.DOKill();

            canvasGroup.DOFade(0f, fadeDuration)
                .SetUpdate(true)
                .OnComplete(() => previewRoot.SetActive(false));
        }
    }
}