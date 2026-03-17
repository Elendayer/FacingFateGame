using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace facingfate
{

    public class CardPilePeekPanel : MonoBehaviour
    {
        [Header("Pile Source")]
        [SerializeField] private Transform pileRoot;
        [SerializeField] private bool isDeckPanel = true;
        private static EntityScript lastActivePlayer;

        [Header("UI")]
        [SerializeField] private TMP_Text countText;
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private Transform listContent;
        [SerializeField] private CardListEntryUI entryPrefab;

        private readonly List<CardListEntryUI> entries = new();
        private static CardPilePeekPanel currentlyOpen;

        public Transform PileRoot => pileRoot;

        private void Awake()
        {
            if (popupRoot != null) popupRoot.SetActive(false);
        }

        public void SetPileRoot(Transform root)
        {
            pileRoot = root;
        }

        public void Refresh()
        {
            if (countText != null)
            {
                DeckManager dm = DeckManager.Instance;
                int c = 0;
                if (dm != null)
                {
                    EntityScript activePlayer = GetActivePlayer();
                    Stack<GameObject> source = isDeckPanel ? dm.cardStack : dm.discardStack;
                    foreach (GameObject go in source)
                    {
                        CardScript cs = go?.GetComponent<CardScript>();
                        if (cs?.cardData == null) continue;
                        if (activePlayer != null && cs.cardData.Owner == activePlayer)
                            c++;
                    }
                }
                else
                    c = pileRoot != null ? pileRoot.childCount : 0;

                countText.text = c.ToString();
            }

            if (popupRoot != null && popupRoot.activeSelf)
                RebuildList();
        }

        public void Toggle()
        {
            if (popupRoot == null) return;

            bool willOpen = !popupRoot.activeSelf;

            // Anderes Panel schließen
            if (willOpen && currentlyOpen != null && currentlyOpen != this)
                currentlyOpen.Close();

            popupRoot.SetActive(willOpen);

            if (willOpen)
            {
                currentlyOpen = this;
                RebuildList();
            }
            else
            {
                currentlyOpen = null;
            }
        }

        public void Close()
        {
            if (popupRoot != null) popupRoot.SetActive(false);
            if (currentlyOpen == this) currentlyOpen = null;
        }

        private void RebuildList()
        {
            if (listContent == null || entryPrefab == null) return;
            ClearEntries();

            // Karten direkt vom DeckManager holen
            DeckManager dm = DeckManager.Instance;
            List<GameObject> cards = new();

            if (dm != null)
            {
                EntityScript activePlayer = GetActivePlayer();
                Stack<GameObject> source = isDeckPanel ? dm.cardStack : dm.discardStack;

                foreach (GameObject go in source)
                {
                    CardScript cs = go?.GetComponent<CardScript>();
                    if (cs?.cardData == null) continue;
                    if (activePlayer != null && cs.cardData.Owner == activePlayer)
                        cards.Add(go);
                }
            }
            else if (pileRoot != null)
            {
                // Fallback: Transform-Children
                for (int i = 0; i < pileRoot.childCount; i++)
                {
                    GameObject go = pileRoot.GetChild(i).gameObject;
                    if (go != null) cards.Add(go);
                }
            }

            // Nach CardType sortieren
            cards = cards
                .OrderBy(go => GetCardType(go))
                .ThenBy(go => GetCardName(go))
                .ToList();

            foreach (GameObject cardGO in cards)
            {
                if (cardGO == null) continue;

                CardScript cs = cardGO.GetComponent<CardScript>();
                if (cs == null) continue;

                CardListEntryUI e = Instantiate(entryPrefab, listContent);
                RectTransform rt = e.GetComponent<RectTransform>();
                if (rt != null) rt.sizeDelta = new Vector2(rt.sizeDelta.x, 280f);
                e.SetCard(cs);
                entries.Add(e);
            }
        }

        private static EntityScript GetActivePlayer()
        {
            TurnManager tm = TurnManager.Instance;
            if (tm == null || tm.TurnOrder == null || tm.TurnOrder.Count == 0)
                return lastActivePlayer;

            int idx = tm.CurrentTurnIndex;
            if (idx < 0 || idx >= tm.TurnOrder.Count) idx = 0;

            EntityScript current = tm.TurnOrder[idx];

            if (current != null && current.GetComponent<PlayerScript>() != null)
                lastActivePlayer = current;

            // ALT: return lastActivePlayer;
            // NEU: Wenn noch kein Player bekannt, ersten PlayerScript in TurnOrder suchen
            if (lastActivePlayer == null)
            {
                foreach (EntityScript e in tm.TurnOrder)
                {
                    if (e != null && e.GetComponent<PlayerScript>() != null)
                    {
                        lastActivePlayer = e;
                        break;
                    }
                }
            }

            return lastActivePlayer;
        }

        private static CardType GetCardType(GameObject go)
        {
            CardScript cs = go?.GetComponent<CardScript>();
            if (cs?.cardData == null) return CardType.Curse;
            return cs.cardData.cardType;
        }

        private static string GetCardName(GameObject go)
        {
            CardScript cs = go?.GetComponent<CardScript>();
            return cs?.cardData?.cardName ?? "";
        }

        private void ClearEntries()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null) Destroy(entries[i].gameObject);
            }
            entries.Clear();

            // safety: also clear any leftover children
            for (int i = listContent.childCount - 1; i >= 0; i--)
            {
                Destroy(listContent.GetChild(i).gameObject);
            }
        }
    }
}
