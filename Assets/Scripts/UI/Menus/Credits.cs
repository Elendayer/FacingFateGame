using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace facingfate
{
    public class Credits : MonoBehaviour
    {
        [SerializeField] private RectTransform creditsContent;
        [SerializeField] private RectTransform maskRect;
        [SerializeField] private float scrollDuration = 20f;
        [SerializeField] private float edgePadding = 150f;
        [SerializeField] private bool looping = true;

        private Tween _scrollTween;

        private void OnEnable()
        {
            StartCoroutine(StartScrollNextFrame());
        }

        private void OnDisable()
        {
            _scrollTween?.Kill();
        }

        private IEnumerator StartScrollNextFrame()
        {
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(creditsContent);
            StartScroll();
        }

        private void StartScroll()
        {
            float contentHeight = creditsContent.rect.height;
            float maskHeight = maskRect != null ? maskRect.rect.height : 600f;

            float startY = -(contentHeight * 0.5f + maskHeight * 0.5f + edgePadding);
            float endY   =  (contentHeight * 0.5f + maskHeight * 0.5f + edgePadding);

            creditsContent.anchoredPosition = new Vector2(0f, startY);

            _scrollTween = creditsContent
                .DOAnchorPosY(endY, scrollDuration)
                .SetEase(Ease.Linear)
                .SetLoops(looping ? -1 : 1, LoopType.Restart);
        }
    }
}
