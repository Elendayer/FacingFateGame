using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace facingfate
{
    public class EncounterResultPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button mainMenuButton;

        [SerializeField] private float fadeInDuration = 0.6f;
        [SerializeField] private string mainMenuSceneName = "TitleScreen";

        private void Awake()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            playAgainButton.onClick.AddListener(OnPlayAgain);
            mainMenuButton.onClick.AddListener(OnMainMenu);
        }

        private void OnEnable()
        {
            GameEvents.OnCombatResult += Show;
        }

        private void OnDisable()
        {
            GameEvents.OnCombatResult -= Show;
        }

        private void Show(bool playerWon)
        {
            titleText.text = playerWon ? "Victory!" : "Defeat";
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutCubic);
        }

        private void OnPlayAgain()
        {
            DOTween.KillAll();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnMainMenu()
        {
            DOTween.KillAll();
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
