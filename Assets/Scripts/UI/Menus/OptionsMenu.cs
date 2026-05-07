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

        public ScrollRollAnimator scrollAnimator;

        private void Start()
        {
            _canvasGroup = optionsPanel.GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = optionsPanel.AddComponent<CanvasGroup>();

            //_canvasGroup.alpha = 1f;
            // Don't disable the panel here - let ScrollRollAnimator handle visibility
            // optionsPanel.SetActive(false);

            _dataManager = OptionDataManager.Instance;

            SetupResolutionDropdown();
            SetupLanguageDropdown();
            LoadSettingsIntoUI();

            // Debug check and auto-find if not assigned
            if (scrollAnimator == null)
            {
                scrollAnimator = GetComponentInChildren<ScrollRollAnimator>();
                if (scrollAnimator == null)
                    Debug.LogError("OptionsMenu: scrollAnimator not assigned and could not be found in children!");
                else
                    Debug.Log("OptionsMenu: scrollAnimator auto-found in children");
            }
            else
            {
                Debug.Log("OptionsMenu: scrollAnimator successfully initialized");
            }
        }

        public void OpenOptionsRoll()
        {
            if (scrollAnimator == null)
            {
                Debug.LogError("OptionsMenu.OpenOptionsRoll: scrollAnimator is null!");
                return;
            }

            if (EventSystem.current != null)
                previousSelected = EventSystem.current.currentSelectedGameObject;

            scrollAnimator.Open(optionSelectedButton);
        }

        public void CloseOptionsRoll()
        {
            if (scrollAnimator == null)
            {
                Debug.LogError("OptionsMenu.CloseOptionsRoll: scrollAnimator is null!");
                return;
            }
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

            // Use a HashSet to track unique resolutions and avoid duplicates
            var uniqueResolutions = new System.Collections.Generic.List<Resolution>();
            var resolutionStrings = new System.Collections.Generic.HashSet<string>();

            // Add resolutions in reverse order (highest resolution last, so we add it first)
            for (var i = _resolutions.Length - 1; i >= 0; i--)
            {
                var res = _resolutions[i];
                var resolutionString = res.width + "x" + res.height;

                // Only add if this resolution hasn't been added yet
                if (resolutionStrings.Add(resolutionString))
                {
                    uniqueResolutions.Add(res);
                }
            }

            // Reverse the list to have them in ascending order for display
            uniqueResolutions.Reverse();

            for (var i = 0; i < uniqueResolutions.Count; i++)
            {
                var res = uniqueResolutions[i];
                resolutionDropdown.options.Add(
                    new TMP_Dropdown.OptionData(res.width + "x" + res.height));
            }

            // Update the internal resolutions array to match the filtered list
            _resolutions = uniqueResolutions.ToArray();

            resolutionDropdown.RefreshShownValue();
            SetDropdownToCurrentResolution();
        }

        private void SetDropdownToCurrentResolution()
        {
            if (_dataManager == null || resolutionDropdown == null || _resolutions == null) return;
            int w = _dataManager.WindowWidth;
            int h = _dataManager.WindowHeight;
            for (int i = 0; i < _resolutions.Length; i++)
            {
                if (_resolutions[i].width == w && _resolutions[i].height == h)
                {
                    resolutionDropdown.SetValueWithoutNotify(i);
                    return;
                }
            }
        }

        public void SetFullscreen(bool isFullscreen)
        {
            if (_dataManager != null) _dataManager.SetFullscreen(isFullscreen);
        }

        public void SetResolution(int dropdownIndex)
        {
            if (_dataManager == null) return;
            if (dropdownIndex < 0 || dropdownIndex >= _resolutions.Length) return;
            var r = _resolutions[dropdownIndex];
            _dataManager.SetResolution(r.width, r.height);
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
                fullscreenToggle.SetIsOnWithoutNotify(_dataManager.FullscreenMode != FullScreenMode.Windowed);

            if (resolutionDropdown != null)
                SetDropdownToCurrentResolution();
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