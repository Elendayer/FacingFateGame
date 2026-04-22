using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace facingfate
{
    /// <summary>
    /// Singleton that orchestrates the in-combat tutorial.
    /// Subscribes to GameEvents; locks/unlocks cards and End Turn per step.
    /// Zero modifications to existing gameplay scripts (except StartupManager: +3 lines).
    /// </summary>
    public class TutorialCombatManager : MonoBehaviour
    {
        public static TutorialCombatManager Instance { get; private set; }

        [Header("Steps")]
        [SerializeField] private TutorialStepData[] steps;

        [Header("UI References")]
        [SerializeField] private TutorialOverlayUI overlayUI;
        [SerializeField] private TutorialHighlightArrow highlightArrow;
        [SerializeField] private Button endTurnButton;

        [Header("Scene")]
        [Tooltip("Name of the main menu scene to load when tutorial ends.")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private int _currentStepIndex;
        private bool _isActive;

        // ── Lifecycle ──────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.OnCombatStart       += StartTutorial;
            GameEvents.OnTurnStart         += OnTurnStart;
            GameEvents.OnTurnEnd           += OnTurnEnd;
            GameEvents.OnCombatEnd         += OnCombatEnd;
            GameEvents.OnGameplayReference += OnGameplayReference;
        }

        private void OnDisable()
        {
            GameEvents.OnCombatStart       -= StartTutorial;
            GameEvents.OnTurnStart         -= OnTurnStart;
            GameEvents.OnTurnEnd           -= OnTurnEnd;
            GameEvents.OnCombatEnd         -= OnCombatEnd;
            GameEvents.OnGameplayReference -= OnGameplayReference;
        }

        // ── Tutorial flow ──────────────────────────────────────────────────────

        private void StartTutorial()
        {
            _isActive = true;
            _currentStepIndex = 0;
            ActivateStep(0);
        }

        private void ActivateStep(int index)
        {
            if (index >= steps.Length) return;

            var step = steps[index];

            LockHandForStep(step);

            if (endTurnButton != null)
                endTurnButton.interactable = !step.lockEndTurn;

            overlayUI.ShowStep(step, AdvanceStep);

            RectTransform target = step.highlightTarget;
            if (target == null && !step.unlockAll && step.allowedCardIds.Length > 0)
                target = FindCardInHand(step.allowedCardIds[0]);

            highlightArrow.PointAt(target);
        }

        public void AdvanceStep()
        {
            if (!_isActive) return;

            UnlockAll();
            _currentStepIndex++;

            if (_currentStepIndex >= steps.Length)
            {
                TutorialComplete();
                return;
            }

            ActivateStep(_currentStepIndex);
        }

        private void TutorialComplete()
        {
            highlightArrow.Hide();
            overlayUI.ShowBackToMenu();
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }

        // ── GameEvent handlers ─────────────────────────────────────────────────

        private void OnTurnStart()
        {
            if (!_isActive || _currentStepIndex >= steps.Length) return;
            LockHandForStep(steps[_currentStepIndex]);
        }

        private void OnTurnEnd()
        {
            if (!_isActive || _currentStepIndex >= steps.Length) return;
            if (steps[_currentStepIndex].condition == CompletionCondition.EndTurnPressed)
                AdvanceStep();
        }

        private void OnCombatEnd(bool playerWon)
        {
            if (!_isActive || _currentStepIndex >= steps.Length) return;
            if (playerWon && steps[_currentStepIndex].condition == CompletionCondition.EnemyDead)
                AdvanceStep();
        }

        private void OnGameplayReference(ToSendTriggerReference r)
        {
            if (!_isActive || _currentStepIndex >= steps.Length) return;
            if (r.OnTriggerReference == null) return;
            if (!r.OnTriggerReference.Contains(GameplayRef.onCardPlayed)) return;

            var step = steps[_currentStepIndex];

            if (step.condition == CompletionCondition.AnyCardPlayed)
            {
                AdvanceStep();
                return;
            }

            if (step.condition == CompletionCondition.CardPlayed
                && step.allowedCardIds.Length > 0
                && r.CardData != null
                && r.CardData.cardID == step.allowedCardIds[0])
            {
                AdvanceStep();
            }
        }

        // ── ActionLock helpers ─────────────────────────────────────────────────

        private void LockHandForStep(TutorialStepData step)
        {
            if (HandManager.Instance == null) return;

            var cards = new List<GameObject>(HandManager.Instance.cardsInHand);
            foreach (var cardGO in cards)
            {
                if (cardGO == null) continue;
                var cs = cardGO.GetComponent<CardScript>();
                if (cs == null) continue;

                bool allowed = step.unlockAll
                    || System.Array.IndexOf(step.allowedCardIds, cs.cardData.cardID) >= 0;

                cs.SetupLock(!allowed);

                var cg = cardGO.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = allowed ? 1f : 0.4f;
            }
        }

        private void UnlockAll()
        {
            if (HandManager.Instance == null) return;
            var cards = new List<GameObject>(HandManager.Instance.cardsInHand);
            foreach (var cardGO in cards)
            {
                if (cardGO == null) continue;
                var cs = cardGO.GetComponent<CardScript>();
                if (cs != null) cs.SetupLock(false);
                var cg = cardGO.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
            }
        }

        private RectTransform FindCardInHand(string cardId)
        {
            foreach (var cardGO in HandManager.Instance.cardsInHand)
            {
                if (cardGO == null) continue;
                var cs = cardGO.GetComponent<CardScript>();
                if (cs != null && cs.cardData.cardID == cardId)
                    return cardGO.GetComponent<RectTransform>();
            }
            return null;
        }
    }
}
