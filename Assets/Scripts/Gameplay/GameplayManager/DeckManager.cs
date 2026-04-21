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

        public static DeckManager Instance { get; private set; }

        [Header("Deck Configuration")]
        public GameObject cardPrefab;                          // Prefab with CardDisplay script
        public Transform deckParent;                           // Parent under the deck
        public Transform discardParent;                        // Parent under the deck
        public Button deckDrawButton;

        [Header("Deck Management")]
        public GameObject deckDockPrefab;

        private Dictionary<EntityScript, EntityDeckStorage> DeckManagement = new Dictionary<EntityScript, EntityDeckStorage>();

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

        #region Player Deck interactions
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
        }
        public void Player_DiscardRandomCardFromHand()
        {
            // Get a random card from the hand
            if (HandManager.Instance.cardsInHand.Count == 0) return;


            GameObject cardobject = HandManager.Instance.cardsInHand[UnityEngine.Random.Range(0, HandManager.Instance.cardsInHand.Count)];

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

            GameEvents.TriggerRefEvent(new ToSendTriggerReference(new() { GameplayRef.onCardDiscarded }, null, null));
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
            if (entity == null || !DeckManagement.ContainsKey(entity))
                return;

            var deckStorage = DeckManagement[entity];
            Transform dockDeck = deckStorage.Deck;
            Transform dockDiscard = deckStorage.Discard;

            if (dockDeck == null || dockDiscard == null)
                return;

            // Collect all cards needing to be stored
            var allCards = new List<GameObject>(cardStack.Count + HandManager.Instance.cardsInHand.Count + discardStack.Count);
            allCards.AddRange(cardStack);
            allCards.AddRange(HandManager.Instance.cardsInHand);
            allCards.AddRange(discardStack);

            // Separate deck cards from discard cards
            var deckCards = allCards.Take(cardStack.Count).ToList();
            var discardCards = allCards.Skip(cardStack.Count).ToList();

            // Move cards to their respective storage locations
            MoveCardsToTransform(deckCards, dockDeck);
            MoveCardsToTransform(discardCards, dockDiscard);

            // Clear the stacks
            cardStack.Clear();
            discardStack.Clear();
        }

        private void MoveCardsToTransform(List<GameObject> cards, Transform target)
        {
            foreach (GameObject card in cards)
            {
                if (card != null)
                {
                    card.transform.SetParent(target);
                    TransformUtility.ZeroLocalRectTransform(card.transform as RectTransform);
                }
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
            if (entity == null || !(entity is PlayerScript))
                return;

            Player_MoveInDeck(entity);

            // Draw opening hand
            for (int i = 0; i < 5; i++)
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

            // Move all discarded cards back into the deck
            Player_ShuffleDiscard();

            // Move out the deck for storage
            Player_MoveOutDeck(entity);
        }
        #endregion
    }
}