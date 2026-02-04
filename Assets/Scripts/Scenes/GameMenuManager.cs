using CardGame.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenuManager : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    public void onMainMenuClick()
    {
        RoundManager.inGameMenu = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void onContinueClick() => returnToGame();

    void Start()
    {
        RoundManager.inGameMenu = true;
        Time.timeScale = 0f;

        InitializeVolumeSliders();
    }

    private void InitializeVolumeSliders()
    {
        if (AudioManager.Instance == null) return;

        // Set slider values to current volume
        if (musicSlider != null)
        {
            musicSlider.value = AudioManager.Instance.GetMusicVolume();
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = AudioManager.Instance.GetSFXVolume();
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) returnToGame();
    }

    private void returnToGame()
    {
        RoundManager.inGameMenu = false;
        SceneManager.UnloadSceneAsync("GameMenu");
        Time.timeScale = 1f;
    }
}
