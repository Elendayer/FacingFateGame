using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace facingfate
{
    public static class ActionQueueUtility
    {
        //Remove all Actions by an EntityScript
        public static void ClearActionsBySource(EntityScript source)
        {
            TimelineManager.GlobalActionQueue.ClearActionsBySource(source);
        }

        public static void EnqueueCardExecution(
            EntityScript source,
            CardData cardData,
            TargetingModeData targetingData,
            GameObject cardObj = null,
            float repeatDelay = 0.25f,
            Action onComplete = null // Added callback
            )
        {
            // Wrap all steps in a coroutine to allow async completion
            TimelineManager.GlobalActionQueue.Enqueue(() => CardExecutionCoroutine(source, cardData, targetingData, cardObj, repeatDelay, onComplete), 0f, source);
        }

        private static IEnumerator CardExecutionCoroutine(
            EntityScript source,
            CardData cardData,
            TargetingModeData targetingData,
            GameObject cardObj,
            float repeatDelay,
            Action onComplete // receive callback
            )
        {
            // 1️ Pre-combat trigger
            CombatUtility.HandlePreCombatTrigger(targetingData.targetedEntities, cardData);
            yield return null; // wait a frame

            // 2️ Card effect repeats
            int repeats = Mathf.Max(cardData.repeats_u, 1);
            for (int i = 0; i < repeats; i++)
            {
                if (i > 0)
                {
                    yield return new WaitForSeconds(repeatDelay);
                }

                // Use CardActionSequence if available, otherwise fall back to legacy actions
                if (cardData.cardActionSequence != null && cardData.cardActionSequence.Count > 0)
                {
                    var sequence = new CardActionSequence(source, targetingData, cardData);
                    sequence.actions = new List<CardAction>(cardData.cardActionSequence);
                    yield return sequence.Execute();
                }

                yield return null; // wait a frame after applying effects
            }

            // 3️ Discard card after effects (only for player)
            if (cardObj != null && source is PlayerScript)
            {
                HandManager.Instance.DiscardCard(cardObj);
                yield return null; // wait a frame
            }

            // 4️ Update stats for owner and targets
            cardData.Owner.entityStats.UpdateStats();
            foreach (EntityScript e in targetingData.targetedEntities)
            {
                e.entityStats.UpdateStats();
            }

            // 5️ Refresh UI after stats update
            if (CombatUIController.Instance != null)
            {
                CombatUIController.Instance.RefreshAll();
            }

            // 6 Post-combat delay to ensure all effects are processed
            yield return new WaitForSeconds(1f);

            // 7 Signal that this action is complete
            onComplete?.Invoke();
        }
        private static IEnumerator MoveCoroutine(
            EntityOnMap entityOnMap,
            NavMeshPathData pathData,
            Action onComplete)
        {
            Vector3 startPos = entityOnMap.transform.position;
            Vector3 endPos = entityOnMap.transform.position;

            // Execute movement and capture actual end position
            yield return entityOnMap.StartMoveRoutineWithPath(pathData);

            // Get the actual position the entity ended up at
            endPos = entityOnMap.transform.position;

            // Calculate the actual distance traveled
            float distanceTraveled = Vector3.Distance(startPos, endPos);

            // For partial paths, measure expected distance to the last reachable corner, not the unreachable goal
            Vector3 expectedEndPos = pathData.End;
            if (pathData.CachedNavMeshPath.status == UnityEngine.AI.NavMeshPathStatus.PathPartial && 
                pathData.CachedNavMeshPath.corners.Length > 0)
            {
                expectedEndPos = pathData.CachedNavMeshPath.corners[pathData.CachedNavMeshPath.corners.Length - 1];
            }

            float expectedDistance = Vector3.Distance(startPos, expectedEndPos);

            // CRITICAL FIX: Only update stats and fire events if movement actually occurred
            // If distanceTraveled is very small and expected distance was significant, movement failed
            const float MOVEMENT_THRESHOLD = 0.1f;
            bool movementSucceeded = distanceTraveled > MOVEMENT_THRESHOLD || expectedDistance < MOVEMENT_THRESHOLD;

            if (!movementSucceeded)
            {
                Debug.LogWarning($"[ActionQueueUtility] Movement for {entityOnMap.name} failed silently! Expected to travel {expectedDistance:F2}m to {expectedEndPos}, actually traveled {distanceTraveled:F2}m to {endPos}. Path may have been blocked.");
            }

            // Update entity stats after movement (only if movement occurred)
            EntityScript entityScript = entityOnMap.GetComponent<EntityScript>();
            if (entityScript != null)
            {
                OpportunityAttackSystem.CheckAndFireOA(entityScript, startPos, endPos);
                entityScript.entityStats.UpdateStats();
            }

            // Refresh UI after movement completes
            if (CombatUIController.Instance != null)
            {
                CombatUIController.Instance.RefreshAll();
            }

            // Signal completion
            onComplete?.Invoke();
        }

        public static void EnqueueMovement(
            EntityOnMap entityOnMap,
            NavMeshPathData pathData,
            Action onComplete = null)
        {
            EntityScript source = entityOnMap.GetComponent<EntityScript>();
            TimelineManager.GlobalActionQueue.Enqueue(() => MoveCoroutine(entityOnMap, pathData, onComplete), 0f, source);
        }

        private static IEnumerator ActionCoroutine(
            Func<IEnumerator> action,
            Action onComplete)
        {
            yield return action();
            onComplete?.Invoke();
        }
        public static void EnqueueActionRoutine(
        MonoBehaviour source,
        Func<IEnumerator> action,
        Action onComplete = null)
        {
            EntityScript entitySource = source as EntityScript;
            TimelineManager.GlobalActionQueue.Enqueue(() => ActionCoroutine(action, onComplete), 0f, entitySource);
        }

        // Simple action enqueue
        public static void EnqueueAction(
            Action value,
            float delay = 0f)
        {
            TimelineManager.GlobalActionQueue.Enqueue(() =>
            {
                value.Invoke();
            },
            delay);
        }

        /// <summary>
        /// Simple action enqueue with source entity tracking
        /// </summary>
        public static void EnqueueAction(
            Action value,
            float delay,
            EntityScript source)
        {
            TimelineManager.GlobalActionQueue.Enqueue(() =>
            {
                value.Invoke();
            },
            delay,
            source);
        }

        /// <summary>
        /// Enqueue a priority action (executed before non-priority actions). Useful for reactions like counters.
        /// </summary>
        public static void EnqueuePriorityAction(
            Action value,
            float delay = 0f,
            EntityScript source = null)
        {
            TimelineManager.GlobalActionQueue.EnqueuePriority(() =>
            {
                value.Invoke();
            },
            delay,
            source);
        }

        /// <summary>
        /// Enqueue a priority action routine (executed before non-priority actions). Useful for reactions like counters.
        /// </summary>
        public static void EnqueuePriorityActionRoutine(
            MonoBehaviour source,
            Func<IEnumerator> action,
            Action onComplete = null)
        {
            EntityScript entitySource = source as EntityScript;
            TimelineManager.GlobalActionQueue.EnqueuePriority(() => ActionCoroutine(action, onComplete), 0f, entitySource);
        }

        /// <summary>
        /// Enqueue a CardActionSequence directly to the global action queue.
        /// </summary>
        public static void EnqueueCardActionSequence(
            EntityScript source,
            CardActionSequence sequence,
            float delay = 0f,
            Action onComplete = null)
        {
            TimelineManager.GlobalActionQueue.Enqueue(
                () => CardActionSequenceCoroutine(sequence, onComplete),
                delay,
                source
            );
        }

        private static IEnumerator CardActionSequenceCoroutine(
            CardActionSequence sequence,
            Action onComplete)
        {
            yield return sequence.Execute(onComplete);
        }

        /// <summary>
        /// Enqueue a CardActionSequence with priority.
        /// </summary>
        public static void EnqueuePriorityCardActionSequence(
            EntityScript source,
            CardActionSequence sequence,
            float delay = 0f,
            Action onComplete = null)
        {
            TimelineManager.GlobalActionQueue.EnqueuePriority(
                () => CardActionSequenceCoroutine(sequence, onComplete),
                delay,
                source
            );
        }
    }
}