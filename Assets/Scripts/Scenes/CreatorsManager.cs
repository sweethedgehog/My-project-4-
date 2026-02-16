using UnityEngine;
using UnityEngine.SceneManagement;
using CardGame.Core;

namespace CardGame.Managers
{
    public class CreatorsManager : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Exit();
            }
        }

        public void Exit()
        {
            SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }
}
