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
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
            PlayerPrefs.SetString("language", LocalizationSettings.SelectedLocale.Identifier.Code);
            yield return null;
            isLoading = false;
        }
    }
}
