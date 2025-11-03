using CardGame.Managers;
using UnityEngine;
using UnityEngine.UI;

public class TileScript : MonoBehaviour
{
    public int index = 1;
    private Image thisImage;
    private Color color;

    void Start()
    {
        thisImage = GetComponent<Image>();
        thisImage.color = Color.white;
    }
    void Update()
    {
        GameObject obj = GameObject.Find("RoundManager");
        RoundManager roundManager = obj.GetComponent<RoundManager>();
        if (roundManager != null && roundManager.getScoreHistoryCount() >= index)
        {
            switch (roundManager.getScoreHistoryValue(index - 1))
            {
                case 0.5f:
                    thisImage.color = Color.yellow;
                    break;
                case 1f:
                    thisImage.color = Color.green;
                    break;
                default:
                    thisImage.color = Color.red;
                    break;
            }
        }
    }
}
