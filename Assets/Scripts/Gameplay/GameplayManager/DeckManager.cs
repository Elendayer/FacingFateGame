using System.Collections.Generic;
using UnityEngine;
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

        foreach (EntityScript entity in FindObjectsByType<EntityScript>(0))
        {
            BuildDeckFromIDs(entity);
        }
    }

    void BuildDeckFromIDs(EntityScript entity)
    {
        GameObject cardDock = Instantiate(deckDockPrefab, transform);
        List<GameObject> cardObjs = new();
        cardDock.name = entity.name + " Deck Dock";
        DeckManagement.Add(entity, cardDock.transform);

        foreach (int cardID in entity.deckCardIDs)
        {
            GameObject cardObj = CreateCard(cardID, cardDock.transform, entity);
            if (cardObj != null)
            {
                cardObjs.Add(cardObj);
            }
        }

        foreach (GameObject child in cardObjs)
        {
            child.transform.SetParent(cardDock.transform);
            UtilityScript.ZeroLocalRectTransform(child.transform as RectTransform);
            cardStack.Push(child.gameObject);
        }

        ShuffleDeck();
    }

    private GameObject CreateCard(int id, Transform t, EntityScript entityScript)
    {

        CardData cardData = CreateCardData(id, entityScript);

        if (cardData == null)
        {
            Debug.LogWarning($"Card ID '{id}' could not be created.");
            return null;
        }

        GameObject cardGO = Instantiate(cardPrefab, t);
        cardGO.name = cardData.cardName;

        CardScript cardScript = cardGO.GetComponent<CardScript>();
        if (cardScript != null)
        {
            cardScript.SetHidden();
            cardScript.SetCard(cardData);
        }
        else
        {
            Debug.LogWarning("CardScript component missing on card prefab.");
            Destroy(cardGO);
            return null;
        }

        return cardGO;
    }

    private CardData CreateCardData(int id, EntityScript entity)
    {
        CardData cardData = CardDatabase.GetCardById(id, entity);

        if (cardData == null)
        {
            Debug.LogWarning($"Card ID '{id}' not found in database.");
            return null;
        }
        return cardData;
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

        //Debug.Log($"Drew card: {topCard.name}");
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

    public void MoveOutDeck(EntityScript entity)
    {
        List<GameObject> cards = new();

        foreach (GameObject card in HandManager.Instance.cardsInHand)
        {
            cards.Add(card);
        }
        foreach (GameObject card in discardStack)
        {
            cards.Add(card);
        }
        foreach (Transform child in deckParent)
        {
            cards.Add(child.gameObject);
        }

        foreach (GameObject card in cards)
        {     
            Transform Dock = DeckManagement[card.GetComponent<CardScript>().cardData.Owner];
            card.transform.SetParent(Dock);
        }
    }

    public void MoveInDeck(EntityScript entity)
    {
        Transform Dock = DeckManagement[entity];
        List<Transform> transforms = new();

        foreach (Transform child in Dock)
        {
            transforms.Add(child);
        }

        foreach (Transform t in transforms)
        {
            t.SetParent(deckParent);
            UtilityScript.ZeroLocalRectTransform(t as RectTransform);
            cardStack.Push(t.gameObject);
        }
    }

    public void EndTurn( EntityScript entity)
    {
        MoveOutDeck(entity);
    }
    public void StartTurn(EntityScript entity)
    {
        MoveInDeck(entity);

        for (int i = 0; i < 5; i++)
        {
            DrawTopCard();
        }
    }
}