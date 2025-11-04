using DefaultNamespace.Tiles;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilesManager : MonoBehaviour
{
    public TileScript[] tiles;
    public TileScript bigTile;
    public Sprite[] storySprites;
    public TextMeshProUGUI failerText;
    public TextMeshProUGUI successText;
    public bool isActive = true;
    private int sumScore = 0;
    private int index = 0;
    private SuccessCodes[] statuses = { SuccessCodes.None, SuccessCodes.None, SuccessCodes.None, SuccessCodes.None, SuccessCodes.None, SuccessCodes.None};

    void Start()
    {
        for (int i = 0; i < tiles.Length; i++) tiles[i].setIndex(i);
        bigTile.setIndex(-1);
    }
    public void setVisibility(SuccessCodes status)
    {
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
        successText.text = statuses[index] == SuccessCodes.Success ? HintsAndFailers.hints[index] : "";
    }
    public float GetScore() => (float) sumScore / 2;
}
