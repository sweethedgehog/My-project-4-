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
            makePostdictionButton.onClick.AddListener(makePostdiction);
            backToGameButton.onClick.AddListener(backToGame);
            makePostdictionButton.interactable = false;
        }

        private void backToGame()
        {
            SceneManager.UnloadSceneAsync(SceneNames.PostdictionScene);
        }
        private void makePostdiction()
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
        }
    }
}
