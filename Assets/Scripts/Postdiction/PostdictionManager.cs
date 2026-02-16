using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CardGame.Core;
using CardGame.UI;

namespace CardGame.Managers
{
    public enum ImageType
    {
        Badger = 0,
        Cat = 1,
        Rabbit = 2,
        Squirrel = 3
    }

    public class PostdictionManager : MonoBehaviour
    {
        [SerializeField] private FinalImage badger;
        [SerializeField] private FinalImage cat;
        [SerializeField] private FinalImage rabbit;
        [SerializeField] private FinalImage squirrel;
        [SerializeField] private Button makePostdictionButton;
        [SerializeField] private Button backToGameButton;
        private bool rightChoice = false;
        private ImageType lastChoice;

        void Start()
        {
            makePostdictionButton.onClick.AddListener(MakePostdiction);
            backToGameButton.onClick.AddListener(BackToGame);
            makePostdictionButton.interactable = false;
        }

        private void BackToGame()
        {
            SceneManager.UnloadSceneAsync(SceneNames.PostdictionScene);
        }
        private void MakePostdiction()
        {
            switch (lastChoice)
            {
                case ImageType.Badger:
                    SceneManager.LoadScene(SceneNames.BadgerEndingScene, LoadSceneMode.Additive);
                    break;
                case ImageType.Cat:
                    SceneManager.LoadScene(SceneNames.CatEndingScene, LoadSceneMode.Additive);
                    break;
                case ImageType.Rabbit:
                    SceneManager.LoadScene(SceneNames.RabbitEndingScene, LoadSceneMode.Additive);
                    break;
                case ImageType.Squirrel:
                    SceneManager.LoadScene(SceneNames.SquirrelEndingScene, LoadSceneMode.Additive);
                    break;
            }
            SceneManager.UnloadSceneAsync(SceneNames.PostdictionScene);
        }
        public void Select(ImageType type)
        {
            makePostdictionButton.interactable = true;
            lastChoice = type;
            rightChoice = type == ImageType.Badger;
            switch (type)
            {
                case ImageType.Badger:
                    cat.ClearSelection();
                    squirrel.ClearSelection();
                    rabbit.ClearSelection();
                    break;
                case ImageType.Cat:
                    badger.ClearSelection();
                    squirrel.ClearSelection();
                    rabbit.ClearSelection();
                    break;
                case ImageType.Rabbit:
                    cat.ClearSelection();
                    squirrel.ClearSelection();
                    badger.ClearSelection();
                    break;
                default:
                    cat.ClearSelection();
                    badger.ClearSelection();
                    rabbit.ClearSelection();
                    break;
            }
        }
    }
}
