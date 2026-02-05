using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace facingfate
{
    /// <summary>
    /// Sehr schlanke Debug-Ansicht: zeigt im Inspector aktuell getrackte EntityModifier
    /// (Name, Value, Rest-Dauer). KEINE Annahmen über Stat/Enum-Namen.
    /// </summary>
    [DisallowMultipleComponent]
    public class StatusDebugView : MonoBehaviour
    {
        [Serializable]
        private class Row
        {
            public string name;
            public int value;
            public int remainingDuratiom;
        }

        [SerializeField] private List<Row> _rows = new List<Row>();

        public void Track(EntityModifier mod)
        {
            if (mod == null) return;
            string key = string.IsNullOrWhiteSpace(mod.ModifierName) ? "(Unnamed)" : mod.ModifierName;

            var row = _rows.Find(r => r.name == key);
            if (row == null)
            {
                _rows.Add(new Row
                {
                    name = key,
                    value = mod.BaseValue,
                    remainingDuratiom = Math.Max(0, mod.Duration)
                });
            }
            else
            {
                row.value = mod.BaseValue;
                row.remainingDuratiom = Math.Max(0, mod.Duration);
            }
        }

        /// <summary>
        /// Praktisch, um EXPLIZIT ein „entfernt/abgelaufen“ zu markieren.
        /// Optional zu verwenden – nicht zwingend.
        /// </summary>
        public void Untrack(string modifierName)
        {
            if (string.IsNullOrWhiteSpace(modifierName)) return;
            _rows.RemoveAll(r => r.name == modifierName);
        }

        // rein kosmetisch: clamp negative Restdauer auf 0 im Inspector
        private void LateUpdate()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i].remainingDuratiom < 0) _rows[i].remainingDuratiom = 0;
            }
        }

        public enum DurationAggregate { Max, Sum, Min }

        public struct DotEntry
        {
            public string Name;
            public int DamagePerRound;
            public int RoundsLeft;
        }

        /// <summary>
        /// Einzelnen DOT updaten/anlegen. Kannst du bei Apply/Merge/Refresh benutzen.
        /// </summary>
        public void SyncDot(string name, int damagePerRound, int roundsLeft)
        {
            if (string.IsNullOrWhiteSpace(name)) name = "(Unnamed)";
            var row = _rows.Find(r => r.name == name);
            if (row == null)
            {
                _rows.Add(new Row { name = name, value = damagePerRound, remainingDuratiom = Mathf.Max(0, roundsLeft) });
            }
            else
            {
                row.value = damagePerRound;
                row.remainingDuratiom = Mathf.Max(0, roundsLeft);
            }
            if (roundsLeft <= 0)
                _rows.RemoveAll(r => r.name == name);
        }

        /// <summary>
        /// Snapshot des aktuellen DOT-Zustands (alle DOTs). 
        /// Entfernt Einträge, die nicht mehr existieren oder abgelaufen sind.
        /// </summary>
        public void SyncDots(IEnumerable<DotEntry> dots)
        {
            if (dots == null)
            {
                _rows.Clear();
                return;
            }

            var seen = new HashSet<string>();
            foreach (var d in dots)
            {
                if (string.IsNullOrWhiteSpace(d.Name)) continue;
                var row = _rows.Find(r => r.name == d.Name);
                if (row == null)
                {
                    _rows.Add(new Row { name = d.Name, value = d.DamagePerRound, remainingDuratiom = Mathf.Max(0, d.RoundsLeft) });
                }
                else
                {
                    row.value = d.DamagePerRound;
                    row.remainingDuratiom = Mathf.Max(0, d.RoundsLeft);
                }
                seen.Add(d.Name);
            }

            // Raus mit Dingen, die nicht mehr existieren oder abgelaufen sind
            _rows.RemoveAll(r => r.remainingDuratiom <= 0 || !seen.Contains(r.name));
        }

        /// <summary>
        /// Komfort: direkt aus deinen aktuellen EntityModifiern synchronisieren.
        /// Gleicher Name => Schaden addieren; Dauer nach Aggregationsregel bestimmen.
        /// </summary>
        public void SyncDotsFrom(IEnumerable<EntityModifier> modifiers, DurationAggregate durationAggregation = DurationAggregate.Max)
        {
            if (modifiers == null)
            {
                _rows.Clear();
                return;
            }

            var aggregated = new Dictionary<string, (int dmg, int dur)>();
            foreach (var m in modifiers)
            {
                if (m == null) continue;

                string key = string.IsNullOrWhiteSpace(m.ModifierName) ? "(Unnamed)" : m.ModifierName;
                int dmg = m.BaseValue;
                int dur = Mathf.Max(0, m.Duration);

                if (aggregated.TryGetValue(key, out var acc))
                {
                    acc.dmg += dmg;
                    switch (durationAggregation)
                    {
                        case DurationAggregate.Max: acc.dur = Mathf.Max(acc.dur, dur); break;
                        case DurationAggregate.Sum: acc.dur += dur; break;
                        case DurationAggregate.Min: acc.dur = Mathf.Min(acc.dur, dur); break;
                    }
                    aggregated[key] = acc;
                }
                else
                {
                    aggregated[key] = (dmg, dur);
                }
            }

            var entries = aggregated.Select(kv => new DotEntry
            {
                Name = kv.Key,
                DamagePerRound = kv.Value.dmg,
                RoundsLeft = kv.Value.dur
            });

            SyncDots(entries);
        }

    }
}
