using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

namespace dungeonduell
{
    public class SkipHintCycle : MonoBehaviour
    {
        [Header("Elements")]
        public CanvasGroup skipText;
        public CanvasGroup lbIcon;
        public CanvasGroup escIcon;

        [Header("Timing")]
        public float fadeDuration = 0.3f;
        public float visibleDuration = 1.2f;

        private void Start()
        {
            // Setze alle auf 0 am Anfang
            skipText.alpha = 0;
            lbIcon.alpha = 0;
            escIcon.alpha = 0;

            StartCoroutine(CycleHints());
        }

        private IEnumerator CycleHints()
        {
            while (true)
            {
                yield return PlaySingle(skipText);
                yield return PlaySingle(lbIcon);
                yield return PlaySingle(escIcon);
            }
        }

        private IEnumerator PlaySingle(CanvasGroup target)
        {
            yield return target.DOFade(1f, fadeDuration).SetUpdate(true).WaitForCompletion();
            yield return new WaitForSeconds(visibleDuration);
            yield return target.DOFade(0f, fadeDuration).SetUpdate(true).WaitForCompletion();
        }
    }
}
