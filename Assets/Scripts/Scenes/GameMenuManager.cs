using CardGame.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuManager : MonoBehaviour
{
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
