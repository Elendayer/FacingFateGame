using facingfate;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnBinder : MonoBehaviour
{
    private bool _pending = false;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnEndTurnClicked);
        GameEvents.OnTurnStart += OnTurnStart;
    }

    void OnDestroy()
    {
        GameEvents.OnTurnStart -= OnTurnStart;
    }

    private void OnEndTurnClicked()
    {
        if (_pending) return;
        if (TurnManager.Instance.CurrentTurnEntity is PlayerScript)
        {
            _pending = true;
            ActionQueueUtility.EnqueueAction(() => GameEvents.TriggerTurnEnd());
        }
    }

    private void OnTurnStart()
    {
        // Reset only when it's the player's turn again
        if (TurnManager.Instance.CurrentTurnEntity is PlayerScript)
            _pending = false;
    }
}
