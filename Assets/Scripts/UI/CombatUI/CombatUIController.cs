using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility;

namespace facingfate
{
    [DisallowMultipleComponent]
    public class CombatUIController : MonoBehaviour
    {
        [Header("Managers (optional, otherwise uses Instance)")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private DeckManager deckManager;

        [Header("Hover")]
        [SerializeField] private Camera hoverCamera;
        [SerializeField, Min(0.01f)] private float hoverPollInterval = 0.05f;

        [Header("UI Panels")]
        [SerializeField] private PlayerStatsPanel playerStatsPanel;     // left
        [SerializeField] private CardPilePeekPanel deckPanel;           // deck
        [SerializeField] private CardPilePeekPanel discardPanel;        // discard
        [SerializeField] private EndTurnPanel endTurnPanel;             // end turn
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
            WirePileRootsIfPossible();

            MarkDirty(DirtyFlags.All);
        }

        private void Update()
        {
            //HandleEntityClick();
           // UpdateHover();

            //ApplyRefreshIfDirty();
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
            if (!InputSystem.actions.FindAction("LeftClick").enabled) return;

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
            if (hoverCamera == null) return null;

            Vector3 mousePos = Mouse.current.position.ReadValue();

            // Guard gegen (inf, inf) / NaN
            if (!IsFinite(mousePos)) return null;

            // Optional: nur wenn im Game-Fenster
            if (mousePos.x < 0f || mousePos.y < 0f || mousePos.x > Screen.width || mousePos.y > Screen.height)
                return null;

            Ray ray = hoverCamera.ScreenPointToRay(mousePos);



            Vector3Int cell = TargetingUtility.GetHoveredTile(ray);

            if (cell == TilemapUtilityScript.InvalidPosition)
                return null;

            if (cachedEntities.Count == 0)
                CacheEntities();

            // Wenn mehrere Entities auf einem Tile sind: Gegner bevorzugen
            EntityScript playerCandidate = null;

            for (int i = 0; i < cachedEntities.Count; i++)
            {
                EntityScript e = cachedEntities[i];
                if (e == null) continue;

                EntityOnMap eom = e.GetComponent<EntityOnMap>();
                if (eom == null) continue;

                if (eom.currentCell != cell) continue;

                // Gegner bevorzugen: alles ohne PlayerScript ist "enemy candidate"
                if (e.GetComponent<PlayerScript>() == null)
                    return e;

                playerCandidate = e;
            }

            return playerCandidate;
        }

        private static bool IsFinite(Vector3 v)
        {
            return IsFinite(v.x) && IsFinite(v.y) && IsFinite(v.z);
        }

        private static bool IsFinite(float f)
        {
            return !(float.IsNaN(f) || float.IsInfinity(f));
        }

        private EntityScript GetActiveEntitySafe()
        {
            TurnManager tm = turnManager != null ? turnManager : TurnManager.Instance;
            if (tm == null) return null;

            if (tm.TurnOrder == null || tm.TurnOrder.Count == 0)
                return null;

            int idx = tm.CurrentTurnIndex;
            if (idx < 0 || idx >= tm.TurnOrder.Count)
                idx = 0;

            return tm.TurnOrder[idx];
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

        private void WirePileRootsIfPossible()
        {
            DeckManager dm = deckManager != null ? deckManager : DeckManager.Instance;
            if (dm == null) return;

            if (deckPanel != null && deckPanel.PileRoot == null && dm.deckParent != null)
                deckPanel.SetPileRoot(dm.deckParent);

            if (discardPanel != null && discardPanel.PileRoot == null && dm.discardParent != null)
                discardPanel.SetPileRoot(dm.discardParent);
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

                endTurnPanel?.RefreshInteractable(currentActiveEntity);
            }

            // Piles (deck/discard preview)
            if ((dirty & DirtyFlags.Piles) != 0)
            {
                deckPanel?.Refresh();
                discardPanel?.Refresh();
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
            WirePileRootsIfPossible();
            MarkDirty(DirtyFlags.All);
        }

        private void HandleCombatEnd()
        {
            hoveredEntity = null;
            currentActiveEntity = null;

            CacheEntities();
            MarkDirty(DirtyFlags.All);
        }

        private void HandleTurnStart()
        {
            MarkDirty(DirtyFlags.Piles);
        }

        private void HandleTurnEnd()
        {
            MarkDirty(DirtyFlags.Piles);
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