using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TimelineManager;
using Utility;

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

    /// <summary>
    /// Starts the NPC's turn by building a plan and executing all actions via the global queue.
    /// </summary>
    public void TakeTurn()
    {
        plan.Clear();

        Debug.Log($"--------- {name}'s Turn ---------");

        plan = npcAIController.BuildTurnPlan();

        Debug.Log($"[NonPlayerScript] {name} has planned {plan.Count} actions for this turn.");

        // Start executing the plan
        StartCoroutine(ExecutePlan(plan));
    }

    /// <summary>
    /// Executes a list of planned actions sequentially via the global action queue.
    /// </summary>
    private IEnumerator ExecutePlan(List<PlannedAction> plan)
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

            // Optional buffer between actions
            yield return new WaitForSeconds(0.25f);
        }

        // Small delay after all actions
        yield return new WaitForSeconds(0.5f);

        // Notify end of turn
        EventManager.Instance.Endturn();
    }

    /// <summary>
    /// Enqueues a movement action through the ActionQueueUtility.
    /// </summary>
    private void EnqueueMoveAction(PlannedAction action)
    {
        var entityOnMap = GetComponent<EntityOnMap>();
        bool moveComplete = false;

        // Enqueue movement with callback
        ActionQueueUtility.EnqueueMovement(entityOnMap, action.PathData, () => moveComplete = true);
    }

    /// <summary>
    /// Enqueues a card action through the ActionQueueUtility.
    /// </summary>
    private void EnqueueCardAction(PlannedAction action)
    {
        Debug.Log($"[NPC] {name} plays {action.Card.cardData.cardName} on " + $"{string.Join(", ", action.TargetingModeData.targetedEntities.Select(t => t.name))}");

        // Enqueue card effects (handles repeats internally)
        ActionQueueUtility.EnqueueCardExecution(this, action.Card.cardData, action.TargetingModeData);

        // After all repeats resolve, discard the card
        ActionQueueUtility.EnqueueAction(() =>
        {
            HandManager.Instance.DiscardCard(action.Card.gameObject);
        });
    }
}
