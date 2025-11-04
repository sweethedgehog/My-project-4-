using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndMenuManager : MonoBehaviour
{
    public Button continueButton;
    public AudioClip audioClip;
    private AudioSource audioSource;

    private void Start()
    {
        continueButton.onClick.AddListener(returnToMainMenu);
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(audioClip);
    }
    private void returnToMainMenu() => SceneManager.LoadScene("MainMenu");
}
