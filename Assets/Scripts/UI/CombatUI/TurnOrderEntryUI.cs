using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace facingfate
{
    public class TurnOrderEntryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image background;
        private EntityScript boundEntity;

        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;

        [Header("Animation")]
        [SerializeField] private float slideAmount = 100f;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            rectTransform = GetComponent<RectTransform>();
        }

        public void Setup(string entityName, Color color, bool isCurrent, EntityScript entity)
        {
            boundEntity = entity;

            if (nameText != null)
            {
                nameText.text = entityName;
                nameText.fontStyle = isCurrent ? FontStyles.Bold : FontStyles.Normal;
                nameText.fontSize  = isCurrent ? 16f : 13f;
            }

            if (background != null)
                background.color = color;
        }

        /// <summary>Kill all tweens and snap to clean resting state. Called by TurnOrderUI on Refresh.</summary>
        public void HardReset()
        {
            if (canvasGroup   == null) canvasGroup   = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            if (rectTransform == null) rectTransform  = GetComponent<RectTransform>();

            canvasGroup.DOKill();
            rectTransform.DOKill();
            canvasGroup.alpha        = 1f;
            rectTransform.localScale = Vector3.one;
            // Reset Y — layout group controls X, but Y drift from DOLocalMoveY must be cleared
            rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, 0f, rectTransform.localPosition.z);

            var le = GetComponent<LayoutElement>();
            if (le != null) le.ignoreLayout = false;
        }

        /// <summary>
        /// Slide down + fade out sequentially.
        /// onSlideComplete fires when the slide finishes (before the fade).
        /// </summary>
        public void AnimateOut(float duration, System.Action onComplete = null, System.Action onSlideComplete = null)
        {
            if (canvasGroup   == null) canvasGroup   = gameObject.AddComponent<CanvasGroup>();
            if (rectTransform == null) rectTransform  = GetComponent<RectTransform>();

            float slideDur = duration * 0.65f;
            float fadeDur  = duration * 0.75f;

            DOTween.Sequence()
                .SetUpdate(true)
                .Append(rectTransform.DOLocalMoveY(rectTransform.localPosition.y - slideAmount, slideDur)
                    .SetEase(Ease.OutQuart))
                .AppendCallback(() => onSlideComplete?.Invoke())
                .Append(canvasGroup.DOFade(0f, fadeDur)
                    .SetEase(Ease.InQuart))
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
        }

        /// <summary>Slide in from below.</summary>
        public void AnimateIn(float duration)
        {
            if (canvasGroup   == null) canvasGroup   = gameObject.AddComponent<CanvasGroup>();
            if (rectTransform == null) rectTransform  = GetComponent<RectTransform>();

            canvasGroup.alpha        = 0f;
            rectTransform.localScale = Vector3.one;

            float startY = rectTransform.localPosition.y - slideAmount;
            rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, startY, rectTransform.localPosition.z);

            rectTransform.DOLocalMoveY(startY + slideAmount, duration).SetEase(Ease.OutCubic).SetUpdate(true);
            canvasGroup.DOFade(1f, duration * 0.6f).SetEase(Ease.OutQuart).SetUpdate(true);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (boundEntity == null) return;
            boundEntity.GetComponent<EntityOutline>()?.SetHover(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (boundEntity == null) return;
            boundEntity.GetComponent<EntityOutline>()?.SetNormal();
        }
    }
}
