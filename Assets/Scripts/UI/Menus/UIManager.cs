using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace facingfate
{
    public class UIManager : MonoBehaviour
    {
        public GameObject creditsPanel;
        public GameObject creditsSelectedButton;
        public GameObject previousSelected;

        [SerializeField] private OptionsMenu optionsMenu;
        [SerializeField] private LevelSelectPanel levelSelectPanel;
        [SerializeField] private CanvasGroup logoCanvasGroup;
        public CanvasGroup fadeCanvas;

        public float fadeDuration = 0.5f;

        private void Update()
        {
            // ESC closes options in main menu (PauseMenuManager handles this in-game)
            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                if (optionsMenu?.scrollAnimator?.IsOpen == true)
                    optionsMenu.CloseOptionsRoll();
            }
        }

        private void Start()
        {
            ShowCanvasGroup();

            if (logoCanvasGroup != null)
            {
                logoCanvasGroup.alpha = 0f;
                logoCanvasGroup.DOFade(1f, 1.2f).SetDelay(0.3f).SetEase(Ease.OutQuart);
            }
        }

        public void HideCanvasGroup()
        {
            if (fadeCanvas != null)
            {
                fadeCanvas.alpha = 1;
                fadeCanvas.DOFade(0, fadeDuration).OnComplete(() =>
                fadeCanvas.gameObject.SetActive(false));
            }
        }

        public void ShowCanvasGroup()
        {
            if (fadeCanvas != null)
            {
                fadeCanvas.gameObject.SetActive(true);
                fadeCanvas.alpha = 0;
                fadeCanvas.DOFade(1, fadeDuration);
            }
        }

        public void OpenLevelSelect()
        {
            if (levelSelectPanel == null) return;

            // Close options first so they don't overlap the level-select panel
            if (optionsMenu?.scrollAnimator?.IsOpen == true)
                optionsMenu.CloseOptionsRoll();

            if (levelSelectPanel.IsShown) levelSelectPanel.Hide();
            else levelSelectPanel.Show();
        }

        public void ChangeScene(string sceneName)
        {
            SceneFader.Load(sceneName);
        }

        public void ReturnToTitleScreen()
        {
            ChangeScene("Titlescreen");
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        public void ToggleCredits()
        {
            if (creditsPanel == null) return;

            if (creditsPanel.activeSelf)
            {
                creditsPanel.transform.DOKill();
                creditsPanel.transform.DOScale(0f, fadeDuration).OnComplete(() =>
                {
                    creditsPanel.SetActive(false);
                    creditsPanel.transform.localScale = Vector3.one;
                    EventSystem.current?.SetSelectedGameObject(null);
                    if (previousSelected != null) EventSystem.current?.SetSelectedGameObject(previousSelected);
                });
            }
            else
            {
                if (EventSystem.current != null) previousSelected = EventSystem.current.currentSelectedGameObject;
                creditsPanel.SetActive(true);
                creditsPanel.transform.localScale = Vector3.zero;
                creditsPanel.transform.DOScale(1f, fadeDuration).OnComplete(() =>
                {
                    if (creditsSelectedButton != null && EventSystem.current != null)
                        EventSystem.current.SetSelectedGameObject(creditsSelectedButton);
                });
            }
        }

        public void ToggleOptions()
        {
            if (optionsMenu == null)
            {
                Debug.LogError("UIManager.ToggleOptions: optionsMenu is null!");
                return;
            }

            // Ensure scrollAnimator is available before checking its state
            ScrollRollAnimator animator = optionsMenu.scrollAnimator;
            if (animator == null)
            {
                Debug.LogError("UIManager.ToggleOptions: scrollAnimator is null!");
                return;
            }

            // If currently animating, don't allow another toggle
            if (animator.IsAnimating)
            {
                Debug.Log("UIManager.ToggleOptions: Still animating, ignoring toggle request");
                return;
            }

            // Toggle based on actual open state
            Debug.Log($"UIManager.ToggleOptions: IsOpen={animator.IsOpen}, toggling to {!animator.IsOpen}");
            if (animator.IsOpen)
                optionsMenu.CloseOptionsRoll();
            else
                optionsMenu.OpenOptionsRoll();
        }

    }
}