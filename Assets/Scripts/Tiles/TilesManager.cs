using TMPro;
using UnityEngine;
using CardGame.Managers;

namespace DefaultNamespace.Tiles
{
    public class TilesManager : MonoBehaviour
    {
        [SerializeField] private TileScript[] tiles;
        [SerializeField] private TileScript bigTile;
        [SerializeField] private Sprite[] storySprites;
        [SerializeField] private TextMeshProUGUI failerText;
        [SerializeField] private TextMeshProUGUI successText;
        [SerializeField] private AudioClip audioClipFail;
        [SerializeField] private AudioClip audioClipSuccess;
        [SerializeField] private AudioClip audioClipFullSuccess;
        public bool isActive = true;
        private int sumScore = 0;
        private int index = 0;
        [SerializeField] private Color succesTextColor;
        [SerializeField] private Color failerTextColor;
        private SuccessCodes[] statuses = { SuccessCodes.None, SuccessCodes.None, SuccessCodes.None, SuccessCodes.None, SuccessCodes.None, SuccessCodes.None};

        void Start()
        {
            for (int i = 0; i < tiles.Length; i++) tiles[i].setIndex(i);
            bigTile.setIndex(-1);
            bigTile.setFailerColor(new  Color(1f, 1f, 1f, 0f));
        }
        public void setVisibility(SuccessCodes status)
        {
            if (AudioManager.Instance != null)
            {
                switch (status)
                {
                    case SuccessCodes.Failer:
                        AudioManager.Instance.PlaySFX(audioClipFail);
                        break;
                    case SuccessCodes.Partial:
                        AudioManager.Instance.PlaySFX(audioClipSuccess);
                        break;
                    case SuccessCodes.Success:
                        AudioManager.Instance.PlaySFX(audioClipFullSuccess);
                        break;
                }
            }
            statuses[index] = status;
            sumScore += (int)status;
            tiles[index].setVisibility(status);
            setHistoryVisibility(index);
            index++;
        }

        public void clickOn(int index)
        {
            if (index == -1) return;
            setHistoryVisibility(index);
        }

        private void setHistoryVisibility(int index)
        {
            bigTile.changeSuccessSprites(storySprites[index]);
            bigTile.setVisibility(statuses[index]);
            failerText.text = statuses[index] == SuccessCodes.Failer ? HintsAndFailers.failers[index] : "";
            failerText.faceColor = failerTextColor;
            successText.text = statuses[index] == SuccessCodes.Success ? HintsAndFailers.hints[index] : "";
            successText.faceColor = succesTextColor;
        }
        public float GetScore() => (float) sumScore / 2;
    }
}
