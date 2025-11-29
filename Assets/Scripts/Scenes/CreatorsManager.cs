using UnityEngine;
using UnityEngine.SceneManagement;

public class CreatorsManager : MonoBehaviour
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
