using System;
using DefaultNamespace.Tiles;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileScript : MonoBehaviour, IPointerClickHandler
{
    public Sprite spriteSuccess;
    public Sprite spriteLose;
    public TilesManager tilesManager;
    private Image thisImage;
    private Color failColor = new (1f, 1f, 1f, 0.5f);
    private int index;

    void Start()
    {
        thisImage = GetComponent<Image>();
        thisImage.color = Color.clear;
    }

    public void setVisability(SuccessCodes status)
    {
        thisImage.sprite = spriteLose;
        if (status == SuccessCodes.None) thisImage.color = Color.clear;
        else if (status == SuccessCodes.Failer) thisImage.color = failColor;
        else
        {
            thisImage.color = Color.white;
            thisImage.sprite = spriteSuccess;
        }
    }
    public void changeSuccessSprites(Sprite sprite) => spriteSuccess = sprite;
    public void setIndex(int index) => this.index = index;
    public void OnPointerClick(PointerEventData eventData) => tilesManager.clickOn(index);
}
