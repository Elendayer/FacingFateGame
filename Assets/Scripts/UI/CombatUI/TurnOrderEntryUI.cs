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
            // Reset Y only — TurnOrderUI owns X via anchoredPosition
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 0f);

            var le = GetComponent<LayoutElement>();
            if (le != null)
            {
                DOTween.Kill(le);
                le.preferredWidth = -1f;
                le.ignoreLayout   = false;
            }
        }

        /// <summary>
        /// Slide down and fade out simultaneously.
        /// onComplete fires when the fade ends (= entryAnimDuration in TurnOrderUI).
        /// </summary>
        public void AnimateOut(float duration, System.Action onComplete = null)
        {
            if (canvasGroup   == null) canvasGroup   = gameObject.AddComponent<CanvasGroup>();
            if (rectTransform == null) rectTransform  = GetComponent<RectTransform>();

            // Slide and fade run in parallel — fade is the longer of the two and carries onComplete
            rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y - slideAmount, duration * 0.65f)
                .SetEase(Ease.OutQuart).SetUpdate(true);
            canvasGroup.DOFade(0f, duration * 0.55f)
                .SetEase(Ease.InQuart).SetUpdate(true)
                .OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>Slide in from below. Always starts slideAmount below the resting Y=0.</summary>
        public void AnimateIn(float duration)
        {
            if (canvasGroup   == null) canvasGroup   = gameObject.AddComponent<CanvasGroup>();
            if (rectTransform == null) rectTransform  = GetComponent<RectTransform>();

            canvasGroup.alpha        = 0f;
            rectTransform.localScale = Vector3.one;

            // Always snap to the fixed start position so accumulated Y from AnimateOut doesn't double the distance.
            rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, -slideAmount, rectTransform.localPosition.z);

            rectTransform.DOLocalMoveY(0f, duration).SetEase(Ease.OutCubic).SetUpdate(true);
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
