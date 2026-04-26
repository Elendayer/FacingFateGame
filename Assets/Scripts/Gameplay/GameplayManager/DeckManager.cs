using System;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;

namespace facingfate
{
    public class DeckManager : MonoBehaviour
    {
        /// <summary>
        /// Stores both Deck and Discard transforms for an entity's deck storage.
        /// </summary>
        private struct EntityDeckStorage
        {
            public Transform Deck { get; set; }
            public Transform Discard { get; set; }

            public EntityDeckStorage(Transform deck, Transform discard)
            {
                Deck = deck;
                Discard = discard;
            }
        }

        /// <summary>
        /// Stores the saved order of deck and discard cards for each entity.
        /// </summary>
        private struct DeckOrderData
        {
            public List<GameObject> DeckOrder { get; set; }
            public List<GameObject> DiscardOrder { get; set; }

            public DeckOrderData(List<GameObject> deckOrder, List<GameObject> discardOrder)
            {
                DeckOrder = deckOrder ?? new List<GameObject>();
                DiscardOrder = discardOrder ?? new List<GameObject>();
            }
        }

        public static DeckManager Instance { get; private set; }

        [Header("Deck Configuration")]
        public GameObject cardPrefab;                          // Prefab with CardDisplay script
        public RectTransform deckParent;                           // Parent under the deck
        public RectTransform discardParent;                        // Parent under the deck
        public Button deckDrawButton;

        [Header("Deck Management")]
        public GameObject deckDockPrefab;

        private Dictionary<EntityScript, EntityDeckStorage> DeckManagement = new Dictionary<EntityScript, EntityDeckStorage>();
        private Dictionary<EntityScript, DeckOrderData> DeckOrderManagement = new Dictionary<EntityScript, DeckOrderData>();

        public Stack<GameObject> cardStack = new Stack<GameObject>();
        public Stack<GameObject> discardStack = new Stack<GameObject>();

        private bool listenersAdded = false;

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

            cardStack.Clear();
            discardStack.Clear();
        }

        public void AddListeners()
        {
            if (listenersAdded) return;
            listenersAdded = true;

            GameEvents.OnCombatEnd += OnCombatEnd;
        }

        private void OnDestroy()
        {
            GameEvents.OnCombatEnd -= OnCombatEnd;
        }


        private void OnCombatEnd(bool playerWon)
        {
            // Tutorial wave transitions fire CombatEnd between waves — don't clear deck.
            if (TutorialCombatManager.Instance != null && TutorialCombatManager.Instance.IsActive)
                return;

            // Clear all decks and discards at combat end
            cardStack.Clear();
            discardStack.Clear();

            // Optionally, also clear the visual elements in the deck and discard parents
            foreach (Transform child in deckParent)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in discardParent)
            {
                Destroy(child.gameObject);
            }

            // Clear deck management storage
            DeckManagement.Clear();
        }

        /// <summary>
        /// Removes destroyed (null) GameObjects from the card stacks.
        /// Call this if you suspect destroyed references have accumulated in the stacks.
        /// </summary>
        public void CleanupDestroyedReferences()
        {
            // Clean up cardStack
            var validCards = new Stack<GameObject>();
            foreach (var card in cardStack)
            {
                if (card != null)
                    validCards.Push(card);
            }
            cardStack.Clear();
            foreach (var card in validCards)
                cardStack.Push(card);

            // Clean up discardStack
            var validDiscards = new Stack<GameObject>();
            foreach (var card in discardStack)
            {
                if (card != null)
                    validDiscards.Push(card);
            }
            discardStack.Clear();
            foreach (var card in validDiscards)
                discardStack.Push(card);
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

                DeckManagement.Add(entity, new EntityDeckStorage(dockDeck.transform, dockDiscard.transform));

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

                // Save the shuffled deck order for this entity
                SaveDeckOrder(entity);
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

                DeckManagement.Add(entity, new EntityDeckStorage(dockDeck.transform, dockDiscard.transform));

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

        /// <summary>
        /// Helper method to clean up animations on a card before re-parenting.
        /// Ensures that DOTween animations don't corrupt the card's position when moved to discard.
        /// </summary>
        private void CleanupCardAnimations(GameObject cardObject)
        {
            if (cardObject == null) return;

            RectTransform cardRect = cardObject.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                cardRect.DOKill();
                cardRect.localScale = Vector3.one;
            }

            CanvasGroup canvasGroup = cardObject.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                canvasGroup.DOKill();
        }

        /// <summary>
        /// Internal method to discard a card from hand with cleanup and event triggering.
        /// </summary>
        private void DiscardCardInternal(GameObject cardObject, GameplayRef eventRef)
        {
            if (cardObject == null) return;

            // Clean up animations before moving card
            CleanupCardAnimations(cardObject);

            // Remove from hand
            HandManager.Instance.RemoveCard(cardObject);

            // Discard the card
            CardScript cs = cardObject.GetComponent<CardScript>();
            HandUtility.Discard(cs);

            // Update visuals and trigger event
            RefreshDiscardVisuals();
            GameEvents.TriggerRefEvent(new ToSendTriggerReference(new() { eventRef }, null, null));
        }

        /// <summary>
        /// Refreshes the discard pile visuals.
        /// </summary>
        private void RefreshDiscardVisuals()
        {
            discardParent?.GetComponent<DiscardPileVisualizer>()?.Refresh();
        }

        #region Player Deck interactions
        public void Player_DrawTopCard()
        {
            HandUtility.Draw(GameplayRef.onCardDrawn);
        }

        public void Player_DiscardRandomCardFromHand()
        {
            if (HandManager.Instance.cardsInHand.Count == 0) return;

            GameObject cardObject = HandManager.Instance.cardsInHand[UnityEngine.Random.Range(0, HandManager.Instance.cardsInHand.Count)];
            DiscardCardInternal(cardObject, GameplayRef.onCardDiscarded);
        }

        public void Player_DiscardCardFromHand(GameObject cardObject)
        {
            DiscardCardInternal(cardObject, GameplayRef.onCardPlayed);
        }

        /// <summary>
        /// Internal helper to perform Fisher-Yates shuffle on a list of cards.
        /// </summary>
        private void ShuffleCardList(List<GameObject> cards)
        {
            if (cards == null || cards.Count < 2) return;

            for (int i = cards.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                (cards[i], cards[randomIndex]) = (cards[randomIndex], cards[i]);
            }
        }

        public void Player_ShuffleDeck()
        {
            List<GameObject> cards = new List<GameObject>(cardStack);
            cardStack.Clear();

            ShuffleCardList(cards);

            foreach (GameObject card in cards)
                cardStack.Push(card);

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

            ShuffleCardList(cards);

            foreach (GameObject card in cards)
                cardStack.Push(card);

            Debug.Log($"[DeckManager] Shuffled {cards.Count} discarded cards back into the deck.");
        }

        public void Player_MoveOutDeck(EntityScript entity)
        {
            if (entity == null || !DeckManagement.ContainsKey(entity))
                return;

            var deckStorage = DeckManagement[entity];
            Transform dockDeck = deckStorage.Deck;
            Transform dockDiscard = deckStorage.Discard;

            if (dockDeck == null || dockDiscard == null)
                return;

            // Save the current deck order before moving cards out
            SaveDeckOrder(entity);

            // Move deck cards
            var deckCards = new List<GameObject>(cardStack);
            MoveCardsToTransform(deckCards, dockDeck);
            cardStack.Clear();

            // Move discard cards
            var discardCards = new List<GameObject>(discardStack);
            MoveCardsToTransform(discardCards, dockDiscard);
            discardStack.Clear();
        }

        private void MoveCardsToTransform(List<GameObject> cards, Transform target)
        {
            foreach (GameObject card in cards)
            {
                if (card != null)
                {
                    // Ensure card remains active
                    if (!card.activeInHierarchy)
                        card.SetActive(true);

                                 card.transform.SetParent(target);
                                TransformUtility.ZeroLocalRectTransform(card.transform as RectTransform);
                            }
                        }
                    }

                    /// <summary>
                    /// Saves the current deck and discard order for an entity.
                    /// This preserves the exact sequence of cards so it can be restored later.
                    /// </summary>
                    private void SaveDeckOrder(EntityScript entity)
                    {
                        if (entity == null)
                            return;

                        // Convert stack to list to preserve order
                        List<GameObject> deckOrder = new List<GameObject>(cardStack);
                        List<GameObject> discardOrder = new List<GameObject>(discardStack);

                        DeckOrderManagement[entity] = new DeckOrderData(deckOrder, discardOrder);
                        Debug.Log($"[DeckManager] Saved deck order for {entity.name}: {deckOrder.Count} deck cards, {discardOrder.Count} discard cards");
                    }

                    /// <summary>
                    /// Restores the saved deck and discard order for an entity.
                    /// The GameObject positions are based on the saved list order, not transform hierarchy.
                    /// </summary>
                    private void RestoreDeckOrder(EntityScript entity, Transform dockDeck, Transform dockDiscard)
                    {
                        if (entity == null || !DeckOrderManagement.ContainsKey(entity))
                        {
                            Debug.LogWarning($"[DeckManager] No saved deck order found for {entity?.name}. Using fallback retrieval.");
                            RestoreDeckOrderFallback(entity, dockDeck, dockDiscard);
                            return;
                        }

                        var deckOrderData = DeckOrderManagement[entity];

                        // Restore deck cards in saved order
                        foreach (GameObject card in deckOrderData.DeckOrder)
                        {
                            if (card != null && card.activeInHierarchy)
                            {
                                RectTransform ct = card.GetComponent<RectTransform>();
                                ct.SetParent(deckParent);
                                TransformUtility.ZeroLocalRectTransform(ct);
                                cardStack.Push(card);
                            }
                        }

                        // Restore discard cards in saved order
                        foreach (GameObject card in deckOrderData.DiscardOrder)
                        {
                            if (card != null && card.activeInHierarchy)
                            {
                                RectTransform ct = card.GetComponent<RectTransform>();
                                ct.SetParent(discardParent);
                                TransformUtility.ZeroLocalRectTransform(ct);
                                discardStack.Push(card);
                            }
                        }

                        Debug.Log($"[DeckManager] Restored deck order for {entity.name}: {deckOrderData.DeckOrder.Count} deck cards, {deckOrderData.DiscardOrder.Count} discard cards");
                    }

                    /// <summary>
                    /// Fallback method to restore deck order if saved data is unavailable.
                    /// Retrieves cards from transform hierarchy using GetComponentsInChildren.
                    /// </summary>
                    private void RestoreDeckOrderFallback(EntityScript entity, Transform dockDeck, Transform dockDiscard)
                    {
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
                            if (c.gameObject == null) continue;

                            c.cardData.Owner = entity;
                            RectTransform ct = c.GetComponent<RectTransform>();

                            // Ensure card is active
                            if (!c.gameObject.activeInHierarchy)
                                c.gameObject.SetActive(true);

                            ct.SetParent(deckParent);
                            TransformUtility.ZeroLocalRectTransform(ct);
                            cardStack.Push(ct.gameObject);
                        }

                        // Move Discard cards to discardStack
                        foreach (CardScript c in discardCards)
                        {
                            if (c.gameObject == null) continue;

                            c.cardData.Owner = entity;
                            RectTransform ct = c.GetComponent<RectTransform>();

                            // Ensure card is active
                            if (!c.gameObject.activeInHierarchy)
                                c.gameObject.SetActive(true);

                            ct.SetParent(discardParent);
                            TransformUtility.ZeroLocalRectTransform(ct);
                            discardStack.Push(ct.gameObject);
                        }
                    }

        public void Player_MoveInDeck(EntityScript entity)
        {
            if (entity == null || !DeckManagement.ContainsKey(entity))
                return;

            cardStack.Clear();
            discardStack.Clear();

            var deckStorage = DeckManagement[entity];
            Transform dockDeck = deckStorage.Deck;
            Transform dockDiscard = deckStorage.Discard;

            if (dockDeck == null || dockDiscard == null)
                return;

            Debug.Log($"[DeckManager] Moving cards into deck of {entity.name}");

            // Restore deck and discard order from saved data
            RestoreDeckOrder(entity, dockDeck, dockDiscard);
        }

        public void StartTurn(EntityScript entity)
        {
            if (entity == null || !(entity is PlayerScript))
                return;

            Player_MoveInDeck(entity);

            // Draw opening hand

            int initialDrawCount = Mathf.Max(1, Mathf.FloorToInt(entity.entityStats.CurrentWisdom / 2f));

            for (int i = 0; i < initialDrawCount; i++)
            {
                Player_DrawTopCard();
            }
        }
        public void EndTurn(EntityScript entity)
        {
            if (entity == null)
                return;

            // Only process player turn end
            if (!(entity is PlayerScript))
                return;

            // Verify it's actually the player's turn
            if (TurnManager.Instance.CurrentTurnEntity != entity)
                return;

            // Move any remaining cards in hand to discard
            HandManager.Instance.DiscardAllInHand();

            // Move out the deck for storage
            // NOTE: Discard pile is NOT reshuffled here - it will only be reshuffled
            // when a player tries to draw and the deck is empty (see HandUtility.Draw())
            Player_MoveOutDeck(entity);
        }
        #endregion
    }
}