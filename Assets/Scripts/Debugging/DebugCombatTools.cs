using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using facingfate;

/// <summary>
/// Debug hotkeys for combat testing. Add to any scene GameObject.
/// F1 = kill all enemies (win)  |  F2 = kill all players (lose)  |  F3 = full scene reset
/// </summary>
public class DebugCombatTools : MonoBehaviour
{
    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.f1Key.wasPressedThisFrame) KillAllEnemies();
        if (kb.f2Key.wasPressedThisFrame) KillAllPlayers();
        if (kb.f3Key.wasPressedThisFrame) ResetScene();
    }

    private static void KillAllEnemies()
    {
        foreach (var e in FindObjectsByType<EntityScript>(FindObjectsSortMode.None))
        {
            if (e == null || !e.enabled || e.GetComponent<PlayerScript>() != null) continue;
            KillEntity(e);
        }
    }

    private static void KillAllPlayers()
    {
        foreach (var e in FindObjectsByType<EntityScript>(FindObjectsSortMode.None))
        {
            if (e == null || !e.enabled || e.GetComponent<PlayerScript>() == null) continue;
            KillEntity(e);
        }
    }

    private static void KillEntity(EntityScript entity)
    {
        if (entity.entityStats == null) return;
        entity.entityStats.CurrentHealth = 0;
        entity.entityStats.UpdateStats();
    }

    private static void ResetScene()
    {
        // Reset TimelineManager static state — these survive object destruction
        TimelineManager.isPaused    = false;
        TimelineManager.Timeline?.Clear();
        TimelineManager.GlobalActionQueue = null;

        // Destroy game-state singletons that use DontDestroyOnLoad so the reloaded
        // scene gets a completely fresh instance (no stale decks, turn order, etc.)
        DestroyPersistent(DeckManager.Instance);
        DestroyPersistent(TurnManager.Instance);
        DestroyPersistent(TimelineManager.Instance);
        DestroyPersistent(StartupManager.Instance);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static void DestroyPersistent(MonoBehaviour singleton)
    {
        if (singleton != null)
            Destroy(singleton.gameObject);
    }
}
