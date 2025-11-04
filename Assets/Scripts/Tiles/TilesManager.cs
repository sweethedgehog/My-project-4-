using DefaultNamespace.Tiles;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilesManager : MonoBehaviour
{
    public TileScript[] tiles;
    public bool isActive = true;
    private int sumScore = 0;
    private int index = 0;
    public void setVisibility(SuccessCodes status)
    {
        sumScore += (int)status;
        tiles[index].setVisability(status);
        index++;
    }
    public float GetScore() => (float) sumScore / 2;
}
