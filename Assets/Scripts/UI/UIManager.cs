using DG.Tweening;
using facingfate;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace dungeonduell
{
    public class UIManager : MonoBehaviour
    {
        public GameObject creditsPanel;
        public GameObject creditsSelectedButton;
        public GameObject previousSelected;

        public OptionsMenu optionsMenu;
        public CanvasGroup fadeCanvas;

        public float fadeDuration = 0.5f;

        private void Start()
        {
            ShowCanvasGroup();
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
                fadeCanvas.DOFade(1, fadeDuration); // Sanftes Einblenden des UI
            }
        }

        // Wechselt die Szene mit Animation
        public void ChangeScene(string sceneName)
        {
            if (fadeCanvas != null)
                fadeCanvas.DOFade(0, fadeDuration).OnComplete(() => SceneManager.LoadScene(sceneName));
            else
                SceneManager.LoadScene(sceneName);
        }

        // Kehrt zum Titelbildschirm zur�ck
        public void ReturnToTitleScreen()
        {
            ChangeScene("TitleScreen");
        }

        // Beendet das Spiel
        public void QuitGame()
        {
            Application.Quit();
        }

        // �ffnet oder schlie�t das Credits-Panel mit Animation
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

        // �ffnet oder schlie�t das Options-Panel mit Animation
        public void ToggleOptions()
        {
            if (optionsMenu == null || optionsMenu.optionsPanel == null) return;

            bool isActive = optionsMenu.optionsPanel.activeSelf;

            if (!isActive)
            {
                if (EventSystem.current != null)
                    optionsMenu.previousSelected = EventSystem.current.currentSelectedGameObject;

                optionsMenu.OpenOptionsScroll();
            }
            else
            {
                optionsMenu.CloseOptionsScroll(true);
            }
        }

        private void OnEnable()
        {
            ShowCanvasGroup();
        }
    }
}