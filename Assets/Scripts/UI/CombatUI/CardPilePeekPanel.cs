using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace facingfate
{
    public class CardPilePeekPanel : MonoBehaviour
    {
        public static CardPilePeekPanel Instance { get; private set; }

        // ── HUD count texts (always show active-player counts) ─────────────────
        [Header("HUD Count Texts")]
        [SerializeField] private TMP_Text deckCountText;
        [SerializeField] private TMP_Text discardCountText;

        // ── Popup ──────────────────────────────────────────────────────────────
        [Header("Popup")]
        [SerializeField] private GameObject  popupRoot;
        [SerializeField] private Transform   listContent;
        [SerializeField] private CardListEntryUI entryPrefab;
        // Optional CanvasGroup on the list wrapper — enables cross-fade on switch.
        [SerializeField] private CanvasGroup listCanvasGroup;

        // ── Tab highlights (active indicator on the tab buttons) ───────────────
        [Header("Tab Highlights")]
        [SerializeField] private GameObject deckTabHighlight;
        [SerializeField] private GameObject discardTabHighlight;

        // ── Player navigation ──────────────────────────────────────────────────
        // Wire these to UI elements inside the popup header.
        // Navigation is hidden automatically when only one player exists.
        [Header("Player Navigation")]
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private Button   prevPlayerButton;
        [SerializeField] private Button   nextPlayerButton;

        // ── Animation ─────────────────────────────────────────────────────────
        [Header("Animation")]
        [SerializeField] private float slideDuration = 0.3f;
        [SerializeField] private Ease  slideEase     = Ease.OutQuart;
        [SerializeField] private float slideOffset   = 50f;
        [SerializeField] private float tabSwitchFade = 0.12f;

        // ── State ──────────────────────────────────────────────────────────────
        private RectTransform popupRectTransform;
        private Vector2       popupRestPosition;
        private bool          isDeckActive = true;

        private readonly List<EntityScript>    playerList  = new();
        private int                            selectedPlayerIndex = 0;
        private EntityScript SelectedPlayer =>
            playerList.Count > 0 && selectedPlayerIndex >= 0 && selectedPlayerIndex < playerList.Count
                ? playerList[selectedPlayerIndex]
                : null;

        private readonly List<CardListEntryUI> entries = new();

        // ── Lifecycle ──────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (popupRoot != null)
            {
                popupRectTransform = popupRoot.GetComponent<RectTransform>();
                popupRestPosition  = popupRectTransform.anchoredPosition;
                popupRoot.SetActive(false);
            }

            if (prevPlayerButton != null) prevPlayerButton.onClick.AddListener(PrevPlayer);
            if (nextPlayerButton != null) nextPlayerButton.onClick.AddListener(NextPlayer);
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Called by the Deck HUD button.</summary>
        public void ToggleDeck() => TogglePanel(true);

        /// <summary>Called by the Discard HUD button.</summary>
        public void ToggleDiscard() => TogglePanel(false);

        /// <summary>Refreshes HUD count texts and the open list (if any).</summary>
        public void Refresh()
        {
            RefreshHUDCounts();
            if (popupRoot != null && popupRoot.activeSelf)
                RebuildList();
        }

        /// <summary>Slides the popup closed.</summary>
        public void Close()
        {
            if (popupRoot == null || !popupRoot.activeSelf) return;

            popupRectTransform.DOAnchorPosY(popupRestPosition.y - slideOffset, slideDuration)
                .SetEase(slideEase).SetUpdate(true)
                .OnComplete(() => popupRoot.SetActive(false));
        }

        // ── Player navigation ──────────────────────────────────────────────────

        public void PrevPlayer()
        {
            if (playerList.Count <= 1) return;
            selectedPlayerIndex = (selectedPlayerIndex - 1 + playerList.Count) % playerList.Count;
            UpdatePlayerNav();
            SwitchListContent();
        }

        public void NextPlayer()
        {
            if (playerList.Count <= 1) return;
            selectedPlayerIndex = (selectedPlayerIndex + 1) % playerList.Count;
            UpdatePlayerNav();
            SwitchListContent();
        }

        // ── Internal ───────────────────────────────────────────────────────────

        private void TogglePanel(bool showDeck)
        {
            if (popupRoot == null) return;
            bool isOpen = popupRoot.activeSelf;

            if (!isOpen)
            {
                isDeckActive = showDeck;
                BuildPlayerList();                          // collect all players
                popupRoot.SetActive(true);
                popupRectTransform.anchoredPosition =
                    new Vector2(popupRestPosition.x, popupRestPosition.y - slideOffset);
                popupRectTransform.DOAnchorPosY(popupRestPosition.y, slideDuration)
                    .SetEase(slideEase).SetUpdate(true);
                UpdateTabHighlights();
                UpdatePlayerNav();
                RebuildList();
            }
            else if (isDeckActive == showDeck)
            {
                Close();
            }
            else
            {
                // Different tab — switch without closing
                isDeckActive = showDeck;
                UpdateTabHighlights();
                SwitchListContent();
            }
        }

        // ── Player list helpers ────────────────────────────────────────────────

        private void BuildPlayerList()
        {
            playerList.Clear();
            TurnManager tm = TurnManager.Instance;
            if (tm?.TurnOrder != null)
            {
                foreach (EntityScript e in tm.TurnOrder)
                {
                    if (e != null && e.GetComponent<PlayerScript>() != null)
                        playerList.Add(e);
                }
            }

            // Default selection: whichever player's turn it currently is
            EntityScript active = GetActivePlayer();
            int idx = active != null ? playerList.IndexOf(active) : -1;
            selectedPlayerIndex = idx >= 0 ? idx : 0;
        }

        private void UpdatePlayerNav()
        {
            if (playerNameText != null)
                playerNameText.text = SelectedPlayer != null ? GetEntityName(SelectedPlayer) : "";

            bool multi = playerList.Count > 1;
            if (prevPlayerButton != null) prevPlayerButton.gameObject.SetActive(multi);
            if (nextPlayerButton != null) nextPlayerButton.gameObject.SetActive(multi);
        }

        // ── Tab helpers ────────────────────────────────────────────────────────

        private void UpdateTabHighlights()
        {
            if (deckTabHighlight    != null) deckTabHighlight.SetActive(isDeckActive);
            if (discardTabHighlight != null) discardTabHighlight.SetActive(!isDeckActive);
        }

        private void SwitchListContent()
        {
            if (listCanvasGroup == null) { RebuildList(); return; }

            listCanvasGroup.DOKill();
            listCanvasGroup.DOFade(0f, tabSwitchFade).SetUpdate(true)
                .OnComplete(() =>
                {
                    RebuildList();
                    listCanvasGroup.DOFade(1f, tabSwitchFade).SetUpdate(true);
                });
        }

        // ── HUD counts (always active-turn player) ─────────────────────────────

        private void RefreshHUDCounts()
        {
            DeckManager  dm     = DeckManager.Instance;
            if (dm == null) return;

            EntityScript active = GetActivePlayer();
            int deckCount = 0, discardCount = 0;

            foreach (GameObject go in dm.cardStack)
            {
                CardScript cs = go?.GetComponent<CardScript>();
                if (cs?.cardData == null) continue;
                if (active == null || cs.cardData.Owner == active) deckCount++;
            }
            foreach (GameObject go in dm.discardStack)
            {
                CardScript cs = go?.GetComponent<CardScript>();
                if (cs?.cardData == null) continue;
                if (active == null || cs.cardData.Owner == active) discardCount++;
            }

            if (deckCountText    != null) deckCountText.text    = deckCount.ToString();
            if (discardCountText != null) discardCountText.text = discardCount.ToString();
        }

        // ── List builder (uses SelectedPlayer) ────────────────────────────────

        private void RebuildList()
        {
            if (listContent == null || entryPrefab == null) return;
            ClearEntries();

            DeckManager      dm     = DeckManager.Instance;
            EntityScript     target = SelectedPlayer;
            List<GameObject> cards  = new();

            if (dm != null)
            {
                Stack<GameObject> source = isDeckActive ? dm.cardStack : dm.discardStack;
                foreach (GameObject go in source)
                {
                    CardScript cs = go?.GetComponent<CardScript>();
                    if (cs?.cardData == null) continue;
                    if (target == null || cs.cardData.Owner == target)
                        cards.Add(go);
                }
            }

            cards = cards
                .OrderBy(go => GetCardType(go))
                .ThenBy(go  => GetCardName(go))
                .ToList();

            foreach (GameObject cardGO in cards)
            {
                if (cardGO == null) continue;
                CardScript cs = cardGO.GetComponent<CardScript>();
                if (cs == null) continue;

                CardListEntryUI e  = Instantiate(entryPrefab, listContent);
                RectTransform   rt = e.GetComponent<RectTransform>();
                if (rt != null) rt.sizeDelta = new Vector2(rt.sizeDelta.x, 280f);
                e.SetCard(cs);
                entries.Add(e);
            }
        }

        // ── Static helpers ─────────────────────────────────────────────────────

        private static EntityScript lastActivePlayer;

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

            if (lastActivePlayer == null)
            {
                foreach (EntityScript e in tm.TurnOrder)
                {
                    if (e != null && e.GetComponent<PlayerScript>() != null)
                    { lastActivePlayer = e; break; }
                }
            }

            return lastActivePlayer;
        }

        private static string GetEntityName(EntityScript entity)
        {
            object npcData = ReflectionUtility.TryGetFieldOrProperty(entity, "npcData");
            if (npcData != null)
            {
                object nameObj = ReflectionUtility.TryGetFieldOrProperty(npcData, "name");
                if (nameObj != null && !string.IsNullOrWhiteSpace(nameObj.ToString()))
                    return nameObj.ToString();
            }
            return entity.gameObject.name;
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
                if (entries[i] != null) Destroy(entries[i].gameObject);
            entries.Clear();

            for (int i = listContent.childCount - 1; i >= 0; i--)
                Destroy(listContent.GetChild(i).gameObject);
        }
    }
}
