using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace facingfate
{
    public class HandManager : MonoBehaviour
    {
        public static HandManager Instance { get; private set; }

        public Transform handAnchor;
        public GameObject cardPrefab;
        [Header("Hand Layout Settings")]
        [SerializeField] private float cardSpacing = 100f;
        [SerializeField] private float cardFanAngle = 4f;
        [SerializeField] private float hoverOffsetY = 30f;
        [SerializeField] private float selectedOffsetY = 30f;
        [SerializeField] private float fanRadius = -2800;
        [SerializeField] private float handHoverRadius = 300f;

        [Header("Animation Settings")]
        [SerializeField] private float animDuration = 0.2f;

        [Header("Hand")]
        public float maxHandsize = 10;
        public List<GameObject> cardsInHand = new List<GameObject>();
        [SerializeField] private GameObject hoveredCard;
        [SerializeField] private GameObject selectedCard;
        public GameObject GetSelectedCard() => selectedCard;

        private bool listenersAdded = false;
        private GameObject lastPreviewedCard = null;

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

            AddListeners();
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
            // Tutorial wave transitions fire CombatEnd between waves — don't destroy cards.
            if (TutorialCombatManager.Instance != null && TutorialCombatManager.Instance.IsActive)
                return;

            // Destroy all card objects in hand
            foreach (GameObject card in cardsInHand)
            {
                if (card != null)
                {
                    Destroy(card);
                }
            }
            cardsInHand.Clear();
            lastPreviewedCard = null;
        }

        private void Update()
        {
            if (cardsInHand.Count == 0) return;

            GameObject currentCardToPreview = selectedCard ?? hoveredCard;

            // Only update preview if the card changed
            if (currentCardToPreview != lastPreviewedCard)
            {
                lastPreviewedCard = currentCardToPreview;

                if (currentCardToPreview != null)
                {
                    CardPreviewPanel.Instance?.Show(currentCardToPreview.GetComponent<CardScript>());
                }
                else
                {
                    CardPreviewPanel.Instance?.Hide();
                }
            }

            // Update layout when hovering
            if (hoveredCard != null)
            {
                UpdateHandLayout(hoveredCard);
            }
        }


        private bool IsMouseNearHand()
        {
            if (handAnchor == null) return false;
            Vector3 handScreenPos = RectTransformUtility.WorldToScreenPoint(null, handAnchor.position);
            return Vector2.Distance(Mouse.current.position.ReadValue(), handScreenPos) < handHoverRadius;
        }

        private GameObject GetHoveredCard()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            // Check from most recent card first (top to bottom)
            for (int i = cardsInHand.Count - 1; i >= 0; i--)
            {
                GameObject card = cardsInHand[i];
                if (card == null || !card.TryGetComponent<RectTransform>(out var rt))
                    continue;

                Vector3[] corners = new Vector3[4];
                rt.GetWorldCorners(corners);

                if (IsPointInQuad(mousePos, corners))
                    return card;
            }
            return null;
        }

        private bool IsPointInQuad(Vector2 point, Vector3[] corners)
        {
            // corners: [0]=BL, [1]=TL, [2]=TR, [3]=BR
            return IsPointInTriangle(point, corners[0], corners[1], corners[2]) ||
                   IsPointInTriangle(point, corners[0], corners[2], corners[3]);
        }

        private bool IsPointInTriangle(Vector2 p, Vector3 a, Vector3 b, Vector3 c)
        {
            float d1 = Sign(p, a, b);
            float d2 = Sign(p, b, c);
            float d3 = Sign(p, c, a);

            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(hasNeg && hasPos);
        }

        private float Sign(Vector2 p1, Vector3 p2, Vector3 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        public void AddCard(GameObject newCard)
        {
            newCard.GetComponent<CardScript>().SetHidden();
            newCard.transform.SetParent(handAnchor, false);
            newCard.transform.position = Vector3.zero;
            cardsInHand.Add(newCard);
            UpdateHandLayout();

            // Apply tutorial lock immediately — LockHandNextFrame only catches cards
            // already in hand after 1 frame; cards drawn later would miss the lock.
            TutorialCombatManager.Instance?.ApplyLockToCard(newCard);

            // Apply stamina lock after tutorial lock so both conditions combine correctly.
            EntityScript currentEntity = TurnManager.Instance?.CurrentTurnEntity;
            HandUI.ApplyStaminaLockToCard(newCard, currentEntity);
        }

        public void RemoveCard(GameObject cardObject)
        {
            if (cardsInHand.Contains(cardObject))
            {
                cardsInHand.Remove(cardObject);
                UpdateHandLayout();
            }
        }

        public void DiscardCard(GameObject cardObject)
        {
            if (cardObject == null) return;
            if (!cardsInHand.Contains(cardObject)) return;

            cardsInHand.Remove(cardObject);
            UpdateHandLayout();

            DeckManager.Instance.Player_DiscardCardFromHand(cardObject);
        }

        public void UpdateHandLayout(GameObject hoveredCard = null)
        {
            int count = cardsInHand.Count;
            if (count == 0) return;

            // Maximaler Gesamtwinkel begrenzen
            float maxTotalAngle = 60f;
            float anglePerCard = count > 1
                ? Mathf.Min(cardFanAngle, maxTotalAngle / (count - 1))
                : 0f;

            float totalAngle = anglePerCard * (count - 1);
            float startAngle = totalAngle / 2f;

            for (int i = 0; i < count; i++)
            {
                GameObject card = cardsInHand[i];
                if (card == null) continue;

                RectTransform rt = card.GetComponent<RectTransform>();
                if (rt == null) continue;

                float angle = startAngle - anglePerCard * i;
                float angleRad = angle * Mathf.Deg2Rad;

                float x = Mathf.Sin(angleRad) * fanRadius;
                float y = (Mathf.Cos(angleRad) - 1f) * fanRadius * 0.3f;

                if (card == hoveredCard)
                    y += hoverOffsetY;
                if (card == selectedCard) y += selectedOffsetY;

                rt.DOKill();
                rt.DOAnchorPos(new Vector2(x, y), animDuration)
                    .SetEase(Ease.OutQuart).SetUpdate(true);
                rt.DOLocalRotate(new Vector3(0f, 0f, angle), animDuration)
                    .SetEase(Ease.OutQuart).SetUpdate(true);
            }
        }

        public void DiscardAllInHand()
        {
            // Create a copy to avoid modification during iteration
            List<GameObject> cardsToDiscard = new List<GameObject>(cardsInHand);
            foreach (GameObject card in cardsToDiscard)
            {
                if (card != null && cardsInHand.Contains(card))
                {
                    DiscardCard(card);
                }
            }
        }

        public void SelectCard(GameObject card)
        {
            // Deselect current card if clicking null or same card (toggle)
            if (card == null || selectedCard == card)
            {
                if (selectedCard != null)
                {
                    selectedCard.GetComponent<CardOutline>()?.SetSelected(false);
                    selectedCard = null;
                    lastPreviewedCard = null;
                }
                AssetManager.Instance?.HideRangeIndicator();
                UpdateHandLayout(hoveredCard);
                return;
            }

            // Deselect previous card
            if (selectedCard != null)
            {
                selectedCard.GetComponent<CardOutline>()?.SetSelected(false);
            }

            // Select new card
            selectedCard = card;
            selectedCard.GetComponent<CardOutline>()?.SetSelected(true);

            CardScript cardScript = card.GetComponent<CardScript>();
            if (cardScript != null && cardScript.cardData != null)
            {
                AssetManager.Instance?.ShowRangeIndicator(cardScript.cardData);
            }

            UpdateHandLayout(hoveredCard);
        }
        public void HoverCard(GameObject card)
        {
            hoveredCard = card;
            if (hoveredCard == null)
            {
                lastPreviewedCard = null;
                CardPreviewPanel.Instance?.Hide();
                if (selectedCard == null)
                {
                    AssetManager.Instance?.HideRangeIndicator();
                }
            }
            else
            {
                CardScript cardScript = card.GetComponent<CardScript>();
                if (cardScript != null && cardScript.cardData != null)
                {
                    AssetManager.Instance?.ShowRangeIndicator(cardScript.cardData);
                }
            }
            UpdateHandLayout(hoveredCard);
        }
    }
}
