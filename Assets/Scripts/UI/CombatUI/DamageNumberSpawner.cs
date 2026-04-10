using UnityEngine;

namespace facingfate
{
    public class DamageNumberSpawner : MonoBehaviour
    {
        public static DamageNumberSpawner Instance { get; private set; }

        [SerializeField] private DamageNumber damageNumberPrefab;

        [Header("Farben")]
        [SerializeField] private Color damageColor = new Color(1f, 0.2f, 0.2f); // Rot
        [SerializeField] private Color healColor = new Color(0.2f, 1f, 0.4f); // Grün
        [SerializeField] private Color dotColor = new Color(1f, 0.5f, 0f);   // Orange (DoT)
        [SerializeField] private Color modifierColor = new Color(1f, 1f, 0.2f);  // Gelb (Status)

        [Header("Spawn Offset")]
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 1.5f, 0f);

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.OnGameplayReference += HandleGameplayReference;
        }

        private void OnDisable()
        {
            GameEvents.OnGameplayReference -= HandleGameplayReference;
        }

        private void HandleGameplayReference(ToSendTriggerReference trigger)
        {
            if (trigger.OnTriggerReference == null) return;
            if (trigger.AffectedEntities == null || trigger.AffectedEntities.Count == 0) return;

            foreach (EntityScript target in trigger.AffectedEntities)
            {
                if (target == null) continue;

                // Schaden
                if (trigger.OnTriggerReference.Contains(GameplayRef.onDamageRecieved))
                {
                    SpawnNumber(target, $"-{trigger.Throughput}", damageColor);
                    return;
                }

                // Heilung
                if (trigger.OnTriggerReference.Contains(GameplayRef.onHealRecieved))
                {
                    SpawnNumber(target, $"+{trigger.Throughput}", healColor);
                    return;
                }

                // DoT (Bleed, Burn, Poison)
                if (trigger.OnTriggerReference.Contains(GameplayRef.onBleed) ||
                    trigger.OnTriggerReference.Contains(GameplayRef.onBurn) ||
                    trigger.OnTriggerReference.Contains(GameplayRef.onPoison))
                {
                    SpawnNumber(target, $"-{trigger.Throughput}", dotColor);
                    return;
                }

                // Status Effekt angewendet
                if (trigger.OnTriggerReference.Contains(GameplayRef.onModifierApplied))
                {
                    SpawnNumber(target, "!", modifierColor);
                    return;
                }
            }
        }

        private void SpawnNumber(EntityScript target, string value, Color color)
        {
            if (damageNumberPrefab == null || target == null) return;

            EntityOnMap eom = target.GetComponent<EntityOnMap>();
            Vector3 spawnPos = eom != null
                ? eom.transform.position + spawnOffset
                : target.transform.position + spawnOffset;

            DamageNumber number = Instantiate(damageNumberPrefab, spawnPos, Quaternion.identity);
            number.Play(value, color);
        }
    }
}