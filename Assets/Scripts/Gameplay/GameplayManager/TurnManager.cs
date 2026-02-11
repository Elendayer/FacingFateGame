using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace facingfate
{
    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance { get; private set; }

        public List<EntityScript> TurnOrder = new List<EntityScript>();

        public EntityScript CurrentTurnEntity => TurnOrder[CurrentTurnIndex];

        public int CurrentTurnIndex = 0;
        public int CurrentRoundIndex = 1;

        private void Awake()
        {
            // Singleton enforcement
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Ensure only one instance exists
            }

            DontDestroyOnLoad(gameObject); // Optional: persist between scenes
            Instance = this;
        }

        public void StartUp()
        {
            AddListeners();
        }
        public void AddListeners()
        {
            GameEvents.OnTurnStart += OnTurnStart;
            GameEvents.OnTurnEnd += OnTurnEnd;
            GameEvents.OnCombatStart += GameEvents_OnCombatStart;
        }

        private void GameEvents_OnCombatStart()
        {
            SetTurnOrder();
            CurrentTurnIndex = 0;
            CurrentRoundIndex = 1;

            GameEvents.TriggerTurnStart();
        }

        private void SetTurnOrder()
        {
            // Find all PlayerCharacter entities
            TurnOrder = FindObjectsByType<EntityScript>(0)
                .OrderByDescending(e => UnityEngine.Random.Range(0, 100))
                .ToList();
        }

        private void OnTurnStart()
        {
            //Trigger Reference Event
            GameEvents.TriggerRefEvent(new ToSendTriggerReference(new() { GameplayRef.onTurnStart }, TurnOrder[CurrentTurnIndex], new() { TurnOrder[CurrentTurnIndex] }));

            // Start the Turn for the Current Entity
            DeckManager.Instance.StartTurn(TurnOrder[CurrentTurnIndex]);
            TurnOrder[CurrentTurnIndex].StartTurn();
        }

        private void OnTurnEnd()
        {
            //Trigger Reference Event
            GameEvents.TriggerRefEvent(new ToSendTriggerReference(new() { GameplayRef.onTurnEnd }, TurnOrder[CurrentTurnIndex], new() { TurnOrder[CurrentTurnIndex] }));

            // End the Turn for the current Entity
            DeckManager.Instance.EndTurn(TurnOrder[CurrentTurnIndex]);

            //Increment Turn Order
            CurrentTurnIndex++;
            if (CurrentTurnIndex >= TurnOrder.Count)
            {
                CurrentTurnIndex = 0;
                CurrentRoundIndex++;
            }

            //Start the next Turn
            ActionQueueUtility.EnqueueAction(new Action(OnTurnStart), 0.5f);
        }
    }
}