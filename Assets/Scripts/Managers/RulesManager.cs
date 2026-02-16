using UnityEngine;
using UnityEngine.SceneManagement;
using CardGame.Core;

namespace CardGame.Managers
{
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
            SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }
}
