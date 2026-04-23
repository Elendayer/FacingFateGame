using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

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

        private readonly List<ArrowInstance> _pool = new();

        private void Awake() => HideAll();

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
                    _pool[i].ShowWorld(entry.worldTarget, entry.direction, arrowDistance);
                }
                else if (entry.target != null)
                {
                    _pool[i].Show(entry.target, entry.direction, arrowDistance);
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
                _pool.Add(new ArrowInstance(go));
            }
        }

        // ── Inner instance wrapper ─────────────────────────────────────────────

        private class ArrowInstance
        {
            private readonly GameObject _root;
            private readonly RectTransform _arrow;
            private readonly PointerPositionScript _tracker;

            public ArrowInstance(GameObject root)
            {
                _root    = root;
                _arrow   = root.transform.Find("Arrow")?.GetComponent<RectTransform>();
                _tracker = _arrow?.GetComponent<PointerPositionScript>();
            }

            /// <summary>Show arrow pointing at a UI RectTransform (static, one-shot position).</summary>
            public void Show(RectTransform target, ArrowDirection dir, float dist)
            {
                if (_tracker != null) _tracker.enabled = false;

                _root.SetActive(true);
                if (_arrow == null) return;

                _arrow.position      = target.position + (Vector3)GetOffset(target, dir, dist);
                _arrow.localRotation = Quaternion.Euler(0f, 0f, GetRotation(dir));
                _arrow.DOKill();
                _arrow.DOScale(1.2f, 0.55f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }

            /// <summary>Show arrow tracking a world-space Transform every frame.</summary>
            public void ShowWorld(Transform worldTarget, ArrowDirection dir, float dist)
            {
                _root.SetActive(true);
                if (_arrow == null || _tracker == null) return;

                _tracker.SetTarget(worldTarget, dir, dist);
                _tracker.enabled = true;

                _arrow.localRotation = Quaternion.Euler(0f, 0f, GetRotation(dir));
                _arrow.DOKill();
                _arrow.DOScale(1.2f, 0.55f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }

            public void SetActive(bool active)
            {
                if (_tracker != null) _tracker.enabled = false;
                _arrow?.DOKill();
                _root.SetActive(active);
            }
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
