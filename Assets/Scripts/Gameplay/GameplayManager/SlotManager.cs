using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SlotManager : MonoBehaviour
{
    public static SlotManager Instance { get; private set; }

    public List<CardSlot> slots = new List<CardSlot>();  // List to hold all the slots
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].slotIndex = i;
        }

        List<CardSlotDivide> divides = FindObjectsByType<CardSlotDivide>(FindObjectsSortMode.InstanceID).ToList();
        divides.Reverse();

        for (int i = 0; i < divides.Count; i++)
        {
            divides[i].divideIndex = i;
        }
    }
    private void OnEnable()
    {
        // Subscribe to the event when the slider value changes and triggers the event
        GameEvents.OnTurnStart += OnTurnStart;
    }

    private void OnDisable()
    {
        // Unsubscribe from the event when this object is disabled or destroyed to avoid memory leaks
        GameEvents.OnTurnStart -= OnTurnStart;
    }

    // This method is called when the event is triggered by the slider
    void OnTurnStart()
    {
        // Call logic to handle the event, like moving cards up or using cards
        HandleCardSlotUpdate();
    }

    // This is the method that handles updating the card slots
    void HandleCardSlotUpdate()
    {
        if (slots.Count == 0) return;

        // Use the card in the first slot
        UseCard(slots[0]);

        // Move all cards in the following slots up one slot (including the slot parent and child card)
        MoveCardsUp();
    }

    private void UseCard(CardSlot slot)
    {
        slot.cardScript = slot.GetComponentInChildren<CardScript>();

        if (slot.cardScript != null)
        {

            slot.cardScript.cardData.ActivateCard();

            UtilityScript.Discard(slot.cardScript);
            slot.cardScript = null;
        }
    }

    private void MoveCardsUp()
    {
        for (int i = 1; i < slots.Count; i++)
        {
            if (slots[i].cardScript != null)
            {
                slots[i-1].AttachCardToSlot(slots[i].cardScript.gameObject);
                slots[i].ResetSlot();
            }
        }
    }
}
