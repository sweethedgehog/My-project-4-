using UnityEngine;
using UnityEngine.SceneManagement;

public class GamePlayManager : MonoBehaviour
{
    public static bool inMenu = false;

    void Start()
    {
        Time.timeScale = 1f;
        inMenu = false;
    }
    void Update()
     {
         if (Input.GetKeyDown(KeyCode.Escape) && !inMenu) 
         {
             SceneManager.LoadScene("GameMenu", LoadSceneMode.Additive);
             Time.timeScale = 0f;
             inMenu = true;
         }
     }
}
