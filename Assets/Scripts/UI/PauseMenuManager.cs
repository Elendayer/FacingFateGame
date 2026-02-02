using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
//using FMODUnity;
//using FMOD.Studio;

namespace facingfate
{
    public class PauseMenuManager : MonoBehaviour
    {
        [Header("Panels & Buttons")] public GameObject pausePanel;
        public GameObject defaultSelectedButton;
        public GameObject optionsPanel;
        public GameObject tutorialPanel;
        public GameObject tutorialSelectedButton;
        public GameObject confirmationPopup;
        public GameObject confirmSelectedButton;
        public GameObject controlPanel;

        [Header("Settings")] public float fadeDuration = 0.25f;

        //private DungeonPhaseInput _controls;
        private bool _isPaused;

        private CanvasGroup _pauseGroup;
        private GameObject _previousSelected;

        private OptionsMenu optionsMenu;

        private bool _allClosing = false;

        //private VCA _vcaSfxNoUi;
        //private VCA _vcaSfx;

        private void Awake()
        {
            //_controls = new DungeonPhaseInput();
            //_controls.CardPhase.Pause.started += ctx => TogglePause(ctx);
        }

        private void Start()
        {
            _pauseGroup = pausePanel.GetComponent<CanvasGroup>();
            if (_pauseGroup == null) _pauseGroup = pausePanel.AddComponent<CanvasGroup>();
            optionsMenu = GetComponent<OptionsMenu>();

            pausePanel.SetActive(false);
            optionsPanel.SetActive(false);
            tutorialPanel.SetActive(false);
            confirmationPopup.SetActive(false);

            //_vcaSfxNoUi = RuntimeManager.GetVCA("vca:/SFX_NO_UI");
            //_vcaSfx = RuntimeManager.GetVCA("vca:/SFX");
        }

        private void OnEnable()
        {
            //_controls.CardPhase.Enable();
        }

        private void OnDisable()
        {
            //_controls.CardPhase.Disable();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                TogglePause();
        }

        private void TogglePause()//InputAction.CallbackContext context
        {
            if (!_isPaused) OpenPauseMenu();// context)
            else ResumeGame();
        }

        public void OpenPauseMenu()//InputAction.CallbackContext context)
        {
            _allClosing = false;
            //GameManager.Instance.Paused = true;
            Cursor.visible = true;
            pausePanel.SetActive(true);
            pausePanel.transform.localScale = Vector3.zero;
            _pauseGroup.alpha = 0;

            //EventSystem.current.sendNavigationEvents = context.control.device is not Keyboard;
            

            /*
            if (_vcaSfxNoUi.isValid())
            {
                _vcaSfxNoUi.setVolume(0f);
            }
             */

            if (EventSystem.current != null)
            {
                _previousSelected = EventSystem.current.currentSelectedGameObject;
                EventSystem.current.SetSelectedGameObject(null);
            }

            pausePanel.transform.DOScale(1, fadeDuration).SetEase(Ease.OutBack).SetUpdate(true);
            _pauseGroup.DOFade(1, fadeDuration).SetUpdate(true).OnComplete(() =>
            {
                if (defaultSelectedButton != null && EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(defaultSelectedButton);
                else
                    EventSystem.current.SetSelectedGameObject(null);
            });
            _isPaused = true;
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            //GameManager.Instance.Paused = false;
            EventSystem.current.sendNavigationEvents = true;

            /*
            float target = 1f;
            if (_vcaSfx.isValid())
            {
                _vcaSfx.getVolume(out target);
            }

            if (_vcaSfxNoUi.isValid())
            {
                _vcaSfxNoUi.setVolume(target);
            }
             */

            Time.timeScale = 1f;
            _isPaused = false;
            pausePanel.transform.DOScale(0, fadeDuration).SetEase(Ease.InBack).SetUpdate(true);
            _pauseGroup.DOFade(0, fadeDuration).SetUpdate(true).OnComplete(() =>
            {
                _isPaused = false;
                Time.timeScale = 1f;
                pausePanel.SetActive(false);
                //DdCodeEventHandler.Trigger_TutorialDone();

                _allClosing = true;

                if (controlPanel.activeInHierarchy)
                {
                    CloseControlPanel();
                }

                if (tutorialPanel.activeInHierarchy)
                {
                    CloseTutorial();
                }

                if (confirmationPopup.activeInHierarchy)
                {
                    CancelGiveUp();
                }

                if (optionsPanel.activeInHierarchy)
                {
                    //optionsMenu.CloseOptionsScroll(true);
                }

                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    if (_previousSelected != null) EventSystem.current.SetSelectedGameObject(_previousSelected);
                }
            });
        }

        public void OpenControlPanel()
        {
            if (controlPanel == null) return;

            controlPanel.SetActive(true);

            var group = controlPanel.GetComponent<CanvasGroup>();
            if (group == null) group = controlPanel.AddComponent<CanvasGroup>();

            group.alpha = 0;
            group.DOFade(1, fadeDuration).SetEase(Ease.OutCubic).SetUpdate(true);

            if (EventSystem.current != null && tutorialSelectedButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(tutorialSelectedButton);
            }
        }

        public void CloseControlPanel()
        {
            if (controlPanel == null) return;

            var group = controlPanel.GetComponent<CanvasGroup>();
            if (group == null) group = controlPanel.AddComponent<CanvasGroup>();

            group.DOFade(0, fadeDuration).SetEase(Ease.InCubic).SetUpdate(true).OnComplete(() =>
            {
                controlPanel.SetActive(false);

                if (EventSystem.current != null && defaultSelectedButton != null & !_allClosing)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(tutorialSelectedButton);
                }
            });
        }


        public void OpenTutorial()
        {
            tutorialPanel.SetActive(true);
            var rect = tutorialPanel.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, -800);
            rect.DOAnchorPosY(0, fadeDuration).SetEase(Ease.OutCubic).SetUpdate(true).OnComplete(() =>
            {
                if (tutorialSelectedButton != null && EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(tutorialSelectedButton);
                }
            });
        }

        public void CloseTutorial()
        {
            var rect = tutorialPanel.GetComponent<RectTransform>();
            rect.DOAnchorPosY(-800, fadeDuration).SetEase(Ease.InCubic).SetUpdate(true).OnComplete(() =>
            {
                tutorialPanel.SetActive(false);
                if (defaultSelectedButton != null && EventSystem.current != null & !_allClosing)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(defaultSelectedButton);
                }
            });
        }

        public void ConfirmGiveUp()
        {
            confirmationPopup.SetActive(true);
            confirmationPopup.transform.localScale = Vector3.zero;
            confirmationPopup.transform.DOScale(0.5f, fadeDuration).SetEase(Ease.OutBack).SetUpdate(true).OnComplete(
                () =>
                {
                    if (confirmSelectedButton != null && EventSystem.current != null)
                    {
                        EventSystem.current.SetSelectedGameObject(null);
                        EventSystem.current.SetSelectedGameObject(confirmSelectedButton);
                    }
                });
        }

        public void CancelGiveUp()
        {
            confirmationPopup.transform.DOScale(0, fadeDuration).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
            {
                confirmationPopup.SetActive(false);
                if (defaultSelectedButton != null && EventSystem.current != null & !_allClosing)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(defaultSelectedButton);
                }
            });
        }

        public void GiveUpConfirmed()
        {
            /*
            float target = 1f;
            if (_vcaSfx.isValid()) _vcaSfx.getVolume(out target);
            if (_vcaSfxNoUi.isValid()) _vcaSfxNoUi.setVolume(target);
             */

            Time.timeScale = 1f;
            //DdCodeEventHandler.Trigger_GameReset();
            SceneManager.LoadScene("Titlescreen");
        }
    }
}