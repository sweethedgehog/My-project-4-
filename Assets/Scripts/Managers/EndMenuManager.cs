using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CardGame.Managers;

public class EndMenuManager : MonoBehaviour
{
    public Button continueButton;
    public AudioClip audioClip;

    private void Start()
    {
        continueButton.onClick.AddListener(returnToMainMenu);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(audioClip);
    }
    private void returnToMainMenu() => SceneManager.LoadScene("MainMenu");
}
