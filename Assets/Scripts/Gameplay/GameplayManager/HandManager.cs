using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace facingfate
{
    public class HandManager : MonoBehaviour
    {
        public static HandManager Instance { get; private set; }

        public Transform handAnchor;
        public GameObject cardPrefab;
        [SerializeField] private float cardSpacing = 100f;
        [SerializeField] private float cardFanAngle = -5f;
        [SerializeField] private float hoverOffsetY = 30f;
        [SerializeField] private float fanRadius = 800f;
        [SerializeField] private float animDuration = 0.2f;

        public float maxHandsize = 8;

        public List<GameObject> cardsInHand = new List<GameObject>();
        private GameObject lastHoveredCard;
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
        }

        private void Update()
        {
            if (cardsInHand.Count == 0) return;

            GameObject hovered = GetHoveredCard();

            // Nur wechseln wenn:
            // - neue Karte gehovert wird
            // - oder Maus ist komplett weg von der Hand
            if (hovered == lastHoveredCard) return;
            if (hovered == null && lastHoveredCard != null)
            {
                // Prüfe ob Maus noch in der Nähe der Hand ist
                if (IsMouseNearHand()) return;
            }

            lastHoveredCard = hovered;
            UpdateHandLayout(hovered);

            if (hovered != null)
                CardPreviewPanel.Instance?.Show(hovered.GetComponent<CardScript>());
            else
                CardPreviewPanel.Instance?.Hide();
        }

        private bool IsMouseNearHand()
        {
            if (handAnchor == null) return false;
            Vector2 mousePos = Input.mousePosition;
            Vector3 handScreenPos = RectTransformUtility.WorldToScreenPoint(null, handAnchor.position);
            return Vector2.Distance(mousePos, handScreenPos) < 300f;
        }

        private GameObject GetHoveredCard()
        {
            // Von vorne nach hinten prüfen (oberste Karte zuerst)
            for (int i = cardsInHand.Count - 1; i >= 0; i--)
            {
                GameObject card = cardsInHand[i];
                if (card == null) continue;

                RectTransform rt = card.GetComponent<RectTransform>();
                if (rt == null) continue;

                if (RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition))
                    return card;
            }
            return null;
        }

        public void AddCard(GameObject newCard)
        {
            newCard.GetComponent<CardScript>().SetHidden();
            newCard.transform.SetParent(handAnchor, false);
            cardsInHand.Add(newCard);
            UpdateHandLayout();
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
            DeckManager.Instance.DiscardCardFromHand(cardObject);
        }

        public void UpdateHandLayout(GameObject hoveredCard = null)
        {
            int count = cardsInHand.Count;
            if (count == 0) return;

            float totalAngle = cardFanAngle * (count - 1);
            float startAngle = totalAngle / 2f; // Links beginnen

            for (int i = 0; i < count; i++)
            {
                GameObject card = cardsInHand[i];
                if (card == null) continue;

                RectTransform rt = card.GetComponent<RectTransform>();
                if (rt == null) continue;

                float angle = startAngle - cardFanAngle * i;
                float angleRad = angle * Mathf.Deg2Rad;

                // Alle Karten laufen unten zusammen
                float x = Mathf.Sin(angleRad) * fanRadius;
                float y = (Mathf.Cos(angleRad) - 1f) * fanRadius * 0.3f;

                if (card == hoveredCard)
                    y += hoverOffsetY;

                rt.DOKill();
                rt.DOAnchorPos(new Vector2(x, y), animDuration)
                    .SetEase(Ease.OutQuart).SetUpdate(true);
                rt.DOLocalRotate(new Vector3(0f, 0f, angle), animDuration)
                    .SetEase(Ease.OutQuart).SetUpdate(true);

                //rt.SetSiblingIndex(card == hoveredCard ? count - 1 : i);
            }
        }

        public void DiscardAllInHand()
        {
            while (cardsInHand.Count > 0)
            {
                DiscardCard(cardsInHand[0]);
            }
        }
    }
}
