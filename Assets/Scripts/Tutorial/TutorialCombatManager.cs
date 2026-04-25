using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        [Header("Player Reference")]
        [Tooltip("Assign the player entity's Transform. Used for MovedToTarget proximity checks.")]
        [SerializeField] private Transform playerTransform;

        [Header("Enemy Waves")]
        [Tooltip("Parent transform under which spawned enemies are placed (e.g. the combat entities container).")]
        [SerializeField] private Transform entitySpawnParent;
        [SerializeField] private TutorialEnemyWave[] enemyWaves;

        private int _currentStepIndex;
        private bool _isActive;
        public bool IsActive => _isActive;

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

        // ── Unity update ──────────────────────────────────────────────────────────

        private void Update()
        {
            if (!_isActive || _currentStepIndex >= steps.Length) return;
            if (playerTransform == null) return;

            var step = steps[_currentStepIndex];
            if (step.condition != CompletionCondition.MovedToTarget) return;
            if (step.movementTarget == null) return;

            float dist = Vector3.Distance(playerTransform.position, step.movementTarget.position);
            if (dist <= step.movementThreshold)
                AdvanceStep();
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

            SpawnWaveForStep(index);

            LockHandForStep(step);

            if (endTurnButton != null)
                endTurnButton.interactable = !step.lockEndTurn;

            overlayUI.ShowStep(step, AdvanceStep);

            // Resolve dynamic card targets (null target + allowedCardIds = find in hand)
            var entries = ResolveHighlights(step);
            highlightArrow.PointAt(entries);
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
            _isActive = false;
            highlightArrow.Hide();
            overlayUI.HidePanel();
            GameEvents.TriggerCombatEnd(true);
        }

        // ── GameEvent handlers ─────────────────────────────────────────────────

        private void OnTurnStart()
        {
            if (!_isActive || _currentStepIndex >= steps.Length) return;
            StartCoroutine(LockHandNextFrame(steps[_currentStepIndex]));
        }

        private IEnumerator LockHandNextFrame(TutorialStepData step)
        {
            yield return null; // wait one frame so DeckManager draws cards first
            LockHandForStep(step);
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
                // Delay so TurnManager.OnCombatEnd finishes clearing TurnOrder before we spawn the next wave.
                ActionQueueUtility.EnqueueAction(AdvanceStep);
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

        // ── Enemy wave spawning ────────────────────────────────────────────────

        private void SpawnWaveForStep(int stepIndex)
        {
            if (enemyWaves == null) return;

            bool anySpawned = false;
            foreach (var wave in enemyWaves)
            {
                if (wave.stepIndex != stepIndex) continue;
                foreach (var entry in wave.entries)
                {
                    if (string.IsNullOrEmpty(entry.npcId)) continue;
                    if (entry.spawnPoint == null)
                    {
                        Debug.LogWarning($"[TutorialCombatManager] spawnPoint null for npcId '{entry.npcId}' at step {stepIndex} — skipped.");
                        continue;
                    }
                    var spawned = CombatUtility.SpawnEntity(entry.spawnPoint.position, entry.npcId, wave.affiliation);
                    anySpawned = true;
                    Debug.Log($"[TutorialCombatManager] Spawned {spawned.name} at step {stepIndex}");
                }
            }

            // When a new wave spawns after the previous combat ended, the TurnManager and
            // EncounterManager have both set combatEnded = true and cleared TurnOrder.
            // Reset them so the new enemies can take turns and deaths are detected again.
            if (anySpawned && TurnManager.Instance != null && TurnManager.Instance.CombatEnded)
            {
                TurnManager.Instance.ResetCombatForNewWave();
                EncounterManager.Instance?.ResetCombatForNewWave();
                ActionQueueUtility.EnqueueAction(() => GameEvents.TriggerTurnStart(), 0.1f);
                Debug.Log($"[TutorialCombatManager] Combat state reset for new wave at step {stepIndex}.");
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

        /// <summary>
        /// Copies the step's highlights array, resolving any entry with null target + matching
        /// allowedCardIds to the actual card RectTransform currently in hand.
        /// </summary>
        private TutorialHighlightEntry[] ResolveHighlights(TutorialStepData step)
        {
            if (step.highlights == null || step.highlights.Length == 0)
            {
                // Auto-highlight first allowed card if no explicit entries defined
                if (!step.unlockAll && step.allowedCardIds.Length > 0)
                {
                    var cardRect = FindCardInHand(step.allowedCardIds[0]);
                    if (cardRect != null)
                        return new[] { new TutorialHighlightEntry { target = cardRect, direction = ArrowDirection.Down } };
                }
                return System.Array.Empty<TutorialHighlightEntry>();
            }

            var resolved = new TutorialHighlightEntry[step.highlights.Length];
            int cardIdIndex = 0;

            for (int i = 0; i < step.highlights.Length; i++)
            {
                var src = step.highlights[i];
                if (src.target != null)
                {
                    resolved[i] = src;
                    continue;
                }

                // Null target — try to resolve to a card in hand
                if (cardIdIndex < step.allowedCardIds.Length)
                {
                    var cardRect = FindCardInHand(step.allowedCardIds[cardIdIndex++]);
                    resolved[i] = new TutorialHighlightEntry { target = cardRect, direction = src.direction };
                }
                else
                {
                    resolved[i] = src; // remains null → arrow hidden for this entry
                }
            }

            return resolved;
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
