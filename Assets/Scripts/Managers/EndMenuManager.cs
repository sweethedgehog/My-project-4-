using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndMenuManager : MonoBehaviour
{
    public Button continueButton;

    private void Start() => continueButton.onClick.AddListener(returnToMainMenu);
    private void returnToMainMenu() => SceneManager.LoadScene("MainMenu");
}
