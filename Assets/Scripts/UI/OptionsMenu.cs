using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
//using FMODUnity;
//using FMOD.Studio;

namespace facingfate
{
    public class OptionsMenu : MonoBehaviour
    {
        public GameObject optionsPanel;
        public GameObject optionSelectedButton;
        public GameObject previousSelected;
        public AudioMixer audioMixer;
        public Slider audioSlider;
        public Toggle muteToggle;
        public Toggle fullscreenToggle;
        public TMP_Dropdown resolutionDropdown;
        public TMP_Dropdown languageDropdown;

        public float fadeDuration = 0.25f;

        private CanvasGroup _canvasGroup;
        private OptionDataManager _dataManager;
        private Resolution[] _resolutions;

        /*
        private VCA _masterVCA;
        private VCA _musicVCA;
        private VCA _sfxVCA;
        */

        private void Start()
        {
            _canvasGroup = optionsPanel.GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = optionsPanel.AddComponent<CanvasGroup>();

            _canvasGroup.alpha = 0;
            optionsPanel.SetActive(false);

            /*
            _masterVCA = RuntimeManager.GetVCA("VCA:/Master");            
            _musicVCA = RuntimeManager.GetVCA("VCA:/Music");
            _sfxVCA = RuntimeManager.GetVCA("VCA:/SFX");
            */

            _dataManager = FindFirstObjectByType<OptionDataManager>();
            SetupResolutionDropdown();
            SetupLanguageDropdown();
            LoadSettings();
        }

        public void OpenOptions()
        {
            optionsPanel.SetActive(true);
            optionsPanel.transform.localScale = Vector3.zero;
            _canvasGroup.alpha = 0;
            optionsPanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            _canvasGroup.DOFade(1, fadeDuration).SetUpdate(true).OnComplete(() =>
            {
                if (optionSelectedButton != null && EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(optionSelectedButton);
            });
        }

        public void CloseOptions()
        {
            optionsPanel.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack).SetUpdate(true);
            _canvasGroup.DOFade(0, fadeDuration).SetUpdate(true).OnComplete(() =>
            {
                optionsPanel.SetActive(false);
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    if (previousSelected != null) EventSystem.current.SetSelectedGameObject(previousSelected);
                }
            });
        }

        public void CloseOptions(bool forceClose)
        {
            optionsPanel.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack).SetUpdate(true);
            _canvasGroup.DOFade(0, fadeDuration).SetUpdate(true).OnComplete(() =>
            {
                optionsPanel.SetActive(false);
                if (EventSystem.current != null & !forceClose)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    if (previousSelected != null) EventSystem.current.SetSelectedGameObject(previousSelected);
                }
            });
        }


        public void SetMasterVolume(float sliderValue)
        {
            float dB = Mathf.Lerp(-80f, 0f, sliderValue); // logarithmisch skalieren
            float volume = Mathf.Pow(10f, dB / 10f); // oder 2.5f je nach Feingef�hl

            /*
            _masterVCA.setVolume(volume);
            if (_dataManager != null)
                _dataManager.SetVolume(sliderValue);
            */
        }

        public void SetMusicVolume(float sliderValue)
        {
            float dB = Mathf.Lerp(-80f, 0f, sliderValue); // logarithmisch skalieren
            float volume = Mathf.Pow(10f, dB / 10f); // oder 2.5f je nach Feingef�hl

            /*
            _musicVCA.setVolume(volume);
            if (_dataManager != null)
                _dataManager.SetMusicVolume(sliderValue);
            */
        }

        public void SetSfxVolume(float sliderValue)
        {
            float dB = Mathf.Lerp(-80f, 0f, sliderValue); // logarithmisch skalieren
            float volume = Mathf.Pow(10f, dB / 10f); // oder 2.5f je nach Feingef�hl

            /*
            _sfxVCA.setVolume(volume);
            if (_dataManager != null)
                _dataManager.SetSfxVolume(sliderValue);
            */
        }

        public void MuteToggle(bool muted)
        {
            float sliderValue = muted ? 0.0001f : audioSlider.value;
            float dB = Mathf.Lerp(-80f, 0f, sliderValue);
            float volume = Mathf.Pow(10f, dB / 20f);

            /*
            _masterVCA.setVolume(volume);
            if (_dataManager != null) _dataManager.MuteToggle(muted);
        */
        }


        private void SetupResolutionDropdown()
        {
            _resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();

            var currentResolutionIndex = 0;
            for (var i = 0; i < _resolutions.Length; i++)
            {
                resolutionDropdown.options.Add(
                    new TMP_Dropdown.OptionData(_resolutions[i].width + "x" + _resolutions[i].height));
                if (_resolutions[i].width == Screen.currentResolution.width &&
                    _resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                    //Debug.Log(currentResolutionIndex);
                }
            }

            //if (_dataManager != null) resolutionDropdown.value = _dataManager.resolutionIndex;

            resolutionDropdown.RefreshShownValue();
        }

        public void SetFullscreen(bool isFullscreen)
        {
            //if (_dataManager != null) _dataManager.SetFullscreen(isFullscreen);
        }

        public void SetResolution(int resolutionIndex)
        {
            //if (_dataManager != null) _dataManager.SetResolution(resolutionIndex);
        }

        private void LoadSettings()
        {
            if (_dataManager != null)
            {
                /*
                audioSlider.value = _dataManager.volume;
                fullscreenToggle.isOn = _dataManager.isFullscreen;
                resolutionDropdown.value = _dataManager.resolutionIndex;
                muteToggle.isOn = _dataManager.isMuted;
                */
            }
        }

        private void SetupLanguageDropdown()
        {
            if (languageDropdown == null) return;

            languageDropdown.ClearOptions();

            var options = new List<string>();
            var locales = LocalizationSettings.AvailableLocales.Locales;
            var currentLocale = LocalizationSettings.SelectedLocale;
            var selectedIndex = 0;

            for (int i = 0; i < locales.Count; i++)
            {
                var locale = locales[i];
                options.Add(locale.Identifier.CultureInfo.NativeName);

                /*
                if (_dataManager != null && _dataManager.selectedLanguageCode == locale.Identifier.Code)
                    selectedIndex = i;
                */            }

            languageDropdown.AddOptions(options);
            languageDropdown.value = selectedIndex;
            languageDropdown.RefreshShownValue();
            languageDropdown.onValueChanged.AddListener(SetLanguageFromDropdown);
        }

        private void SetLanguageFromDropdown(int index)
        {
            var locale = LocalizationSettings.AvailableLocales.Locales[index];
            if (_dataManager != null)
            {
                //_dataManager.SetLanguage(locale.Identifier.Code);
            }
        }
    }
}