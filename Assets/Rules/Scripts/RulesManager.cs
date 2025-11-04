using UnityEngine;
using UnityEngine.SceneManagement;

public class RulesManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            exit();
        }
    }

    public void exit()
    { 
        SceneManager.LoadScene("MainMenu");
    }
}
