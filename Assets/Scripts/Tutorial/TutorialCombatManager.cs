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

        private Coroutine _lockRoutine;
        // Cached so OnTurnEnd knows whose turn just ended, independent of TurnManager index timing.
        private EntityScript _currentTurnEntity;

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

        // Exposed so DraggableCharacter can read the current step for movement locking.
        public TutorialStepData CurrentStep =>
            (_isActive && _currentStepIndex < steps.Length) ? steps[_currentStepIndex] : null;

        private void ActivateStep(int index)
        {
            if (index >= steps.Length) return;

            var step = steps[index];

            SpawnWaveForStep(index);

            // Fallback: SpawnWaveForStep only resets combat state when a wave spawns.
            // If the next step has no wave (e.g. MovedToTarget after EnemyDead), combatEnded
            // stays true — EncounterManager won't detect future deaths and player turn flags break.
            // Reset state only (no TriggerTurnStart — player turn is already mid-flight).
            if (TurnManager.Instance != null && TurnManager.Instance.CombatEnded)
            {
                ResetCombatKeepPlayerTurn();
                Debug.Log($"[TutorialCombatManager] No wave for step {index} — combat state reset, player turn continues.");
            }

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

            if (_lockRoutine != null) { StopCoroutine(_lockRoutine); _lockRoutine = null; }
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
            // Always cache before the guard — needed by OnTurnEnd even when tutorial is not yet active.
            _currentTurnEntity = TurnManager.Instance?.CurrentTurnEntity;

            if (!_isActive || _currentStepIndex >= steps.Length) return;
            if (_lockRoutine != null) StopCoroutine(_lockRoutine);
            _lockRoutine = StartCoroutine(LockHandNextFrame());
        }

        private IEnumerator LockHandNextFrame()
        {
            yield return null; // wait one frame so DeckManager draws cards first
            _lockRoutine = null;
            if (_isActive && _currentStepIndex < steps.Length)
                LockHandForStep(steps[_currentStepIndex]);
        }

        private void OnTurnEnd()
        {
            if (!_isActive || _currentStepIndex >= steps.Length) return;
            if (steps[_currentStepIndex].condition != CompletionCondition.EndTurnPressed) return;

            // Use the entity cached at TurnStart — CurrentTurnEntity in TurnManager may already
            // point to the NEXT entity by the time this handler fires (TurnManager subscribed first).
            if (_currentTurnEntity == null || _currentTurnEntity.GetComponent<PlayerScript>() == null) return;

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
                    if (entry.healthOverride > 0f)
                        spawned.entityStats.CurrentHealth = entry.healthOverride;
                    anySpawned = true;
                    Debug.Log($"[TutorialCombatManager] Spawned {spawned.name} at step {stepIndex}" +
                              (entry.healthOverride > 0f ? $" (health overridden to {entry.healthOverride})" : ""));
                }
            }

            // When a new wave spawns after the previous combat ended, the TurnManager and
            // EncounterManager have both set combatEnded = true and cleared TurnOrder.
            // Reset them so the new enemies can take turns and deaths are detected again.
            // ResetCombatKeepPlayerTurn ensures player remains the active turn entity.
            if (anySpawned && TurnManager.Instance != null && TurnManager.Instance.CombatEnded)
            {
                ResetCombatKeepPlayerTurn();
                ActionQueueUtility.EnqueueAction(() => GameEvents.TriggerTurnStart(), 0.1f);
                Debug.Log($"[TutorialCombatManager] Combat state reset for new wave at step {stepIndex}.");
            }
        }

        /// <summary>
        /// Resets TurnManager + EncounterManager for a new wave, then restores
        /// CurrentTurnIndex to the player so their turn continues uninterrupted.
        /// ResetCombatForNewWave rebuilds TurnOrder sorted by Dexterity and sets
        /// index to 0 — the highest-Dex entity (often an enemy) would go first without this fix.
        /// </summary>
        private void ResetCombatKeepPlayerTurn()
        {
            TurnManager.Instance.ResetCombatForNewWave();
            EncounterManager.Instance?.ResetCombatForNewWave();

            var tm = TurnManager.Instance;
            if (tm.TurnOrder.Count > 0)
            {
                int playerIdx = tm.TurnOrder.FindIndex(e => e.GetComponent<PlayerScript>() != null);
                if (playerIdx >= 0)
                    tm.CurrentTurnIndex = playerIdx;
            }
        }

        // ── ActionLock helpers ─────────────────────────────────────────────────

        /// <summary>Called by HandManager.AddCard so every card drawn mid-tutorial is locked immediately.</summary>
        public void ApplyLockToCard(GameObject cardGO)
        {
            if (!_isActive || _currentStepIndex >= steps.Length) return;
            ApplyLock(steps[_currentStepIndex], cardGO);
        }

        private void LockHandForStep(TutorialStepData step)
        {
            if (HandManager.Instance == null) return;
            var cards = new List<GameObject>(HandManager.Instance.cardsInHand);
            foreach (var cardGO in cards)
                ApplyLock(step, cardGO);
            // NOTE: do NOT call HandUI.RefreshHandLocks here.
            // LockHandForStep fires before entity.StartTurn() resets stamina — calling
            // RefreshHandLocks with stale (0) stamina would lock all cards even allowed ones.
            // Stamina-based dimming is handled by:
            //   • HandManager.AddCard  → ApplyStaminaLockToCard (per-card, correct timing)
            //   • PlayerScript.StartTurn → RefreshHandLocks (full hand, after stamina reset)
            //   • CardData.PlayCard     → RefreshHandLocks (after each spend)
        }

        private static void ApplyLock(TutorialStepData step, GameObject cardGO)
        {
            if (cardGO == null) return;
            var cs = cardGO.GetComponent<CardScript>();
            if (cs == null) return;

            bool allowed = step.unlockAll
                || System.Array.IndexOf(step.allowedCardIds, cs.cardData.cardID) >= 0;

            cs.SetupLock(!allowed);

            var cg = cardGO.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = allowed ? 1f : 0.4f;
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
        /// Returns the step's highlights array as-is. Only world-space (worldTarget) and
        /// Inspector-assigned (target) entries produce arrows — card-in-hand arrows removed.
        /// </summary>
        private TutorialHighlightEntry[] ResolveHighlights(TutorialStepData step)
        {
            if (step.highlights == null || step.highlights.Length == 0)
                return System.Array.Empty<TutorialHighlightEntry>();

            return step.highlights;
        }
    }
}
