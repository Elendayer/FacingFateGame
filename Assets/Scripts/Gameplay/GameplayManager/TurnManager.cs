using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public List<EntityScript> TurnOrder = new List<EntityScript>();

    public int CurrentTurnIndex = 0;

    void Start()
    {
        // Singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Ensure only one instance exists
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: persist between scenes

        SetTurnOrder();

        AddListeners();
    }

    public void AddListeners()
    {
        GameEvents.OnTurnStart += OnTurnStart;
    }

    private void OnTurnStart()
    {
        CurrentTurnIndex++;
        CurrentTurnIndex %= TurnOrder.Count;

        DeckManager.Instance.DeckManagement.TryGetValue(TurnOrder[CurrentTurnIndex],out Transform transform);
        DeckManager.Instance.EndTurn(transform);
    }

    private void SetTurnOrder()
    {
        EntityScript[] entities =  FindObjectsByType<EntityScript>(0);

        TurnOrder = entities.OrderByDescending(e => UnityEngine.Random.Range(1, 21)).ToList();
    }
}