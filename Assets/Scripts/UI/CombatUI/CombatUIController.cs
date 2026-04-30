using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace facingfate
{
    [DisallowMultipleComponent]
    public class CombatUIController : MonoBehaviour
    {
        public static CombatUIController Instance { get; private set; }

        [Header("Managers (optional, otherwise uses Instance)")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private DeckManager deckManager;

        [Header("Hover")]
        [SerializeField] private Camera hoverCamera;
        [SerializeField, Min(0.01f)] private float hoverPollInterval = 0.05f;

        [Header("UI Panels")]
        [SerializeField] private PlayerStatsPanel playerStatsPanel;     // left
        [SerializeField] private HoverTargetPanel hoverTargetPanel;     // top right

        [Header("Refresh Behavior")]
        [Tooltip("If true, deck/discard panels refresh when gameplay reference events fire (draw/discard mid-turn).")]
        [SerializeField] private bool refreshPilesOnGameplayEvents = true;

        [Tooltip("Optional throttle: 0 = no throttle. Example: 0.03 updates max ~33 times/sec.")]
        [SerializeField, Min(0f)] private float minRefreshInterval = 0f;

        private readonly List<EntityScript> cachedEntities = new();

        private float hoverTimer;
        private float lastRefreshTime;

        private EntityScript currentActiveEntity;
        private EntityScript hoveredEntity;

        private EntityScript lockedEntity;  // per Klick gesperrte Entity
        private EntityScript lastHoverOutlined; // für sauberes Outline-Reset


        [Flags]
        private enum DirtyFlags
        {
            None = 0,
            Active = 1 << 0,
            Hover = 1 << 1,
            Piles = 1 << 2,
            All = Active | Hover | Piles
        }

        private DirtyFlags dirty = DirtyFlags.All;

        private void Reset()
        {
            hoverCamera = Camera.main;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (hoverCamera == null) hoverCamera = Camera.main;
        }

        private void OnEnable()
        {
            GameEvents.OnCombatStart += HandleCombatStart;
            GameEvents.OnCombatEnd += HandleCombatEnd;
            GameEvents.OnTurnStart += HandleTurnStart;
            GameEvents.OnTurnEnd += HandleTurnEnd;
            GameEvents.OnGameplayReference += HandleGameplayReference;
            GameEvents.OnActivePlayerChanged += HandleActivePlayerChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnCombatStart -= HandleCombatStart;
            GameEvents.OnCombatEnd -= HandleCombatEnd;
            GameEvents.OnTurnStart -= HandleTurnStart;
            GameEvents.OnTurnEnd -= HandleTurnEnd;
            GameEvents.OnGameplayReference -= HandleGameplayReference;
            GameEvents.OnActivePlayerChanged -= HandleActivePlayerChanged;
        }

        private void Start()
        {
            if (turnManager == null) turnManager = TurnManager.Instance;
            if (deckManager == null) deckManager = DeckManager.Instance;

            CacheEntities();
            MarkDirty(DirtyFlags.All);
        }

        private void Update()
        {
            HandleEntityClick();
            UpdateHover();

            ApplyRefreshIfDirty();
        }

        public void RefreshAll()
        {
            MarkDirty(DirtyFlags.All);
        }

        private void UpdateHover()
        {
            hoverTimer += Time.unscaledDeltaTime;
            if (hoverTimer < hoverPollInterval) return;
            hoverTimer = 0f;

            // Wenn eine Entity gelockt ist, bleibt der HoverTargetPanel darauf
            if (lockedEntity != null)
            {
                if (hoveredEntity != lockedEntity)
                {
                    hoveredEntity = lockedEntity;
                    MarkDirty(DirtyFlags.Hover);
                }
                return;
            }

            // Normales Hovern
            EntityScript found = FindHoveredEntity();

            if (found != lastHoverOutlined)
            {
                if (lastHoverOutlined != null)
                    SetOutline(lastHoverOutlined, OutlineState.Normal);

                lastHoverOutlined = found;

                if (found != null)
                    SetOutline(found, OutlineState.Hover);
            }

            if (found != hoveredEntity)
            {
                hoveredEntity = found;
                MarkDirty(DirtyFlags.Hover);
            }

            if (hoveredEntity != null && hoveredEntity.gameObject == null)
            {
                hoveredEntity = null;
                MarkDirty(DirtyFlags.Hover);
            }
        }

        private void HandleEntityClick()
        {
            if (!InputManager.Instance.IsLeftMouseButtonPressed) return;

            EntityScript clicked = FindHoveredEntity();
            if (clicked == null) return;

            if (lockedEntity == clicked)
            {
                SetOutline(lockedEntity, OutlineState.Normal);
                lockedEntity = null;
            }
            else
            {
                if (lockedEntity != null)
                    SetOutline(lockedEntity, OutlineState.Normal);

                lockedEntity = clicked;
                SetOutline(lockedEntity, OutlineState.Locked);
            }

            hoveredEntity = lockedEntity ?? clicked;
            MarkDirty(DirtyFlags.Hover);
        }

        private EntityScript FindHoveredEntity()
        {
            Vector2 mousePos = InputManager.Instance.MousePositionScreen;

            // Guard gegen (inf, inf) / NaN
            if (!IsFinite(mousePos)) return null;

            // Optional: nur wenn im Game-Fenster
            if (mousePos.x < 0f || mousePos.y < 0f || mousePos.x > Screen.width || mousePos.y > Screen.height)
                return null;

            // Use InputManager's raycast to detect entity under mouse
            RaycastHit[] hits = InputManager.Instance.RaycastAllFromMouse();

            if (hits.Length == 0)
                return null;

            EntityScript enemyCandidate = null;
            EntityScript playerCandidate = null;

            // Iterate through all hits and prioritize enemies over player
            for (int i = 0; i < hits.Length; i++)
            {
                EntityScript entity = hits[i].collider.GetComponent<EntityScript>();
                if (entity == null) continue;

                // Gegner bevorzugen: alles ohne PlayerScript ist "enemy candidate"
                if (entity.GetComponent<PlayerScript>() == null)
                {
                    if (enemyCandidate == null)
                        enemyCandidate = entity;
                }
                else
                {
                    if (playerCandidate == null)
                        playerCandidate = entity;
                }

                // Return enemy immediately if found (prioritized)
                if (enemyCandidate != null)
                    return enemyCandidate;
            }

            return playerCandidate;
        }

        private static bool IsFinite(Vector3 v)
        {
            return IsFinite(v.x) && IsFinite(v.y) && IsFinite(v.z);
        }

        private static bool IsFinite(Vector2 v)
        {
            return IsFinite(v.x) && IsFinite(v.y);
        }

        private static bool IsFinite(float f)
        {
            return !(float.IsNaN(f) || float.IsInfinity(f));
        }

        private void CacheEntities()
        {
            cachedEntities.Clear();

            EntityScript[] all = FindObjectsByType<EntityScript>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null)
                    cachedEntities.Add(all[i]);
            }
        }

        private void ApplyRefreshIfDirty()
        {
            if (dirty == DirtyFlags.None) return;

            if (minRefreshInterval > 0f)
            {
                if (Time.unscaledTime - lastRefreshTime < minRefreshInterval)
                    return;
            }

            // Active (left panel + end turn)
            if ((dirty & DirtyFlags.Active) != 0)
            {
                if (playerStatsPanel != null)
                {
                    playerStatsPanel.Bind(currentActiveEntity);
                    playerStatsPanel.Refresh();
                }
            }

            // Piles (deck/discard preview)
            if ((dirty & DirtyFlags.Piles) != 0)
            {
                CardPilePeekPanel.Instance?.Refresh();
            }

            // Hover (top right)
            if ((dirty & DirtyFlags.Hover) != 0)
            {
                if (hoverTargetPanel != null)
                {
                    hoverTargetPanel.Bind(hoveredEntity);
                    hoverTargetPanel.Refresh();

                    //hoverTargetPanel.gameObject.SetActive(hoveredEntity != null);
                }
            }

            dirty = DirtyFlags.None;
            lastRefreshTime = Time.unscaledTime;
        }

        private void MarkDirty(DirtyFlags flags)
        {
            dirty |= flags;
        }

        private void HandleCombatStart()
        {
            CacheEntities();
            MarkDirty(DirtyFlags.All);
        }

        private void HandleCombatEnd(bool playerWon)
        {
            // Tutorial wave transitions fire CombatEnd between waves — keep UI state intact.
            if (TutorialCombatManager.Instance != null && TutorialCombatManager.Instance.IsActive)
                return;

            hoveredEntity = null;
            currentActiveEntity = null;

            CacheEntities();
            MarkDirty(DirtyFlags.All);
        }

        private void HandleTurnStart()
        {
            // Active: ensures stamina display refreshes when a new turn begins.
            // Brief 1-frame stale read is acceptable; card-draw GameplayReference events
            // trigger a second Active refresh with fully correct values.
            MarkDirty(DirtyFlags.Active | DirtyFlags.Piles);
        }

        private void HandleTurnEnd()
        {
            MarkDirty(DirtyFlags.Active | DirtyFlags.Piles);
        }

        private void HandleGameplayReference(ToSendTriggerReference _)
        {
            // Coalesce: refresh once per frame (dirty flag). No direct UI writes here.
            MarkDirty(DirtyFlags.Active | DirtyFlags.Hover);

            if (refreshPilesOnGameplayEvents)
                MarkDirty(DirtyFlags.Piles);
        }

        private enum OutlineState { Normal, Hover, Locked }

        private static void SetOutline(EntityScript entity, OutlineState state)
        {
            if (entity == null) return;
            EntityOutline outline = entity.GetComponent<EntityOutline>();
            if (outline == null) return;

            switch (state)
            {
                case OutlineState.Hover: outline.SetHover(true); break;
                case OutlineState.Locked: outline.SetLocked(true); break;
                case OutlineState.Normal: outline.SetNormal(); break;
            }
        }
        private void HandleActivePlayerChanged(EntityScript entity)
        {
            currentActiveEntity = entity;
            MarkDirty(DirtyFlags.Active);
        }
    }
}