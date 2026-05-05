using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace facingfate
{
    /// <summary>
    /// Defines how targets are executed: individually with delays, or all at once.
    /// </summary>
    public enum ExecutionMode
    {
        EachIndividual,
        AllAtOnce,
        Once
    }
    public enum TargetingMode
    {
        Entities,
        Ground,
        Aim,
        Caster,
        Coroutine,

    }

    /// <summary>
    /// Represents a single action within a card effect sequence.
    /// </summary>
    [System.Serializable]
    public class CardAction
    {
        /// <summary>
        /// How this action executes (all at once, each individually, or once).
        /// </summary>
        public ExecutionMode executionMode = ExecutionMode.AllAtOnce;

        /// <summary>
        /// Whether this action targets entities or ground positions.
        /// </summary>
        public TargetingMode targetingMode = TargetingMode.Entities;

        public TargetingModeData targetingModeData;

        /// <summary>
        /// Delay before this action executes (in seconds).
        /// </summary>
        public float delayBeforeExecution = 0f;

        /// <summary>
        /// Delay between individual target executions (only applies to EachIndividual mode).
        /// </summary>
        public float delayBetweenTargets = 0.2f;

        /// <summary>
        /// The action delegate to execute. For Entities: (caster, targets, cardData). For Ground: (caster, positions, cardData).
        /// </summary>
        public object actionDelegate;


        public CardAction(ExecutionMode mode, TargetingMode targeting = TargetingMode.Entities, float delayBefore = 0f, float delayBetween = 0.2f, Action<EntityScript, EntityScript, CardData> action = null)
        {
            executionMode = mode;
            targetingMode = targeting;
            delayBeforeExecution = delayBefore;
            delayBetweenTargets = delayBetween;
            actionDelegate = action;
        }
        public CardAction(ExecutionMode mode, TargetingMode targeting = TargetingMode.Entities, float delayBefore = 0f, float delayBetween = 0.2f, Action<EntityScript, Vector3, CardData> action = null)
        {
            executionMode = mode;
            targetingMode = targeting;
            delayBeforeExecution = delayBefore;
            delayBetweenTargets = delayBetween;
            actionDelegate = action;
        }
        public CardAction(ExecutionMode mode, TargetingMode targeting, float delayBefore, float delayBetween, Func<EntityScript, TargetingModeData, CardData, IEnumerator> coroutine)
        {
            executionMode = mode;
            targetingMode = TargetingMode.Coroutine;
            delayBeforeExecution = delayBefore;
            delayBetweenTargets = delayBetween;
            actionDelegate = coroutine;
        }
        /// <summary>
        /// Constructor for caster-only actions (TargetingMode.Caster). The action receives only the caster, with no target parameter.
        /// </summary>
        public CardAction(ExecutionMode mode, TargetingMode targeting, float delayBefore, float delayBetween, Action<EntityScript, CardData> action)
        {
            executionMode = mode;
            targetingMode = targeting;
            delayBeforeExecution = delayBefore;
            delayBetweenTargets = delayBetween;
            actionDelegate = action;
        }
    }

    /// <summary>
    /// A sequence of actions to be executed for a card effect.
    /// Supports different execution modes and timing.
    /// </summary>
    public class CardActionSequence
    {
        public List<CardAction> actions = new();

        private EntityScript caster;
        private TargetingModeData targetingData;
        private CardData cardData;
        private Action onComplete;

        public CardActionSequence(EntityScript caster, TargetingModeData targetingData, CardData cardData)
        {
            this.caster = caster;
            this.targetingData = targetingData;
            this.cardData = cardData;
        }

        /// <summary>
        /// Executes the entire action sequence.
        /// </summary>
        public IEnumerator Execute(Action onComplete = null)
        {
            this.onComplete = onComplete;

            if (actions.Count == 0)
            {
                Debug.LogWarning($"[CardActionSequence] No actions in sequence for card {cardData.cardName}");
                onComplete?.Invoke();
                yield break;
            }

            foreach (var action in actions)
            {
                if (action == null || action.actionDelegate == null)
                    continue;

                // Wait before executing this action
                if (action.delayBeforeExecution > 0)
                    yield return new WaitForSeconds(action.delayBeforeExecution);

                // Execute the action based on its execution mode
                yield return ExecuteAction(action);
            }

            onComplete?.Invoke();
        }

        /// <summary>
        /// Executes a single action based on its execution mode and targeting mode.
        /// </summary>
        private IEnumerator ExecuteAction(CardAction action)
        {
            if (action.targetingMode == TargetingMode.Coroutine)
            {
                var coroutine = action.actionDelegate as Func<EntityScript, TargetingModeData, CardData, IEnumerator>;
                if (coroutine == null)
                    yield break;

                yield return TimelineManager.GlobalActionQueue.StartCoroutine(coroutine(caster, targetingData, cardData));
                yield break;
            }

            if (action.targetingMode == TargetingMode.Aim)
            {
                var aimAction = action.actionDelegate as Action<EntityScript, Vector3, CardData>;
                if (aimAction == null)
                    yield break;

                aimAction.Invoke(caster, targetingData.aimPosition, cardData);
                yield return null;
                yield break;
            }

            if (action.targetingMode == TargetingMode.Caster)
            {
                var casterAction = action.actionDelegate as Action<EntityScript, CardData>;
                if (casterAction != null)
                {
                    casterAction.Invoke(caster, cardData);
                    yield return null;
                    yield break;
                }
                // Legacy fallback: Action<EntityScript, EntityScript, CardData> with caster passed as both args
                var legacyCasterAction = action.actionDelegate as Action<EntityScript, EntityScript, CardData>;
                if (legacyCasterAction == null)
                    yield break;
                legacyCasterAction.Invoke(caster, caster, cardData);
                yield return null;
                yield break;
            }

            // Determine if this is an entity or ground targeting action
            bool isGroundTargeting = action.targetingMode == TargetingMode.Ground;

            if (isGroundTargeting)
            {
                // Ground targeting - use targetedPositions
                var groundAction = action.actionDelegate as Action<EntityScript, List<Vector3>, CardData>;
                if (groundAction == null)
                    yield break;

                switch (action.executionMode)
                {
                    case ExecutionMode.AllAtOnce:
                        // Execute on all ground positions at once
                        groundAction.Invoke(caster, targetingData.targetedPositions, cardData);
                        yield return null;
                        break;

                    case ExecutionMode.EachIndividual:
                        // Execute on each ground position individually with delay
                        foreach (var position in targetingData.targetedPositions)
                        {
                            var singlePositionList = new List<Vector3> { position };
                            groundAction.Invoke(caster, singlePositionList, cardData);

                            if (action.delayBetweenTargets > 0)
                                yield return new WaitForSeconds(action.delayBetweenTargets);
                            else
                                yield return null;
                        }
                        break;

                    case ExecutionMode.Once:
                        // Execute once on casting position
                        groundAction.Invoke(caster, new List<Vector3> { targetingData.castingPosition }, cardData);
                        yield return null;
                        break;

                    default:
                        Debug.LogWarning($"[CardActionSequence] Unknown execution mode: {action.executionMode}");
                        yield return null;
                        break;
                }
            }
            else
            {
                // Entity targeting - try list-based delegate first, then fall back to single-target
                var entityAction = action.actionDelegate as Action<EntityScript, List<EntityScript>, CardData>;

                if (entityAction != null)
                {
                    // List-based entity action
                    switch (action.executionMode)
                    {
                        case ExecutionMode.AllAtOnce:
                            // Execute on all targets at once
                            entityAction.Invoke(caster, targetingData.targetedEntities, cardData);
                            yield return null;
                            break;

                        case ExecutionMode.EachIndividual:
                            // Execute on each target individually with delay
                            foreach (var target in targetingData.targetedEntities)
                            {
                                var singleTargetList = new List<EntityScript> { target };
                                entityAction.Invoke(caster, singleTargetList, cardData);

                                if (action.delayBetweenTargets > 0)
                                    yield return new WaitForSeconds(action.delayBetweenTargets);
                                else
                                    yield return null;
                            }
                            break;

                        case ExecutionMode.Once:
                            // Execute once without individual targeting
                            entityAction.Invoke(caster, targetingData.targetedEntities, cardData);
                            yield return null;
                            break;

                        default:
                            Debug.LogWarning($"[CardActionSequence] Unknown execution mode: {action.executionMode}");
                            yield return null;
                            break;
                    }
                }
                else
                {
                    // Try single-target delegate (legacy support)
                    var singleTargetAction = action.actionDelegate as Action<EntityScript, EntityScript, CardData>;
                    if (singleTargetAction == null)
                        yield break;

                    switch (action.executionMode)
                    {
                        case ExecutionMode.AllAtOnce:
                            // Execute on all targets at once
                            foreach (var target in targetingData.targetedEntities)
                            {
                                singleTargetAction.Invoke(caster, target, cardData);
                            }
                            yield return null;
                            break;

                        case ExecutionMode.EachIndividual:
                            // Execute on each target individually with delay
                            foreach (var target in targetingData.targetedEntities)
                            {
                                singleTargetAction.Invoke(caster, target, cardData);

                                if (action.delayBetweenTargets > 0)
                                    yield return new WaitForSeconds(action.delayBetweenTargets);
                                else
                                    yield return null;
                            }
                            break;

                        case ExecutionMode.Once:
                            // Execute once on first target
                            if (targetingData.targetedEntities.Count > 0)
                            {
                                singleTargetAction.Invoke(caster, targetingData.targetedEntities[0], cardData);
                            }
                            yield return null;
                            break;

                        default:
                            Debug.LogWarning($"[CardActionSequence] Unknown execution mode: {action.executionMode}");
                            yield return null;
                            break;
                    }
                }
            }
        }
    }
}
