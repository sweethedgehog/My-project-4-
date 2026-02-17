using System;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void onPlayButtonClick() => SceneManager.LoadScene("MainScene");
    public void onTutorialButtonClick() => SceneManager.LoadScene("TutorialScene");
    public void onRulesButtonClick() => SceneManager.LoadScene("Rules");
    public void onCreatorsButtonClick() => SceneManager.LoadScene("Creators");
}
