using DefaultNamespace.Tiles;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using CardGame.Managers;

public class TilesManager : MonoBehaviour
{
    public TileScript[] tiles;
    public TileScript bigTile;
    public Sprite[] storySprites;
    public TextMeshProUGUI failerText;
    public TextMeshProUGUI successText;
    public AudioClip audioClipFail;
    public AudioClip audioClipSuccess;
    public AudioClip audioClipFullSuccess;
    public bool isActive = true;
    private int sumScore = 0;
    private int index = 0;
    public Color succesTextColor;
    public Color failerTextColor;
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
                    AudioManager.Instance.PlayRoundResult(audioClipFail);
                    break;
                case SuccessCodes.Patrial:
                    AudioManager.Instance.PlayRoundResult(audioClipSuccess);
                    break;
                case SuccessCodes.Success:
                    AudioManager.Instance.PlayRoundResult(audioClipFullSuccess);
                    break;
            }
        }
        statuses[index] = status;
        sumScore += (int)status;
        tiles[index].setVisability(status);
        setHistoryVisability(index);
        index++;
    }

    public void clickOn(int index)
    {
        if (index == -1) return;
        setHistoryVisability(index);
    }

    private void setHistoryVisability(int index)
    {
        bigTile.changeSuccessSprites(storySprites[index]);
        bigTile.setVisability(statuses[index]);
        failerText.text = statuses[index] == SuccessCodes.Failer ? HintsAndFailers.failers[index] : "";
        failerText.faceColor = failerTextColor;
        successText.text = statuses[index] == SuccessCodes.Success ? HintsAndFailers.hints[index] : "";
        successText.faceColor = succesTextColor;
    }
    public float GetScore() => (float) sumScore / 2;
}
