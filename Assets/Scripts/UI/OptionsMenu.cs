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

        [Header("Scroll Animation (Main Menu)")]
        [SerializeField] private RectTransform scrollReveal;   
        [SerializeField] private RectTransform bottomRoll;    
        [SerializeField] private float unfoldDuration = 0.45f;
        [SerializeField] private Ease unfoldEase = Ease.OutCubic;
        [SerializeField] private float closedHeight = 0f;

        private float _openHeight;
        private bool _heightCached;
        private Sequence _scrollSequence;

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

        public void OpenOptionsScroll()
        {
            if (optionsPanel == null) return;

            optionsPanel.SetActive(true);

            if (scrollReveal == null)
            {
                scrollReveal = optionsPanel.GetComponent<RectTransform>();
            }

            CacheOpenHeightIfNeeded();

            KillScrollSequence();

            // Startzustand: geschlossen
            optionsPanel.transform.localScale = Vector3.one; // wichtig: wir skalieren nicht mehr
            SetRevealHeight(closedHeight);

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            float h = closedHeight;

            _scrollSequence = DOTween.Sequence().SetUpdate(true);

            _scrollSequence.Join(_canvasGroup.DOFade(1f, fadeDuration));

            _scrollSequence.Join(
                DOTween.To(() => h, x =>
                {
                    h = x;
                    SetRevealHeight(h);
                }, _openHeight, unfoldDuration)
                .SetEase(unfoldEase)
            );

            _scrollSequence.OnComplete(() =>
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;

                if (optionSelectedButton != null && EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(optionSelectedButton);
                }
            });
        }

        public void CloseOptionsScroll(bool forceClose)
        {
            if (optionsPanel == null || scrollReveal == null) return;

            KillScrollSequence();

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            float h = scrollReveal.rect.height;

            _scrollSequence = DOTween.Sequence().SetUpdate(true);

            _scrollSequence.Join(_canvasGroup.DOFade(0f, fadeDuration));

            _scrollSequence.Join(
                DOTween.To(() => h, x =>
                {
                    h = x;
                    SetRevealHeight(h);
                }, closedHeight, unfoldDuration * 0.9f)
                .SetEase(Ease.InCubic)
            );

            _scrollSequence.OnComplete(() =>
            {
                optionsPanel.SetActive(false);

                if (EventSystem.current != null & !forceClose)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    if (previousSelected != null)
                        EventSystem.current.SetSelectedGameObject(previousSelected);
                }
            });
        }

        private void CacheOpenHeightIfNeeded()
        {
            if (_heightCached) return;

            // Layout aktualisieren, damit rect.height korrekt ist
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollReveal);

            _openHeight = scrollReveal.rect.height;

            // Falls du beim ersten Öffnen bereits "geschlossen" warst,
            // kannst du als Fallback auch eine feste Höhe im Inspector speichern.
            if (_openHeight <= 0.01f)
            {
                _openHeight = 600f; // sinnvoller Default, im Zweifel im Inspector/Scene anpassen
            }

            _heightCached = true;
        }

        private void SetRevealHeight(float height)
        {
            // Höhe des Reveal-Rects setzen (top-down)
            scrollReveal.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(0f, height));

            // Optional: BottomRoll folgt der unteren Kante des Reveal-Bereichs
            if (bottomRoll != null)
            {
                // Annahme: bottomRoll ist oben geankert und bewegt sich nach unten (negative Y)
                var pos = bottomRoll.anchoredPosition;
                pos.y = -Mathf.Max(0f, height);
                bottomRoll.anchoredPosition = pos;
            }
        }

        private void KillScrollSequence()
        {
            if (_scrollSequence != null && _scrollSequence.IsActive())
            {
                _scrollSequence.Kill();
                _scrollSequence = null;
            }
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