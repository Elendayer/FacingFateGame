using System;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace facingfate
{
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

        public Dictionary<EntityScript, Transform> DeckManagement = new Dictionary<EntityScript, Transform>();

        public Stack<GameObject> cardStack = new Stack<GameObject>();
        public Stack<GameObject> discardStack = new Stack<GameObject>();

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
        }
        public void StartUp()
        {
            if (deckDrawButton != null)
                deckDrawButton.onClick.AddListener(Player_DrawTopCard);
        }

        public void BuildDeckFromIDs(EntityScript entity)
        {
            if (entity is PlayerScript)
            {
                List<GameObject> cardObjs = new();

                GameObject cardDock = Instantiate(deckDockPrefab, transform);
                cardDock.name = entity.name + "_Dock";

                GameObject dockDeck = Instantiate(deckDockPrefab, cardDock.transform);
                dockDeck.name = entity.name + "_Deck";

                GameObject dockDiscard = Instantiate(deckDockPrefab, cardDock.transform);
                dockDiscard.name = entity.name + "_Discard";

                DeckManagement.Add(entity, dockDeck.transform);

                foreach (string cardID in entity.deckCardIDs)
                {
                    GameObject cardObj = CreateCard(cardID, dockDeck.transform, entity);
                    if (cardObj != null)
                    {
                        cardObjs.Add(cardObj);
                    }
                }

                foreach (GameObject child in cardObjs)
                {
                    child.transform.SetParent(dockDeck.transform);
                    TransformUtility.ZeroLocalRectTransform(child.transform as RectTransform);
                    cardStack.Push(child.gameObject);
                }

                Player_ShuffleDeck();
            }
            // For non-player entities
            if (entity is NonPlayerScript nonPlayer)
            {
                List<GameObject> cardObjs = new();

                GameObject cardDock = Instantiate(deckDockPrefab, transform);
                cardDock.name = entity.name + "_Dock";

                GameObject dockDeck = Instantiate(deckDockPrefab, cardDock.transform);
                dockDeck.name = entity.name + "_Deck";
                nonPlayer.npcAIController.deck = dockDeck.transform;

                GameObject dockDiscard = Instantiate(deckDockPrefab, cardDock.transform);
                dockDiscard.name = entity.name + "_Discard";
                nonPlayer.npcAIController.discard = dockDiscard.transform;


                DeckManagement.Add(entity, dockDeck.transform);

                foreach (string cardID in entity.deckCardIDs)
                {
                    GameObject cardObj = CreateCard(cardID, dockDeck.transform, entity);
                    if (cardObj != null)
                    {
                        cardObjs.Add(cardObj);
                    }
                }

                foreach (GameObject child in cardObjs)
                {
                    child.transform.SetParent(dockDeck.transform);
                    TransformUtility.ZeroLocalRectTransform(child.transform as RectTransform);
                }
            }
        }

        private GameObject CreateCard(string id, Transform t, EntityScript entityScript)
        {
            CardData cardData = CreateCardData(id, entityScript);

            if (cardData == null)
            {
                Debug.LogWarning($"[DeckManager] Card ID '{id}' could not be created.");
                return null;
            }

            GameObject cardGO = Instantiate(cardPrefab, t);
            cardGO.name = cardData.cardName;

            CardScript cardScript = cardGO.GetComponent<CardScript>();
            if (cardScript != null)
            {
                cardScript.SetHidden();
                cardScript.cardData = cardData;
            }
            else
            {
                Debug.LogWarning("[DeckManager] CardScript component missing on card prefab.");
                Destroy(cardGO);
                return null;
            }

            return cardGO;
        }

        private CardData CreateCardData(string id, EntityScript entity)
        {
            CardData cardData = CardDatabase.GetCardById(id, entity);

            if (cardData == null)
            {
                Debug.LogWarning($"[DeckManager] Card ID '{id}' not found in database.");
                return null;
            }
            return cardData;
        }

        #region player Deck interactions
        public void Player_DrawTopCard()
        {
            if (cardStack.Count == 0)
            {
                Debug.Log("[DeckManager] Deck is empty.");
                Player_ShuffleDiscard();
                return;
            }
            if (HandManager.Instance.cardsInHand.Count >= HandManager.Instance.maxHandsize)
            {
                Debug.Log("DeckManager] Hand is full.");
                return;
            }

            GameObject topCard = cardStack.Pop();
            HandManager.Instance.AddCard(topCard);

            CardScript cs = topCard.GetComponent<CardScript>();
            cs.SetRevealed();

            deckParent?.GetComponent<DiscardPileVisualizer>()?.Refresh();

            GameEvents.TriggerRefEvent(new ToSendTriggerReference(new() { GameplayRef.onCardDrawn }, null, null));
            //Debug.Log($"Drew card: {topCard.name}");
        }
        public void Player_DiscardCardFromHand(GameObject cardobject)
        {
            if (cardobject == null) return;

            // Kill all running DOTween animations before re-parenting.
            // Hand layout tweens (DOAnchorPos / DOLocalRotate) would otherwise
            // keep running relative to the new discardParent and corrupt position.
            RectTransform cardRect = cardobject.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                cardRect.DOKill();
                cardRect.localScale = Vector3.one;
            }
            cardobject.GetComponent<CanvasGroup>()?.DOKill();

            HandManager.Instance.RemoveCard(cardobject);

            CardScript cs = cardobject.GetComponent<CardScript>();
            // HandUtility.Discard: SetParent(discardParent), SetHidden, ResetCard,
            //                      discardStack.Push (guarded – no duplicate needed here)
            HandUtility.Discard(cs);

            // Zero local transform AFTER HandUtility re-parented the card
            if (cardRect != null)
                TransformUtility.ZeroLocalRectTransform(cardRect);

            // NOTE: discardStack.Push removed – HandUtility.Discard already handles it.

            // Rebuild the stacked pile visuals
            discardParent?.GetComponent<DiscardPileVisualizer>()?.Refresh();

            GameEvents.TriggerRefEvent(new ToSendTriggerReference(new() { GameplayRef.onCardPlayed }, null, null));
        }

        public void Player_ShuffleDeck()
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

            Debug.Log("[DeckManager] Deck shuffled.");
        }

        public void Player_ShuffleDiscard()
        {
            if (discardStack.Count == 0)
            {
                Debug.Log("[DeckManager] Discard pile is empty. Cannot reshuffle.");
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
                cardStack.Push(card);
            }

            Debug.Log($"[DeckManager] Shuffled {cards.Count} discarded cards back into the deck.");
        }

        public void Player_MoveOutDeck(EntityScript entity)
        {
            Transform dock = DeckManagement[entity].parent;
            Transform dockDeck = DeckManagement[entity];
            Transform dockDiscard = dock.Find(entity.name + "_Discard");

            List<GameObject> deckCards = new();
            List<GameObject> discardCards = new();

            // Remaining deck cards move to the Deck child
            foreach (GameObject card in cardStack)
            {
                deckCards.Add(card);
            }

            // Hand cards move to the Discard child
            foreach (GameObject card in HandManager.Instance.cardsInHand)
            {
                discardCards.Add(card);
            }

            // Discard stack cards move to the Discard child
            foreach (GameObject card in discardStack)
            {
                discardCards.Add(card);
            }

            foreach (GameObject card in deckCards)
            {
                card.transform.SetParent(dockDeck);
                TransformUtility.ZeroLocalRectTransform(card.transform as RectTransform);
            }

            foreach (GameObject card in discardCards)
            {
                card.transform.SetParent(dockDiscard);
                TransformUtility.ZeroLocalRectTransform(card.transform as RectTransform);
            }

            // Clear the stacks when moving out to Dock
            cardStack.Clear();
            discardStack.Clear();
        }

        public void Player_MoveInDeck(EntityScript entity)
        {
            cardStack.Clear();
            discardStack.Clear();

            Transform dockDeck = DeckManagement[entity];
            Transform dockDiscard = dockDeck.parent.Find(entity.name + "_Discard");

            Debug.Log($"[DeckManager] Moving cards into deck of {entity.name}");

            // Get all cards from the Deck child
            List<CardScript> deckCards = dockDeck.GetComponentsInChildren<CardScript>()
                .Where(c => c.cardData != null && c.cardData.Owner == entity)
                .ToList();

            // Get all cards from the Discard child
            List<CardScript> discardCards = dockDiscard.GetComponentsInChildren<CardScript>()
                .Where(c => c.cardData != null && c.cardData.Owner == entity)
                .ToList();

            // Move Deck cards to cardStack
            foreach (CardScript c in deckCards)
            {
                c.cardData.Owner = entity;
                RectTransform ct = c.GetComponent<RectTransform>();
                ct.SetParent(deckParent);
                TransformUtility.ZeroLocalRectTransform(ct);
                cardStack.Push(ct.gameObject);
            }

            // Move Discard cards to discardStack
            foreach (CardScript c in discardCards)
            {
                c.cardData.Owner = entity;
                RectTransform ct = c.GetComponent<RectTransform>();
                discardStack.Push(ct.gameObject);
            }
        }

        public void StartTurn(EntityScript entity)
        {
            // Only for player for now
            if (entity.GetType() == typeof(PlayerScript))
            {
                if (entity.GetType() == typeof(PlayerScript))
                {
                    Player_MoveInDeck(entity);

                    for (int i = 0; i < 5; i++)
                    {
                        Player_DrawTopCard();
                    }
                }
                else
                {
                    Player_DrawTopCard();
                }
            }
        }
        public void EndTurn(EntityScript entity)
        {
            if (entity.GetType() == typeof(PlayerScript))
            {
                // Move any remaining cards in hand to discard
                HandManager.Instance.DiscardAllInHand();

                // Move all discarded cards back into the deck
                Player_ShuffleDiscard();

                // Move out the deck for storage
                Player_MoveOutDeck(entity);
            }
        }
        #endregion


    }
}