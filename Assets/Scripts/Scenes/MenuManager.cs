using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using CardGame.Core;

namespace CardGame.Managers
{
    public class MenuManager : MonoBehaviour
    {
        public Texture2D cursorTexture;
        public Vector2 hotspot = Vector2.zero;
        private void Start()
        {
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
        }

        public void OnPlayButtonClick() => SceneManager.LoadScene(SceneNames.MainScene);
        public void OnTutorialButtonClick() => SceneManager.LoadScene(SceneNames.TutorialScene);
        public void OnRulesButtonClick() => SceneManager.LoadScene(SceneNames.Rules);
        public void OnCreatorsButtonClick() => SceneManager.LoadScene(SceneNames.Creators);
    }
}
