using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NonPlayerScript : EntityScript
{
    [Header("AI")]
    public NpcAI enemyAI;

    [SerializeField]
    private List<PlannedAction> plan;

    public override void Startup()
    {
        base.Startup();

        enemyAI = new NpcAI(this);
        Debug.Log($"NonPlayerScript Setup complete for {name}");
    }
    public void TakeTurn()
    {
        plan.Clear();

        Debug.Log("-------------------------------------------------------------------------------------------------------");

        plan = enemyAI.BuildTurnPlan();

        StartCoroutine(ExecutePlan(plan));
    }

    private System.Collections.IEnumerator ExecutePlan(List<PlannedAction> plan)
    {
        foreach (PlannedAction action in plan)
        {
            if (action.Type == PlannedAction.ActionType.Move)
            {
                GetComponent<EntityOnMap>().MoveTo(action.TargetCell); 
                yield return new WaitForSeconds(2f); // pacing for readability

            }
            else if (action.Type == PlannedAction.ActionType.PlayCard)
            {
                Debug.Log($"NPC plays {action.Card.cardData.cardName} on {string.Join(", ", action.Targets.Select(t => t.name))}");
                action.Card.cardData.ActivateCard(action.Targets, gameObject);
                yield return new WaitForSeconds(1f); // pacing for readability
            }
        }
        EventManager.Instance.Endturn();
    }
}