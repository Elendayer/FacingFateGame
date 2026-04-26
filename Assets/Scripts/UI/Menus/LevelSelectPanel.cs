using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace facingfate
{
    public class LevelSelectPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private CanvasGroup backdrop;
        [SerializeField] private UIManager uiManager;

        [Header("Scene Names")]
        [SerializeField] private string tutorialScene = "Tutorial";
        [SerializeField] private string presetEncounterScene = "Gameplay_Combat_Map";
        [SerializeField] private string randomEncounterScene = "RandomEncounter_Map";

        [Header("First Selected Button")]
        [SerializeField] private GameObject firstSelectedButton;

        [Header("Animation")]
        [SerializeField] private float animDuration = 0.35f;
        [SerializeField] private float slideOffsetY = 200f;

        private Vector2 _shownPos;
        private Vector2 _hiddenPos;
        private GameObject _previousSelected;
        private bool _isShown = false;

        public bool IsShown => _isShown;

        private void Awake()
        {
            _shownPos = panelRect.anchoredPosition;
            _hiddenPos = _shownPos - new Vector2(0f, slideOffsetY);

            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;

            if (backdrop != null) { backdrop.alpha = 0f; backdrop.blocksRaycasts = false; }
        }

        public void Show()
        {
            if (_isShown) return;
            _isShown = true;

            if (EventSystem.current != null)
                _previousSelected = EventSystem.current.currentSelectedGameObject;

            panelRect.anchoredPosition = _hiddenPos;
            panelCanvasGroup.alpha = 0f;

            if (backdrop != null) { backdrop.blocksRaycasts = true; backdrop.DOFade(0.4f, animDuration).SetEase(Ease.OutQuart); }

            var seq = DOTween.Sequence();
            seq.Join(panelRect.DOAnchorPos(_shownPos, animDuration).SetEase(Ease.OutCubic));
            seq.Join(panelCanvasGroup.DOFade(1f, animDuration * 0.7f).SetEase(Ease.OutQuart));
            seq.OnComplete(() =>
            {
                panelCanvasGroup.interactable = true;
                panelCanvasGroup.blocksRaycasts = true;
                if (firstSelectedButton != null && EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            });
        }

        private void Update()
        {
            if (_isShown && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Hide();
        }

        public void Hide()
        {
            if (!_isShown) return;
            _isShown = false;

            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
            if (backdrop != null) { backdrop.blocksRaycasts = false; backdrop.DOFade(0f, animDuration * 0.5f); }

            var seq = DOTween.Sequence();
            seq.Join(panelRect.DOAnchorPos(_hiddenPos, animDuration * 0.75f).SetEase(Ease.InCubic));
            seq.Join(panelCanvasGroup.DOFade(0f, animDuration * 0.5f).SetEase(Ease.InQuart));
            seq.OnComplete(() =>
            {
                if (_previousSelected != null && EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(_previousSelected);
            });
        }

        public void LoadTutorial() => StartLoad(tutorialScene);
        public void LoadPresetEncounter() => StartLoad(presetEncounterScene);
        public void LoadRandomEncounter() => StartLoad(randomEncounterScene);

        private void StartLoad(string sceneName)
        {
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
            uiManager.ChangeScene(sceneName);
        }
    }
}
