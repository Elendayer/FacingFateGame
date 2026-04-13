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
        [SerializeField] private float handHoverRadius = 300f;

        public float maxHandsize = 8;

        public List<GameObject> cardsInHand = new List<GameObject>();
        private GameObject lastHoveredCard;

        [SerializeField] private float selectedOffsetY = 60f;
        private GameObject selectedCard;
        public GameObject GetSelectedCard() => selectedCard;

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

            // Klick auf Tile wenn Karte ausgewählt
            if (selectedCard != null && Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3Int cell = TargetingUtility.GetHoveredTile(ray);

                if (cell != TilemapUtilityScript.InvalidPosition)
                {
                    DraggableCard dc = selectedCard.GetComponent<DraggableCard>();
                    if (dc != null)
                    {
                        dc.PlayCardOnCell(cell);
                        return;
                    }
                }
            }

            GameObject hovered = GetHoveredCard();

            if (hovered == lastHoveredCard) return;
            if (hovered == null && lastHoveredCard != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3Int cell = TargetingUtility.GetHoveredTile(ray);
                bool overEntity = cell != TilemapUtilityScript.InvalidPosition;

                if (!overEntity && IsMouseNearHand()) return;
            }

            lastHoveredCard = hovered;
            UpdateHandLayout(hovered);

            if (hovered != null)
            {
                CardPreviewPanel.Instance?.Show(hovered.GetComponent<CardScript>());
            }
            else
            {
                // Preview nur verstecken wenn KEINE Karte ausgewählt ist
                if (selectedCard == null)
                    CardPreviewPanel.Instance?.Hide();
                else
                    CardPreviewPanel.Instance?.Show(selectedCard.GetComponent<CardScript>());
            }
        }


        private bool IsMouseNearHand()
        {
            if (handAnchor == null) return false;
            Vector3 handScreenPos = RectTransformUtility.WorldToScreenPoint(null, handAnchor.position);
            return Vector2.Distance(Input.mousePosition, handScreenPos) < handHoverRadius;
        }

        private GameObject GetHoveredCard()
        {
            Vector2 mousePos = Input.mousePosition;

            // Von oben nach unten (zuletzt hinzugefügte Karte zuerst)
            for (int i = cardsInHand.Count - 1; i >= 0; i--)
            {
                GameObject card = cardsInHand[i];
                if (card == null) continue;

                RectTransform rt = card.GetComponent<RectTransform>();
                if (rt == null) continue;

                // Weltkoordinaten der 4 Ecken holen
                Vector3[] corners = new Vector3[4];
                rt.GetWorldCorners(corners);

                // Polygon-Test statt AABB – funktioniert bei rotierten Rects
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
            while (cardsInHand.Count > 0)
            {
                DiscardCard(cardsInHand[0]);
            }
        }

        public void SelectCard(GameObject card)
        {
            if (card == null)
            {
                if (selectedCard != null)
                {
                    selectedCard.GetComponent<CardOutline>()?.SetSelected(false);
                    TilemapUtilityScript.ResetMaphightlight(TilemapUtilityScript.BaseTilemap);
                    selectedCard = null;
                }
                return;
            }
            // Toggle
            if (selectedCard == card)
            {
                selectedCard.GetComponent<CardOutline>()?.SetSelected(false);
                TilemapUtilityScript.ResetMaphightlight(TilemapUtilityScript.BaseTilemap);
                selectedCard = null;
            }
            else
            {
                // Vorherige Auswahl aufheben
                if (selectedCard != null)
                {
                    selectedCard.GetComponent<CardOutline>()?.SetSelected(false);
                    TilemapUtilityScript.ResetMaphightlight(TilemapUtilityScript.BaseTilemap);
                }

                selectedCard = card;
                selectedCard.GetComponent<CardOutline>()?.SetSelected(true);

                // Range anzeigen
                CardScript cs = card.GetComponent<CardScript>();
                if (cs?.cardData?.Owner != null)
                {
                    var ownerCell = cs.cardData.Owner.GetComponent<EntityOnMap>().currentCell;
                    var rangeTiles = TilemapUtilityScript.GetTilesInRadius(ownerCell, cs.cardData.Range);
                    TilemapUtilityScript.SetTilesHighlight(rangeTiles, TilemapUtilityScript.HighlightType.Range);
                }
            }

            UpdateHandLayout(lastHoveredCard);
        }

    }
}
