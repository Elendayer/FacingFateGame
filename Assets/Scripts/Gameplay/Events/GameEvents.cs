using System;

public static class GameEvents
{
    public static event Action OnTurnStart;
    public static void TriggerTurnStart() => OnTurnStart?.Invoke();

    public static event Action<gameplayReference> OnReferenceEvent;
    public static void TriggerReferenceEvent(gameplayReference reference)
        => OnReferenceEvent?.Invoke(reference);
}