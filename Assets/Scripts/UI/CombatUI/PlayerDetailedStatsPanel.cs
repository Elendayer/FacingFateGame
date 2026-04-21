using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace facingfate
{
    public class PlayerDetailedStatsPanel : MonoBehaviour
    {
        public static PlayerDetailedStatsPanel Instance { get; private set; }

        [Header("Popup")]
        [SerializeField] private GameObject     panelRoot;
        [SerializeField] private Transform      rowContainer;       // VerticalLayoutGroup here
        [SerializeField] private StatRowEntryUI rowPrefab;
        [SerializeField] private GameObject     sectionHeaderPrefab;

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

        // ── Stat definitions ───────────────────────────────────────────────────
        private readonly struct StatDef
        {
            public readonly string Label;
            public readonly Func<EntityStats, string> Getter;
            public readonly string TipHeader;
            public readonly string TipBody;
            public readonly bool   IsSection;

            public StatDef(string label, Func<EntityStats, string> getter,
                           string tipHeader, string tipBody)
            { Label = label; Getter = getter; TipHeader = tipHeader; TipBody = tipBody; IsSection = false; }

            public StatDef(string label)
            { Label = label; Getter = null; TipHeader = null; TipBody = null; IsSection = true; }
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
            new("Foresight",  s => $"{s.CurrentIntelligence:0}",
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
            new("Damage Dealt",  s => FormatMod(s.DamageOutModifier_Flat.Value(),    s.DamageOutModifier_Increase.Value()),
                "Damage Dealt",  "+Flat adds to base value; +% scales result multiplicatively."),
            new("Damage Taken",  s => FormatMod(s.DamageTakenModifier_Flat.Value(),  s.DamageTakenModifier_Increase.Value()),
                "Damage Taken",  "Positive values increase damage received; negative reduce it."),
            new("Healing Dealt", s => FormatMod(s.HealingOutModifier_Flat.Value(),   s.HealingOutModifier_Increase.Value()),
                "Healing Dealt", "Modifies all healing this entity applies."),
            new("Healing Taken", s => FormatMod(s.HealingTakenModifier_Flat.Value(), s.HealingTakenModifier_Increase.Value()),
                "Healing Taken", "Modifies all incoming healing received."),
            new("Card Cost",     s => FormatMod(s.CardCostModifier_Flat.Value(),     s.CardCostModifier_Increase.Value()),
                "Card Cost",     "Modifies Stamina cost of all played cards. Negative = cheaper."),
            new("Power",         s => FormatMod(s.PowerModifier_Flat.Value(),        s.PowerModifier_Increase.Value()),
                "Power",         "Global multiplier on the power of all card effects."),
            new("Duration",      s => FormatMod(s.DurationModifier_Flat.Value(),     s.DurationModifier_Increase.Value()),
                "Duration",      "Modifies duration of all status effects applied by this entity."),
            new("Lifesteal",     s => $"{s.Lifesteal.Value():0}%",
                "Lifesteal",     "% of all damage dealt returned as healing to this entity."),
            new("Ignore Armour", s => $"{s.IgnoreArmour.Value():0}%",
                "Ignore Armour", "% of target's Armour bypassed when dealing damage."),
            new("Ignore Block",  s => $"{s.IgnoreBlock.Value():0}%",
                "Ignore Block",  "% of target's Block bypassed when dealing damage."),

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
            //if (Input.GetKeyDown(toggleKey)) Toggle();
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
            for (int i = rowContainer.childCount - 1; i >= 0; i--)
                Destroy(rowContainer.GetChild(i).gameObject);

            EntityStats stats = GetStats(SelectedEntity);

            foreach (StatDef def in Definitions)
            {
                if (def.IsSection)
                {
                    if (sectionHeaderPrefab != null)
                    {
                        GameObject header = Instantiate(sectionHeaderPrefab, rowContainer);
                        TMP_Text t = header.GetComponentInChildren<TMP_Text>();
                        if (t != null) t.text = def.Label;
                    }
                    else if (rowPrefab != null)
                    {
                        StatRowEntryUI r = Instantiate(rowPrefab, rowContainer);
                        r.Setup(def.Label, "", "", "");
                    }
                    continue;
                }

                if (rowPrefab == null) continue;

                string value = stats != null ? def.Getter(stats) : "—";
                StatRowEntryUI row = Instantiate(rowPrefab, rowContainer);
                row.Setup(def.Label, value, def.TipHeader, def.TipBody);
                if (stats != null) liveRows.Add((row, def.Getter));
            }
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
