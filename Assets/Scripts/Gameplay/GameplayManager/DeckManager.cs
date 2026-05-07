using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
        /// Stores the deck and discard stacks for each entity.
        /// </summary>
        private struct DeckOrderData
        {
            public Stack<GameObject> DeckOrder { get; set; }
            public Stack<GameObject> DiscardOrder { get; set; }

            public DeckOrderData(Stack<GameObject> deckOrder, Stack<GameObject> discardOrder)
            {
                DeckOrder = deckOrder ?? new Stack<GameObject>();
                DiscardOrder = discardOrder ?? new Stack<GameObject>();
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

        // Reference to the current player's deck stacks (updated when moving deck in/out)
        public Stack<GameObject> cardStack = new Stack<GameObject>();
        public Stack<GameObject> discardStack = new Stack<GameObject>();

        private EntityScript currentPlayerEntity = null;
        private bool listenersAdded = false;

        void Start()
        {
            // Singleton enforcement
            if (Instance != null && Instance != this)
            {
                // Transfer fresh scene refs to the persistent instance before destroying this duplicate
                Instance.deckParent = deckParent;
                Instance.discardParent = discardParent;
                Instance.deckDrawButton = deckDrawButton;
                Destroy(gameObject);
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

            // Clear the visual elements in the deck and discard parents
            foreach (Transform child in deckParent)
                Destroy(child.gameObject);
            foreach (Transform child in discardParent)
                Destroy(child.gameObject);

            // Destroy all deck dock objects parented to this DeckManager (created by BuildDeckFromIDs).
            // Without this they persist across scenes — cards keep their DescriptionUpdate coroutines
            // running with a destroyed Owner, causing NullReferenceException spam.
            var docks = new List<Transform>();
            foreach (Transform child in transform) docks.Add(child);
            foreach (var dock in docks) Destroy(dock.gameObject);

            // Clear deck management storage
            DeckManagement.Clear();
            DeckOrderManagement.Clear();
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
                }

                ShuffleCardList(cardObjs);

                Stack<GameObject> initialDeck = new Stack<GameObject>();
                for (int i = cardObjs.Count - 1; i >= 0; i--)
                    initialDeck.Push(cardObjs[i]);

                DeckOrderManagement[entity] = new DeckOrderData(initialDeck, new Stack<GameObject>());
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
            AudioManager.Instance?.PostEvent(AudioManager.Instance.playCardDrawSound, gameObject);
        }

        public void Player_DiscardRandomCardFromHand()
        {
            if (HandManager.Instance.cardsInHand.Count == 0) return;

            GameObject cardObject = HandManager.Instance.cardsInHand[UnityEngine.Random.Range(0, HandManager.Instance.cardsInHand.Count)];
            DiscardCardInternal(cardObject, GameplayRef.onCardDiscarded);
            AudioManager.Instance?.PostEvent(AudioManager.Instance.playCardDiscardSound, gameObject);
        }

        public void Player_DiscardCardFromHand(GameObject cardObject)
        {
            DiscardCardInternal(cardObject, GameplayRef.onCardPlayed);
            AudioManager.Instance?.PostEvent(AudioManager.Instance.playCardDiscardSound, gameObject);
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

            deckParent?.GetComponent<DiscardPileVisualizer>()?.Refresh();
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

            // Cards were in discardParent; move them to deckParent for correct pile display
            if (deckParent != null)
            {
                foreach (var card in cards)
                {
                    if (card != null)
                    {
                        card.transform.SetParent(deckParent);
                        TransformUtility.ZeroLocalRectTransform(card.transform as RectTransform);
                    }
                }
                deckParent.GetComponent<DiscardPileVisualizer>()?.Refresh();
            }
            discardParent?.GetComponent<DiscardPileVisualizer>()?.Refresh();

            Debug.Log($"[DeckManager] Shuffled {cards.Count} discarded cards back into the deck.");
        }

        private IEnumerator Player_MoveOutDeck_Coroutine(EntityScript entity)
        {
            if (entity == null || !DeckManagement.ContainsKey(entity))
                yield break;

            var deckStorage = DeckManagement[entity];
            Transform dockDeck = deckStorage.Deck;
            Transform dockDiscard = deckStorage.Discard;

            if (dockDeck == null || dockDiscard == null)
                yield break;

            // Save the current deck order before moving cards out
            SaveDeckOrder(entity);

            // Move deck cards
            var deckCards = new List<GameObject>(cardStack);
            yield return StartCoroutine(MoveCardsToTransform_Coroutine(deckCards, dockDeck));
            cardStack.Clear();

            // Move discard cards
            var discardCards = new List<GameObject>(discardStack);
            yield return StartCoroutine(MoveCardsToTransform_Coroutine(discardCards, dockDiscard));
            discardStack.Clear();

            // Refresh pile visuals to reflect empty state now that cards moved to dock storage
            deckParent?.GetComponent<DiscardPileVisualizer>()?.Refresh();
            discardParent?.GetComponent<DiscardPileVisualizer>()?.Refresh();

            // Wait for UI layout to update
            yield return new WaitForEndOfFrame();
        }

        private IEnumerator MoveCardsToTransform_Coroutine(List<GameObject> cards, Transform target)
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
                yield return null;
            }
        }

        /// <summary>
        /// Saves the current deck and discard order for an entity as stacks.
        /// This preserves the exact sequence of cards so it can be restored later.
        /// </summary>
        private void SaveDeckOrder(EntityScript entity)
        {
            if (entity == null)
                return;

            // Create new stacks by copying current ones
            // Stack(IEnumerable) reverses the order, so we reverse twice to maintain proper order
            Stack<GameObject> deckOrder = new Stack<GameObject>();
            Stack<GameObject> discardOrder = new Stack<GameObject>();

            // Copy cardStack
            var deckList = new List<GameObject>(cardStack);
            deckList.Reverse();
            foreach (var card in deckList)
                deckOrder.Push(card);

            // Copy discardStack
            var discardList = new List<GameObject>(discardStack);
            discardList.Reverse();
            foreach (var card in discardList)
                discardOrder.Push(card);

            DeckOrderManagement[entity] = new DeckOrderData(deckOrder, discardOrder);
            Debug.Log($"[DeckManager] Saved deck order for {entity.name}: {deckOrder.Count} deck cards, {discardOrder.Count} discard cards");
        }

        /// <summary>
        /// Restores the saved deck and discard order for an entity.
        /// The stacks maintain the exact order of cards for proper drawing.
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

            // Clear current stacks
            cardStack.Clear();
            discardStack.Clear();

            // Restore deck cards from the saved stack by copying
            var deckList = new List<GameObject>(deckOrderData.DeckOrder);
            foreach (GameObject card in deckList)
            {
                if (card != null && card.activeInHierarchy)
                {
                    RectTransform ct = card.GetComponent<RectTransform>();
                    ct.SetParent(dockDeck);
                    TransformUtility.ZeroLocalRectTransform(ct);
                    cardStack.Push(card);
                }
            }

            // Restore discard cards from the saved stack by copying
            var discardList = new List<GameObject>(deckOrderData.DiscardOrder);
            foreach (GameObject card in discardList)
            {
                if (card != null && card.activeInHierarchy)
                {
                    RectTransform ct = card.GetComponent<RectTransform>();
                    ct.SetParent(dockDiscard);
                    TransformUtility.ZeroLocalRectTransform(ct);
                    discardStack.Push(card);
                }
            }

            Debug.Log($"[DeckManager] Restored deck order for {entity.name}: {cardStack.Count} deck cards, {discardStack.Count} discard cards");
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

            // Clear current stacks
            cardStack.Clear();
            discardStack.Clear();

            // Move Deck cards to cardStack (push in order for proper stack)
            foreach (CardScript c in deckCards)
            {
                if (c.gameObject == null) continue;

                c.cardData.Owner = entity;
                RectTransform ct = c.GetComponent<RectTransform>();

                // Ensure card is active
                if (!c.gameObject.activeInHierarchy)
                    c.gameObject.SetActive(true);

                ct.SetParent(dockDeck);
                TransformUtility.ZeroLocalRectTransform(ct);
                cardStack.Push(ct.gameObject);
            }

            // Move Discard cards to discardStack (push in order for proper stack)
            foreach (CardScript c in discardCards)
            {
                if (c.gameObject == null) continue;

                c.cardData.Owner = entity;
                RectTransform ct = c.GetComponent<RectTransform>();

                // Ensure card is active
                if (!c.gameObject.activeInHierarchy)
                    c.gameObject.SetActive(true);

                ct.SetParent(dockDiscard);
                TransformUtility.ZeroLocalRectTransform(ct);
                discardStack.Push(ct.gameObject);
            }
        }

        private IEnumerator Player_MoveInDeck_Coroutine(EntityScript entity)
        {
            if (entity == null || !DeckManagement.ContainsKey(entity))
                yield break;

            cardStack.Clear();
            discardStack.Clear();

            var deckStorage = DeckManagement[entity];
            Transform dockDeck = deckStorage.Deck;
            Transform dockDiscard = deckStorage.Discard;

            if (dockDeck == null || dockDiscard == null)
                yield break;

            Debug.Log($"[DeckManager] Moving cards into deck of {entity.name}");

            // Restore deck and discard order from saved data
            RestoreDeckOrder(entity, dockDeck, dockDiscard);

            // Move deck cards into the visual pile parent so DiscardPileVisualizer can show them
            if (deckParent != null)
            {
                foreach (var card in cardStack)
                {
                    if (card != null)
                    {
                        card.transform.SetParent(deckParent);
                        TransformUtility.ZeroLocalRectTransform(card.transform as RectTransform);
                    }
                }
                deckParent.GetComponent<DiscardPileVisualizer>()?.Refresh();
                LayoutRebuilder.ForceRebuildLayoutImmediate(deckParent);
            }

            // Move discard cards into the visual discard parent
            if (discardParent != null)
            {
                foreach (var card in discardStack)
                {
                    if (card != null)
                    {
                        card.transform.SetParent(discardParent);
                        TransformUtility.ZeroLocalRectTransform(card.transform as RectTransform);
                    }
                }
                discardParent.GetComponent<DiscardPileVisualizer>()?.Refresh();
            }

            // Wait for UI layout to update
            yield return new WaitForEndOfFrame();
        }

        public Coroutine StartTurn(EntityScript entity)
        {
            if (entity == null || !(entity is PlayerScript))
                return null;

            return StartCoroutine(StartTurn_Coroutine(entity));
        }

        private IEnumerator StartTurn_Coroutine(EntityScript entity)
        {
            // Move deck in with proper timing
            yield return Player_MoveInDeck_Coroutine(entity);

            // Draw opening hand after deck is properly loaded
            int initialDrawCount = Mathf.Max(1, Mathf.FloorToInt(entity.entityStats.CurrentWisdom / 2f));

            for (int i = 0; i < initialDrawCount; i++)
            {
                Player_DrawTopCard();
                yield return new WaitForEndOfFrame();
            }
        }
        public Coroutine EndTurn(EntityScript entity)
        {
            if (entity == null)
                return null;

            // Only process player turn end
            if (!(entity is PlayerScript))
                return null;

            // Verify it's actually the player's turn
            if (TurnManager.Instance.CurrentTurnEntity != entity)
                return null;

            return StartCoroutine(EndTurn_Coroutine(entity));
        }

        private IEnumerator EndTurn_Coroutine(EntityScript entity)
        {
            // Move any remaining cards in hand to discard
            HandManager.Instance.DiscardAllInHand();

            // Wait for discard animation/layout
            yield return new WaitForEndOfFrame();

            // Move out the deck for storage
            // NOTE: Discard pile is NOT reshuffled here - it will only be reshuffled
            // when a player tries to draw and the deck is empty (see HandUtility.Draw())
            yield return Player_MoveOutDeck_Coroutine(entity);
        }
        #endregion
    }
}