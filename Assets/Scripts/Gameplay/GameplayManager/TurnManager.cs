using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public List<EntityScript> TurnOrder = new List<EntityScript>();

    public EntityScript CurrentTurnEntity => TurnOrder[CurrentTurnIndex];

    public int CurrentTurnIndex = 0;
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
        SetTurnOrder();

        AddListeners();
    }
    public void AddListeners()
    {
        GameEvents.OnTurnStart += OnTurnStart;
        GameEvents.OnTurnEnd += OnTurnEnd;
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
        GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onTurnStart }, TurnOrder[CurrentTurnIndex].GetInstanceID()));

        DeckManager.Instance.StartTurn(TurnOrder[CurrentTurnIndex]);
    }
    private void OnTurnEnd()
    {
        GameEvents.TriggerRefEvent(new TriggerRef(new() { GameplayRef.onTurnEnd }, 0, TurnOrder[CurrentTurnIndex].GetInstanceID()));
        DeckManager.Instance.EndTurn(TurnOrder[CurrentTurnIndex]);

        CurrentTurnIndex++;
        CurrentTurnIndex %= TurnOrder.Count;
    }
}