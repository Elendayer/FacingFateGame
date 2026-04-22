using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace facingfate
{
    public class PlayerDetailedStatsPanel : MonoBehaviour
    {
        private enum StatCategory
        {
            None,
            Core,
            Attribute,
            Offensive,
            Defensive,
        }
        public static PlayerDetailedStatsPanel Instance { get; private set; }

        [Header("Popup")]
        [SerializeField] private GameObject     panelRoot;
        [SerializeField] private Transform      rowContainer;       // VerticalLayoutGroup here
        [SerializeField] private GameObject     statRowPrefab;
        [SerializeField] private GameObject     statHeaderPrefab;
        [SerializeField] private GameObject     sectionHeaderRowPrefab;

        [Header("Entity Navigation")]
        [SerializeField] private TMP_Text entityNameText;
        [SerializeField] private Button   prevButton;
        [SerializeField] private Button   nextButton;

        [Header("Tab Highlights (optional)")]
        [SerializeField] private GameObject partyTabHighlight;
        [SerializeField] private GameObject enemiesTabHighlight;

        [Header("Animation")]
        [SerializeField] private float slideDuration = 0.3f;
        [SerializeField] private Ease  slideEase     = Ease.OutQuart;
        [SerializeField] private float slideOffset   = 60f;

        [Header("Input")]
        [SerializeField] private KeyCode toggleKey = KeyCode.C;

        // ── State ──────────────────────────────────────────────────────────────
        private RectTransform panelRect;
        private Vector2       panelRestPos;
        private CanvasGroup   panelGroup;

        private readonly List<EntityScript> partyList  = new();
        private readonly List<EntityScript> enemyList  = new();
        private bool showingEnemies;
        private int  selectedIndex;

        private List<EntityScript> ActiveList => showingEnemies ? enemyList : partyList;
        private EntityScript SelectedEntity =>
            ActiveList.Count > 0 && selectedIndex >= 0 && selectedIndex < ActiveList.Count
                ? ActiveList[selectedIndex] : null;

        private readonly List<(StatRowEntryUI row, Func<EntityStats, string> getter)> liveRows = new();
        private readonly List<(StatRowEntryUI row, ConditionalModifierDef modifier)> liveConditionalRows = new();
        private readonly List<(StatHeaderEntryUI header, Func<EntityStats, string> getter)> liveHeaderRows = new();

        // ── Stat definitions ───────────────────────────────────────────────────
        private readonly struct StatDef
        {
            public readonly string Label;
            public readonly Func<EntityStats, string> Getter;
            public readonly string TipHeader;
            public readonly string TipBody;
            public readonly bool   IsSection;
            public readonly StatCategory Category;
            public readonly Func<EntityStats, bool> ShouldDisplay;
            public readonly Func<EntityStats, Stat> StatReference;

            public StatDef(string label, Func<EntityStats, string> getter,
                           string tipHeader, string tipBody, StatCategory category = StatCategory.None)
            { Label = label; Getter = getter; TipHeader = tipHeader; TipBody = tipBody; IsSection = false; Category = category; ShouldDisplay = null; StatReference = null; }

            public StatDef(string label, StatCategory category = StatCategory.None)
            { Label = label; Getter = null; TipHeader = null; TipBody = null; IsSection = true; Category = category; ShouldDisplay = null; StatReference = null; }

            public StatDef(string label, Func<EntityStats, string> getter,
                           string tipHeader, string tipBody, Func<EntityStats, bool> shouldDisplay)
            { Label = label; Getter = getter; TipHeader = tipHeader; TipBody = tipBody; IsSection = false; Category = StatCategory.None; ShouldDisplay = shouldDisplay; StatReference = null; }

            public StatDef(string label, Func<EntityStats, string> getter,
                           string tipHeader, string tipBody, Func<EntityStats, Stat> statReference)
            { Label = label; Getter = getter; TipHeader = tipHeader; TipBody = tipBody; IsSection = false; Category = StatCategory.None; ShouldDisplay = null; StatReference = statReference; }
        }

        /// <summary>Represents a conditional modifier entry with its metadata</summary>
        private readonly struct ConditionalModifierDef
        {
            public readonly string ModifierName;
            public readonly Func<EntityStats, string> Getter;
            public readonly ConditionalModifierInfo Condition;
            public readonly string ModifierType;  // "Flat", "Increase", "Multiplier"

            public ConditionalModifierDef(string modifierName, Func<EntityStats, string> getter, ConditionalModifierInfo condition, string modifierType = "")
            {
                ModifierName = modifierName;
                Getter = getter;
                Condition = condition;
                ModifierType = modifierType;
            }

            public string GetDisplayLabel()
            {
                string baseLabel = Condition?.DisplayName ?? ModifierName;
                if (!string.IsNullOrEmpty(ModifierType))
                    return $"    {baseLabel} ({ModifierType})";
                return $"    {baseLabel}";
            }

            public string GetDisplayValue(EntityStats stats) => Getter?.Invoke(stats) ?? "—";
            public string GetDisplayTooltipHeader() => Condition?.DisplayName ?? ModifierName;
            public string GetDisplayTooltipBody() => Condition?.Description ?? "";
        }

        private static readonly StatDef[] Definitions =
        {
            // ── Core ──────────────────────────────────────────────────────────
            new("── Core ──"),
            new("Health",    s => $"{s.CurrentHealth:0} / {s.MaxHealth:0}",
                "Health",
                "Current and maximum HP. Max = Tenacity × 50, scaled by MaxHealth modifiers."),
            new("Stamina",   s => $"{s.CurrentStamina:0} / {s.MaxStamina:0}",
                "Stamina",
                "Current and maximum Stamina (Action Points). Max = Endurance × 5."),
            new("Block",     s => $"{s.CurrentBlock:0}",
                "Block",
                "Absorbs incoming damage before HP is reduced. Resets at turn start."),
            new("Armour",    s => $"{s.CurrentArmour:0}",
                "Armour",
                "Reduces all incoming damage by a flat amount before Block is applied."),

            // ── Attributes ────────────────────────────────────────────────────
            new("Attributes", StatCategory.Attribute),

            new("Strength",     s => $"{s.CurrentStrength:0.0}",
                "Strength",  "Final calculated Strength value including all modifiers.", s => s.Strength_Flat),
            new("Dexterity",     s => $"{s.CurrentDexterity:0.0}",
                "Dexterity",  "Final calculated Dexterity value including all modifiers.", s => s.Dexterity_Flat),
            new("Wisdom",       s => $"{s.CurrentWisdom:0.0}",
                "Wisdom",  "Final calculated Wisdom value including all modifiers.", s => s.Wisdom_Flat),
            new("Foresight",    s => $"{s.CurrentIntelligence:0.0}",
                "Foresight",  "Final calculated Foresight value including all modifiers.", s => s.Intelligence_Flat),
            new("Endurance",    s => $"{s.CurrentEndurance:0.0}",
                "Endurance",  "Final calculated Endurance value including all modifiers.", s => s.Endurance_Flat),
            new("Tenacity",     s => $"{s.CurrentTenacity:0.0}",
                "Tenacity",  "Final calculated Tenacity value including all modifiers.", s => s.Tenacity_Flat),

            // ── Combat Modifiers ──────────────────────────────────────────────
            new("── Combat ──"),
            new("── Offensive ──", StatCategory.Offensive),

            new("Damage Dealt", s => FormatMod(s.DamageOutModifier_Flat.Value(), s.DamageOutModifier_Increase.Value()),
                "Damage Dealt", "Increases damage dealt. Flat = direct bonus, % = percentage increase.", s => s.DamageOutModifier_Increase),

            new("Healing Dealt", s => FormatMod(s.HealingOutModifier_Flat.Value(), s.HealingOutModifier_Increase.Value()),
                "Healing Dealt", "Increases healing dealt. Flat = direct bonus, % = percentage increase.", s => s.HealingOutModifier_Increase),

            new("Power",         s => FormatMod(s.PowerModifier_Flat.Value(),        s.PowerModifier_Increase.Value()),
                "Power",         "Global multiplier on the power of all card effects.", s => s.PowerModifier_Increase),

            new("Duration",      s => FormatMod(s.DurationModifier_Flat.Value(),     s.DurationModifier_Increase.Value()),
                "Duration",      "Modifies duration of all status effects applied by this entity.", s => s.DurationModifier_Increase),

            new("Lifesteal",     s => $"{s.Lifesteal.Value():0}%",
                "Lifesteal",     "% of all damage dealt returned as healing to this entity.", s => s.Lifesteal),

            new("Ignore Armour", s => $"{s.IgnoreArmour.Value():0}%",
                "Ignore Armour", "% of target's Armour bypassed when dealing damage.", s => s.IgnoreArmour),

            new("Ignore Block",  s => $"{s.IgnoreBlock.Value():0}%",
                "Ignore Block",  "% of target's Block bypassed when dealing damage.", s => s.IgnoreBlock),

            new("── Defensive ──", StatCategory.Defensive),
            new("Damage Reduction", s => FormatMod(s.DamageTakenModifier_Flat.Value(), s.DamageTakenModifier_Increase.Value()),
                "Damage Reduction", "Reduces damage taken. Flat = direct reduction, % = percentage reduction.", s => s.DamageTakenModifier_Increase),
            new("Healing Taken",    s => FormatMod(s.HealingTakenModifier_Flat.Value(), s.HealingTakenModifier_Increase.Value()),
                "Healing Taken",    "Increases healing received. Flat = direct increase, % = percentage increase.", s => s.HealingTakenModifier_Increase),

            // ── Costs ─────────────────────────────────────────────────────────
            new("── Costs ──"),
            new("Card Cost",     s => FormatMod(s.CardCostModifier_Flat.Value(),     s.CardCostModifier_Increase.Value()),
                "Card Cost",     "Modifies Stamina cost of all played cards. Negative = cheaper."),
            new("Movement Cost", s => FormatMod(s.MovementCostModifier_Flat.Value(), s.MovementCostModifier_Increase.Value()),
                "Movement Cost", "Modifies the Stamina cost of movement actions."),

            // ── Range & Area ──────────────────────────────────────────────────
            new("── Range & Area ──"),
            new("Range",       s => FormatMod(s.RangeModifier_Flat.Value(),      s.RangeModifier_Increase.Value()),
                "Range",       "Bonus flat range and % increase on all card range values."),
            new("Radius",      s => FormatMod(s.RadiusModifier_Flat.Value(),     s.RadiusModifier_Increase.Value()),
                "Radius",      "Bonus flat radius and % increase on area-of-effect cards."),
            new("Max Targets", s => FormatMod(s.MaxTargetModifier_Flat.Value(),  s.MaxTargetModifier_Increase.Value()),
                "Max Targets", "Additional targets that can be hit by multi-target cards."),
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
                panelGroup   = panelRoot.GetComponent<CanvasGroup>()
                            ?? panelRoot.AddComponent<CanvasGroup>();
                panelRoot.SetActive(false);
            }

            if (prevButton != null) prevButton.onClick.AddListener(Prev);
            if (nextButton != null) nextButton.onClick.AddListener(Next);
        }

        private void Update()
        {
        }

        private void OnEnable()  => GameEvents.OnTurnStart += Refresh;
        private void OnDisable() => GameEvents.OnTurnStart -= Refresh;

        // ── Public API — wire to OnClick in Unity Inspector ────────────────────

        public void Toggle()
        {
            if (panelRoot == null) return;
            if (panelRoot.activeSelf) Hide();
            else Show();
        }

        public void Show()
        {
            if (panelRoot == null) return;
            BuildEntityLists();
            panelRoot.SetActive(true);
            panelGroup.alpha = 0f;
            panelRect.anchoredPosition = new Vector2(panelRestPos.x - slideOffset, panelRestPos.y);

            panelRect.DOAnchorPosX(panelRestPos.x, slideDuration).SetEase(slideEase).SetUpdate(true);
            panelGroup.DOFade(1f, slideDuration * 0.6f).SetUpdate(true);

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

        /// <summary>Wire to Party tab button OnClick.</summary>
        public void ShowPartyTab()
        {
            showingEnemies = false;
            selectedIndex  = 0;
            UpdateTabHighlights();
            UpdateNav();
            RebuildRows();
        }

        /// <summary>Wire to Enemies tab button OnClick.</summary>
        public void ShowEnemiesTab()
        {
            showingEnemies = true;
            selectedIndex  = 0;
            UpdateTabHighlights();
            UpdateNav();
            RebuildRows();
        }

        public void Prev()
        {
            if (ActiveList.Count <= 1) return;
            selectedIndex = (selectedIndex - 1 + ActiveList.Count) % ActiveList.Count;
            UpdateNav();
            RebuildRows();
        }

        public void Next()
        {
            if (ActiveList.Count <= 1) return;
            selectedIndex = (selectedIndex + 1) % ActiveList.Count;
            UpdateNav();
            RebuildRows();
        }

        /// <summary>Refreshes live stat values without rebuilding the layout.</summary>
        public void Refresh()
        {
            if (panelRoot == null || !panelRoot.activeSelf) return;
            EntityStats stats = GetStats(SelectedEntity);
            if (stats == null) return;
            foreach (var (row, getter) in liveRows)
                row.UpdateValue(getter(stats));
            foreach (var (header, getter) in liveHeaderRows)
                header.UpdateValue(getter(stats));
            foreach (var (row, modifier) in liveConditionalRows)
                row.UpdateValue(modifier.GetDisplayValue(stats));
        }

        // ── Internal ───────────────────────────────────────────────────────────

        private void BuildEntityLists()
        {
            partyList.Clear();
            enemyList.Clear();

            TurnManager tm = TurnManager.Instance;
            if (tm?.TurnOrder == null) return;

            foreach (EntityScript e in tm.TurnOrder)
            {
                if (e == null) continue;
                if (e.GetComponent<PlayerScript>() != null)      partyList.Add(e);
                else if (e.GetComponent<NonPlayerScript>() != null) enemyList.Add(e);
            }

            // Open on the tab of the currently active entity
            EntityScript active = tm.CurrentTurnEntity;
            if (active != null && active.GetComponent<PlayerScript>() != null)
            {
                showingEnemies = false;
                int idx = partyList.IndexOf(active);
                selectedIndex = idx >= 0 ? idx : 0;
            }
            else if (active != null)
            {
                showingEnemies = true;
                int idx = enemyList.IndexOf(active);
                selectedIndex = idx >= 0 ? idx : 0;
            }
            else
            {
                showingEnemies = false;
                selectedIndex  = 0;
            }

            UpdateTabHighlights();
        }

        private void UpdateTabHighlights()
        {
            if (partyTabHighlight   != null) partyTabHighlight.SetActive(!showingEnemies);
            if (enemiesTabHighlight != null) enemiesTabHighlight.SetActive(showingEnemies);
        }

        private void UpdateNav()
        {
            if (entityNameText != null)
                entityNameText.text = SelectedEntity != null ? GetEntityName(SelectedEntity) : "—";

            bool multi = ActiveList.Count > 1;
            if (prevButton != null) prevButton.gameObject.SetActive(multi);
            if (nextButton != null) nextButton.gameObject.SetActive(multi);
        }

        private void RebuildRows()
        {
            liveRows.Clear();
            liveHeaderRows.Clear();
            liveConditionalRows.Clear();
            for (int i = rowContainer.childCount - 1; i >= 0; i--)
                Destroy(rowContainer.GetChild(i).gameObject);

            EntityStats stats = GetStats(SelectedEntity);

            foreach (StatDef def in Definitions)
            {
                if (def.IsSection)
                {
                    // Use sectionHeaderRowPrefab for main sections (no category), use statHeaderPrefab for subsections
                    if (def.Category == StatCategory.None)
                    {
                        if (sectionHeaderRowPrefab != null)
                        {
                            GameObject header = Instantiate(sectionHeaderRowPrefab, rowContainer);
                            TMP_Text t = header.GetComponentInChildren<TMP_Text>();
                            if (t != null) t.text = def.Label;
                        }
                        else if (statRowPrefab != null)
                        {
                            GameObject sectionRowObj = Instantiate(statRowPrefab, rowContainer);
                            StatRowEntryUI r = sectionRowObj.GetComponent<StatRowEntryUI>();
                            if (r != null) r.Setup(def.Label, "", "", "");
                        }
                    }
                    else
                    {
                        // Use statHeaderPrefab for all subsections with StatCategory
                        if (statHeaderPrefab != null && stats != null && def.Category != StatCategory.None)
                        {
                            string displayLabel = def.Label.Replace("── ", "").Replace(" ──", "");

                            GameObject header = Instantiate(statHeaderPrefab, rowContainer);
                            StatHeaderEntryUI headerUI = header.GetComponent<StatHeaderEntryUI>();
                            if (headerUI != null)
                            {
                                string tooltipBody = def.StatReference != null 
                                    ? $"Final calculated {displayLabel} value including all modifiers."
                                    : $"Modifiers and stats for {displayLabel}.";

                                headerUI.Setup(displayLabel, "", displayLabel, tooltipBody);

                                // Only track for live updates if there's a getter
                                if (def.Getter != null)
                                    liveHeaderRows.Add((headerUI, def.Getter));
                            }
                        }
                        else if (statRowPrefab != null)
                        {
                            GameObject sectionRowObj = Instantiate(statRowPrefab, rowContainer);
                            StatRowEntryUI r = sectionRowObj.GetComponent<StatRowEntryUI>();
                            if (r != null) r.Setup(def.Label, "", "", "");
                        }

                         // Extract and display individual modifier breakdowns for stat entries with StatReference
                        if (stats != null && def.StatReference != null)
                        {
                            Stat stat = def.StatReference(stats);
                            if (stat != null && stat.statModifiers != null)
                            {
                                // Determine modifier type from the stat reference
                                string modifierType = DetermineModifierType(def.Label);

                                // Get the stat name for display
                                string statName = def.Label.Replace("── ", "").Replace(" ──", "");

                                // Group non-expired conditional modifiers by condition
                                var conditionalMods = stat.statModifiers
                                    .OfType<StatModifier>()
                                    .Where(m => !m.IsExpired && m.Condition != null)
                                    .ToList();

                                // Group by condition and aggregate values
                                var conditionGroups = conditionalMods.GroupBy(m => m.Condition.Name);
                                foreach (var group in conditionGroups)
                                {
                                    var firstMod = group.First();
                                    float totalValue = group.Sum(m => m.BaseValue);

                                    if (statRowPrefab == null) continue;

                                    Func<EntityStats, string> getter = s => FormatModifierValueFlat(totalValue, modifierType);
                                    string modValue = getter(stats);
                                    GameObject modRowObj = Instantiate(statRowPrefab, rowContainer);
                                    StatRowEntryUI modRow = modRowObj.GetComponent<StatRowEntryUI>();
                                    if (modRow != null)
                                    {
                                        string displayLabel = $"    {firstMod.Condition.DisplayName}";
                                        modRow.Setup(displayLabel, modValue, firstMod.Condition.DisplayName, firstMod.Condition.Description);
                                        liveConditionalRows.Add((modRow, new ConditionalModifierDef(firstMod.ModifierName, getter, firstMod.Condition, modifierType)));
                                    }
                                }
                            }
                        }
                    }

                    // Only extract conditional modifiers for Attribute category
                    if (stats != null && def.Category == StatCategory.Attribute)
                    {
                        var conditionals = ExtractConditionalModifiersByCategory(stats, "Attribute");
                        foreach (var modifier in conditionals)
                        {
                            if (statRowPrefab == null) continue;
                            string modValue = modifier.GetDisplayValue(stats);
                            GameObject modRowObj = Instantiate(statRowPrefab, rowContainer);
                            StatRowEntryUI modRow = modRowObj.GetComponent<StatRowEntryUI>();
                            if (modRow != null)
                            {
                                modRow.Setup(modifier.GetDisplayLabel(), modValue, modifier.GetDisplayTooltipHeader(), modifier.GetDisplayTooltipBody());
                                liveConditionalRows.Add((modRow, modifier));
                            }
                        }
                    }

                    continue;
                }

                if (statRowPrefab == null) continue;

                // Check if this row should be displayed
                if (def.ShouldDisplay != null && !def.ShouldDisplay(stats))
                    continue;

                string value = stats != null ? def.Getter(stats) : "—";
                GameObject rowObj = Instantiate(statRowPrefab, rowContainer);
                StatRowEntryUI row = rowObj.GetComponent<StatRowEntryUI>();
                if (row != null)
                {
                    row.Setup(def.Label, value, def.TipHeader, def.TipBody);
                    if (stats != null) liveRows.Add((row, def.Getter));
                }

                // Display conditional modifiers for this stat if it has any
                if (stats != null && def.StatReference != null)
                {
                    Stat stat = def.StatReference(stats);
                    if (stat != null && stat.statModifiers != null)
                    {
                        // Determine modifier type from the stat reference
                        string modifierType = DetermineModifierType(def.Label);

                        var statModifiers = stat.statModifiers
                            .OfType<StatModifier>()
                            .Where(m => !m.IsExpired && m.Condition != null)
                            .ToList();

                        // Group by condition and aggregate values
                        var conditionGroups = statModifiers.GroupBy(m => m.Condition.Name);
                        foreach (var group in conditionGroups)
                        {
                            var firstMod = group.First();
                            float totalValue = group.Sum(m => m.BaseValue);

                            if (statRowPrefab == null) continue;

                            Func<EntityStats, string> getter = s => FormatModifierValueFlat(totalValue, modifierType);
                            string modValue = getter(stats);
                            GameObject modRowObj = Instantiate(statRowPrefab, rowContainer);
                            StatRowEntryUI modRow = modRowObj.GetComponent<StatRowEntryUI>();
                            if (modRow != null)
                            {
                                string displayLabel = $"    {firstMod.Condition.DisplayName}";
                                modRow.Setup(displayLabel, modValue, firstMod.Condition.DisplayName, firstMod.Condition.Description);
                                liveConditionalRows.Add((modRow, new ConditionalModifierDef(firstMod.ModifierName, getter, firstMod.Condition, modifierType)));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>Determines if a stat category should display conditional modifiers</summary>
        private bool IsConditionalModifierCategory(StatCategory category)
        {
            return category == StatCategory.Attribute ||
                   category == StatCategory.Offensive ||
                   category == StatCategory.Defensive;
        }


        /// <summary>Cached mapping of category names to their stats for performance</summary>
        private static readonly Dictionary<string, List<(string type, string statName)>> StatsByCategoryDefinition = new()
        {
        };

        /// <summary>Extracts conditional modifiers for a specific stat category (e.g., "Damage Dealt" or "Attribute")</summary>
        private List<ConditionalModifierDef> ExtractConditionalModifiersByCategory(EntityStats stats, string categoryName)
        {
            var conditionals = new List<ConditionalModifierDef>();
            if (stats == null) return conditionals;

            // Map category names to stat lists with their type information
            var statsByCategory = new Dictionary<string, List<(string type, Stat stat)>>
            {
                { "Damage Dealt", new List<(string, Stat)>
                    { ("Flat", stats.DamageOutModifier_Flat), ("Increase %", stats.DamageOutModifier_Increase), ("Multiplier", stats.DamageOutModifier_Multiplier) }
                },
                { "Damage Taken", new List<(string, Stat)>
                    { ("Flat", stats.DamageTakenModifier_Flat), ("Increase %", stats.DamageTakenModifier_Increase), ("Multiplier", stats.DamageTakenModifier_Multiplier) }
                },
                { "Healing Dealt", new List<(string, Stat)>
                    { ("Flat", stats.HealingOutModifier_Flat), ("Increase %", stats.HealingOutModifier_Increase), ("Multiplier", stats.HealingOutModifier_Multiplier) }
                },
                { "Healing Taken", new List<(string, Stat)>
                    { ("Flat", stats.HealingTakenModifier_Flat), ("Increase %", stats.HealingTakenModifier_Increase), ("Multiplier", stats.HealingTakenModifier_Multiplier) }
                },
                { "Strength", new List<(string, Stat)>
                    { ("Flat", stats.Strength_Flat), ("Increase %", stats.Strength_Increase), ("Multiplier", stats.Strength_Multiplier) }
                },
                { "Dexterity", new List<(string, Stat)>
                    { ("Flat", stats.Dexterity_Flat), ("Increase %", stats.Dexterity_Increase), ("Multiplier", stats.Dexterity_Multiplier) }
                },
                { "Wisdom", new List<(string, Stat)>
                    { ("Flat", stats.Wisdom_Flat), ("Increase %", stats.Wisdom_Increase), ("Multiplier", stats.Wisdom_Multiplier) }
                },
                { "Foresight", new List<(string, Stat)>
                    { ("Flat", stats.Intelligence_Flat), ("Increase %", stats.Intelligence_Increase), ("Multiplier", stats.Intelligence_Multiplier) }
                },
                { "Endurance", new List<(string, Stat)>
                    { ("Flat", stats.Endurance_Flat), ("Increase %", stats.Endurance_Increase), ("Multiplier", stats.Endurance_Multiplier) }
                },
                { "Tenacity", new List<(string, Stat)>
                    { ("Flat", stats.Tenacity_Flat), ("Increase %", stats.Tenacity_Increase), ("Multiplier", stats.Tenacity_Multiplier) }
                },
            };

            // Handle generic "Attribute" category by extracting from all attributes
            if (categoryName == "Attribute")
            {
                var attributeStats = new List<(string type, Stat stat)>();
                attributeStats.AddRange(statsByCategory["Strength"]);
                attributeStats.AddRange(statsByCategory["Dexterity"]);
                attributeStats.AddRange(statsByCategory["Wisdom"]);
                attributeStats.AddRange(statsByCategory["Foresight"]);
                attributeStats.AddRange(statsByCategory["Endurance"]);
                attributeStats.AddRange(statsByCategory["Tenacity"]);
                statsByCategory["Attribute"] = attributeStats;
            }

            if (!statsByCategory.TryGetValue(categoryName, out var categoryStats))
                return conditionals;

            var seenModifierNames = new HashSet<string>();

            foreach (var (modifierType, stat) in categoryStats)
            {
                if (stat == null) continue;

                foreach (var modifier in stat.statModifiers)
                {
                    var statMod = modifier as StatModifier;
                    if (statMod == null || statMod.IsExpired || statMod.Condition == null) continue;

                    string uniqueKey = $"{categoryName}_{statMod.ModifierName}_{statMod.Condition.Name}";
                    if (seenModifierNames.Contains(uniqueKey)) continue;

                    seenModifierNames.Add(uniqueKey);

                    // Create a getter that formats the modifier value appropriately
                    Func<EntityStats, string> getter = s => FormatModifierValue(statMod, s);

                    // Format label as "ConditionName CategoryName" (e.g., "Ice Damage Dealt")
                    string conditionLabel = $"{statMod.Condition.DisplayName} {categoryName}";

                    conditionals.Add(new ConditionalModifierDef(
                        $"    {conditionLabel}",  // Indent to show it's under the parent category
                        getter,
                        statMod.Condition,
                        modifierType  // Pass the modifier type (Flat, Increase %, Multiplier)
                    ));
                }
            }

            return conditionals;
        }

        /// <summary>Formats a conditional modifier value for display</summary>
        private string FormatModifierValue(StatModifier modifier, EntityStats stats, string modifierType = "")
        {
            if (modifier == null) return "—";

            float value = modifier.BaseValue;
            if (Mathf.Abs(value) < 0.01f) return "—";

            // Format based on the modifier type if provided
            if (!string.IsNullOrEmpty(modifierType))
            {
                if (modifierType.Contains("Increase"))
                    return $"{value:+0;-0}%";
                else if (modifierType.Contains("Multiplier"))
                    return $"{value:+0;-0}%";
                else if (modifierType.Contains("Flat"))
                    return $"{value:+0;-0}";
            }

            // Fallback to name-based detection
            if (modifier.ModifierName.Contains("Damage") || modifier.ModifierName.Contains("Healing"))
            {
                return FormatMod(value, 0f);
            }
            else if (modifier.ModifierName.Contains("Multiplier"))
            {
                return $"{value:+0;-0}%";
            }
            else
            {
                return $"{value:+0;-0}";
            }
        }

        /// <summary>Formats a flat modifier value (used for aggregated modifiers)</summary>
        private string FormatModifierValueFlat(float value, string modifierType = "")
        {
            if (Mathf.Abs(value) < 0.01f) return "—";

            if (!string.IsNullOrEmpty(modifierType))
            {
                if (modifierType.Contains("Increase"))
                    return $"{value:+0;-0}%";
                else if (modifierType.Contains("Multiplier"))
                    return $"{value:+0;-0}%";
                else if (modifierType.Contains("Flat"))
                    return $"{value:+0;-0}";
            }

            return $"{value:+0;-0}";
        }

        /// <summary>Determines the modifier type (Flat, Increase %, Multiplier) from the stat label</summary>
        private string DetermineModifierType(string statLabel)
        {
            if (statLabel.Contains("Multiplier"))
                return "Multiplier";
            if (statLabel.Contains("Increase") || 
                statLabel.Contains("Damage Dealt") || 
                statLabel.Contains("Healing Dealt") ||
                statLabel.Contains("Power") ||
                statLabel.Contains("Duration") ||
                statLabel.Contains("Damage Reduction") ||
                statLabel.Contains("Healing Taken") ||
                statLabel.Contains("Card Cost") ||
                statLabel.Contains("Movement Cost") ||
                statLabel.Contains("Range") ||
                statLabel.Contains("Radius") ||
                statLabel.Contains("Max Targets") ||
                statLabel.Contains("Lifesteal") ||
                statLabel.Contains("Ignore"))
                return "Increase %";
            return "";
        }

        private static EntityStats GetStats(EntityScript entity)
        {
            if (entity == null) return null;
            FieldInfo field = typeof(EntityScript).GetField(
                "entityStats",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(entity) as EntityStats;
        }


        private static string FormatMod(float flat, float pct)
        {
            bool hasFlat = Mathf.Abs(flat) > 0.01f;
            bool hasPct  = Mathf.Abs(pct)  > 0.01f;
            if (!hasFlat && !hasPct) return "—";
            if (hasFlat && hasPct)   return $"{flat:+0;-0} / {pct:+0;-0}%";
            if (hasFlat)             return $"{flat:+0;-0}";
            return                          $"{pct:+0;-0}%";
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
