using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NonPlayerScript : EntityScript
{
    [Header("AI")]
    public NpcAIController npcAIController;

    public NpcAiBias npcAIBias = new();

    [SerializeField]
    private List<PlannedAction> plan;

    public override void StartUp()
    {
        base.StartUp();

        npcAIController = new NpcAIController(this);
        Debug.Log($"[NonPlayerScript] Setup complete for {name}");
    }
    public void TakeTurn()
    {
        plan.Clear();

        Debug.Log("-------------------------------------------------------------------------------------------------------");

        plan = npcAIController.BuildTurnPlan();

        StartCoroutine(ExecutePlan(plan));
    }

    private System.Collections.IEnumerator ExecutePlan(List<PlannedAction> plan)
    {
        foreach (PlannedAction action in plan)
        {
            if (action.Type == PlannedAction.ActionType.Move)
            {
                Debug.Log($"[NPC] {name} moves to {action.TargetCell}");
                var entityOnMap = GetComponent<EntityOnMap>();
                bool moveComplete = false;
                // Start the move and wait for completion
                System.Action onMoveComplete = () => moveComplete = true;
                StartCoroutine(MoveToViaPathWithCallback(entityOnMap, action.Path, onMoveComplete));
                while (!moveComplete)
                    yield return null;
                yield return new WaitForSeconds(1f); // pacing for readability
            }
            else if (action.Type == PlannedAction.ActionType.PlayCard)
            {
                Debug.Log($"[NPC] {name} plays {action.Card.cardData.cardName} on {string.Join(", ", action.Targets.Select(t => t.name))}");
                //bool cardComplete = false;
                // If ActivateCard is async, you should hook a callback/event here. For now, just yield for pacing.
                action.Card.cardData.ActivateCard(action.Targets, gameObject);
                yield return new WaitForSeconds(2f); // pacing for readability
            }
        }
        EventManager.Instance.Endturn();
    }

    // Helper coroutine to wait for MoveToViaPath to finish
    private System.Collections.IEnumerator MoveToViaPathWithCallback(EntityOnMap entityOnMap, List<Vector3Int> path, System.Action onComplete)
    {
        yield return entityOnMap.StartCoroutine("StartMove", path);
        onComplete?.Invoke();
    }
} 