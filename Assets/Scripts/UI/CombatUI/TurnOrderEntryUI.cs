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
                nameText.fontSize = isCurrent ? 16f : 13f;
            }

            if (background != null)
                background.color = color;
        }

        public void AnimateOut(float duration, System.Action onComplete = null)
        {
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            canvasGroup.DOFade(0f, duration)
                .SetEase(Ease.InQuart)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                    Destroy(gameObject);
                });
        }

        public void AnimateIn(float duration)
        {
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, duration)
                .SetEase(Ease.OutQuart)
                .SetUpdate(true);
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