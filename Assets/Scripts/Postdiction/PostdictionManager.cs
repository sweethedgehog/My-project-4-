using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum ImageType
{
    Badger = 0,
    Cat = 1,
    Rabbit = 2,
    Squirrel = 3
}
public class PostdictionManager : MonoBehaviour
{
    public FinalImage badger;
    public FinalImage cat;
    public FinalImage rabbit;
    public FinalImage squirrel;
    public Button makePostdictionButton;
    public Button backToGameButton;
    private bool rightChoise = false;
    private ImageType lastChoise;

    void Start()
    {
        makePostdictionButton.onClick.AddListener(makePostdiction);
        backToGameButton.onClick.AddListener(backToGame);
        makePostdictionButton.interactable = false;
    }

    private void backToGame()
    {
        SceneManager.UnloadSceneAsync("PostdictionScene");
    }
    private void makePostdiction()
    {
        switch (lastChoise)
        {
            case ImageType.Badger:
                SceneManager.LoadScene("BadgerEndingScene", LoadSceneMode.Additive);
                break;
            case ImageType.Cat:
                SceneManager.LoadScene("CatEndingScene", LoadSceneMode.Additive);
                break;
            case ImageType.Rabbit:
                SceneManager.LoadScene("RabbitEndingScene", LoadSceneMode.Additive);
                break;
            case ImageType.Squirrel:
                SceneManager.LoadScene("SquirrelEndingScene", LoadSceneMode.Additive);
                break;
        }
        // SceneManager.LoadScene(rightChoise ? "Win" : "Lose", LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync("PostdictionScene");
    }
    public void select(ImageType type)
    {
        makePostdictionButton.interactable = true;
        lastChoise = type;
        rightChoise = type == ImageType.Badger;
        switch (type)
        {
            case ImageType.Badger:
                cat.clearSelection();
                squirrel.clearSelection();
                rabbit.clearSelection();
                break;
            case ImageType.Cat:
                badger.clearSelection();
                squirrel.clearSelection();
                rabbit.clearSelection();
                break;
            case ImageType.Rabbit:
                cat.clearSelection();
                squirrel.clearSelection();
                badger.clearSelection();
                break;
            default:
                cat.clearSelection();
                badger.clearSelection();
                rabbit.clearSelection();
                break;
        }
    }
}
