using System;
using System.Collections.Generic;
using UnityEngine;

namespace facingfate
{
    public class GroundEffectScript : MonoBehaviour
    {
        [Header("Ground Effect (Single)")]
        public GroundEffectData EffectData;

        // Entities currently inside the zone
        private readonly HashSet<EntityScript> affectedEntities = new();
        // Entities that have exited but whose modifier persists (RemoveOnExit = false)
        private readonly HashSet<EntityScript> exitedEntities = new();
        // Modifier applied per entity
        private readonly Dictionary<EntityScript, IEntityModifier> appliedModifiers = new();

        [Header("Debug (Read-Only)")]
        [SerializeField] private List<EntityScript> _affectedEntities = new();
        [SerializeField] private List<EntityScript> _exitedEntities = new();

        // =============================
        // Unity Lifecycle
        // =============================
        private void Awake()
        {
            GameEvents.OnGameplayReference += OnGameplayRef;
        }

        private void Start()
        {
            if (EffectData == null) return;
            EffectData.OnSpawn?.Invoke(null);
            RefreshEntitiesInZone();
        }

        private void OnDestroy()
        {
            GameEvents.OnGameplayReference -= OnGameplayRef;
        }

        // =============================
        // Trigger-Based Application
        // =============================
        private void OnGameplayRef(ToSendTriggerReference trigger)
        {
            if (EffectData == null) return;

            // Tick duration and refresh zone once per owner turn start
            if (trigger.UserEntity == EffectData.CardData.Owner &&
                trigger.OnTriggerReference.Contains(GameplayRef.onTurnStart))
            {
                RefreshEntitiesInZone();
                TickDuration();
            }

            // Fire OnRef for every valid entity inside the zone
            if (GameEvents.CheckIfRelevantTrigger(trigger, EffectData.RelevantTrigger))
            {
                var snapshot = new List<EntityScript>(affectedEntities);
                foreach (var target in snapshot)
                {
                    if (!IsAlive(target) || !TargetingUtility.IsTargetValid(EffectData.CardData, target))
                        continue;

                    EffectData.OnRef?.Invoke(target);
                }
            }
        }

        private void TickDuration()
        {
            if (EffectData.Duration < 9999)
                EffectData.Duration--;

            if (EffectData.Duration > 0) return;

            // Duration has elapsed — fire OnEnd for all tracked entities then destroy
            var snapshot = new List<EntityScript>(affectedEntities);
            foreach (var target in snapshot)
            {
                if (EffectData.RemoveOnEnd)
                    RemoveModifierFrom(target);

                EffectData.OnExit?.Invoke(target);
                EffectData.OnEnd?.Invoke(target);
            }

            // Also fire OnEnd for entities that exited early but kept their modifier
            var exitedSnapshot = new List<EntityScript>(exitedEntities);
            foreach (var target in exitedSnapshot)
            {
                if (EffectData.RemoveOnEnd)
                    RemoveModifierFrom(target);

                EffectData.OnEnd?.Invoke(target);
            }

            appliedModifiers.Clear();
            affectedEntities.Clear();
            exitedEntities.Clear();
            SyncDebugLists();

            Destroy(gameObject);
        }

        // =============================
        // Zone Query
        // =============================
        private void RefreshEntitiesInZone()
        {
            var currentInZone = QueryEntitiesInZone();

            // Entities that entered since last refresh
            foreach (var target in currentInZone)
            {
                if (affectedEntities.Contains(target)) continue;

                // Was outside the zone (exitedEntities) — re-entering
                if (exitedEntities.Remove(target))
                {
                    affectedEntities.Add(target);
                    SyncDebugLists();
                    EffectData.OnEnter?.Invoke(target);
                }
                else
                {
                    ApplyEffect(target);
                }
            }

            // Entities that left since last refresh
            var currentSet = new HashSet<EntityScript>(currentInZone);
            var departed = new List<EntityScript>();
            foreach (var target in affectedEntities)
            {
                if (!currentSet.Contains(target))
                    departed.Add(target);
            }

            foreach (var target in departed)
            {
                if (EffectData.RemoveOnExit)
                {
                    RemoveEffect(target);
                }
                else
                {
                    affectedEntities.Remove(target);
                    exitedEntities.Add(target);
                    SyncDebugLists();
                    EffectData.OnExit?.Invoke(target);
                }
            }
        }

        private List<EntityScript> QueryEntitiesInZone()
        {
            var cd = EffectData.CardData;
            var pos = EffectData.SpawnPosition;
            var owner = cd.Owner;

            switch (EffectData.TargetingMode)
            {
                case CardTargetingMode.Sphere:
                    return TargetingUtility.GetEntitiesInPhysicsSphere(pos, cd.Radius, cd);

                case CardTargetingMode.Ring:
                    return TargetingUtility.GetEntitiesInPhysicsRing(pos, cd.Radius, cd.Radius + cd.Area, cd);

                case CardTargetingMode.RingSelf:
                    return TargetingUtility.GetEntitiesInPhysicsRing(owner.transform.position, cd.Radius, cd.Area, cd);

                case CardTargetingMode.Cone:
                {
                    var dir = (pos - owner.transform.position).normalized;
                    return TargetingUtility.GetEntitiesInPhysicsCone(owner.transform.position, dir, cd.Range, cd.Area, cd);
                }

                case CardTargetingMode.LineSelf:
                {
                    var dir = (pos - owner.transform.position).normalized;
                    return TargetingUtility.GetEntitiesInPhysicsLine(owner.transform.position, dir, cd.Range, cd.Area, cd);
                }

                case CardTargetingMode.LineFree:
                    return TargetingUtility.GetEntitiesInPhysicsLine(pos, Vector3.forward, cd.Range, cd.Area, cd);

                default:
                    return TargetingUtility.GetEntitiesInPhysicsSphere(pos, cd.Radius, cd);
            }
        }

        // =============================
        // Modifier Application
        // =============================
        private void ApplyEffect(EntityScript target)
        {
            if (!affectedEntities.Add(target)) return;

            SyncDebugLists();

            if (EffectData.ModifierFactory != null)
            {
                var modifier = EffectData.ModifierFactory(target);
                appliedModifiers[target] = modifier;
                target.AddModifier(modifier);
            }

            EffectData.OnEnter?.Invoke(target);
        }

        private void RemoveEffect(EntityScript target)
        {
            RemoveModifierFrom(target);
            EffectData.OnExit?.Invoke(target);
            affectedEntities.Remove(target);
            SyncDebugLists();
        }

        private void RemoveModifierFrom(EntityScript target)
        {
            if (!appliedModifiers.TryGetValue(target, out var modifier)) return;
            target.RemoveModifier(modifier);
            appliedModifiers.Remove(target);
        }

        // =============================
        // Helpers
        // =============================
        private static bool IsAlive(EntityScript entity) => entity != null && entity.enabled;

        private void SyncDebugLists()
        {
            _affectedEntities.Clear();
            _affectedEntities.AddRange(affectedEntities);
            _exitedEntities.Clear();
            _exitedEntities.AddRange(exitedEntities);
        }
    }

    // =============================
    // Ground Effect Data
    // =============================
    [System.Serializable]
    public class GroundEffectData
    {
        public CardData CardData;
        public RelevantTriggerCheck RelevantTrigger;
        public int Duration;
        public bool RemoveOnExit = true;
        public bool RemoveOnEnd = true;

        public CardTargetingMode TargetingMode;
        public Vector3 SpawnPosition;

        public Action<EntityScript> OnSpawn;
        public Action<EntityScript> OnRef;
        public Action<EntityScript> OnEnter;
        public Action<EntityScript> OnExit;
        public Action<EntityScript> OnEnd;

        public Func<EntityScript, IEntityModifier> ModifierFactory;

        public VFXData VFXData;

        public GroundEffectData(
            CardData cardData,
            RelevantTriggerCheck relevantTrigger,
            int duration,
            CardTargetingMode targetingMode = CardTargetingMode.Sphere,
            bool removeOnExit = true,
            bool removeOnEnd = true,
            Action<EntityScript> onSpawn = null,
            Action<EntityScript> onRef = null,
            Action<EntityScript> onEnter = null,
            Action<EntityScript> onExit = null,
            Action<EntityScript> onEnd = null,
            Func<EntityScript, IEntityModifier> modifierFactory = null,
            VFXData vfxData = null)
        {
            CardData = cardData;
            RelevantTrigger = relevantTrigger;
            Duration = duration;
            TargetingMode = targetingMode;
            RemoveOnExit = removeOnExit;
            RemoveOnEnd = removeOnEnd;

            OnSpawn = onSpawn ?? (_ => { });
            OnRef   = onRef   ?? (_ => { });
            OnEnter = onEnter ?? (_ => { });
            OnExit  = onExit  ?? (_ => { });
            OnEnd   = onEnd   ?? (_ => { });

            ModifierFactory = modifierFactory;
            VFXData = vfxData;
        }
    }
}