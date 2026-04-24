using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace dungeonduell
{
    public class LanguageSelector : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown languageDropdown;
        private bool isLoading = false;

        void Start()
        {
            StartCoroutine(SetupDropdown());
        }

        private IEnumerator SetupDropdown()
        {
            yield return LocalizationSettings.InitializationOperation;

            // Restore previously saved language
            string savedCode = PlayerPrefs.GetString("opt_language_code", "");
            if (!string.IsNullOrEmpty(savedCode))
            {
                var savedLocale = LocalizationSettings.AvailableLocales.GetLocale(savedCode);
                if (savedLocale != null)
                    LocalizationSettings.SelectedLocale = savedLocale;
            }

            languageDropdown.ClearOptions();
            List<string> options = new List<string>();
            int currentIndex = 0;

            for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
            {
                var locale = LocalizationSettings.AvailableLocales.Locales[i];
                options.Add(locale.Identifier.CultureInfo.NativeName);

                if (LocalizationSettings.SelectedLocale == locale)
                    currentIndex = i;
            }

            languageDropdown.AddOptions(options);
            languageDropdown.value = currentIndex;
            languageDropdown.RefreshShownValue();

            languageDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        private void OnDropdownValueChanged(int index)
        {
            if (isLoading) return;
            StartCoroutine(SetLocale(index));
        }

        private IEnumerator SetLocale(int index)
        {
            isLoading = true;
            var locale = LocalizationSettings.AvailableLocales.Locales[index];
            if (facingfate.OptionDataManager.Instance != null)
                facingfate.OptionDataManager.Instance.SetLanguage(locale.Identifier.Code);
            else
                LocalizationSettings.SelectedLocale = locale;
            yield return null;
            isLoading = false;
        }
    }
}
