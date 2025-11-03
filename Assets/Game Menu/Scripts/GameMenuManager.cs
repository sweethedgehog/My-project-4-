using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuManager : MonoBehaviour
{
    public void onMainMenuClick()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void onContinueClick() => returnToGame();
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) returnToGame();
    }

    private void returnToGame()
    {
        GamePlayManager.inMenu = false;
        SceneManager.UnloadSceneAsync("GameMenu");
        Time.timeScale = 1f;
    }
}
