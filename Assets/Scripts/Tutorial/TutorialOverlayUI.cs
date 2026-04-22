using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace facingfate
{
    /// <summary>
    /// Manages the tutorial text panel, Continue button, and Back to Menu button.
    /// Fades between steps using DOTween.
    /// </summary>
    public class TutorialOverlayUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private CanvasGroup panelGroup;

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI stepText;

        [Header("Buttons")]
        [SerializeField] private Button continueButton;
        [SerializeField] private GameObject backToMenuButton;

        private const float FadeTime = 0.3f;
        private LocalizedString _currentString;

        private void OnDestroy()
        {
            UnsubscribeCurrentString();
        }

        /// <summary>
        /// Called by TutorialCombatManager when a step activates.
        /// onContinue is wired to ContinueButton click.
        /// </summary>
        public void ShowStep(TutorialStepData step, System.Action onContinue)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => onContinue?.Invoke());

            bool showContinue = step.condition == CompletionCondition.ContinueButton;
            continueButton.gameObject.SetActive(showContinue);
            backToMenuButton.SetActive(false);

            panelGroup.DOKill();
            panelGroup.DOFade(0f, FadeTime).OnComplete(() =>
            {
                BindLocalizedText(step.localizedText);
                panelGroup.DOFade(1f, FadeTime);
            });
        }

        /// <summary>Shows the Back to Menu button after tutorial complete.</summary>
        public void ShowBackToMenu()
        {
            backToMenuButton.SetActive(true);
            continueButton.gameObject.SetActive(false);
        }

        private void BindLocalizedText(LocalizedString newString)
        {
            UnsubscribeCurrentString();
            _currentString = newString;
            _currentString.StringChanged += OnStringChanged;
            _currentString.RefreshString();
        }

        private void OnStringChanged(string value)
        {
            stepText.text = value;
        }

        private void UnsubscribeCurrentString()
        {
            if (_currentString != null)
                _currentString.StringChanged -= OnStringChanged;
        }
    }
}
