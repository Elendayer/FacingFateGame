using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace facingfate
{
    [System.Serializable]
    public struct SceneAtmoMapping
    {
        public string sceneName;  // Unity scene name, e.g. "Gameplay_Combat_Map"
        public string stateName;  // Wwise LVL_State value, e.g. "Combat", "Title_Screen"
    }

    public class AtmoManager : MonoBehaviour
    {
        [Header("Wwise Events")]
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event atmoEvent;
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event musicEvent;

        [Header("Wwise State Group")]
        // Must match State Group name in Wwise exactly.
        [SerializeField] private string stateGroup = "LVL_State";
        // Player state set on startup so Music Switch Container finds a valid path.
        // Override per-scene or on player death via SetState("Player_State", ...).
        [SerializeField] private string playerStateGroup = "Player_State";
        [SerializeField] private string playerAliveState = "PlayerAlive_State";

        [Header("Scene → State Mappings")]
        // One entry per scene.
        // stateName must match Wwise State name exactly, e.g. "Combat", "Title_Screen", "Tutorial", "Random_Combat"
        [SerializeField] private List<SceneAtmoMapping> sceneMappings = new();

        [Header("Debug (read-only)")]
        [SerializeField] private string currentStateName = "None";

        private uint _atmoPlayingId;
        private uint _musicPlayingId;

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Set player state so Music Switch Container finds a valid path
            if (!string.IsNullOrEmpty(playerStateGroup) && !string.IsNullOrEmpty(playerAliveState))
                AkUnitySoundEngine.SetState(playerStateGroup, playerAliveState);

            // Set scene state FIRST so correct variant plays from frame 1
            ApplyAtmoForScene(SceneManager.GetActiveScene().name);
            if (musicEvent != null && musicEvent.IsValid())
                _musicPlayingId = musicEvent.Post(gameObject);

            if (atmoEvent != null && atmoEvent.IsValid())
                _atmoPlayingId = atmoEvent.Post(gameObject);

        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyAtmoForScene(scene.name);
        }

        private void ApplyAtmoForScene(string sceneName)
        {
            foreach (var mapping in sceneMappings)
            {
                if (mapping.sceneName != sceneName) continue;
                SetState(mapping.stateName);
                return;
            }
            // No mapping found → keep current state
        }

        // Call from other scripts for manual override (e.g. boss fight, cutscene).
        public void SetState(string stateName)
        {
            if (string.IsNullOrEmpty(stateName) || stateName == currentStateName) return;
            currentStateName = stateName;
            AkUnitySoundEngine.SetState(stateGroup, stateName);
            Debug.Log($"[AtmoManager] State → {stateGroup}/{stateName}");
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (_atmoPlayingId != 0)
                AkUnitySoundEngine.StopPlayingID(_atmoPlayingId, 500);

            if (_musicPlayingId != 0)
                AkUnitySoundEngine.StopPlayingID(_musicPlayingId, 500);
        }
    }
}
