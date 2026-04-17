using System;
using System.Collections.Generic;
using UnityEngine;

namespace facingfate
{
    public class NonPlayerScript : EntityScript
    {
        [Header("AI")]
        public string NpcID = "0001";
        public NpcAIController npcAIController;
        public NpcData npcData = new();

        /// <summary>
        /// When true, StartUp skips NpcDatabase lookup and uses the pre-assigned npcData + deckCardIDs.
        /// Set by RandomEncounterManager before the StartUp loop.
        /// </summary>
        [HideInInspector] public bool usePresetConfig = false;

        [SerializeField]
        private List<PlannedAction> plan = new();

        public override void StartUp()
        {
            base.StartUp();

            if (usePresetConfig)
            {
                // Preset by RandomEncounterManager — skip database lookup
                npcAIController = new NpcAIController(this, npcData);
                // deckCardIDs already set; name already set on the GameObject
            }
            else
            {
                // Normal flow — load from NpcDatabase
                npcData = NpcDatabase.GetNpcById(NpcID, this);
                npcAIController = new NpcAIController(this, npcData);
                name = $"{entityAffiliation}_{npcData.name}";
                deckCardIDs = npcData.cardIds;
            }

            DeckManager.Instance.BuildDeckFromIDs(this);

            Debug.Log($"[NonPlayerScript] Setup complete for {name}");
        }

        public override void StartTurn()
        {
            base.StartTurn();

            if (entityStats.IsStunned)
            {
                ActionQueueUtility.EnqueueAction(() =>
                {
                    Debug.Log($"[NonPlayerScript] {name} is stunned and skips their turn.");
                    EventManager.Instance.Endturn();
                });

                entityStats.IsStunned = false;

                return;
            }
            else
            {
                TakeTurn();
            }
        }

        /// <summary>
        /// Starts the NPC's turn by building a plan and executing all actions via the global queue.
        /// </summary>
        public void TakeTurn()
        {
            plan.Clear();

            // Step 1: Build plan
            ActionQueueUtility.EnqueueAction(() =>
            {
                npcAIController.BuildTurnPlan((builtPlan) =>
                {
                    plan = builtPlan;

                    // Step 2: Execute plan AFTER planning finishes
                    ExecutePlanSequentially(plan, () =>
                    {
                        // Step 3: End turn after plan finishes
                        ActionQueueUtility.EnqueueAction(() =>
                        {
                            EventManager.Instance.Endturn();
                        }, 1f);
                    });
                });
            });
        }

        /// <summary>
        /// Executes a list of planned actions sequentially, waiting for each to complete.
        /// </summary>
        private void ExecutePlanSequentially(List<PlannedAction> plan, Action onAllActionsComplete)
        {
            ExecuteNextAction(plan, 0, onAllActionsComplete);
        }

        /// <summary>
        /// Recursively executes the next action in the plan, waiting for completion.
        /// </summary>
        private void ExecuteNextAction(List<PlannedAction> plan, int actionIndex, Action onAllActionsComplete)
        {
            // If all actions are complete, invoke callback
            if (actionIndex >= plan.Count)
            {
                onAllActionsComplete?.Invoke();
                return;
            }

            PlannedAction action = plan[actionIndex];

            // Execute current action with completion callback to move to next action
            switch (action.Type)
            {
                case PlannedAction.ActionType.Move:
                    EnqueueMoveAction(action, () =>
                    {
                        ExecuteNextAction(plan, actionIndex + 1, onAllActionsComplete);
                    });
                    break;

                case PlannedAction.ActionType.PlayCard:
                    EnqueueCardAction(action, () =>
                    {
                        ExecuteNextAction(plan, actionIndex + 1, onAllActionsComplete);
                    });
                    break;
            }
        }

        /// <summary>
        /// Enqueues a movement action through the ActionQueueUtility with completion callback.
        /// Uses the cached NavMesh path for optimized movement.
        /// </summary>
        private void EnqueueMoveAction(PlannedAction action, Action onActionComplete)
        {
            Debug.Log($"[NpcAI] {name} moving to {action.PathData.End}");

            ActionQueueUtility.EnqueueActionRoutine(this, () =>
                entityOnMap.StartMoveRoutineWithPath(action.PathData), () =>
            {
                Debug.Log($"[NpcAI] {name} finished moving");
                onActionComplete?.Invoke();
            });
        }

        /// <summary>
        /// Enqueues a card action through the ActionQueueUtility with completion callback.
        /// </summary>
        private void EnqueueCardAction(PlannedAction action, Action onActionComplete)
        {
            Debug.Log($"[NpcAI] {name} playing card {action.Name}");

            // Enqueue card effects with callback to signal completion
            ActionQueueUtility.EnqueueCardExecution(this, action.Card.cardData, action.TargetingModeData, null, 0.25f, () =>
            {
                Debug.Log($"[NpcAI] {name} finished playing card");
                onActionComplete?.Invoke();
            });
        }
    }
}