using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace facingfate
{

    public class CardPilePeekPanel : MonoBehaviour
    {
        [Header("Pile Source")]
        [SerializeField] private Transform pileRoot;
        [SerializeField] private bool isDeckPanel = true;

        [Header("UI")]
        [SerializeField] private TMP_Text countText;
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private Transform listContent;
        [SerializeField] private CardListEntryUI entryPrefab;

        private readonly List<CardListEntryUI> entries = new();

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
                    c = isDeckPanel ? dm.cardStack.Count : dm.discardStack.Count;
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
            popupRoot.SetActive(!popupRoot.activeSelf);

            if (popupRoot.activeSelf)
            {
                RebuildList();
            }
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
                if (isDeckPanel)
                    cards = new List<GameObject>(dm.cardStack);
                else
                    cards = new List<GameObject>(dm.discardStack);
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
