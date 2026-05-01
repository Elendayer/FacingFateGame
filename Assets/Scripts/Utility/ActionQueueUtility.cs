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

                ApplyCardEffect(source, cardData, targetingData);
                // Trying to ensure all effects are applied before moving on to VFX
                ApplyCardFX(source, cardData, targetingData);

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

        private static void ApplyCardEffect(
            EntityScript source,
            CardData cardData,
            TargetingModeData targetingData)
        {
            foreach (Vector3 tile in targetingData.targetedPositions)
            {
                cardData.cardEffectGroundAction?.Invoke(source, tile, cardData);
            }
            foreach (EntityScript target in targetingData.targetedEntities)
            {
                cardData.cardEffectAction?.Invoke(source, target, cardData);
            }
        }
        private static void ApplyCardFX(
            EntityScript source,
            CardData cardData,
            TargetingModeData targetingData,
            bool atEach = false)
        {            
            if (atEach)
            {
                foreach (EntityScript target in targetingData.targetedEntities)
                {
                    cardData.cardVfx?.Invoke(cardData, new TargetingModeData
                    {
                        targetedEntities = new List<EntityScript> { target },
                        targetedPositions = new List<Vector3>(),
                        castingPosition = source.transform.position,
                    });
                }
            }
            else
                cardData.cardVfx?.Invoke(cardData, targetingData);
        }

        private static IEnumerator MoveCoroutine(
            EntityOnMap entityOnMap,
            NavMeshPathData pathData,
            Action onComplete)
        {
            Vector3 startPos = entityOnMap.transform.position;

            yield return entityOnMap.StartMoveRoutineWithPath(pathData);

            // Update entity stats after movement completes
            EntityScript entityScript = entityOnMap.GetComponent<EntityScript>();
            if (entityScript != null)
            {
                OpportunityAttackSystem.CheckAndFireOA(entityScript, startPos, entityOnMap.transform.position);
                entityScript.entityStats.UpdateStats();
            }

            // Refresh UI after movement completes
            if (CombatUIController.Instance != null)
            {
                CombatUIController.Instance.RefreshAll();
            }

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
    }
}