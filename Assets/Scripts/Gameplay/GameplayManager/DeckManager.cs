using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("Deck Configuration")]
    public GameObject cardPrefab;                          // Prefab with CardDisplay script
    public Transform deckParent;                           // Parent under the deck
    public Transform discardParent;                        // Parent under the deck
    public Button deckDrawButton;

    [Header("Deck Management")]
    public GameObject deckDockPrefab;
    public GameObject previousDock;

    public Dictionary<EntityScript,Transform> DeckManagement = new Dictionary<EntityScript,Transform>();

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

        foreach (PlayerCharacterScript p in FindObjectsByType<PlayerCharacterScript>(0))
        {
            BuildDeckFromIDs(p);
        }
        foreach (EnemyScript e in FindObjectsByType<EnemyScript>(0))
        {
            BuildDeckFromIDs(e);
        }
    }

    public void SwapDeck(Transform DockDeck)
    {
        // Cache children
        List<Transform> DockChildren = new List<Transform>();
        List<Transform> StackChildren = new List<Transform>();

        cardStack.Clear();

        foreach (Transform child in DockDeck)
            DockChildren.Add(child);

        foreach (Transform child in deckParent)
            StackChildren.Add(child);


        // Move B's children to A
        foreach (Transform child in StackChildren)
        {
            child.SetParent(previousDock.transform);
        }

        // Move A's children to B
        foreach (Transform child in DockChildren)
        {
            child.SetParent(deckParent);
            UtilityScript.ZeroLocalRectTransform(child as RectTransform);

            cardStack.Push(child.gameObject);
        }

        previousDock = DockDeck.gameObject;
    }

    void BuildDeckFromIDs(PlayerCharacterScript p)
    {
        GameObject cardDock = Instantiate(deckDockPrefab, transform);
        DeckManagement.Add(p, cardDock.transform);

        foreach (int id in p.deckCardIDs)
        {
            CardData cardData = CardDatabase.GetCardById(id,p);
            if (cardData == null)
            {
                Debug.LogWarning($"Card ID '{id}' not found in database.");
                continue;
            }
            GameObject cardGO = Instantiate(cardPrefab, parent:cardDock.transform);
            cardGO.name = cardData.cardName;

            CardScript cardScript = cardGO.GetComponent<CardScript>();
            if (cardScript != null)
            {
                cardScript.SetHidden();
                cardScript.SetCard(cardData);
            }
        }
    }

    void BuildDeckFromIDs(EnemyScript e)
    {
        GameObject cardDock = Instantiate(deckDockPrefab, transform);
        DeckManagement.Add(e, cardDock.transform);

        foreach (int id in e.enemyAI.EnemyCardsByID)
        {
            CardData cardData = CardDatabase.GetCardById(id, e);
            if (cardData == null)
            {
                Debug.LogWarning($"Card ID '{id}' not found in database.");
                continue;
            }
            GameObject cardGO = Instantiate(cardPrefab, parent: cardDock.transform);
            cardGO.name = cardData.cardName;

            CardScript cardScript = cardGO.GetComponent<CardScript>();
            if (cardScript != null)
            {
                cardScript.SetHidden();
                cardScript.SetCard(cardData);
            }
        }
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

    public void EndTurn( Transform transform)
    {
        foreach (GameObject card in HandManager.Instance.cardsInHand)
        {
          card.transform.SetParent(deckParent);
        }
        foreach(GameObject card in discardStack)
        {
            card.transform.SetParent(deckParent);
        }

        SwapDeck(transform);
    }
}