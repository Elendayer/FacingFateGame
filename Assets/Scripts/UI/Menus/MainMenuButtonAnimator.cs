using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace facingfate
{
    public class MainMenuButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private float hoverScale = 1.06f;
        [SerializeField] private float clickScale = 0.93f;
        [SerializeField] private float hoverDuration = 0.15f;
        [SerializeField] private float clickDuration = 0.08f;

        private Vector3 _baseScale;

        private void Awake() => _baseScale = transform.localScale;

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.DOKill();
            transform.DOScale(_baseScale * hoverScale, hoverDuration).SetEase(Ease.OutCubic);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.DOKill();
            transform.DOScale(_baseScale, hoverDuration).SetEase(Ease.OutCubic);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            transform.DOKill();
            transform.DOScale(_baseScale * clickScale, clickDuration).SetEase(Ease.OutQuart)
                .OnComplete(() =>
                    transform.DOScale(_baseScale, clickDuration).SetEase(Ease.OutBack));
        }
    }
}
