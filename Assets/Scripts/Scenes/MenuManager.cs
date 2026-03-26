using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using CardGame.Core;

namespace CardGame.Managers
{
    public class MenuManager : MonoBehaviour
    {
        [Header("Cursor")]
        [SerializeField] private Texture2D cursorTexture;
        [SerializeField] private Vector2 hotspot = Vector2.zero;

        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject authorsPanel;

        [Header("Settings — Volume Sliders")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Settings — Language Buttons")]
        [SerializeField] private Button ruButton;
        [SerializeField] private Button enButton;

        private const string LocalePrefKey = "unity.localization";

        private void Start()
        {
            ApplyCustomCursor();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMenuMusic();

            // Ensure panels are closed on start
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (authorsPanel != null) authorsPanel.SetActive(false);

            InitializeSettings();
        }

        // ===== Navigation Buttons =====

        public void OnPlayButtonClick() => SceneManager.LoadScene(SceneNames.MainScene);
        public void OnTutorialButtonClick() => SceneManager.LoadScene(SceneNames.TutorialScene);

        public void OnSettingsButtonClick()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                RefreshSliders();
                RefreshLanguageButtons();
            }
        }

        public void OnAuthorsButtonClick()
        {
            if (authorsPanel != null)
                authorsPanel.SetActive(true);
        }

        public void OnExitButtonClick()
        {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        }

        // ===== Panel Back Buttons =====

        public void OnSettingsBackClick()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        public void OnAuthorsBackClick()
        {
            if (authorsPanel != null)
                authorsPanel.SetActive(false);
        }

        // ===== Settings: Volume =====

        private void InitializeSettings()
        {
            if (musicSlider != null)
                musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

            if (sfxSlider != null)
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            if (ruButton != null)
                ruButton.onClick.AddListener(SetRussian);

            if (enButton != null)
                enButton.onClick.AddListener(SetEnglish);
        }

        private void RefreshSliders()
        {
            if (AudioManager.Instance == null) return;

            if (musicSlider != null)
                musicSlider.value = AudioManager.Instance.GetMusicVolume();

            if (sfxSlider != null)
                sfxSlider.value = AudioManager.Instance.GetSFXVolume();
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetMusicVolume(value);
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetSFXVolume(value);
        }

        // ===== Settings: Language =====

        private void SetRussian() => ApplyLocale("ru");
        private void SetEnglish() => ApplyLocale("en");

        private void ApplyLocale(string code)
        {
            var locale = LocalizationSettings.AvailableLocales.GetLocale(code);
            if (locale == null) return;

            LocalizationSettings.SelectedLocale = locale;
            PlayerPrefs.SetString(LocalePrefKey, code);
            RefreshLanguageButtons();
        }

        private void RefreshLanguageButtons()
        {
            string current = PlayerPrefs.GetString(LocalePrefKey, "ru");
            if (ruButton != null) ruButton.interactable = current != "ru";
            if (enButton != null) enButton.interactable = current != "en";
        }

        // ===== Cursor =====

        private void ApplyCustomCursor()
        {
            if (cursorTexture == null)
            {
                Cursor.visible = true;
                return;
            }

    #if UNITY_WEBGL
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.ForceSoftware);
    #else
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
    #endif

            Cursor.visible = true;
        }

        public void RefreshCursor()
        {
            ApplyCustomCursor();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (settingsPanel != null && settingsPanel.activeSelf)
                    OnSettingsBackClick();
                else if (authorsPanel != null && authorsPanel.activeSelf)
                    OnAuthorsBackClick();
            }
        }
    }
}
