using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace facingfate
{
    // Must match Switch names in Wwise Switch Group "SceneType" exactly.
    public enum AtmoLevel
    {
        None,
        TitleScreen,
        Tutorial,
        Combat,
        RandomCombat
    }

    [System.Serializable]
    public struct SceneAtmoMapping
    {
        public string    sceneName;      // Unity scene name, e.g. "Gameplay_Combat_Map"
        public AtmoLevel atmoLevel;      // Wwise Switch value → SceneType group (for atmo)
        public string    musicStateName; // Wwise State value → LVL_State group (for music)
                                         // Must match exactly: "Combat", "Title_Screen", "Tutorial", "Random_Combat"
    }

    public class AtmoManager : MonoBehaviour
    {
        [Header("Wwise Events")]
        // Both events post on this gameObject. One SetSwitch call controls both atmo sounds.
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event atmoEvent;
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event musicEvent;

        [Header("Wwise Groups")]
        // Switch Group for atmo sounds — must match Wwise Switch Group name exactly.
        [SerializeField] private string switchGroup = "SceneType";
        // State Group for music — must match Wwise State Group name exactly.
        [SerializeField] private string musicStateGroup = "LVL_State";

        [Header("Scene → Atmo Mappings")]
        // One entry per scene.
        // atmoLevel     = Wwise Switch value (SceneType) — controls atmo sounds
        // musicStateName = Wwise State value (LVL_State) — controls music
        //                  e.g. "Title_Screen", "Combat", "Tutorial", "Random_Combat"
        [SerializeField] private List<SceneAtmoMapping> sceneMappings = new();

        [Header("Debug (read-only)")]
        [SerializeField] private AtmoLevel currentAtmoLevel = AtmoLevel.None;

        private uint _atmoPlayingId;
        private uint _musicPlayingId;

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Set switch + state FIRST so music starts with correct variant from frame 1
            ApplyAtmoForScene(SceneManager.GetActiveScene().name);

            if (atmoEvent != null && atmoEvent.IsValid())
                _atmoPlayingId = atmoEvent.Post(gameObject);

            if (musicEvent != null && musicEvent.IsValid())
                _musicPlayingId = musicEvent.Post(gameObject);
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

                // Atmo sounds — Switch (per-GO, affects atmoEvent on this gameObject)
                SetAtmoLevel(mapping.atmoLevel);

                // Music — State (global, drives MUSIC_Demo Switch Container via LVL_State)
                if (!string.IsNullOrEmpty(mapping.musicStateName))
                {
                    AkUnitySoundEngine.SetState(musicStateGroup, mapping.musicStateName);
                    Debug.Log($"[AtmoManager] State → {musicStateGroup}/{mapping.musicStateName}");
                }
                return;
            }
            // No mapping found → keep current state, no change
        }

        // Call from other scripts for manual override (e.g. boss fight, cutscene).
        public void SetAtmoLevel(AtmoLevel level)
        {
            if (level == currentAtmoLevel) return;
            currentAtmoLevel = level;

            // Affects atmo sounds posted on this gameObject
            AkUnitySoundEngine.SetSwitch(switchGroup, level.ToString(), gameObject);
            Debug.Log($"[AtmoManager] Switch → {switchGroup}/{level}");
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
