using CardGame.Core;
using UnityEngine;

namespace CardGame.UI
{
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
                Debug.LogError("\u041d\u0430 \u043e\u0431\u044a\u0435\u043a\u0442\u0435 " + gameObject.name + " \u043e\u0442\u0441\u0443\u0442\u0441\u0442\u0432\u0443\u0435\u0442 SpriteRenderer!", this);
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
}
