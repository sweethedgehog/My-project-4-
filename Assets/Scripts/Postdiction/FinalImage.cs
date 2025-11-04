using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FinalImage : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public PostdictionManager postdictionManager;
    public ImageType imageType;
    public Sprite light;
    public Sprite dark;
    private Image image;
    private bool selected = false;
    void Start()
    {
        image = GetComponent<Image>();
        image.sprite = dark;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        image.sprite = light;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!selected) image.sprite = dark;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        selected = true;
        postdictionManager.select(imageType);
    }

    public void clearSelection()
    {
        selected = false;
        image.sprite = dark;
    }
}
