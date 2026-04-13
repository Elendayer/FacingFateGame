using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace facingfate
{
    public class ScrollRollAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panelRoot;          
        [SerializeField] private RectTransform scrollRoot;      
        [SerializeField] private RectTransform revealRect;      
        [SerializeField] private CanvasGroup contentGroup;      
        [SerializeField] private CanvasGroup rootGroup;         

        [Header("Layout / Sizing")]
        [Tooltip("Offene Höhe der Reveal-Fläche. Empfohlen: im Inspector fest setzen.")]
        [SerializeField] private float openHeight = 650f;

        [Tooltip("Zusätzlicher Abstand oberhalb des Screens beim Start (offscreen).")]
        [SerializeField] private float offscreenPadding = 80f;

        [Header("Timings")]
        [SerializeField] private float moveDuration = 0.45f;
        [SerializeField] private float unfoldDuration = 0.50f;
        [SerializeField] private float contentFadeIn = 0.12f;
        [SerializeField] private float contentFadeOut = 0.10f;

        [Header("Ease")]
        [SerializeField] private Ease moveEaseOpen = Ease.OutCubic;
        [SerializeField] private Ease unfoldEaseOpen = Ease.OutQuart;
        [SerializeField] private Ease moveEaseClose = Ease.InCubic;
        [SerializeField] private Ease unfoldEaseClose = Ease.InCubic;

        private Vector2 _onScreenPos;
        private Vector2 _offScreenPos;
        private bool _cached;
        private Sequence _seq;

        private void Awake()
        {
            CachePositionsIfNeeded();

            if (panelRoot != null)
                panelRoot.SetActive(false);

            if (rootGroup != null)
            {
                rootGroup.alpha = 1f;
                rootGroup.interactable = false;
                rootGroup.blocksRaycasts = false;
            }

            if (contentGroup != null)
            {
                contentGroup.alpha = 0f;
                contentGroup.interactable = false;
                contentGroup.blocksRaycasts = false;
            }
        }

        private void CachePositionsIfNeeded()
        {
            if (_cached) return;
            if (scrollRoot == null) return;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRoot);

            _onScreenPos = scrollRoot.anchoredPosition;
            _offScreenPos = _onScreenPos + new Vector2(0f, openHeight + offscreenPadding);

            _cached = true;
        }

        public void Open(GameObject selectedAfterOpen = null)
        {
            if (panelRoot == null || scrollRoot == null || revealRect == null) return;
            CachePositionsIfNeeded();
            KillSequence();

            panelRoot.SetActive(true);

            // Startzustand (geschlossen + offscreen)
            scrollRoot.anchoredPosition = _offScreenPos;
            SetRevealHeight(0f);

            if (rootGroup != null)
            {
                rootGroup.interactable = false;
                rootGroup.blocksRaycasts = false;
            }

            if (contentGroup != null)
            {
                contentGroup.alpha = 0f;
                contentGroup.interactable = false;
                contentGroup.blocksRaycasts = false;
            }

            float h = 0f;

            _seq = DOTween.Sequence().SetUpdate(true);

            // reinfahren + entrollen parallel
            _seq.Join(scrollRoot.DOAnchorPos(_onScreenPos, moveDuration).SetEase(moveEaseOpen));
            _seq.Join(DOTween.To(() => h, x => { h = x; SetRevealHeight(h); }, openHeight, unfoldDuration)
                .SetEase(unfoldEaseOpen));

            // erst nach vollständigem Öffnen: Content einblenden
            if (contentGroup != null)
            {
                _seq.Append(contentGroup.DOFade(1f, contentFadeIn).SetUpdate(true));
                _seq.AppendCallback(() =>
                {
                    contentGroup.interactable = true;
                    contentGroup.blocksRaycasts = true;
                });
            }

            _seq.AppendCallback(() =>
            {
                if (rootGroup != null)
                {
                    rootGroup.interactable = true;
                    rootGroup.blocksRaycasts = true;
                }

                if (selectedAfterOpen != null && EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(selectedAfterOpen);
                }
            });
        }

        public void Close(GameObject restoreSelection = null)
        {
            if (panelRoot == null || scrollRoot == null || revealRect == null) return;
            CachePositionsIfNeeded();
            KillSequence();

            if (rootGroup != null)
            {
                rootGroup.interactable = false;
                rootGroup.blocksRaycasts = false;
            }

            if (contentGroup != null)
            {
                contentGroup.interactable = false;
                contentGroup.blocksRaycasts = false;
            }

            _seq = DOTween.Sequence().SetUpdate(true);

            // 1) Content zuerst ausblenden
            if (contentGroup != null)
                _seq.Append(contentGroup.DOFade(0f, contentFadeOut).SetUpdate(true));
            else
                _seq.AppendInterval(contentFadeOut);

            // 2) hochrollen + nach oben raus parallel
            float h = revealRect.rect.height;

            _seq.Join(scrollRoot.DOAnchorPos(_offScreenPos, moveDuration).SetEase(moveEaseClose));
            _seq.Join(DOTween.To(() => h, x => { h = x; SetRevealHeight(h); }, 0f, unfoldDuration)
                .SetEase(unfoldEaseClose));

            _seq.AppendCallback(() =>
            {
                panelRoot.SetActive(false);

                if (restoreSelection != null && EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(restoreSelection);
                }
            });
        }

        private void SetRevealHeight(float height)
        {
            revealRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(0f, height));
        }

        private void KillSequence()
        {
            if (_seq != null && _seq.IsActive())
            {
                _seq.Kill();
                _seq = null;
            }
        }
    }
}
