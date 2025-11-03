using UnityEngine;
using UnityEngine.SceneManagement;

public class CreatorsManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
