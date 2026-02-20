using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace facingfate
{

    public class CardPilePeekPanel : MonoBehaviour
    {
        [Header("Pile Source")]
        [SerializeField] private Transform pileRoot;

        [Header("UI")]
        [SerializeField] private TMP_Text countText;
        [SerializeField] private Button toggleButton;
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private Transform listContent;
        [SerializeField] private CardListEntryUI entryPrefab;

        private readonly List<CardListEntryUI> entries = new();

        public Transform PileRoot => pileRoot;

        private void Awake()
        {
            if (toggleButton != null) toggleButton.onClick.AddListener(Toggle);
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
                int c = pileRoot != null ? pileRoot.childCount : 0;
                countText.text = c.ToString();
            }

            if (popupRoot != null && popupRoot.activeSelf)
            {
                RebuildList();
            }
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
            if (listContent == null || entryPrefab == null)
                return;

            ClearEntries();

            if (pileRoot == null) return;

            int childCount = pileRoot.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform cardT = pileRoot.GetChild(i);
                if (cardT == null) continue;

                string label = CardReflectionReader.GetCardLabel(cardT.gameObject);

                CardListEntryUI e = Instantiate(entryPrefab, listContent);
                e.SetText(label);

                // Optional tooltip: show full card data if available
                e.SetTooltip(CardReflectionReader.GetCardTooltip(cardT.gameObject));

                entries.Add(e);
            }
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
