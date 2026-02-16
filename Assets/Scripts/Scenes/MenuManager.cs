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

        public void onPlayButtonClick() => SceneManager.LoadScene(SceneNames.MainScene);
        public void onTutorialButtonClick() => SceneManager.LoadScene(SceneNames.TutorialScene);
        public void onRulesButtonClick() => SceneManager.LoadScene(SceneNames.Rules);
        public void onCreatorsButtonClick() => SceneManager.LoadScene(SceneNames.Creators);
    }
}
