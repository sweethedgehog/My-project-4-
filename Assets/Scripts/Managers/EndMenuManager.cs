using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CardGame.Core;

namespace CardGame.Managers
{
    public class EndMenuManager : MonoBehaviour
    {
        public Button continueButton;
        public AudioClip audioClip;

        private void Start()
        {
            continueButton.onClick.AddListener(ReturnToMainMenu);
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(audioClip);
        }
        private void ReturnToMainMenu() => SceneManager.LoadScene(SceneNames.MainMenu);
    }
}
