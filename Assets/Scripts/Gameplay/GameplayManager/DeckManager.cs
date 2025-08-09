using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("Deck Configuration")]
    public List<int> deckCardIDs = new List<int>();  // Populate with card IDs
    public GameObject cardPrefab;                          // Prefab with CardDisplay script
    public Transform deckParent;                           // Parent under the deck
    public Transform discardParent;                        // Parent under the deck
    public Button deckDrawButton;

    private Stack<GameObject> cardStack = new Stack<GameObject>();
    private Stack<GameObject> discardStack = new Stack<GameObject>();

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

        CardDatabase.RegisterAll();

        if (deckDrawButton != null)
            deckDrawButton.onClick.AddListener(DrawTopCard);

        BuildDeckFromIDs();
    }

    void BuildDeckFromIDs()
    {
        cardStack.Clear();

        foreach (Transform child in deckParent)
            Destroy(child.gameObject); // Clean up existing cards

        foreach (int id in deckCardIDs)
        {
            CardData cardData = CardDatabase.GetCardById(id, PlayerManager.Instance);
            if (cardData == null)
            {
                Debug.LogWarning($"Card ID '{id}' not found in database.");
                continue;
            }

            GameObject cardGO = Instantiate(cardPrefab, deckParent);
            cardGO.name = cardData.cardName;


            CardScript cardScript = cardGO.GetComponent<CardScript>();
            if (cardScript != null)
            {
                cardScript.SetHidden();
                cardScript.SetCard(cardData);
            }

            cardStack.Push(cardGO);
        }

        Debug.Log($"Deck built with {cardStack.Count} cards.");
    }

    public void DrawTopCard()
    {
        if (cardStack.Count == 0)
        {
            Debug.Log("Deck is empty.");
            ShuffleDiscard();
            return;
        }
        if (HandManager.Instance.handAnchor.childCount > HandManager.Instance.maxHandsize)
        {       
            Debug.Log("Hand is full.");
            return;
        }
        
        GameObject topCard = cardStack.Pop();
        HandManager.Instance.AddCard(topCard);

        CardScript cs = topCard.GetComponent<CardScript>();
        cs.SetRevealed(); // Hide discarded card

        Debug.Log($"Drew card: {topCard.name}");
    }
    public void DiscardCardFromHand(GameObject cardobject)
    {
        if (cardobject == null) return;

        CardScript cs = cardobject.GetComponent<CardScript>();

        UtilityScript.Discard(cs);
 
        discardStack.Push(cardobject);
    }

    public void ShuffleDeck()
    {
        // Convert stack to list for shuffling
        List<GameObject> cards = new List<GameObject>(cardStack);
        cardStack.Clear();

        // Fisher-Yates Shuffle
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            GameObject temp = cards[i];
            cards[i] = cards[j];
            cards[j] = temp;
        }

        // Push back into the stack
        foreach (GameObject card in cards)
        {
            cardStack.Push(card);
        }

        Debug.Log("Deck shuffled.");
    }

    public void ShuffleDiscard()
    {
        if (discardStack.Count == 0)
        {
            Debug.Log("Discard pile is empty. Cannot reshuffle.");
            return;
        }

        List<GameObject> cards = new List<GameObject>(discardStack);
        discardStack.Clear();

        // Fisher-Yates shuffle
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }

        foreach (GameObject card in cards)
        {
            card.transform.SetParent(deckParent);
            UtilityScript.ZeroTransform(card.transform);
            cardStack.Push(card);
        }

        Debug.Log($"Shuffled {cards.Count} discarded cards back into the deck.");
    }
}
