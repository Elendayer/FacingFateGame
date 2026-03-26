using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace facingfate
{
    public class TurnOrderUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform entryContainer;   // HorizontalLayoutGroup
        [SerializeField] private TurnOrderEntryUI entryPrefab;
        [SerializeField] private GameObject panelRoot;       // das auf/zuklappbare Panel
        [SerializeField] private Button toggleButton;
        [SerializeField] private TMP_Text toggleButtonText;

        [Header("Settings")]
        [SerializeField] private int visibleTurns = 10;
        [SerializeField] private Color currentTurnColor = new Color(1f, 0.85f, 0.2f); // Gelb
        [SerializeField] private Color playerTurnColor = new Color(0.4f, 0.8f, 1f);  // Blau
        [SerializeField] private Color enemyTurnColor = new Color(1f, 0.4f, 0.4f);  // Rot

        private readonly List<TurnOrderEntryUI> spawnedEntries = new();
        private bool isOpen = true;

        private void Awake()
        {
            if (toggleButton != null)
                toggleButton.onClick.AddListener(Toggle);
        }

        private void OnEnable()
        {
            GameEvents.OnTurnStart += Refresh;
            GameEvents.OnTurnEnd += Refresh;
            GameEvents.OnCombatStart += Refresh;
        }

        private void OnDisable()
        {
            GameEvents.OnTurnStart -= Refresh;
            GameEvents.OnTurnEnd -= Refresh;
            GameEvents.OnCombatStart -= Refresh;
        }

        private void Refresh()
        {
            TurnManager tm = TurnManager.Instance;
            if (tm == null || tm.TurnOrder == null || tm.TurnOrder.Count == 0) return;

            ClearEntries();

            // Nächste X Züge zyklisch berechnen
            List<(EntityScript entity, bool isCurrent)> turns = new();
            int count = tm.TurnOrder.Count;
            int startIdx = tm.CurrentTurnIndex;

            for (int i = 0; i < visibleTurns; i++)
            {
                int idx = (startIdx + i) % count;
                bool isCur = (i == 0);
                turns.Add((tm.TurnOrder[idx], isCur));
            }

            // Einträge spawnen
            foreach (var (entity, isCurrent) in turns)
            {
                if (entity == null) continue;

                TurnOrderEntryUI entry = Instantiate(entryPrefab, entryContainer);

                // Name holen
                string name = GetEntityName(entity);

                // Farbe bestimmen
                Color color = isCurrent ? currentTurnColor
                    : entity.GetComponent<PlayerScript>() != null ? playerTurnColor
                    : enemyTurnColor;

                entry.Setup(name, color, isCurrent, entity);
                spawnedEntries.Add(entry);
            }
        }

        private void Toggle()
        {
            isOpen = !isOpen;
            if (panelRoot != null) panelRoot.SetActive(isOpen);
            if (toggleButtonText != null)
                toggleButtonText.text = isOpen ? "▲" : "▼";
        }

        private void ClearEntries()
        {
            foreach (var e in spawnedEntries)
                if (e != null) Destroy(e.gameObject);
            spawnedEntries.Clear();
        }

        private static string GetEntityName(EntityScript entity)
        {
            // NPC Name
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
