using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using CardGame.Core;

namespace CardGame.Managers
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private Texture2D cursorTexture;    
        [SerializeField] private Vector2 hotspot = Vector2.zero;
    
        private void Start()
        {
            ApplyCustomCursor();
        }
    
        private void ApplyCustomCursor()
        {
            if (cursorTexture == null)
            {
                Cursor.visible = true;
                return;
            }
    
    #if UNITY_WEBGL
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.ForceSoftware);
    #else
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
    #endif
    
            Cursor.visible = true;  
        }
    
        public void RefreshCursor()
        {
            ApplyCustomCursor();
        }

        public void OnPlayButtonClick() => SceneManager.LoadScene(SceneNames.MainScene);
        public void OnTutorialButtonClick() => SceneManager.LoadScene(SceneNames.TutorialScene);
        public void OnRulesButtonClick() => SceneManager.LoadScene(SceneNames.Rules);
        public void OnCreatorsButtonClick() => SceneManager.LoadScene(SceneNames.Creators);
    }
}
