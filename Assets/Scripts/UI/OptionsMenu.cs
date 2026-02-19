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

        [Header("Audio UI")]
        public Slider masterSlider;
        public Slider musicSlider;
        public Slider sfxSlider;
        public Slider atmoSlider;
        public Slider dialogSlider;
        public Toggle muteToggle;

        public Toggle fullscreenToggle;
        public TMP_Dropdown resolutionDropdown;
        public TMP_Dropdown languageDropdown;

        public float fadeDuration = 0.25f;

        private CanvasGroup _canvasGroup;
        private OptionDataManager _dataManager;
        private Resolution[] _resolutions;

        [SerializeField] private ScrollRollAnimator scrollAnimator;

        private void Start()
        {
            _canvasGroup = optionsPanel.GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = optionsPanel.AddComponent<CanvasGroup>();

            //_canvasGroup.alpha = 1f;
            optionsPanel.SetActive(false);

            _dataManager = FindFirstObjectByType<OptionDataManager>();

            SetupResolutionDropdown();
            SetupLanguageDropdown();
            LoadSettingsIntoUI();
        }

        public void OpenOptionsRoll()
        {
            if (EventSystem.current != null)
                previousSelected = EventSystem.current.currentSelectedGameObject;

            scrollAnimator.Open(optionSelectedButton);
        }

        public void CloseOptionsRoll()
        {
            scrollAnimator.Close(previousSelected);
        }

        public void SetMasterVolume(float sliderValue)
        {

            if (_dataManager != null)
                _dataManager.SetMasterVolume01(sliderValue);
        }

        public void SetMusicVolume(float sliderValue)
        {
            if (_dataManager != null)
                _dataManager.SetMusicVolume01(sliderValue);
        }

        public void SetSfxVolume(float sliderValue)
        {
            if (_dataManager != null)
                _dataManager.SetSfxVolume01(sliderValue);
        }
        public void SetAtmoVolume(float sliderValue)
        {
            if (_dataManager != null)
                _dataManager.SetAtmoVolume01(sliderValue);
        }
        public void SetDialogueVolume(float sliderValue)
        {
            if (_dataManager != null)
                _dataManager.SetDialogueVolume01(sliderValue);
        }

        public void MuteToggle(bool muted)
        {
            if (_dataManager != null) 
                _dataManager.MuteToggle(muted);
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
            if (_dataManager != null) _dataManager.SetFullscreen(isFullscreen);
        }

        public void SetResolution(int resolutionIndex)
        {
            if(_dataManager != null) _dataManager.SetResolution(resolutionIndex);
        }

        private void LoadSettingsIntoUI()
        {
            if (_dataManager == null) return;

            if (masterSlider != null) masterSlider.SetValueWithoutNotify(_dataManager.Master01);
            if (musicSlider != null) musicSlider.SetValueWithoutNotify(_dataManager.Music01);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(_dataManager.Sfx01);
            if (dialogSlider != null) dialogSlider.SetValueWithoutNotify(_dataManager.Dialogue01);
            if (atmoSlider != null) atmoSlider.SetValueWithoutNotify(_dataManager.Atmo01);
            if (muteToggle != null) muteToggle.SetIsOnWithoutNotify(_dataManager.IsMuted);

            if (fullscreenToggle != null)
                fullscreenToggle.SetIsOnWithoutNotify(_dataManager.IsFullscreen);

            if (resolutionDropdown != null)
                resolutionDropdown.SetValueWithoutNotify(_dataManager.ResolutionIndex);
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
                _dataManager.SetLanguage(locale.Identifier.Code);
            }
        }

        private void OnEnable()
        {
            _dataManager = OptionDataManager.Instance;
            LoadSettingsIntoUI();
        }
    }
}