using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace facingfate
{
    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance { get; private set; }

        [HideInInspector]
        public List<EntityScript> TurnOrder = new List<EntityScript>();

        public EntityScript CurrentTurnEntity =>
            (TurnOrder != null && CurrentTurnIndex < TurnOrder.Count)
            ? TurnOrder[CurrentTurnIndex]
            : null;

        public int CurrentTurnIndex = 0;
        public int CurrentRoundIndex = 1;

        // Combat state tracking
        private bool combatEnded = false;
        private bool listenersAdded = false;

        private void Awake()
        {
            // Singleton enforcement
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Ensure only one instance exists
                return;
            }

            DontDestroyOnLoad(gameObject); // Optional: persist between scenes
            Instance = this;
        }

        public void StartUp()
        {
            combatEnded = false;
            CurrentTurnIndex = 0;
            CurrentRoundIndex = 1;

            AddListeners();
        }

        /// <summary>
        /// Builds the turn order sorted by Dexterity descending.
        /// Must be called AFTER all entities have had StartUp() called so stats are initialized.
        /// </summary>
        public void BuildTurnOrder()
        {
            SetTurnOrder();
        }

        public void AddListeners()
        {
            if (listenersAdded) return;
            listenersAdded = true;

            GameEvents.OnTurnStart += OnTurnStart;
            GameEvents.OnTurnEnd += OnTurnEnd;
            GameEvents.OnCombatStart += OnCombatStart;
            GameEvents.OnCombatEnd += OnCombatEnd;

        }

        private void OnDestroy()
        {
            GameEvents.OnTurnStart -= OnTurnStart;
            GameEvents.OnTurnEnd -= OnTurnEnd;
            GameEvents.OnCombatStart -= OnCombatStart;
            GameEvents.OnCombatEnd -= OnCombatEnd;
        }

        private void OnCombatEnd(bool playerWon)
        {
            combatEnded = true;
            TurnOrder.Clear();
            CurrentTurnIndex = 0;
            CurrentRoundIndex = 1;
        }

        private void OnCombatStart()
        {
            // 1-second pause before first turn — gives the turn-order UI time to settle.
            ActionQueueUtility.EnqueueAction(GameEvents.TriggerTurnStart, 1f);
        }

        public bool CombatEnded => combatEnded;

        private void SetTurnOrder()
        {
            // Filter by e.enabled: HandleEntityDeath disables the EntityScript component on death,
            // safely excluding dead entities without relying on entityStats being initialized yet.
            TurnOrder = FindObjectsByType<EntityScript>(0)
                .Where(e => e.enabled)
                .OrderByDescending(e => e.entityStats?.CurrentDexterity ?? 0)
                .ToList();
        }

        public void ResetCombatForNewWave()
        {
            combatEnded = false;
            SetTurnOrder();
            CurrentTurnIndex = 0;
        }

        private void OnTurnStart()
        {
            if (combatEnded || TurnOrder.Count == 0) return;

            // Skip dead or disabled entities — they may still be in TurnOrder if RemoveTurn
            // hasn't fired yet (e.g. death processed mid-queue, turn already scheduled).
            var entity = TurnOrder[CurrentTurnIndex];
            if (entity == null || !entity.enabled || !entity.gameObject.activeInHierarchy
                || (entity.entityStats != null && entity.entityStats.CurrentHealth <= 0))
            {
                Debug.Log($"[TurnManager] Skipping turn for dead/inactive entity '{entity?.name}'. Advancing.");
                TurnOrder.RemoveAt(CurrentTurnIndex);
                if (CurrentTurnIndex >= TurnOrder.Count) CurrentTurnIndex = 0;
                if (TurnOrder.Count > 0)
                    ActionQueueUtility.EnqueueAction(GameEvents.TriggerTurnStart, 0.1f);
                return;
            }

            //Trigger Reference Event — may enqueue DoT/poison actions that kill the entity
            GameEvents.TriggerRefEvent(new ToSendTriggerReference(new() { GameplayRef.onTurnStart }, TurnOrder[CurrentTurnIndex], new() { TurnOrder[CurrentTurnIndex] }));
            GameEvents.TriggerTurnEntityChanged(TurnOrder[CurrentTurnIndex]);

            if (TurnOrder[CurrentTurnIndex].GetComponent<PlayerScript>() != null)
                GameEvents.TriggerActivePlayerChanged(TurnOrder[CurrentTurnIndex]);

            // Defer StartTurn into the action queue so all turn-start effects (DoT etc.) resolve first.
            // If the entity died from those effects, skip its turn rather than starting it dead.
            ActionQueueUtility.EnqueueAction(() =>
            {
                if (combatEnded || TurnOrder.Count == 0) return;

                var entity = CurrentTurnIndex < TurnOrder.Count ? TurnOrder[CurrentTurnIndex] : null;
                if (entity == null || !entity.enabled || !entity.gameObject.activeInHierarchy
                    || (entity.entityStats != null && entity.entityStats.CurrentHealth <= 0))
                {
                    if (entity != null) TurnOrder.Remove(entity);
                    if (CurrentTurnIndex >= TurnOrder.Count) CurrentTurnIndex = 0;
                    if (TurnOrder.Count > 0)
                        ActionQueueUtility.EnqueueAction(GameEvents.TriggerTurnStart, 0.1f);
                    return;
                }

                var startTurnCoroutine = DeckManager.Instance.StartTurn(entity);
                if (startTurnCoroutine != null)
                    StartCoroutine(WaitForTurnStartThenContinue(entity, startTurnCoroutine));
                else
                    entity.StartTurn();
            });
        }

        private IEnumerator WaitForTurnStartThenContinue(EntityScript entity, Coroutine deckCoroutine)
        {
            yield return deckCoroutine;
            entity.StartTurn();
        }

        private void OnTurnEnd()
        {
            if (combatEnded || TurnOrder.Count == 0 || CurrentTurnIndex >= TurnOrder.Count) return;

            TurnOrder[CurrentTurnIndex].EndTurn();

            //Trigger Reference Event
            GameEvents.TriggerRefEvent(new ToSendTriggerReference(new() { GameplayRef.onTurnEnd }, TurnOrder[CurrentTurnIndex], new() { TurnOrder[CurrentTurnIndex] }));


            TurnOrder[CurrentTurnIndex].EntityVisual.ClearHighlightEndTurn();

            // End the Turn for the current Entity
            var endTurnCoroutine = DeckManager.Instance.EndTurn(TurnOrder[CurrentTurnIndex]);
            if (endTurnCoroutine != null)
            {
                StartCoroutine(WaitForTurnEndThenContinue(endTurnCoroutine));
            }
            else
            {
                ProgressToNextTurn();
            }
        }

        private IEnumerator WaitForTurnEndThenContinue(Coroutine deckCoroutine)
        {
            yield return deckCoroutine;
            ProgressToNextTurn();
        }

        private void ProgressToNextTurn()
        {
            //Increment Turn Order
            CurrentTurnIndex++;
            if (CurrentTurnIndex >= TurnOrder.Count)
            {
                CurrentTurnIndex = 0;
                CurrentRoundIndex++;
            }

            //Start the next Turn
            //ActionQueueUtility.EnqueueAction(new Action(OnTurnStart), 0.5f);
            ActionQueueUtility.EnqueueAction(GameEvents.TriggerTurnStart, 0.5f);
        }
        public void AddTurn(EntityScript entityScript)
        {
            if (entityScript == null || TurnOrder.Contains(entityScript))
                return;

            // Insert in descending order of dexterity
            int insertIndex = TurnOrder.FindIndex(e => e.entityStats.CurrentDexterity < entityScript.entityStats.CurrentDexterity);

            if (insertIndex < 0)
            {
                // Add at the end if no entity has lower dexterity
                TurnOrder.Add(entityScript);
            }
            else
            {
                // Adjust CurrentTurnIndex if inserting before or at current position
                if (insertIndex <= CurrentTurnIndex)
                    CurrentTurnIndex++;

                TurnOrder.Insert(insertIndex, entityScript);
            }
        }

        public void RemoveTurn(EntityScript entityScript)
        {
            if (entityScript == null)
                return;

            int indexToRemove = TurnOrder.IndexOf(entityScript);

            if (indexToRemove < 0)
                return; // Entity not in turn order

            TurnOrder.RemoveAt(indexToRemove);

            // Adjust CurrentTurnIndex if needed
            if (indexToRemove < CurrentTurnIndex)
            {
                // Entity was before current — shift index back so current entity is unchanged
                CurrentTurnIndex--;
            }
            else if (indexToRemove == CurrentTurnIndex)
            {
                // The active entity was removed. After RemoveAt, CurrentTurnIndex either points
                // to the next entity (correct) or is out-of-range (wrap to 0).
                if (CurrentTurnIndex >= TurnOrder.Count)
                    CurrentTurnIndex = 0;
            }
            // indexToRemove > CurrentTurnIndex: entity was after current — no adjustment needed
        }
    }
}