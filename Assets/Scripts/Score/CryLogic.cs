using CardGame.Core;
using UnityEngine;
using UnityEngine.UI;

public class CryLogic : MonoBehaviour
{
    public Sprite grayCristal;
    public Sprite roseCristal;
    public Sprite crownCristal;
    public Sprite coinsCristal;
    public Sprite skullCristal;
    private Image sprite;

    void Start()
    {
        sprite = GetComponent<Image>();
    }
    public void setTexture(Suits? suits)
    {
        // if (suits == null) return;
        switch (suits)
        {
            case Suits.Coins:
                sprite.sprite = coinsCristal;
                break;
            case Suits.Roses:
                sprite.sprite = roseCristal;
                break;
            case Suits.Crowns:
                sprite.sprite = crownCristal;
                break;
            case Suits.Skulls:
                sprite.sprite = skullCristal;
                break;
            default:
                sprite.sprite = grayCristal;
                break;
        }
    }
}
