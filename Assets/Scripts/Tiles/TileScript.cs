using CardGame.Managers;
using DefaultNamespace.Tiles;
using UnityEngine;
using UnityEngine.UI;

public class TileScript : MonoBehaviour
{
    private Image thisImage;
    private Color color;

    void Start()
    {
        thisImage = GetComponent<Image>();
        thisImage.color = Color.white;
    }

    public void setVisability(SuccessCodes status)
    {
        switch (status)
        {
            case SuccessCodes.Success:
                thisImage.color = Color.green;
                break;
            case SuccessCodes.Patrial:
                thisImage.color = Color.yellow;
                break;
            default:
                thisImage.color = Color.red;
                break;
        }
    }
}
