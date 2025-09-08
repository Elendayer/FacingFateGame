using System;

public static class GameEvents
{
    public static event Action OnTurnStart;
    public static event Action OnTurnEnd;
    public static void TriggerTurnStart() => OnTurnStart?.Invoke();
    public static void TriggerTurnEnd() => OnTurnEnd?.Invoke();

    public static event Action<gameplayReference> OnReferenceEvent;
    public static void TriggerReferenceEvent(gameplayReference reference)
        => OnReferenceEvent?.Invoke(reference);
}