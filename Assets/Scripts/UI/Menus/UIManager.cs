using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

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
            if (levelSelectPanel != null) levelSelectPanel.Show();
        }

        public void ChangeScene(string sceneName)
        {
            SceneFader.Instance.FadeToScene(sceneName);
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
            if (creditsPanel != null)
            {
                var isActive = creditsPanel.activeSelf;
                creditsPanel.SetActive(true);
                creditsPanel.transform.localScale = Vector3.zero;
                creditsPanel.transform.DOScale(isActive ? 0 : 1, fadeDuration).OnComplete(() =>
                {
                    if (isActive)
                    {
                        creditsPanel.SetActive(false);
                        EventSystem.current.SetSelectedGameObject(null);
                        if (previousSelected != null) EventSystem.current.SetSelectedGameObject(previousSelected);
                    }
                    else
                    {
                        if (creditsSelectedButton != null && EventSystem.current != null)
                        EventSystem.current.SetSelectedGameObject(creditsSelectedButton);
                    }
                        
                });
            }
        }

        public void ToggleOptions()
        {
            if (optionsMenu == null || optionsMenu.optionsPanel == null) return;

            bool isActive = optionsMenu.optionsPanel.activeSelf;
            if (!isActive) optionsMenu.OpenOptionsRoll();
            else optionsMenu.CloseOptionsRoll();
        }

        private void OnEnable()
        {
            ShowCanvasGroup();
        }
    }
}