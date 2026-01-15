using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
            .OrderByDescending(e => UnityEngine.Random.Range(1, 21))
            .ToList();
    }

    private void OnTurnStart()
    {
        GameEvents.TriggerRefEvent(new ToSendTriggerReference(new() { GameplayRef.onTurnStart }, TurnOrder[CurrentTurnIndex], new() {TurnOrder[CurrentTurnIndex] }));

        DeckManager.Instance.StartTurn(TurnOrder[CurrentTurnIndex]);
    }
    private void OnTurnEnd()
    {
        GameEvents.TriggerRefEvent(new ToSendTriggerReference(new() { GameplayRef.onTurnEnd }, TurnOrder[CurrentTurnIndex], new() { TurnOrder[CurrentTurnIndex] }));
        DeckManager.Instance.EndTurn(TurnOrder[CurrentTurnIndex]);

        CurrentTurnIndex++;
        if (CurrentTurnIndex >= TurnOrder.Count)
        {
            CurrentTurnIndex = 0;
            CurrentRoundIndex++;
        }
    }
}