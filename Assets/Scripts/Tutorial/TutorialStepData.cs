using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

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

        [Tooltip("Extra screen-space offset for this arrow only (pixels). " +
                 "Use to push world-space arrows off the entity they are pointing at.")]
        public Vector2 worldTargetOffset;

        [Tooltip("Optional. When assigned, this arrow is only visible while the CanvasGroup's alpha > 0 " +
                 "(e.g. point at a status panel that fades in/out — arrow follows its visibility).")]
        public CanvasGroup visibilityDrivenBy;
    }

    public enum CompletionCondition
    {
        ContinueButton,   // Player clicks the "Continue" button
        CardPlayed,       // allowedCardIds[0] card played
        AnyCardPlayed,    // Any card played (allowedCardIds unused)
        EndTurnPressed,   // End Turn button clicked
        EnemyDead,        // GameEvents.OnCombatEnd(true) fired
        MovedToTarget,    // Player entity reaches movementTarget within movementThreshold world units
    }

    [System.Serializable]
    public class TutorialEnemySpawnEntry
    {
        public string npcId;
        [Tooltip("Empty GameObject placed at the desired spawn position in the scene.")]
        public Transform spawnPoint;
        [Tooltip("If > 0, overrides CurrentHealth after spawn. Use to start enemy at low HP without touching NPC stats.")]
        public float healthOverride;
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

        [Header("Movement (MovedToTarget only)")]
        [Tooltip("World-space Transform the player must reach to complete this step. Required when condition = MovedToTarget.")]
        public Transform movementTarget;
        [Tooltip("World-space distance threshold: step completes when player is within this many units of movementTarget.")]
        public float movementThreshold = 1.5f;

        [Header("Highlight")]
        // Each entry = one arrow+glow. Leave target null on a card entry — manager finds it dynamically.
        public TutorialHighlightEntry[] highlights = System.Array.Empty<TutorialHighlightEntry>();
    }
}
