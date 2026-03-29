using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
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
        private int currentHistoryIndex = -1;
        [SerializeField] private Color succesTextColor;
        [SerializeField] private Color failerTextColor;
        private SuccessCodes[] statuses = { SuccessCodes.None, SuccessCodes.None, SuccessCodes.None, SuccessCodes.None, SuccessCodes.None, SuccessCodes.None};

        void Start()
        {
            for (int i = 0; i < tiles.Length; i++) tiles[i].SetIndex(i);
            bigTile.SetIndex(-1);
            bigTile.SetFailColor(new  Color(1f, 1f, 1f, 0f));
        }
        public void SetVisibility(SuccessCodes status)
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
            currentHistoryIndex = index;
            sumScore += (int)status;
            tiles[index].SetVisibility(status);
            SetHistoryVisibility(index);
            index++;
        }

        public void ClickOn(int index)
        {
            if (index == -1) return;
            currentHistoryIndex = index;
            SetHistoryVisibility(index);
        }

        public void NavigateLeft()
        {
            int next = FindPrevRevealedIndex(currentHistoryIndex);
            if (next >= 0) ClickOn(next);
        }

        public void NavigateRight()
        {
            int next = FindNextRevealedIndex(currentHistoryIndex);
            if (next >= 0) ClickOn(next);
        }

        private int FindNextRevealedIndex(int from)
        {
            for (int i = from + 1; i < statuses.Length; i++)
                if (statuses[i] != SuccessCodes.None) return i;
            for (int i = 0; i <= from; i++)
                if (statuses[i] != SuccessCodes.None) return i;
            return -1;
        }

        private int FindPrevRevealedIndex(int from)
        {
            int start = from < 0 ? statuses.Length : from;
            for (int i = start - 1; i >= 0; i--)
                if (statuses[i] != SuccessCodes.None) return i;
            for (int i = statuses.Length - 1; i >= start; i--)
                if (statuses[i] != SuccessCodes.None) return i;
            return -1;
        }

        private void SetHistoryVisibility(int index)
        {
            foreach (var tile in tiles) tile.SetSelected(false);
            if (index >= 0 && index < tiles.Length) tiles[index].SetSelected(true);
            bigTile.ChangeSuccessSprites(storySprites[index]);
            bigTile.SetVisibility(statuses[index]);
            failerText.text = statuses[index] == SuccessCodes.Failer
                ? LocalizationSettings.StringDatabase.GetLocalizedString("MainScene", $"tile_failer_{index}")
                : "";
            failerText.faceColor = failerTextColor;
            successText.text = statuses[index] == SuccessCodes.Success
                ? LocalizationSettings.StringDatabase.GetLocalizedString("MainScene", $"tile_hint_{index}")
                : "";
            successText.faceColor = succesTextColor;
        }
        public float GetScore() => (float) sumScore / 2;
    }
}
