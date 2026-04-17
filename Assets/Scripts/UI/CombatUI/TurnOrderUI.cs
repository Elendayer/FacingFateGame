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

        [Header("Panel Toggle Animation")]
        [SerializeField] private float toggleDuration       = 0.45f;
        [SerializeField] private float contentsFadeDuration = 0.2f;

        // ── Pool ──────────────────────────────────────────────────────────────
        // Entries are never destroyed — they are recycled. Entry[0] animates out,
        // moves to the back of the list, gets new data, then animates back in.
        private readonly List<TurnOrderEntryUI> pool = new();

        private bool isOpen = true;
        private float panelOpenY;
        private float buttonOpenY;
        private RectTransform toggleButtonRect;
        private CanvasGroup panelCanvasGroup;
        private bool panelAnimating = false;

        private void Awake()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(Toggle);
                toggleButtonRect = toggleButton.GetComponent<RectTransform>();
            }

            if (panelRoot != null)
            {
                panelOpenY       = panelRoot.anchoredPosition.y;
                panelCanvasGroup = panelRoot.GetComponent<CanvasGroup>(); // only use if already present
                if (panelCanvasGroup != null)
                    panelCanvasGroup.blocksRaycasts = false; // never block underlying card clicks
            }

            if (toggleButtonRect != null)
                buttonOpenY = toggleButtonRect.anchoredPosition.y;

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
                pool[i].Setup(GetEntityName(entity), color, isCurrent, entity);
                pool[i].gameObject.name = entity.gameObject.name;
                pool[i].gameObject.SetActive(true);
            }
        }

        // ── Turn End ──────────────────────────────────────────────────────────

        private void AnimateTurnEnd()
        {
            if (pool.Count == 0) { Refresh(); return; }

            TurnOrderEntryUI outgoing = pool[0];

            // Move outgoing to back of logical list immediately
            pool.RemoveAt(0);
            pool.Add(outgoing);

            // Pin the current width so the layout doesn't shift until we're ready
            var layoutEl = outgoing.GetComponent<LayoutElement>();
            if (layoutEl == null) layoutEl = outgoing.gameObject.AddComponent<LayoutElement>();
            float entryWidth = outgoing.GetComponent<RectTransform>().rect.width;
            layoutEl.preferredWidth  = entryWidth;
            layoutEl.ignoreLayout    = false;

            // Slide down + fade
            outgoing.AnimateOut(entryAnimDuration, onComplete: () =>
            {
                DOTween.Kill(layoutEl);
                layoutEl.preferredWidth = -1f;         // hand sizing back to the layout
                outgoing.transform.SetAsLastSibling();
                SetupLastPoolEntry(outgoing);
                outgoing.AnimateIn(entryAnimDuration);
            });

            // After the slide finishes, collapse the reserved width slowly so the
            // remaining entries drift into place rather than jumping.
            float collapseDelay = entryAnimDuration * 0.60f;   // wait for slide to almost finish
            float collapseDur   = entryAnimDuration * 0.55f;   // then ease the gap closed
            DOTween.To(() => layoutEl.preferredWidth,
                       x  => layoutEl.preferredWidth = x,
                       0f, collapseDur)
                .SetDelay(collapseDelay)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true)
                .SetTarget(layoutEl);
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

            float slideAmount  = panelRoot.rect.height;
            float panelTarget  = isOpen ? panelOpenY  : panelOpenY  + slideAmount;
            float buttonTarget = isOpen ? buttonOpenY : buttonOpenY + slideAmount;
            Ease  ease         = isOpen ? Ease.OutBack : Ease.InBack;

            panelAnimating = true;

            panelRoot.DOAnchorPosY(panelTarget, toggleDuration)
                .SetEase(ease).SetUpdate(true)
                .OnComplete(() => panelAnimating = false);

            toggleButtonRect?.DOAnchorPosY(buttonTarget, toggleDuration)
                .SetEase(ease).SetUpdate(true);

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.DOKill();
                if (isOpen)
                {
                    panelCanvasGroup.alpha = 0f;
                    panelCanvasGroup.DOFade(1f, contentsFadeDuration)
                        .SetDelay(toggleDuration * 0.55f)
                        .SetEase(Ease.OutCubic).SetUpdate(true);
                }
                else
                {
                    panelCanvasGroup.DOFade(0f, contentsFadeDuration)
                        .SetEase(Ease.InCubic).SetUpdate(true);
                }
            }

            if (toggleButtonText != null)
            {
                toggleButtonText.text = isOpen ? "▲" : "▼";
                toggleButtonText.transform.DOKill();
                toggleButtonText.transform.localScale = Vector3.one;
                toggleButtonText.transform
                    .DOPunchScale(Vector3.one * 0.35f, 0.3f, vibrato: 1, elasticity: 0.5f)
                    .SetUpdate(true);
            }
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
