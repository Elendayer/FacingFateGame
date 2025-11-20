using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NonPlayerScript : EntityScript
{
    [Header("AI")]
    public string NpcID = "0001";
    public NpcAIController npcAIController;

    [SerializeField]
    private List<PlannedAction> plan = new();

    public override void StartUp()
    {
        base.StartUp();

        // Load NPC data and initialize AI
        NpcData npcData = NpcDatabase.GetNpcById(NpcID, this);
        npcAIController = new NpcAIController(this, npcData);

        name = $"{entityAffiliation}_{npcData.name}";

        // Load Cards from NpcData
        deckCardIDs = npcData.cardIds;
        DeckManager.Instance.BuildDeckFromIDs(this);

        Debug.Log($"[NonPlayerScript] Setup complete for {name}");
    }

    public void TakeTurn()
    {
        plan.Clear();
        Debug.Log($" ---------{name}'s Turn ----------------------------------------------------------------------------------------------");

        plan = npcAIController.BuildTurnPlan();

        StartCoroutine(ExecutePlan(plan));
    }

    private IEnumerator ExecutePlan(List<PlannedAction> plan)
    {
        foreach (PlannedAction action in plan)
        {
            switch (action.Type)
            {
                case PlannedAction.ActionType.Move:
                    yield return ExecuteMove(action);
                    break;

                case PlannedAction.ActionType.PlayCard:
                    yield return ExecuteCard(action);
                    break;
            }

            // Optional pacing for readability
            yield return new WaitForSeconds(1f);
        }

        EventManager.Instance.Endturn();
    }

    private IEnumerator ExecuteMove(PlannedAction action)
    {
        Debug.Log($"[NPC] {name} moves to {action.TargetCell} because {action.Name}");

        GameEvents.TriggerRefEvent(new TriggerRef
        {
            OnTriggerReference = new() { GameplayRef.onMove },
            UserEntity = this,
            AffectedEntity = this
        });

        var entityOnMap = GetComponent<EntityOnMap>();
        bool moveComplete = false;

        // Start the move coroutine
        StartCoroutine(MoveToViaPathWithCallback(entityOnMap, action.Path, () => moveComplete = true));

        // Wait until movement finishes
        while (!moveComplete)
            yield return null;
    }

    private IEnumerator ExecuteCard(PlannedAction action)
    {
        Debug.Log($"[NPC] {name} plays {action.Card.cardData.cardName} on {string.Join(", ", action.Targets.Select(t => t.name))}");

        bool cardComplete = false;

        StartCoroutine(PlayCardWithCallback(action, () => cardComplete = true));

        while (!cardComplete)
            yield return null;
    }

    private IEnumerator MoveToViaPathWithCallback(EntityOnMap entityOnMap, List<Vector3Int> path, System.Action onComplete)
    {
        yield return entityOnMap.StartCoroutine("StartMove", path);
        onComplete?.Invoke();
    }

    private IEnumerator PlayCardWithCallback(PlannedAction action, System.Action onComplete)
    {
        // Activate card effects
        action.Card.cardData.ActivateCard(action.Targets, gameObject);
        onComplete?.Invoke();
        yield return null;
    }
}
