using CardGame.Core;
using UnityEngine;

namespace CardGame.UI
{
    public class CrystalDisplay : MonoBehaviour
    {
        public Sprite grayCrystal;
        public Sprite roseCrystal;
        public Sprite crownCrystal;
        public Sprite coinsCrystal;
        public Sprite skullCrystal;
        private SpriteRenderer spriteRenderer;

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("\u041d\u0430 \u043e\u0431\u044a\u0435\u043a\u0442\u0435 " + gameObject.name + " \u043e\u0442\u0441\u0443\u0442\u0441\u0442\u0432\u0443\u0435\u0442 SpriteRenderer!", this);
            }
        }
        public void SetTexture(Suits? suits)
        {
            if (spriteRenderer == null) return;

            switch (suits)
            {
                case Suits.Coins:
                    spriteRenderer.sprite = coinsCrystal;
                    break;

                case Suits.Roses:
                    spriteRenderer.sprite = roseCrystal;
                    break;

                case Suits.Crowns:
                    spriteRenderer.sprite = crownCrystal;
                    break;

                case Suits.Skulls:
                    spriteRenderer.sprite = skullCrystal;
                    break;

                default:
                    spriteRenderer.sprite = grayCrystal;
                    break;
            }
        }
    }
}
