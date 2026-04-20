using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace facingfate
{
    /// <summary>
    /// Win/Lose result panel shown at combat end.
    ///
    /// Unity Hierarchy setup:
    ///   EncounterResultPanel (this script + CanvasGroup)
    ///   ├── Overlay         (Image, dark bg + CanvasGroup)
    ///   └── Panel           (RectTransform + CanvasGroup)
    ///       ├── Title        (TextMeshProUGUI)
    ///       ├── PlayAgainButton  (Button + CanvasGroup)
    ///       └── MainMenuButton   (Button + CanvasGroup)
    /// </summary>
    public class EncounterResultPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup rootGroup;
        [SerializeField] private CanvasGroup overlayGroup;
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private CanvasGroup playAgainGroup;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private CanvasGroup mainMenuGroup;

        [Header("Colors")]
        [SerializeField] private Color winColor  = new Color(1f,   0.85f, 0.2f);  // gold
        [SerializeField] private Color loseColor = new Color(0.9f, 0.2f,  0.2f);  // red

        [Header("Timing")]
        [SerializeField] private float overlayFadeDuration  = 0.35f;
        [SerializeField] private float panelSlideDuration   = 0.55f;
        [SerializeField] private float titlePopDuration     = 0.4f;
        [SerializeField] private float buttonFadeDuration   = 0.3f;
        [SerializeField] private float buttonStagger        = 0.1f;
        [SerializeField] private float panelSlideOffset     = 80f;     // px below start
        [SerializeField] private float overlayTargetAlpha   = 0.65f;
        [SerializeField] private string mainMenuSceneName   = "TitleScreen";

        private void Awake()
        {
            // Start fully hidden
            rootGroup.alpha = 0f;
            rootGroup.interactable = false;
            rootGroup.blocksRaycasts = false;

            if (overlayGroup != null) overlayGroup.alpha = 0f;
            if (panelGroup   != null) panelGroup.alpha   = 0f;
            if (playAgainGroup != null) playAgainGroup.alpha = 0f;
            if (mainMenuGroup  != null) mainMenuGroup.alpha  = 0f;

            playAgainButton.onClick.AddListener(OnPlayAgain);
            mainMenuButton.onClick.AddListener(OnMainMenu);
        }

        private void OnEnable()  => GameEvents.OnCombatResult += Show;
        private void OnDisable() => GameEvents.OnCombatResult -= Show;

        private void Show(bool playerWon)
        {
            // Kill any in-flight tweens so a re-triggered Show doesn't stack sequences.
            if (panelRect     != null) DOTween.Kill(panelRect);
            if (overlayGroup  != null) DOTween.Kill(overlayGroup);
            if (panelGroup    != null) DOTween.Kill(panelGroup);
            if (titleText     != null) DOTween.Kill(titleText.transform);
            if (playAgainGroup != null) DOTween.Kill(playAgainGroup);
            if (mainMenuGroup  != null) DOTween.Kill(mainMenuGroup);

            titleText.text  = playerWon ? "Victory!" : "Defeat";
            titleText.color = playerWon ? winColor : loseColor;

            rootGroup.interactable   = true;
            rootGroup.blocksRaycasts = true;
            rootGroup.alpha          = 1f;

            // Reset panel position & scale for animation
            if (panelRect != null)
            {
                panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, -panelSlideOffset);
                panelRect.localScale = Vector3.one * 0.85f;
            }

            // Reset title scale
            titleText.transform.localScale = Vector3.one * 0.6f;

            Sequence seq = DOTween.Sequence().SetUpdate(true);

            // 1. Overlay fade in
            if (overlayGroup != null)
                seq.Append(overlayGroup.DOFade(overlayTargetAlpha, overlayFadeDuration).SetEase(Ease.OutCubic));

            // 2. Panel slide up + scale + fade
            if (panelGroup != null && panelRect != null)
            {
                seq.Append(panelGroup.DOFade(1f, panelSlideDuration * 0.6f).SetEase(Ease.OutCubic));
                seq.Join(panelRect.DOAnchorPosY(0f, panelSlideDuration).SetEase(Ease.OutBack));
                seq.Join(panelRect.DOScale(1f, panelSlideDuration).SetEase(Ease.OutBack));
            }

            // 3. Title pop
            seq.Append(titleText.transform.DOScale(1f, titlePopDuration).SetEase(Ease.OutBack));

            // 4. Buttons fade in staggered
            seq.AppendInterval(0.05f);
            if (playAgainGroup != null)
                seq.Append(playAgainGroup.DOFade(1f, buttonFadeDuration).SetEase(Ease.OutCubic));
            seq.AppendInterval(buttonStagger);
            if (mainMenuGroup != null)
                seq.Append(mainMenuGroup.DOFade(1f, buttonFadeDuration).SetEase(Ease.OutCubic));
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
