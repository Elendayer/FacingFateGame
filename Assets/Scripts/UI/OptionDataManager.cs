using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
//using FMODUnity;
//using FMOD.Studio;

namespace facingfate
{
    public class OptionDataManager : MonoBehaviour
    {
        /*
        public static OptionDataManager Instance;

        [FormerlySerializedAs("ShowTutorial")]
        public bool showTutorial = true;

        [FormerlySerializedAs("Volume")] public float volume = 0.8f;
        [FormerlySerializedAs("IsFullscreen")] public bool isFullscreen = true;
        public bool isMuted = false;

        [FormerlySerializedAs("ResolutionIndex")]
        public int resolutionIndex;

        public string selectedLanguageCode = "en"; // Standard auf Englisch

        private VCA _masterVCA;
        private VCA _musicVCA;
        private VCA _sfxVCA;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);

            // FMOD VCAs initialisieren
            _masterVCA = RuntimeManager.GetVCA("vca:/Master");
            _musicVCA = RuntimeManager.GetVCA("vca:/Music");
            _sfxVCA = RuntimeManager.GetVCA("vca:/SFX");
            
        }

        void Start()
        {
            ApplyLanguageSetting();
            ApplyVolumeSettings();
        }

        public void SetShowTutorial(bool show)
        {
            showTutorial = show;
            Debug.Log("Tutorial Show is " + show);
        }

        public void SetVolume(float volume)
        {
            this.volume = volume;
            _masterVCA.setVolume(volume);
        }

        public void SetMusicVolume(float volume)
        {
            _musicVCA.setVolume(volume);
        }

        public void SetSfxVolume(float volume)
        {
            _sfxVCA.setVolume(volume);
        }

        public void MuteToggle(bool muted)
        {
            isMuted = muted;
            float muteVolume = muted ? 0f : volume;
            _masterVCA.setVolume(muteVolume);
        }

        public void SetFullscreen(bool isFullscreen)
        {
            this.isFullscreen = isFullscreen;
            Screen.fullScreen = isFullscreen;
        }

        public void SetResolution(int resolutionIndex)
        {
            this.resolutionIndex = resolutionIndex;
            var resolution = Screen.resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }

        public void SetLanguage(string localeCode)
        {
            selectedLanguageCode = localeCode;
            var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
            if (locale != null)
            {
                LocalizationSettings.SelectedLocale = locale;
            }
        }

        private void ApplyLanguageSetting()
        {
            if (!string.IsNullOrEmpty(selectedLanguageCode))
            {
                var locale = LocalizationSettings.AvailableLocales.GetLocale(selectedLanguageCode);
                if (locale != null)
                {
                    LocalizationSettings.SelectedLocale = locale;
                }
            }
        }

        private void ApplyVolumeSettings()
        {
            _masterVCA.setVolume(isMuted ? 0f : volume);
            // Optional: Set default volumes for other VCAs if needed
        }
        */
    }
}
