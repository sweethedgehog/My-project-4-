using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void onPlayButtonClick() => SceneManager.LoadScene("MainScene");
    public void onRulesButtonClick() => SceneManager.LoadScene("Rules");
    public void onCreatorsButtonClick() => SceneManager.LoadScene("Creators");
}
