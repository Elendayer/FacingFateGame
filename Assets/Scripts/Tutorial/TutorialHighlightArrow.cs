using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace facingfate
{
    /// <summary>
    /// Manages N animated arrows, one per TutorialHighlightEntry.
    /// Instantiates arrow instances from a prefab on demand; reuses or hides extras.
    /// Arrow prefab root must have a child named "Arrow" (RectTransform + Image).
    /// Arrow sprite must point RIGHT at 0° rotation.
    /// </summary>
    public class TutorialHighlightArrow : MonoBehaviour
    {
        [Header("Prefab")]
        [Tooltip("Prefab with a child named 'Arrow' (RectTransform). Sprite must point right at 0°.")]
        [SerializeField] private GameObject arrowPrefab;

        [Header("Settings")]
        [Tooltip("Distance in pixels from target edge to arrow center.")]
        [SerializeField] private float arrowDistance = 60f;

        private const float VisibilityThreshold = 0.05f;

        private readonly List<ArrowInstance> _pool = new();

        private void Awake() => HideAll();

        private void Update()
        {
            foreach (var inst in _pool)
                inst.UpdateVisibilityBinding();
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Show one arrow per entry. Hides unused pool instances.</summary>
        public void PointAt(TutorialHighlightEntry[] entries)
        {
            if (entries == null || entries.Length == 0) { HideAll(); return; }

            EnsurePoolSize(entries.Length);

            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];

                if (entry.worldTarget != null)
                {
                    _pool[i].ShowWorld(entry.worldTarget, entry.direction, arrowDistance, entry.worldTargetOffset, entry.visibilityDrivenBy);
                }
                else if (entry.target != null)
                {
                    _pool[i].Show(entry.target, entry.direction, arrowDistance, entry.visibilityDrivenBy);
                }
                else
                {
                    _pool[i].SetActive(false);
                }
            }

            for (int i = entries.Length; i < _pool.Count; i++)
                _pool[i].SetActive(false);
        }

        public void HideAll()
        {
            foreach (var inst in _pool) inst.SetActive(false);
        }

        public void Hide() => HideAll();

        // ── Pool management ────────────────────────────────────────────────────

        private void EnsurePoolSize(int required)
        {
            while (_pool.Count < required)
            {
                var go = Instantiate(arrowPrefab, transform);
                go.SetActive(false);
                _pool.Add(new ArrowInstance(go, VisibilityThreshold));
            }
        }

        // ── Inner instance wrapper ─────────────────────────────────────────────

        private class ArrowInstance
        {
            private readonly GameObject _root;
            private readonly RectTransform _arrow;
            private readonly PointerPositionScript _tracker;
            private readonly float _visThreshold;

            private CanvasGroup _visibilityBinding;
            private bool _wantsActive;          // true = would be shown if no binding blocks it
            private ArrowDirection _lastDir;    // stored so UpdateVisibilityBinding can replay pulse

            public ArrowInstance(GameObject root, float visThreshold)
            {
                _root         = root;
                _arrow        = root.transform.Find("Arrow")?.GetComponent<RectTransform>();
                _tracker      = _arrow?.GetComponent<PointerPositionScript>();
                _visThreshold = visThreshold;
            }

            /// <summary>Show arrow pointing at a UI RectTransform (static, one-shot position).</summary>
            public void Show(RectTransform target, ArrowDirection dir, float dist, CanvasGroup binding = null)
            {
                _visibilityBinding = binding;
                _wantsActive       = true;

                if (_tracker != null) _tracker.enabled = false;

                // Always set position — even if binding hides the arrow now,
                // UpdateVisibilityBinding re-enables root later and position must be correct.
                if (_arrow != null)
                {
                    _arrow.position      = target.position + (Vector3)GetOffset(target, dir, dist);
                    _arrow.localRotation = Quaternion.Euler(0f, 0f, GetRotation(dir));
                }

                _lastDir = dir;

                bool shouldShow = binding == null || binding.alpha > _visThreshold;
                _root.SetActive(shouldShow);
                if (shouldShow && _arrow != null)
                    PlayPulse(_arrow, dir);
            }

            /// <summary>Show arrow tracking a world-space Transform every frame.</summary>
            public void ShowWorld(Transform worldTarget, ArrowDirection dir, float dist, Vector2 instanceOffset = default, CanvasGroup binding = null)
            {
                _visibilityBinding = binding;
                _wantsActive       = true;
                _lastDir           = dir;

                bool shouldShow = binding == null || binding.alpha > _visThreshold;
                _root.SetActive(shouldShow);
                if (_arrow == null || _tracker == null) return;

                _tracker.SetTarget(worldTarget, dir, dist, instanceOffset);
                _tracker.enabled = shouldShow;

                _arrow.localRotation = Quaternion.Euler(0f, 0f, GetRotation(dir));
                if (shouldShow)
                    PlayPulse(_arrow, dir);
            }

            /// <summary>Called every frame by TutorialHighlightArrow.Update() to sync binding visibility.</summary>
            public void UpdateVisibilityBinding()
            {
                if (!_wantsActive || _visibilityBinding == null) return;

                bool shouldShow = _visibilityBinding.alpha > _visThreshold;
                if (_root.activeSelf == shouldShow) return;

                if (shouldShow)
                {
                    _root.SetActive(true);
                    if (_tracker != null) _tracker.enabled = true;
                    if (_arrow != null) PlayPulse(_arrow, _lastDir);
                }
                else
                {
                    if (_tracker != null) _tracker.enabled = false;
                    StopPulse(_arrow);
                    _root.SetActive(false);
                }
            }

            public void SetActive(bool active)
            {
                _wantsActive       = active;
                _visibilityBinding = null;
                if (_tracker != null) _tracker.enabled = false;
                StopPulse(_arrow);
                _root.SetActive(active);
            }

            // ── Pulse helpers ──────────────────────────────────────────────────

            private static readonly string TweenId = "ArrowPulse";

            private static void PlayPulse(RectTransform arrow, ArrowDirection dir)
            {
                // Kill previous tweens and reset to clean state
                arrow.DOKill();
                arrow.localScale    = Vector3.one;
                arrow.localPosition = Vector3.zero;

                // Scale pulse: 1.0 → 1.18 → 1.0, looping
                arrow.DOScale(1.18f, 0.55f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetUpdate(true);

                // Position bob: arrow nudges 10px in its pointing direction
                Vector3 bobOffset = GetBobOffset(dir, 10f);
                arrow.DOLocalMove(bobOffset, 0.55f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetUpdate(true);
            }

            private static void StopPulse(RectTransform arrow)
            {
                if (arrow == null) return;
                arrow.DOKill();
                arrow.localScale    = Vector3.one;
                arrow.localPosition = Vector3.zero;
            }

            /// <summary>Returns a local-space offset in the direction the arrow points.</summary>
            private static Vector3 GetBobOffset(ArrowDirection dir, float amount) => dir switch
            {
                ArrowDirection.Left  => new Vector3(-amount,  0f, 0f),
                ArrowDirection.Right => new Vector3( amount,  0f, 0f),
                ArrowDirection.Up    => new Vector3(0f,  amount,  0f),
                ArrowDirection.Down  => new Vector3(0f, -amount,  0f),
                _                    => new Vector3(-amount,  0f, 0f),
            };
        }

        // ── Direction helpers ──────────────────────────────────────────────────

        private static Vector2 GetOffset(RectTransform target, ArrowDirection dir, float dist)
        {
            float hw = target.sizeDelta.x * 0.5f;
            float hh = target.sizeDelta.y * 0.5f;
            return dir switch
            {
                ArrowDirection.Left  => new Vector2(-(hw + dist),  0f),
                ArrowDirection.Right => new Vector2(  hw + dist,   0f),
                ArrowDirection.Up    => new Vector2(0f,   hh + dist),
                ArrowDirection.Down  => new Vector2(0f, -(hh + dist)),
                _                    => new Vector2(-(hw + dist),  0f),
            };
        }

        /// <summary>Arrow sprite assumed to point RIGHT at 0°.</summary>
        private static float GetRotation(ArrowDirection dir) => dir switch
        {
            ArrowDirection.Left  =>   0f,
            ArrowDirection.Right => 180f,
            ArrowDirection.Up    => 270f,
            ArrowDirection.Down  =>  90f,
            _                    =>   0f,
        };
    }
}
