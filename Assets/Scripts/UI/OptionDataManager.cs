using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
//using FMODUnity;
//using FMOD.Studio;

namespace facingfate
{
    public class OptionDataManager : MonoBehaviour
    {
        public static OptionDataManager Instance { get; private set; }

        [SerializeField] private bool showTutorial = true;
        [SerializeField, Range(0f, 1f)] private float masterSlider01 = 0.8f;
        [SerializeField, Range(0f, 1f)] private float musicSlider01 = 0.8f;
        [SerializeField, Range(0f, 1f)] private float sfxSlider01 = 0.8f;
        [SerializeField, Range(0f, 1f)] private float dialogueSlider01 = 0.8f;
        [SerializeField, Range(0f, 1f)] private float atmoSlider01 = 0.8f;
        [SerializeField] private bool isMuted = false;
        [SerializeField] private FullScreenMode fullscreenMode = FullScreenMode.FullScreenWindow;
        [SerializeField] private int windowWidth = 1920;
        [SerializeField] private int windowHeight = 1080;
        public string selectedLanguageCode = "en"; // Standard auf Englisch

        [Header("Wwise RTPC Names")]
        [SerializeField] private string masterVolumeRtpc = "RTPC_master_Vol";
        [SerializeField] private string musicVolumeRtpc = "RTPC_music_Vol";
        [SerializeField] private string sfxVolumeRtpc = "RTPC_sfx_Vol";
        [SerializeField] private string atmoVolumeRtpc = "RTPC_atmo_Vol";
        [SerializeField] private string dialogueVolumeRtpc = "RTPC_dialogue_Vol";

        [Header("RTPC Range")]
        [SerializeField] private float rtpcMin = 0f;
        [SerializeField] private float rtpcMax = 10f;

        private const string PREF_MASTER = "opt_master_01";
        private const string PREF_MUSIC = "opt_music_01";
        private const string PREF_SFX = "opt_sfx_01";
        private const string PREF_ATMO = "opt_atmo_01";
        private const string PREF_DIALOGUE = "opt_dialogue_01";
        private const string PREF_MUTED = "opt_muted";
        private const string PREF_FULLSCREEN_MODE = "opt_fullscreen_mode";
        private const string PREF_WINDOW_WIDTH = "opt_window_width";
        private const string PREF_WINDOW_HEIGHT = "opt_window_height";
        private const string PREF_LANGUAGE = "opt_language_code";
        private const string PREF_SETTINGS_VERSION = "opt_settings_version";
        private const int SETTINGS_VERSION = 5;

        public float Master01 => masterSlider01;
        public float Music01 => musicSlider01;
        public float Sfx01 => sfxSlider01;
        public float Atmo01 => atmoSlider01;
        public float Dialogue01 => dialogueSlider01;
        public bool IsMuted => isMuted;
        public FullScreenMode FullscreenMode => fullscreenMode;
        public int WindowWidth => windowWidth;
        public int WindowHeight => windowHeight;
        public string SelectedLanguageCode => selectedLanguageCode;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);

            LoadFromPrefs();
            // Apply display settings immediately before scene fully initializes
            ApplyDisplaySettings();
        }

        private IEnumerator Start()
        {
            yield return LocalizationSettings.InitializationOperation;
            ApplyAudioRtpcs();
        }

        public void SetMasterVolume01(float slider01)
        {
            masterSlider01 = Mathf.Clamp01(slider01);
            PlayerPrefs.SetFloat(PREF_MASTER, masterSlider01);
            PlayerPrefs.Save();

            if (!isMuted)
                ApplyAudioRtpcs();
        }

        public void SetMusicVolume01(float slider01)
        {
            musicSlider01 = Mathf.Clamp01(slider01);
            PlayerPrefs.SetFloat(PREF_MUSIC, musicSlider01);
            PlayerPrefs.Save();

            ApplyAudioRtpcs();
        }

        public void SetSfxVolume01(float slider01)
        {
            sfxSlider01 = Mathf.Clamp01(slider01);
            PlayerPrefs.SetFloat(PREF_SFX, sfxSlider01);
            PlayerPrefs.Save();

            ApplyAudioRtpcs();
        }

        public void SetDialogueVolume01(float slider01)
        {
            dialogueSlider01 = Mathf.Clamp01(slider01);
            PlayerPrefs.SetFloat(PREF_DIALOGUE, dialogueSlider01);
            PlayerPrefs.Save();

            ApplyAudioRtpcs();
        }

        public void SetAtmoVolume01(float slider01)
        {
            atmoSlider01 = Mathf.Clamp01(slider01);
            PlayerPrefs.SetFloat(PREF_ATMO, atmoSlider01);
            PlayerPrefs.Save();

            ApplyAudioRtpcs();
        }

        public void MuteToggle(bool muted)
        {
            isMuted = muted;
            PlayerPrefs.SetInt(PREF_MUTED, muted ? 1 : 0);
            PlayerPrefs.Save();

            ApplyAudioRtpcs();
        }

        public void SetFullscreen(bool fullscreen)
        {
            fullscreenMode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            PlayerPrefs.SetInt(PREF_FULLSCREEN_MODE, (int)fullscreenMode);
            PlayerPrefs.Save();

            ApplyDisplaySettings();
        }

        public void SetFullscreenMode(FullScreenMode mode)
        {
            fullscreenMode = mode;
            PlayerPrefs.SetInt(PREF_FULLSCREEN_MODE, (int)fullscreenMode);
            PlayerPrefs.Save();

            ApplyDisplaySettings();
        }

        public void SetWindowSize(int width, int height)
        {
            windowWidth = Mathf.Max(640, width);
            windowHeight = Mathf.Max(480, height);
            PlayerPrefs.SetInt(PREF_WINDOW_WIDTH, windowWidth);
            PlayerPrefs.SetInt(PREF_WINDOW_HEIGHT, windowHeight);
            PlayerPrefs.Save();

            if (fullscreenMode == FullScreenMode.Windowed)
            {
                Screen.SetResolution(windowWidth, windowHeight, fullscreenMode);
            }
        }

        public void SetResolution(int width, int height)
        {
            windowWidth = Mathf.Max(640, width);
            windowHeight = Mathf.Max(480, height);
            PlayerPrefs.SetInt(PREF_WINDOW_WIDTH, windowWidth);
            PlayerPrefs.SetInt(PREF_WINDOW_HEIGHT, windowHeight);
            PlayerPrefs.Save();
            ApplyDisplaySettings();
        }

        public void SetLanguage(string localeCode)
        {
            selectedLanguageCode = localeCode;
            PlayerPrefs.SetString(PREF_LANGUAGE, selectedLanguageCode);
            PlayerPrefs.Save();

            ApplyLanguageSetting();
        }

        public void ApplyAllSettings()
        {
            ApplyDisplaySettings();
            ApplyLanguageSetting();
            ApplyAudioRtpcs();
        }

        private void ApplyDisplaySettings()
        {
            if (fullscreenMode == FullScreenMode.FullScreenWindow)
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
            else
                Screen.SetResolution(windowWidth, windowHeight, fullscreenMode);
        }

        private void ApplyLanguageSetting()
        {
            if (string.IsNullOrEmpty(selectedLanguageCode)) return;

            var locale = LocalizationSettings.AvailableLocales.GetLocale(selectedLanguageCode);
            if (locale != null)
                LocalizationSettings.SelectedLocale = locale;
        }

        private void ApplyAudioRtpcs()
        {
            float masterRtpc = isMuted ? rtpcMin : Slider01ToRtpc(masterSlider01);
            float musicRtpc = Slider01ToRtpc(musicSlider01);
            float sfxRtpc = Slider01ToRtpc(sfxSlider01);
            float atmoRtpc = Slider01ToRtpc(atmoSlider01);
            float dialogueRtpc = Slider01ToRtpc(dialogueSlider01);

            // global RTPCs
            AkUnitySoundEngine.SetRTPCValue(masterVolumeRtpc, masterRtpc);
            AkUnitySoundEngine.SetRTPCValue(musicVolumeRtpc, musicRtpc);
            AkUnitySoundEngine.SetRTPCValue(sfxVolumeRtpc, sfxRtpc);
            AkUnitySoundEngine.SetRTPCValue(atmoVolumeRtpc, atmoRtpc);
            AkUnitySoundEngine.SetRTPCValue(dialogueVolumeRtpc, dialogueRtpc);
        }

        private float Slider01ToRtpc(float slider01)
        {
            float t = Mathf.Clamp01(slider01);
            return Mathf.Lerp(rtpcMin, rtpcMax, t);
        }

        private void LoadFromPrefs()
        {
            bool resetDisplay = PlayerPrefs.GetInt(PREF_SETTINGS_VERSION, 0) < SETTINGS_VERSION;

            masterSlider01 = PlayerPrefs.GetFloat(PREF_MASTER, masterSlider01);
            musicSlider01 = PlayerPrefs.GetFloat(PREF_MUSIC, musicSlider01);
            sfxSlider01 = PlayerPrefs.GetFloat(PREF_SFX, sfxSlider01);
            atmoSlider01 = PlayerPrefs.GetFloat(PREF_ATMO, atmoSlider01);
            dialogueSlider01 = PlayerPrefs.GetFloat(PREF_DIALOGUE, dialogueSlider01);
            isMuted = PlayerPrefs.GetInt(PREF_MUTED, isMuted ? 1 : 0) == 1;
            selectedLanguageCode = PlayerPrefs.GetString(PREF_LANGUAGE, selectedLanguageCode);

            if (resetDisplay)
            {
                fullscreenMode = FullScreenMode.FullScreenWindow;
                windowWidth = Screen.currentResolution.width;
                windowHeight = Screen.currentResolution.height;
                PlayerPrefs.SetInt(PREF_FULLSCREEN_MODE, (int)fullscreenMode);
                PlayerPrefs.SetInt(PREF_WINDOW_WIDTH, windowWidth);
                PlayerPrefs.SetInt(PREF_WINDOW_HEIGHT, windowHeight);
                PlayerPrefs.SetInt(PREF_SETTINGS_VERSION, SETTINGS_VERSION);
                PlayerPrefs.Save();
            }
            else
            {
                fullscreenMode = (FullScreenMode)PlayerPrefs.GetInt(PREF_FULLSCREEN_MODE, (int)FullScreenMode.FullScreenWindow);
                windowWidth = PlayerPrefs.GetInt(PREF_WINDOW_WIDTH, Screen.currentResolution.width);
                windowHeight = PlayerPrefs.GetInt(PREF_WINDOW_HEIGHT, Screen.currentResolution.height);
            }

            masterSlider01 = Mathf.Clamp01(masterSlider01);
            musicSlider01 = Mathf.Clamp01(musicSlider01);
            sfxSlider01 = Mathf.Clamp01(sfxSlider01);
            atmoSlider01 = Mathf.Clamp01(atmoSlider01);
            dialogueSlider01 = Mathf.Clamp01(dialogueSlider01);
            windowWidth = Mathf.Max(640, windowWidth);
            windowHeight = Mathf.Max(480, windowHeight);
        }

    }
}
