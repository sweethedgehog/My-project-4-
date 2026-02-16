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
    private bool rightChoice = false;
    private ImageType lastChoice;

    void Start()
    {
        makePostdictionButton.onClick.AddListener(makePostdiction);
        backToGameButton.onClick.AddListener(backToGame);
        makePostdictionButton.interactable = false;
        UpdateButtonVisual();
    }

    private void backToGame()
    {
        SceneManager.UnloadSceneAsync("PostdictionScene");
    }
    private void makePostdiction()
    {
        switch (lastChoice)
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
        // SceneManager.LoadScene(rightChoice ? "Win" : "Lose", LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync("PostdictionScene");
    }
    public void select(ImageType type)
    {
        makePostdictionButton.interactable = true;
        lastChoice = type;
        rightChoice = type == ImageType.Badger;
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
        UpdateButtonVisual();
    }
    private void UpdateButtonVisual()
    {
        if (makePostdictionButton == null) return;

        Image btnImage = makePostdictionButton.GetComponent<Image>();
        if (btnImage == null) return;

        if (makePostdictionButton.interactable)
        {
            btnImage.color = Color.white;           // или твой нормальный цвет
        }
        else
        {
            btnImage.color = new Color(0.65f, 0.65f, 0.65f, 1f);  // затемнение, alpha = 1
        }

    }
}
