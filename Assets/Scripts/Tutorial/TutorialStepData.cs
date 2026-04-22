using UnityEngine;
using UnityEngine.Localization;

namespace facingfate
{
    public enum CompletionCondition
    {
        ContinueButton,   // Player clicks the "Continue" button
        CardPlayed,       // allowedCardIds[0] card played
        AnyCardPlayed,    // Any card played (allowedCardIds unused)
        EndTurnPressed,   // End Turn button clicked
        EnemyDead,        // GameEvents.OnCombatEnd(true) fired
    }

    [System.Serializable]
    public class TutorialStepData
    {
        [Header("Text")]
        public LocalizedString localizedText;

        [Header("Completion")]
        public CompletionCondition condition;
        public string[] allowedCardIds = System.Array.Empty<string>();
        public bool lockEndTurn = true;
        public bool unlockAll;    // If true, all cards playable — use for EnemyDead / free-play steps

        [Header("Highlight")]
        // Assign a static UI RectTransform (panels, buttons).
        // For cards in hand, leave null — TutorialCombatManager finds the card dynamically.
        public RectTransform highlightTarget;
    }
}
