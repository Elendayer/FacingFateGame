using DG.Tweening;
using UnityEngine;

namespace facingfate
{
    /// <summary>
    /// Attach to discardParent or deckParent.
    /// Call Refresh() after every card is added/removed to rebuild the pile look.
    ///
    /// Jitter mode  (discard pile): random offset + rotation per card, top card always straight.
    /// Clean mode   (deck pile):    no jitter, tiny depth offset per card for paper-stack feel.
    /// </summary>
    public class DiscardPileVisualizer : MonoBehaviour
    {
        [Header("Card Display")]
        [Tooltip("RectTransform size each card is forced to inside the pile.")]
        [SerializeField] private Vector2 cardDisplaySize = new Vector2(100f, 140f);

        [Header("Pile Layout")]
        [SerializeField] private int  maxVisibleCards = 4;
        [Tooltip("Enable for discard pile (random offsets + rotation). Disable for clean deck stack.")]
        [SerializeField] private bool jitterEnabled   = true;

        [Header("Jitter (discard mode)")]
        [SerializeField] private float maxOffsetXY  = 8f;    // px random offset per card
        [SerializeField] private float maxRotation  = 12f;   // degrees random rotation

        [Header("Clean Stack (deck mode)")]
        [Tooltip("Tiny Y offset per buried card to give a paper-stack depth illusion.")]
        [SerializeField] private float depthOffsetPerCard = 0.5f;  // px upward per level below top

        [Header("Animation")]
        [SerializeField] private float animDuration = 0.15f;
        [SerializeField] private Ease  animEase     = Ease.OutBack;

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Rebuild the pile visuals. Call after adding a card to the pile.</summary>
        public void Refresh()
        {
            int total = transform.childCount;

            for (int i = 0; i < total; i++)
            {
                Transform child = transform.GetChild(i);
                if (child == null) continue;

                // Stack position from the top: 0 = topmost, 1 = one below, etc.
                int fromTop = total - 1 - i;

                bool visible = fromTop < maxVisibleCards;
                child.gameObject.SetActive(visible);
                if (!visible) continue;

                RectTransform rt = child.GetComponent<RectTransform>();
                if (rt == null) continue;

                rt.DOKill();
                rt.sizeDelta = cardDisplaySize;

                if (!jitterEnabled)
                {
                    // ── Clean stack (deck mode) ──────────────────────────────
                    // Tiny upward shift per buried card → paper-stack depth illusion
                    float dy = fromTop * depthOffsetPerCard;
                    rt.DOAnchorPos(new Vector2(0f, dy), animDuration)
                        .SetEase(animEase).SetUpdate(true);
                    rt.DOLocalRotate(Vector3.zero, animDuration)
                        .SetEase(animEase).SetUpdate(true);
                }
                else if (fromTop == 0)
                {
                    // ── Jitter mode: top card always centred + straight ──────
                    rt.DOAnchorPos(Vector2.zero, animDuration)
                        .SetEase(animEase).SetUpdate(true);
                    rt.DOLocalRotate(Vector3.zero, animDuration)
                        .SetEase(animEase).SetUpdate(true);
                }
                else
                {
                    // ── Jitter mode: buried cards get deterministic random jitter
                    // Seed = child index so each card always gets the same offset
                    Random.State prevState = Random.state;
                    Random.InitState(i * 2053 + 7);

                    float ox  = Random.Range(-maxOffsetXY, maxOffsetXY);
                    float oy  = Random.Range(-maxOffsetXY, maxOffsetXY);
                    float rot = Random.Range(-maxRotation,  maxRotation);

                    Random.state = prevState; // restore global RNG

                    rt.DOAnchorPos(new Vector2(ox, oy), animDuration)
                        .SetEase(animEase).SetUpdate(true);
                    rt.DOLocalRotate(new Vector3(0f, 0f, rot), animDuration)
                        .SetEase(animEase).SetUpdate(true);
                }

                rt.localScale = Vector3.one;

                // Sibling order: topmost card on top visually
                child.SetSiblingIndex(i);
            }
        }
    }
}
