using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DefaultNamespace.Tiles
{
    public class TileScript : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Sprite spriteSuccess;
        [SerializeField] private Sprite spriteLose;
        [SerializeField] private TilesManager tilesManager;
        private Image thisImage;
        private Color failColor = new (1f, 1f, 1f, 0.5f);
        private int index;

        void Start()
        {
            thisImage = GetComponent<Image>();
            thisImage.color = Color.clear;
        }

        public void SetVisibility(SuccessCodes status)
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
        public void ChangeSuccessSprites(Sprite sprite) => spriteSuccess = sprite;
        public void SetIndex(int index) => this.index = index;
        public void OnPointerClick(PointerEventData eventData) => tilesManager.ClickOn(index);
        public void SetFailColor(Color color) => failColor = color;
    }
}
