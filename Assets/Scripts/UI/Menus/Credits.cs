using DG.Tweening;
using UnityEngine;

namespace facingfate
{
    public class Credits : MonoBehaviour
    {
        [SerializeField] private RectTransform creditsContent;
        [SerializeField] private float scrollDuration = 12f;
        [SerializeField] private float startY = -600f;
        [SerializeField] private float endY = 600f;
        [SerializeField] private bool looping = false;

        private Tween _scrollTween;

        private void OnEnable()
        {
            StartScroll();
        }

        private void OnDisable()
        {
            _scrollTween?.Kill();
        }

        private void StartScroll()
        {
            creditsContent.anchoredPosition = new Vector2(creditsContent.anchoredPosition.x, startY);
            _scrollTween = creditsContent
                .DOAnchorPosY(endY, scrollDuration)
                .SetEase(Ease.Linear)
                .SetLoops(looping ? -1 : 1, LoopType.Restart);
        }
    }
}
