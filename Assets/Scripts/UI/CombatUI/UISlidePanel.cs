using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace facingfate
{
    public class UISlidePanel : MonoBehaviour
    {
        [Header("Referenzen")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button toggleButton;
        [SerializeField] private TMP_Text toggleButtonText;

        [Header("Mitzubewegende Objekte")]
        [SerializeField] private RectTransform[] additionalRects;

        [Header("Positionen (AnchoredPosition Y)")]
        [SerializeField] private float panelOpenY = 0f;
        [SerializeField] private float panelClosedY = 100f;
        [SerializeField] private float extrasOpenY = -30f;
        [SerializeField] private float extrasClosedY = 0f;

        [Header("Animation")]
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private Ease ease = Ease.OutQuart;
        [SerializeField] private bool startOpen = false;

        [Header("Button Text")]
        [SerializeField] private string openText = "▲";
        [SerializeField] private string closedText = "▼";

        private RectTransform panelRect;
        private bool isOpen;

        private void Awake()
        {
            if (panel != null)
                panelRect = panel.GetComponent<RectTransform>();

            if (toggleButton != null)
                toggleButton.onClick.AddListener(Toggle);

            // Initialzustand setzen
            isOpen = startOpen;

            if (panelRect != null)
                panelRect.anchoredPosition = new Vector2(
                    panelRect.anchoredPosition.x,
                    startOpen ? panelOpenY : panelClosedY);

            panel?.SetActive(startOpen);
            SetExtrasY(startOpen ? extrasOpenY : extrasClosedY, instant: true);
            UpdateButtonText();
        }

        public void Toggle()
        {
            isOpen = !isOpen;
            UpdateButtonText();

            panelRect?.DOKill();

            if (isOpen)
            {
                panel?.SetActive(true);
                panelRect?.DOAnchorPosY(panelOpenY, duration)
                    .SetEase(ease).SetUpdate(true);
                SetExtrasY(extrasOpenY);
            }
            else
            {
                panelRect?.DOAnchorPosY(panelClosedY, duration)
                    .SetEase(ease).SetUpdate(true)
                    .OnComplete(() => panel?.SetActive(false));
                SetExtrasY(extrasClosedY);
            }
        }

        private void SetExtrasY(float targetY, bool instant = false)
        {
            if (additionalRects == null) return;
            foreach (RectTransform rt in additionalRects)
            {
                if (rt == null) continue;
                rt.DOKill();

                if (instant)
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, targetY);
                else
                    rt.DOAnchorPosY(targetY, duration).SetEase(ease).SetUpdate(true);
            }
        }

        private void UpdateButtonText()
        {
            if (toggleButtonText != null)
                toggleButtonText.text = isOpen ? openText : closedText;
        }
    }
}