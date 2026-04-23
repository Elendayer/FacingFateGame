using UnityEngine;
using UnityEngine.Localization;

namespace facingfate
{
    public enum ArrowDirection
    {
        Left,   // Arrow left of target, points right  (rotation   0°)
        Right,  // Arrow right of target, points left  (rotation 180°)
        Up,     // Arrow above target, points down      (rotation  90°)
        Down,   // Arrow below target, points up        (rotation 270°)
    }

    [System.Serializable]
    public class TutorialHighlightEntry
    {
        // Leave null for card-in-hand targets — TutorialCombatManager resolves dynamically.
        public RectTransform target;

        // Set this instead of target to track a world-space entity (player, enemy).
        // PointerPositionScript updates position every frame via WorldToScreenPoint.
        public Transform worldTarget;

        public ArrowDirection direction = ArrowDirection.Left;
    }

    public enum CompletionCondition
    {
        ContinueButton,   // Player clicks the "Continue" button
        CardPlayed,       // allowedCardIds[0] card played
        AnyCardPlayed,    // Any card played (allowedCardIds unused)
        EndTurnPressed,   // End Turn button clicked
        EnemyDead,        // GameEvents.OnCombatEnd(true) fired
    }

    [System.Serializable]
    public class TutorialEnemySpawnEntry
    {
        public string npcId;
        [Tooltip("Empty GameObject placed at the desired spawn position in the scene.")]
        public Transform spawnPoint;
    }

    [System.Serializable]
    public class TutorialEnemyWave
    {
        [Tooltip("Step index at which these enemies spawn.")]
        public int stepIndex;
        [Tooltip("Affiliation for all enemies in this wave.")]
        public EntityAffiliation affiliation = EntityAffiliation.Enemy;
        public TutorialEnemySpawnEntry[] entries = System.Array.Empty<TutorialEnemySpawnEntry>();
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
        // Each entry = one arrow+glow. Leave target null on a card entry — manager finds it dynamically.
        public TutorialHighlightEntry[] highlights = System.Array.Empty<TutorialHighlightEntry>();
    }
}
