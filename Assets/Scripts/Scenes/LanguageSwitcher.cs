using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

namespace CardGame.Managers
{
    public class LanguageSwitcher : MonoBehaviour
    {
        [SerializeField] private Button ruButton;
        [SerializeField] private Button enButton;

        private const string LocalePrefKey = "unity.localization";

        private IEnumerator Start()
        {
            yield return LocalizationSettings.InitializationOperation;
            string saved = PlayerPrefs.GetString(LocalePrefKey, "ru");
            ApplyLocale(saved);
        }

        public void SetRussian() => ApplyLocale("ru");
        public void SetEnglish() => ApplyLocale("en");

        private void ApplyLocale(string code)
        {
            var locale = LocalizationSettings.AvailableLocales.GetLocale(code);
            if (locale == null) return;

            LocalizationSettings.SelectedLocale = locale;
            PlayerPrefs.SetString(LocalePrefKey, code);

            UpdateButtonVisuals(code);
        }

        private void UpdateButtonVisuals(string activeCode)
        {
            if (ruButton != null) ruButton.interactable = activeCode != "ru";
            if (enButton != null) enButton.interactable = activeCode != "en";
        }
    }
}
