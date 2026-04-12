using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace facingfate
{
    public class DamageNumberSpawner : MonoBehaviour
    {
        public static DamageNumberSpawner Instance { get; private set; }

        [SerializeField] private DamageNumber damageNumberPrefab;

        [Header("Farben")]
        [SerializeField] private Color damageColor = new Color(1f, 0.2f, 0.2f);
        [SerializeField] private Color healColor = new Color(0.2f, 1f, 0.4f);
        [SerializeField] private Color dotColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color modifierColor = new Color(1f, 1f, 0.2f);

        [Header("Spawn Offset")]
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.8f, 0f);

        [Header("Batching")]
        [SerializeField] private float batchWindow = 0.1f; // Zeitfenster zum Zusammenfassen

        // Pro Entity: akkumulierte Werte pro Typ
        private class BatchEntry
        {
            public int damage;
            public int healing;
            public int dot;
            public bool hasModifier;
            public Coroutine coroutine;
        }

        private Dictionary<EntityScript, BatchEntry> batches = new();

        private void Awake() => Instance = this;

        private void OnEnable() => GameEvents.OnGameplayReference += HandleGameplayReference;
        private void OnDisable()
        {
            GameEvents.OnGameplayReference -= HandleGameplayReference;
            batches.Clear();
        }

        private void HandleGameplayReference(ToSendTriggerReference trigger)
        {
            if (trigger.OnTriggerReference == null) return;
            if (trigger.AffectedEntities == null || trigger.AffectedEntities.Count == 0) return;

            foreach (EntityScript target in trigger.AffectedEntities)
            {
                if (target == null) continue;

                if (!batches.TryGetValue(target, out BatchEntry entry))
                {
                    entry = new BatchEntry();
                    batches[target] = entry;
                }

                // Werte akkumulieren
                if (trigger.OnTriggerReference.Contains(GameplayRef.onDamageRecieved))
                    entry.damage += trigger.Throughput;

                else if (trigger.OnTriggerReference.Contains(GameplayRef.onHealRecieved))
                    entry.healing += trigger.Throughput;

                else if (trigger.OnTriggerReference.Contains(GameplayRef.onBleed) ||
                         trigger.OnTriggerReference.Contains(GameplayRef.onBurn) ||
                         trigger.OnTriggerReference.Contains(GameplayRef.onPoison))
                    entry.dot += trigger.Throughput;

                else if (trigger.OnTriggerReference.Contains(GameplayRef.onModifierApplied))
                    entry.hasModifier = true;

                // Coroutine neu starten (reset batch window)
                if (entry.coroutine != null)
                    StopCoroutine(entry.coroutine);

                entry.coroutine = StartCoroutine(FlushBatch(target, entry));
            }
        }

        private IEnumerator FlushBatch(EntityScript target, BatchEntry entry)
        {
            yield return new WaitForSeconds(batchWindow);

            Vector3 spawnPos = target.transform.position + spawnOffset;
            float offsetX = 0f;

            // Schaden anzeigen
            if (entry.damage > 0)
            {
                SpawnAt(spawnPos + new Vector3(offsetX, 0f, 0f), $"-{entry.damage}", damageColor);
                offsetX += 0.4f;
            }

            // Heilung anzeigen
            if (entry.healing > 0)
            {
                SpawnAt(spawnPos + new Vector3(offsetX, 0f, 0f), $"+{entry.healing}", healColor);
                offsetX += 0.4f;
            }

            // DoT anzeigen
            if (entry.dot > 0)
            {
                SpawnAt(spawnPos + new Vector3(offsetX, 0f, 0f), $"-{entry.dot}", dotColor);
                offsetX += 0.4f;
            }

            // Status Effekt
            if (entry.hasModifier)
                SpawnAt(spawnPos + new Vector3(offsetX, 0f, 0f), "!", modifierColor);

            batches.Remove(target);
        }

        private void SpawnAt(Vector3 pos, string value, Color color)
        {
            if (damageNumberPrefab == null) return;
            DamageNumber number = Instantiate(damageNumberPrefab, pos, Quaternion.identity);
            number.Play(value, color);
        }
    }
}