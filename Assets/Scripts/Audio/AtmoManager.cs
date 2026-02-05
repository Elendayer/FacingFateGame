using UnityEngine;
//using FMODUnity;
//using FMOD.Studio;
using UnityEngine.SceneManagement;

namespace facingfate
{
    public enum AtmoLevel
    {
        Atmo_title = 0,
        Atmo_gym = 1,
        Atmo_card = 2,
        Atmo_dungeon = 3,
        Atmo_final = 4,
        Atmo_win = 5
    }

    public enum GameScene
    {
        TitleScreen = 0,
        CardPhase = 1,
        DungeonPhase = 2,
        GymTranstitionSzene = 3
        // Weitere bei Bedarf ergänzen
    }

    public class AtmoManager : MonoBehaviour
    {
        public AtmoLevel atmoLevel;

        /*
        public EventReference fmodAtmoEvent;
        public EventReference fmodBGMusicEvent;
        private EventInstance _atmoInstance;
        private EventInstance _BGInstance;

        void Start()
        {
            IsPlaying(_atmoInstance);

            if (!IsPlaying(_atmoInstance))
            {
            _atmoInstance = RuntimeManager.CreateInstance(fmodAtmoEvent);
            _atmoInstance.start();

            }

            IsPlaying(_BGInstance);

            if (!IsPlaying(_BGInstance))
            {
                _BGInstance = RuntimeManager.CreateInstance(fmodBGMusicEvent);
                _BGInstance.start();
            }

            DdCodeEventHandler.AtmosphereLevelChanged += OnAtmosphereLevelChanged;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            GameScene sceneEnum = (GameScene)scene.buildIndex;

            switch (sceneEnum)
            {
                case GameScene.TitleScreen:
                    DdCodeEventHandler.Trigger_AtmosphereLevelChanged(AtmoLevel.Atmo_title);
                    break;

                case GameScene.GymTranstitionSzene:
                    DdCodeEventHandler.Trigger_AtmosphereLevelChanged(AtmoLevel.Atmo_gym);
                    break;

                case GameScene.CardPhase:
                    DdCodeEventHandler.Trigger_AtmosphereLevelChanged(AtmoLevel.Atmo_card);
                    break;

                case GameScene.DungeonPhase:
                    // Noch kein Duell → nur auf konzentriert stellen
                    DdCodeEventHandler.Trigger_AtmosphereLevelChanged(AtmoLevel.Atmo_dungeon);
                    break;

            }
        }

        private void OnAtmosphereLevelChanged(AtmoLevel level)
        {
            atmoLevel = level;
            _atmoInstance.setParameterByName("Atmo_sections", (float)level);
            _BGInstance.setParameterByName("Music_Sections",(float)level);
            Debug.Log($"[AtmoManager] Atmo_sections set to: {level} ({(int)level})");
        }

        bool IsPlaying(FMOD.Studio.EventInstance instance)
        {
            FMOD.Studio.PLAYBACK_STATE state;
            instance.getPlaybackState(out state);
            return state != FMOD.Studio.PLAYBACK_STATE.STOPPED;
        }

        void OnDestroy()
        {
            DdCodeEventHandler.AtmosphereLevelChanged -= OnAtmosphereLevelChanged;
            SceneManager.sceneLoaded -= OnSceneLoaded;

            _atmoInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _atmoInstance.release();
        }
        */
    }
}
