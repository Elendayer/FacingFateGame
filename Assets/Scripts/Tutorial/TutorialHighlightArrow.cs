using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace facingfate
{
    /// <summary>
    /// Positions an animated arrow and glow panel over a target RectTransform.
    /// Both live in the TutorialUI Canvas (overlay canvas, high sort order).
    /// </summary>
    public class TutorialHighlightArrow : MonoBehaviour
    {
        [Header("Arrow")]
        [SerializeField] private RectTransform arrowRect;
        [Tooltip("Offset from target center to arrow position (screen space).")]
        [SerializeField] private Vector2 arrowOffset = new(-80f, 0f);

        [Header("Glow")]
        [SerializeField] private RectTransform glowRect;
        [SerializeField] private CanvasGroup glowGroup;
        [Tooltip("Extra pixels added to each side of the target bounds.")]
        [SerializeField] private float glowPadding = 12f;

        private void Awake()
        {
            Hide();
        }

        /// <summary>Point arrow and glow at target. Pass null to hide.</summary>
        public void PointAt(RectTransform target)
        {
            if (target == null) { Hide(); return; }

            arrowRect.gameObject.SetActive(true);
            glowRect.gameObject.SetActive(true);

            arrowRect.position = target.position + (Vector3)arrowOffset;
            glowRect.position  = target.position;
            glowRect.sizeDelta = target.sizeDelta + Vector2.one * glowPadding * 2f;

            arrowRect.DOKill();
            arrowRect.DOScale(1.2f, 0.55f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

            glowGroup.DOKill();
            glowGroup.alpha = 0f;
            glowGroup.DOFade(0.55f, 0.8f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        public void Hide()
        {
            if (arrowRect != null)
            {
                arrowRect.DOKill();
                arrowRect.gameObject.SetActive(false);
            }
            if (glowRect != null)
            {
                glowGroup.DOKill();
                glowRect.gameObject.SetActive(false);
            }
        }
    }
}
