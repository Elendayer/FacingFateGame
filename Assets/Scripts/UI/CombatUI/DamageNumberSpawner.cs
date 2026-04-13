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
        [SerializeField] private float batchWindow = 0.1f;

        public enum NumberType { Damage, Heal, Dot, Modifier }

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

        private void OnDisable() => batches.Clear();

        /// <summary>
        /// Wird direkt aus CombatUtility aufgerufen – kein Event-Umweg.
        /// </summary>
        public void SpawnDamage(EntityScript target, int value, NumberType type)
        {
            if (target == null) return;

            if (!batches.TryGetValue(target, out BatchEntry entry))
            {
                entry = new BatchEntry();
                batches[target] = entry;
            }

            switch (type)
            {
                case NumberType.Damage: entry.damage += value; break;
                case NumberType.Heal: entry.healing += value; break;
                case NumberType.Dot: entry.dot += value; break;
                case NumberType.Modifier: entry.hasModifier = true; break;
            }

            if (entry.coroutine != null)
                StopCoroutine(entry.coroutine);

            entry.coroutine = StartCoroutine(FlushBatch(target, entry));
        }

        private IEnumerator FlushBatch(EntityScript target, BatchEntry entry)
        {
            yield return new WaitForSeconds(batchWindow);

            Vector3 spawnPos = target.transform.position + spawnOffset;
            float offsetY = 0f;

            if (entry.damage > 0)
            {
                SpawnAt(spawnPos + new Vector3(0f, offsetY, 0f), $"-{entry.damage}", damageColor);
                offsetY += 0.3f;
            }

            if (entry.healing > 0)
            {
                SpawnAt(spawnPos + new Vector3(0f, offsetY, 0f), $"+{entry.healing}", healColor);
                offsetY += 0.3f;
            }

            if (entry.dot > 0)
            {
                SpawnAt(spawnPos + new Vector3(0f, offsetY, 0f), $"-{entry.dot}", dotColor);
                offsetY += 0.3f;
            }

            if (entry.hasModifier)
                SpawnAt(spawnPos + new Vector3(0f, offsetY, 0f), "!", modifierColor);

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