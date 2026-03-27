using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CardGame.Core;

namespace CardGame.Managers
{
    public class EndMenuManager : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private bool isWinEnding;

        private void Start()
        {
            continueButton.onClick.AddListener(ReturnToMainMenu);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopMusic();

                if (isWinEnding)
                    AudioManager.Instance.PlayWinSound();
                else
                    AudioManager.Instance.PlayLoseSound();
            }
        }

        private void ReturnToMainMenu() => SceneManager.LoadScene(SceneNames.MainMenu);
    }
}
