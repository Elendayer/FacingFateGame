using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility;
using static TimelineManager;

public class NonPlayerScript : EntityScript
{
    [Header("AI")]
    public string NpcID = "0001";
    public NpcAIController npcAIController;
    public NpcData npcData = new();

    [SerializeField]
    private List<PlannedAction> plan = new();

    public override void StartUp()
    {
        base.StartUp();

        // Load NPC data
        npcData = NpcDatabase.GetNpcById(NpcID, this);

        // Initialize AI
        npcAIController = new NpcAIController(this, npcData);

        // Set entity name
        name = $"{entityAffiliation}_{npcData.name}";

        // Build deck from NPC data
        deckCardIDs = npcData.cardIds;
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
                Debug.Log($"[NonPlayerScript] {name} has completed all planned actions for this turn.");
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

        ActionQueueUtility.EnqueueAction(() =>
        {
            // Step 1: Build the plan
            plan = npcAIController.BuildTurnPlan();
        });

        ActionQueueUtility.EnqueueAction(() =>
        {
            // Step 2: Execute the plan (enqueue actions)
            ExecutePlan(plan);

            // Step 3: Enqueue Endturn AFTER the plan actions
            ActionQueueUtility.EnqueueAction(() =>
            {
                EventManager.Instance.Endturn();
            }, 1f);
        });
    }


    /// <summary>
    /// Executes a list of planned actions sequentially via the global action queue.
    /// </summary>
    private void ExecutePlan(List<PlannedAction> plan)
    {
        foreach (PlannedAction action in plan)
        {
            switch (action.Type)
            {
                case PlannedAction.ActionType.Move:
                    EnqueueMoveAction(action);
                    break;

                case PlannedAction.ActionType.PlayCard:
                    EnqueueCardAction(action);
                    break;
            }
        }
    }

    /// <summary>
    /// Enqueues a movement action through the ActionQueueUtility.
    /// </summary>
    private void EnqueueMoveAction(PlannedAction action)
    {
        var entityOnMap = GetComponent<EntityOnMap>();
        bool moveComplete = false;

        // Enqueue movement with callback
        ActionQueueUtility.EnqueueMovement(entityOnMap, action.PathData);
    }

    /// <summary>
    /// Enqueues a card action through the ActionQueueUtility.
    /// </summary>
    private void EnqueueCardAction(PlannedAction action)
    {
        // Enqueue card effects (handles repeats internally)
        ActionQueueUtility.EnqueueCardExecution(this, action.Card.cardData, action.TargetingModeData);

        // After all repeats resolve, discard the card
        ActionQueueUtility.EnqueueAction(() =>
        {
            HandManager.Instance.DiscardCard(action.Card.gameObject);
        });
    }
}
