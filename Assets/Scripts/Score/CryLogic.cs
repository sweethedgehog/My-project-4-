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
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("На объекте " + gameObject.name + " отсутствует SpriteRenderer!", this);
        }
    }
    public void setTexture(Suits? suits)
    {
        if (spriteRenderer == null) return;

        switch (suits)
        {
            case Suits.Coins:
                spriteRenderer.sprite = coinsCristal;
                break;

            case Suits.Roses:
                spriteRenderer.sprite = roseCristal;
                break;

            case Suits.Crowns:
                spriteRenderer.sprite = crownCristal;
                break;

            case Suits.Skulls:
                spriteRenderer.sprite = skullCristal;
                break;

            default:
                spriteRenderer.sprite = grayCristal;
                break;
        }
    }
}
