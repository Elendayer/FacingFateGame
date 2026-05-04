using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace facingfate
{
    public enum AtmoLevel
    {
        None     = -1,
        Title    = 0,
        Tutorial = 1,
        Encounter = 2
    }

    [System.Serializable]
    public struct SceneAtmoMapping
    {
        public string    sceneName;
        public AtmoLevel atmoLevel;
    }

    public class AtmoManager : MonoBehaviour
    {
        [Header("Wwise Events")]
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event atmoEvent;
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event musicEvent;

        [Header("Wwise State Group")]
        [SerializeField] private string stateGroup = "GameScene";

        [Header("Scene → Atmo Mappings")]
        [SerializeField] private List<SceneAtmoMapping> sceneMappings = new();

        [Header("Debug (read-only)")]
        [SerializeField] private AtmoLevel currentAtmoLevel = AtmoLevel.None;

        private uint _atmoPlayingId;
        private uint _musicPlayingId;

        private void Start()
        {
            if (atmoEvent != null && atmoEvent.IsValid())
                _atmoPlayingId = atmoEvent.Post(gameObject);

            if (musicEvent != null && musicEvent.IsValid())
                _musicPlayingId = musicEvent.Post(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;

            // Apply immediately for the scene that is already active
            ApplyAtmoForScene(SceneManager.GetActiveScene().name);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyAtmoForScene(scene.name);
        }

        private void ApplyAtmoForScene(string sceneName)
        {
            foreach (var mapping in sceneMappings)
            {
                if (mapping.sceneName == sceneName)
                {
                    SetAtmoLevel(mapping.atmoLevel);
                    return;
                }
            }
            // No mapping found → no state change, atmo stays as-is
        }

        // Call from other scripts if a manual override is needed
        public void SetAtmoLevel(AtmoLevel level)
        {
            if (level == currentAtmoLevel) return;
            currentAtmoLevel = level;
            AkUnitySoundEngine.SetState(stateGroup, level.ToString());
            Debug.Log($"[AtmoManager] State → {stateGroup}/{level}");
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
