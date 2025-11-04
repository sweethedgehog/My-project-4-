using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FinalImage : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public PostdictionManager postdictionManager;
    public ImageType imageType;
    private Image image;
    private bool selected = false;
    void Start()
    {
        image = GetComponent<Image>();
        image.color = Color.darkGray;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        image.color = Color.white;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!selected) image.color = Color.darkGray;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        selected = true;
        postdictionManager.select(imageType);
    }

    public void clearSelection()
    {
        selected = false;
        image.color = Color.darkGray;
    }
}
