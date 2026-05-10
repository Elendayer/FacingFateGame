using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace facingfate
{
    public class TurnOrderUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform entryContainer;
        [SerializeField] private TurnOrderEntryUI entryPrefab;
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private Button toggleButton;
        [SerializeField] private TMP_Text toggleButtonText;

        [Header("Settings")]
        [SerializeField] private int visibleTurns = 10;
        [SerializeField] private Color currentTurnColor = new Color(1f, 0.85f, 0.2f);
        [SerializeField] private Color playerTurnColor  = new Color(0.4f, 0.8f, 1f);
        [SerializeField] private Color enemyTurnColor   = new Color(1f, 0.4f, 0.4f);

        [Header("Entry Animation")]
        [SerializeField] private float entryAnimDuration = 0.55f;

        [Header("Entry Layout")]
        [SerializeField] private float entryWidth   = 120f;  // match your prefab width in the Inspector
        [SerializeField] private float entrySpacing = 8f;

        private float SlotWidth        => entryWidth + entrySpacing;
        private float GetTargetX(int i) => i * SlotWidth;

        // ── Pool ──────────────────────────────────────────────────────────────
        // Entries are never destroyed — they are recycled. Entry[0] animates out,
        // moves to the back of the list, gets new data, then animates back in.
        private readonly List<TurnOrderEntryUI> pool = new();

        [Header("Panel Toggle Animation")]
        [SerializeField] private float toggleDuration       = 0.45f;
        [SerializeField] private float contentsFadeDuration = 0.2f;

        private bool isOpen = true;
        private float panelOpenY;
        private CanvasGroup panelCanvasGroup;
        private bool panelAnimating = false;
        private bool turnEndAnimating = false;

        private void Awake()
        {
            if (toggleButton != null)
                toggleButton.onClick.AddListener(Toggle);

            if (panelRoot != null)
            {
                panelOpenY       = panelRoot.anchoredPosition.y;
                panelCanvasGroup = panelRoot.GetComponent<CanvasGroup>(); // only use if already present
                if (panelCanvasGroup != null)
                    panelCanvasGroup.blocksRaycasts = false; // never block underlying card clicks
            }

            InitPool();
        }

        private void OnEnable()
        {
            GameEvents.OnTurnStart  += Refresh;
            GameEvents.OnTurnEnd    += AnimateTurnEnd;
            GameEvents.OnCombatStart += Refresh;
        }

        private void OnDisable()
        {
            GameEvents.OnTurnStart  -= Refresh;
            GameEvents.OnTurnEnd    -= AnimateTurnEnd;
            GameEvents.OnCombatStart -= Refresh;
        }

        // ── Pool Init ─────────────────────────────────────────────────────────

        private void InitPool()
        {
            for (int i = 0; i < visibleTurns; i++)
            {
                var entry = Instantiate(entryPrefab, entryContainer);
                entry.gameObject.SetActive(false);
                pool.Add(entry);
            }
        }

        // ── Refresh ───────────────────────────────────────────────────────────
        // Updates all pool entries in-place. No destroy/create.

        private void Refresh()
        {
            turnEndAnimating = false;
            TurnManager tm = TurnManager.Instance;
            if (tm == null || tm.TurnOrder == null || tm.TurnOrder.Count == 0) return;

            int count    = tm.TurnOrder.Count;
            int startIdx = tm.CurrentTurnIndex;

            for (int i = 0; i < pool.Count; i++)
            {
                int idx = (startIdx + i) % count;
                EntityScript entity = tm.TurnOrder[idx];

                if (entity == null)
                {
                    pool[i].gameObject.SetActive(false);
                    continue;
                }

                bool  isCurrent = (i == 0);
                Color color     = isCurrent        ? currentTurnColor
                    : entity.GetComponent<PlayerScript>() != null ? playerTurnColor
                    : enemyTurnColor;

                pool[i].HardReset();
                // Snap to the correct horizontal slot — no LayoutGroup needed
                pool[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(GetTargetX(i), 0f);
                pool[i].Setup(GetEntityName(entity), color, isCurrent, entity);
                pool[i].gameObject.name = entity.gameObject.name;
                pool[i].gameObject.SetActive(true);
            }
        }

        // ── Turn End ──────────────────────────────────────────────────────────

        private void AnimateTurnEnd()
        {
            if (pool.Count == 0) { Refresh(); return; }

            // Guard: rapid turn-end fires before animation finished — snap to clean state.
            if (turnEndAnimating)
            {
                foreach (var e in pool) e.HardReset();
                Refresh();
                return;
            }
            turnEndAnimating = true;

            TurnOrderEntryUI outgoing = pool[0];

            // ── Slide all remaining entries one slot to the left ──────────────
            // Runs simultaneously with the exit animation.
            for (int i = 1; i < pool.Count; i++)
            {
                pool[i].GetComponent<RectTransform>()
                    .DOAnchorPosX(GetTargetX(i - 1), entryAnimDuration)
                    .SetEase(Ease.OutCubic).SetUpdate(true);
            }

            // ── Exit: current-turn entry slides down + fades ──────────────────
            // onComplete fires at exactly entryAnimDuration (= same frame the left-shift finishes).
            outgoing.AnimateOut(entryAnimDuration, onComplete: () =>
            {
                pool.RemoveAt(0);
                pool.Add(outgoing);

                SetupLastPoolEntry(outgoing);
                outgoing.gameObject.SetActive(true);
                outgoing.transform.SetAsLastSibling();

                // ── New entry slides in from the right ────────────────────────
                int lastIdx = pool.Count - 1;
                RectTransform rt = outgoing.GetComponent<RectTransform>();
                CanvasGroup   cg = outgoing.GetComponent<CanvasGroup>()
                                   ?? outgoing.gameObject.AddComponent<CanvasGroup>();

                rt.anchoredPosition = new Vector2(GetTargetX(pool.Count), 0f); // one slot off-screen right
                cg.alpha            = 0f;
                rt.localScale       = Vector3.one;

                rt.DOAnchorPosX(GetTargetX(lastIdx), entryAnimDuration)
                    .SetEase(Ease.OutCubic).SetUpdate(true);
                cg.DOFade(1f, entryAnimDuration * 0.6f)
                    .SetEase(Ease.OutQuart).SetUpdate(true);

                turnEndAnimating = false;
            });
        }

        private void SetupLastPoolEntry(TurnOrderEntryUI entry)
        {
            TurnManager tm = TurnManager.Instance;
            if (tm == null || tm.TurnOrder == null) return;

            int count     = tm.TurnOrder.Count;
            int lastSlot  = pool.Count - 1;                       // position of this entry in the list
            int idx       = (tm.CurrentTurnIndex + lastSlot) % count;
            EntityScript entity = tm.TurnOrder[idx];

            if (entity == null) { entry.gameObject.SetActive(false); return; }

            Color color = entity.GetComponent<PlayerScript>() != null ? playerTurnColor : enemyTurnColor;
            entry.Setup(GetEntityName(entity), color, false, entity);
            entry.gameObject.name = entity.gameObject.name;
            entry.gameObject.SetActive(true);
        }

        // ── Toggle ────────────────────────────────────────────────────────────

        private void Toggle()
        {
            if (panelAnimating || panelRoot == null) return;

            isOpen = !isOpen;

            float slideAmount = panelRoot.rect.height;
            float panelTarget = isOpen ? panelOpenY : panelOpenY + slideAmount;
            Ease  ease        = isOpen ? Ease.OutBack : Ease.InBack;

            panelAnimating = true;

            panelRoot.DOAnchorPosY(panelTarget, toggleDuration)
                .SetEase(ease).SetUpdate(true)
                .OnComplete(() =>
                {
                    panelAnimating = false;
                    // Reset alpha after slide finishes — panel is off-screen at this point,
                    // so the instant reset is invisible. Needed so next open starts from alpha 0.
                    if (!isOpen && panelCanvasGroup != null)
                        panelCanvasGroup.alpha = 0f;
                });

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.DOKill();
                if (isOpen)
                {
                    // Fade in after slide brings the panel into view
                    panelCanvasGroup.alpha = 0f;
                    panelCanvasGroup.DOFade(1f, contentsFadeDuration)
                        .SetDelay(toggleDuration * 0.55f)
                        .SetEase(Ease.OutCubic).SetUpdate(true);
                }
                else
                {
                    // Stay fully visible during slide — panel disappears off-screen naturally.
                    // Alpha reset happens in OnComplete above, after slide is done.
                    panelCanvasGroup.alpha = 1f;
                }
            }

            if (toggleButtonText != null)
                toggleButtonText.text = isOpen ? "▲" : "▼";
                // DOPunchScale removed — button text stays still
        }

        // ── Helpers ───────────────────────────────────────────────────────────

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
    }
}
