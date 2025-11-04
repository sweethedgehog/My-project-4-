using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Vector2 hotspot = Vector2.zero;
    private void Start()
    {
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
    }

    public void onPlayButtonClick() => SceneManager.LoadScene("MainScene");
    public void onRulesButtonClick() => SceneManager.LoadScene("Rules");
    public void onCreatorsButtonClick() => SceneManager.LoadScene("Creators");
}
