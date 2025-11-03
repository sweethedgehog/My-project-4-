using UnityEngine;
using UnityEngine.SceneManagement;

public class RulesManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
