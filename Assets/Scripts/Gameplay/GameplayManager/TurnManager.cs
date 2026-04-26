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
            SetTurnOrder();
            CurrentTurnIndex = 0;
            CurrentRoundIndex = 1;

            AddListeners();
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
            GameEvents.TriggerTurnStart();
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

            //Trigger Reference Event
            GameEvents.TriggerRefEvent(new ToSendTriggerReference(new() { GameplayRef.onTurnStart }, TurnOrder[CurrentTurnIndex], new() { TurnOrder[CurrentTurnIndex] }));
            GameEvents.TriggerTurnEntityChanged(TurnOrder[CurrentTurnIndex]);

            if (TurnOrder[CurrentTurnIndex].GetComponent<PlayerScript>() != null)
                GameEvents.TriggerActivePlayerChanged(TurnOrder[CurrentTurnIndex]);

            // Start the Turn for the Current Entity - use coroutine for player, direct call for others
            var startTurnCoroutine = DeckManager.Instance.StartTurn(TurnOrder[CurrentTurnIndex]);
            if (startTurnCoroutine != null)
            {
                StartCoroutine(WaitForTurnStartThenContinue(TurnOrder[CurrentTurnIndex], startTurnCoroutine));
            }
            else
            {
                TurnOrder[CurrentTurnIndex].StartTurn();
            }
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
                // Entity was before current, shift index back
                CurrentTurnIndex--;
            }
            else if (indexToRemove == CurrentTurnIndex && CurrentTurnIndex >= TurnOrder.Count && TurnOrder.Count > 0)
            {
                // We removed the current entity and it was the last one, wrap to beginning
                CurrentTurnIndex = 0;
            }
        }
    }
}