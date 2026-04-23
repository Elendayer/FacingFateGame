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
        [SerializeField] private RectTransform panelRect;

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI stepText;

        [Header("Buttons")]
        [SerializeField] private Button continueButton;

        private const float FadeTime = 0.3f;
        private LocalizedString _currentString;

        private void Awake()
        {
            // Hide panel before encounter starts — shown by TutorialCombatManager.StartTutorial()
            if (panelGroup != null)
            {
                panelGroup.alpha          = 0f;
                panelGroup.interactable   = false;
                panelGroup.blocksRaycasts = false;
            }
        }

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

            panelGroup.DOKill();
            panelGroup.interactable   = false;
            panelGroup.blocksRaycasts = false;
            panelGroup.DOFade(0f, FadeTime).OnComplete(() =>
            {
                BindLocalizedText(step.localizedText);
                panelGroup.DOFade(1f, FadeTime).OnComplete(() =>
                {
                    panelGroup.interactable   = true;
                    panelGroup.blocksRaycasts = true;
                });
            });
        }

        /// <summary>Fades out the tutorial panel. Called by TutorialCombatManager.TutorialComplete().</summary>
        public void HidePanel()
        {
            panelGroup.DOKill();
            panelGroup.DOFade(0f, FadeTime).OnComplete(() =>
            {
                panelGroup.interactable   = false;
                panelGroup.blocksRaycasts = false;
            });
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
            if (panelRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
        }

        private void UnsubscribeCurrentString()
        {
            if (_currentString != null)
                _currentString.StringChanged -= OnStringChanged;
        }
    }
}
