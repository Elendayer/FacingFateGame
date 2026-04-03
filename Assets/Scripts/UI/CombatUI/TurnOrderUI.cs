using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

        [Header("Entry Animation")]
        [SerializeField] private float entryAnimDuration = 0.3f;
        [SerializeField] private float shiftDelay = 0.15f;

        private void Awake()
        {
            if (toggleButton != null)
                toggleButton.onClick.AddListener(Toggle);
        }

        private void OnEnable()
        {
            GameEvents.OnTurnStart += Refresh;
            GameEvents.OnTurnEnd += AnimateTurnEnd;
            GameEvents.OnCombatStart += Refresh;
        }

        private void OnDisable()
        {
            GameEvents.OnTurnStart -= Refresh;
            GameEvents.OnTurnEnd -= AnimateTurnEnd;
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

        private void AnimateTurnEnd()
        {
            if (spawnedEntries.Count == 0)
            {
                Refresh();
                return;
            }

            TurnOrderEntryUI first = spawnedEntries[0];
            spawnedEntries.RemoveAt(0);

            // Erst komplett ausfaden...
            first.AnimateOut(entryAnimDuration, onComplete: () =>
            {
                // ...dann kurz warten damit Layout sich setzt...
                StartCoroutine(SpawnAfterDelay(shiftDelay));
            });
        }

        private IEnumerator SpawnAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            SpawnNewLastEntry();
        }
        private void SpawnNewLastEntry()
        {
            TurnManager tm = TurnManager.Instance;
            if (tm == null || tm.TurnOrder == null) return;

            int count = tm.TurnOrder.Count;

            // Aktuell angezeigte Entries zählen
            int shownCount = spawnedEntries.Count;

            // Nächster Index nach den bereits gezeigten
            int newIdx = (tm.CurrentTurnIndex + shownCount) % count;
            EntityScript entity = tm.TurnOrder[newIdx];

            if (entity == null) return;

            // Prüfen ob diese Entity bereits als letzter Entry vorhanden ist
            if (spawnedEntries.Count > 0)
            {
                TurnOrderEntryUI last = spawnedEntries[spawnedEntries.Count - 1];
                // Verhindert Duplikat
                if (last != null && last.gameObject.name == entity.gameObject.name)
                    return;
            }

            TurnOrderEntryUI entry = Instantiate(entryPrefab, entryContainer);
            entry.gameObject.name = entity.gameObject.name;
            string name = GetEntityName(entity);
            Color color = entity.GetComponent<PlayerScript>() != null
                ? playerTurnColor : enemyTurnColor;

            entry.Setup(name, color, false, entity);
            entry.AnimateIn(entryAnimDuration);
            spawnedEntries.Add(entry);
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
