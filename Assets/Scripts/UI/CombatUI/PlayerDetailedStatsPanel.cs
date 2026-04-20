using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace facingfate
{
    /// <summary>
    /// Full stat sheet for a player character. Shows all EntityStats values grouped into
    /// sections, each row with a tooltip explanation.
    ///
    /// Setup
    ///   • Attach to a panel GameObject in the combat HUD.
    ///   • Wire the serialized fields in the Inspector.
    ///   • Call Show() / Hide() from a HUD button, or call Toggle().
    ///   • Call Refresh() on GameEvents.OnTurnStart to keep values current.
    /// </summary>
    public class PlayerDetailedStatsPanel : MonoBehaviour
    {
        public static PlayerDetailedStatsPanel Instance { get; private set; }

        [Header("Popup")]
        [SerializeField] private GameObject  panelRoot;
        [SerializeField] private Transform   rowContainer;    // Vertical Layout Group here
        [SerializeField] private StatRowEntryUI rowPrefab;
        // Optional separator prefab shown between sections (just a visual line).
        [SerializeField] private GameObject  sectionHeaderPrefab;

        [Header("Player Navigation")]
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private Button   prevPlayerButton;
        [SerializeField] private Button   nextPlayerButton;

        [Header("Animation")]
        [SerializeField] private float slideDuration = 0.3f;
        [SerializeField] private Ease  slideEase     = Ease.OutQuart;
        [SerializeField] private float slideOffset   = 60f;   // px, panel slides from left

        // ── State ──────────────────────────────────────────────────────────────
        private RectTransform panelRect;
        private Vector2       panelRestPos;
        private CanvasGroup   panelGroup;

        private readonly List<EntityScript>   playerList = new();
        private int                           selectedIndex;
        private EntityScript SelectedPlayer =>
            playerList.Count > 0 && selectedIndex >= 0 && selectedIndex < playerList.Count
                ? playerList[selectedIndex] : null;

        // Cached rows — rebuilt on open, values refreshed each Refresh() call.
        private readonly List<(StatRowEntryUI row, Func<EntityStats, string> getter)> liveRows = new();

        // ── Stat definitions ───────────────────────────────────────────────────

        private readonly struct StatDef
        {
            public readonly string Label;
            public readonly Func<EntityStats, string> Getter;
            public readonly string TipHeader;
            public readonly string TipBody;
            public readonly bool   IsSection;   // true → renders as a section header, no tooltip

            public StatDef(string label, Func<EntityStats, string> getter,
                           string tipHeader, string tipBody)
            { Label = label; Getter = getter; TipHeader = tipHeader; TipBody = tipBody; IsSection = false; }

            // Section header constructor
            public StatDef(string label)
            { Label = label; Getter = null; TipHeader = null; TipBody = null; IsSection = true; }
        }

        private static readonly StatDef[] Definitions =
        {
            // ── Core ──────────────────────────────────────────────────────────
            new("── Core ──"),
            new("Max Health",  s => $"{s.MaxHealth:0}",
                "Max Health",
                "Maximum HP. Calculated as Tenacity × 50, then scaled by MaxHealth modifiers."),
            new("Max Stamina", s => $"{s.MaxStamina:0}",
                "Max Stamina",
                "Maximum Stamina (Action Points). Calculated as Endurance × 5, then scaled by MaxStamina modifiers."),
            new("Block",       s => $"{s.CurrentBlock:0}",
                "Block",
                "Absorbs incoming damage before HP is reduced. Typically resets at the start of each turn."),
            new("Armour",      s => $"{s.CurrentArmour:0}",
                "Armour",
                "Reduces all incoming damage by a flat amount before Block is applied."),

            // ── Attributes ────────────────────────────────────────────────────
            new("── Attributes ──"),
            new("Strength",   s => $"{s.CurrentStrength:0}",
                "Strength",
                "Physical power. Increases the damage of Strength-scaling abilities.\nBase value: 10."),
            new("Dexterity",  s => $"{s.CurrentDexterity:0}",
                "Dexterity",
                "Agility and precision. Increases critical hit chance and may affect evasion.\nBase value: 10."),
            new("Wisdom",     s => $"{s.CurrentWisdom:0}",
                "Wisdom",
                "Magical insight. Increases the power of Wisdom-scaling abilities and status-effect durations.\nBase value: 10."),
            new("Foresight",  s => $"{s.CurrentForesight:0}",
                "Foresight",
                "Perception and initiative. Influences draw order and may unlock special ability thresholds.\nBase value: 10."),
            new("Endurance",  s => $"{s.CurrentEndurance:0}",
                "Endurance",
                "Stamina reserve. Directly determines Max Stamina (Endurance × 5).\nBase value: 10."),
            new("Tenacity",   s => $"{s.CurrentTenacity:0}",
                "Tenacity",
                "Resilience and vitality. Directly determines Max Health (Tenacity × 50).\nBase value: 10."),

            // ── Combat Modifiers ──────────────────────────────────────────────
            new("── Combat ──"),
            new("Damage Dealt",   s => FormatMod(s.DamageOutModifier_Flat.Value(),  s.DamageOutModifier_Increase.Value()),
                "Damage Dealt",
                "Modifies all outgoing damage.\n+Flat adds to the base value; +% increases the result multiplicatively."),
            new("Damage Taken",   s => FormatMod(s.DamageTakenModifier_Flat.Value(), s.DamageTakenModifier_Increase.Value()),
                "Damage Taken",
                "Modifies all incoming damage.\nPositive values increase damage received; negative values reduce it."),
            new("Healing Dealt",  s => FormatMod(s.HealingOutModifier_Flat.Value(),  s.HealingOutModifier_Increase.Value()),
                "Healing Dealt",
                "Modifies all healing this entity applies to others or itself."),
            new("Healing Taken",  s => FormatMod(s.HealingTakenModifier_Flat.Value(), s.HealingTakenModifier_Increase.Value()),
                "Healing Taken",
                "Modifies all incoming healing received by this entity."),
            new("Card Cost",      s => FormatMod(s.CardCostModifier_Flat.Value(),    s.CardCostModifier_Increase.Value()),
                "Card Cost",
                "Modifies the Stamina cost of all played cards. Negative values make cards cheaper."),
            new("Power",          s => FormatMod(s.PowerModifier_Flat.Value(),       s.PowerModifier_Increase.Value()),
                "Power",
                "Global multiplier on the power (base values) of all card effects."),
            new("Duration",       s => FormatMod(s.DurationModifier_Flat.Value(),    s.DurationModifier_Increase.Value()),
                "Duration",
                "Modifies the duration of all status effects applied by this entity."),
            new("Lifesteal",      s => $"{s.Lifesteal.Value():0}%",
                "Lifesteal",
                "Percentage of all damage dealt that is returned as healing to this entity."),
            new("Ignore Armour",  s => $"{s.IgnoreArmour.Value():0}%",
                "Ignore Armour",
                "Percentage of the target's Armour that is bypassed when dealing damage."),
            new("Ignore Block",   s => $"{s.IgnoreBlock.Value():0}%",
                "Ignore Block",
                "Percentage of the target's Block that is bypassed when dealing damage."),

            // ── Range & Area ──────────────────────────────────────────────────
            new("── Range & Area ──"),
            new("Range",       s => FormatMod(s.RangeModifier_Flat.Value(),      s.RangeModifier_Increase.Value()),
                "Range Modifier",
                "Bonus flat range and % increase applied to all card range values."),
            new("Radius",      s => FormatMod(s.RadiusModifier_Flat.Value(),     s.RadiusModifier_Increase.Value()),
                "Radius Modifier",
                "Bonus flat radius and % increase applied to area-of-effect cards."),
            new("Max Targets", s => FormatMod(s.MaxTargetModifier_Flat.Value(),  s.MaxTargetModifier_Increase.Value()),
                "Max Targets Modifier",
                "Additional targets that can be hit by multi-target cards."),
        };

        // ── Lifecycle ──────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (panelRoot != null)
            {
                panelRect    = panelRoot.GetComponent<RectTransform>();
                panelRestPos = panelRect.anchoredPosition;

                panelGroup = panelRoot.GetComponent<CanvasGroup>()
                          ?? panelRoot.AddComponent<CanvasGroup>();
                panelRoot.SetActive(false);
            }

            if (prevPlayerButton != null) prevPlayerButton.onClick.AddListener(PrevPlayer);
            if (nextPlayerButton != null) nextPlayerButton.onClick.AddListener(NextPlayer);
        }

        private void OnEnable()  => GameEvents.OnTurnStart += Refresh;
        private void OnDisable() => GameEvents.OnTurnStart -= Refresh;

        // ── Public API ─────────────────────────────────────────────────────────

        public void Toggle()
        {
            if (panelRoot == null) return;
            if (panelRoot.activeSelf) Hide();
            else Show();
        }

        public void Show()
        {
            if (panelRoot == null) return;
            BuildPlayerList();
            panelRoot.SetActive(true);
            panelGroup.alpha = 0f;
            panelRect.anchoredPosition = new Vector2(panelRestPos.x - slideOffset, panelRestPos.y);

            panelRect.DOAnchorPosX(panelRestPos.x, slideDuration)
                .SetEase(slideEase).SetUpdate(true);
            panelGroup.DOFade(1f, slideDuration * 0.6f)
                .SetUpdate(true);

            UpdateNav();
            RebuildRows();
        }

        public void Hide()
        {
            if (panelRoot == null || !panelRoot.activeSelf) return;

            panelRect.DOAnchorPosX(panelRestPos.x - slideOffset, slideDuration)
                .SetEase(Ease.InQuart).SetUpdate(true);
            panelGroup.DOFade(0f, slideDuration * 0.5f)
                .SetUpdate(true)
                .OnComplete(() => panelRoot.SetActive(false));
        }

        /// <summary>Updates displayed values without rebuilding the layout.</summary>
        public void Refresh()
        {
            if (panelRoot == null || !panelRoot.activeSelf) return;

            EntityStats stats = GetStats(SelectedPlayer);
            if (stats == null) return;

            foreach (var (row, getter) in liveRows)
                row.UpdateValue(getter(stats));
        }

        // ── Player navigation ──────────────────────────────────────────────────

        public void PrevPlayer()
        {
            if (playerList.Count <= 1) return;
            selectedIndex = (selectedIndex - 1 + playerList.Count) % playerList.Count;
            UpdateNav();
            RebuildRows();
        }

        public void NextPlayer()
        {
            if (playerList.Count <= 1) return;
            selectedIndex = (selectedIndex + 1) % playerList.Count;
            UpdateNav();
            RebuildRows();
        }

        // ── Internal ───────────────────────────────────────────────────────────

        private void BuildPlayerList()
        {
            playerList.Clear();
            TurnManager tm = TurnManager.Instance;
            if (tm?.TurnOrder != null)
            {
                foreach (EntityScript e in tm.TurnOrder)
                    if (e != null && e.GetComponent<PlayerScript>() != null)
                        playerList.Add(e);
            }

            // Default to active-turn player
            EntityScript active = GetActivePlayer();
            int idx = active != null ? playerList.IndexOf(active) : -1;
            selectedIndex = idx >= 0 ? idx : 0;
        }

        private void UpdateNav()
        {
            if (playerNameText != null)
                playerNameText.text = SelectedPlayer != null
                    ? GetEntityName(SelectedPlayer) : "—";

            bool multi = playerList.Count > 1;
            if (prevPlayerButton != null) prevPlayerButton.gameObject.SetActive(multi);
            if (nextPlayerButton != null) nextPlayerButton.gameObject.SetActive(multi);
        }

        private void RebuildRows()
        {
            // Clear previous rows
            liveRows.Clear();
            for (int i = rowContainer.childCount - 1; i >= 0; i--)
                Destroy(rowContainer.GetChild(i).gameObject);

            EntityStats stats = GetStats(SelectedPlayer);

            foreach (StatDef def in Definitions)
            {
                if (def.IsSection)
                {
                    // Section header
                    if (sectionHeaderPrefab != null)
                    {
                        GameObject header = Instantiate(sectionHeaderPrefab, rowContainer);
                        TMP_Text t = header.GetComponentInChildren<TMP_Text>();
                        if (t != null) t.text = def.Label;
                    }
                    else if (rowPrefab != null)
                    {
                        // Fallback: reuse row prefab as a dimmed header
                        StatRowEntryUI r = Instantiate(rowPrefab, rowContainer);
                        r.Setup(def.Label, "", "", "");
                    }
                    continue;
                }

                if (rowPrefab == null) continue;

                string value = stats != null ? def.Getter(stats) : "—";
                StatRowEntryUI row = Instantiate(rowPrefab, rowContainer);
                row.Setup(def.Label, value, def.TipHeader, def.TipBody);

                if (stats != null)
                    liveRows.Add((row, def.Getter));
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>
        /// Retrieves EntityStats from an EntityScript via reflection so the panel
        /// does not depend on the exact field visibility.
        /// </summary>
        private static EntityStats GetStats(EntityScript entity)
        {
            if (entity == null) return null;
            FieldInfo field = typeof(EntityScript).GetField(
                "entityStats",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(entity) as EntityStats;
        }

        /// <summary>Formats a flat + percent modifier pair into a compact display string.</summary>
        private static string FormatMod(float flat, float pct)
        {
            bool hasFlat = Mathf.Abs(flat) > 0.01f;
            bool hasPct  = Mathf.Abs(pct)  > 0.01f;
            if (!hasFlat && !hasPct) return "—";
            if (hasFlat && hasPct)   return $"{flat:+0;-0} / {pct:+0;-0}%";
            if (hasFlat)             return $"{flat:+0;-0}";
            return                          $"{pct:+0;-0}%";
        }

        private static EntityScript GetActivePlayer()
        {
            TurnManager tm = TurnManager.Instance;
            if (tm?.TurnOrder == null || tm.TurnOrder.Count == 0) return null;
            int idx = Mathf.Clamp(tm.CurrentTurnIndex, 0, tm.TurnOrder.Count - 1);
            EntityScript current = tm.TurnOrder[idx];
            if (current?.GetComponent<PlayerScript>() != null) return current;
            foreach (EntityScript e in tm.TurnOrder)
                if (e != null && e.GetComponent<PlayerScript>() != null) return e;
            return null;
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
    }
}
