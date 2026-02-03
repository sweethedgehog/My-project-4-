using UnityEngine;
using UnityEngine.UI;
using CardGame.Core;
using CardGame.Managers;

namespace CardGame.UI
{
    public class BallSpriteByGoalSuit : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RoundManager roundManager;

        [Header("Suit Sprites")]
        [SerializeField] private Sprite coinsSprite;
        [SerializeField] private Sprite crownsSprite;
        [SerializeField] private Sprite skullsSprite;
        [SerializeField] private Sprite rosesSprite;

        private Image imageComponent;
        private SpriteRenderer spriteRenderer;
        private Suits lastSuit;

        private void Awake()
        {
            imageComponent = GetComponent<Image>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            if (roundManager == null)
            {
                roundManager = FindFirstObjectByType<RoundManager>();
            }
        }

        private void Update()
        {
            if (roundManager == null) return;

            Suits goalSuit = roundManager.GetGoalSuit();

            if (goalSuit != lastSuit)
            {
                UpdateSprite(goalSuit);
                lastSuit = goalSuit;
            }
        }

        private void UpdateSprite(Suits suit)
        {
            Sprite targetSprite = GetSpriteForSuit(suit);

            if (targetSprite == null) return;

            if (imageComponent != null)
            {
                imageComponent.sprite = targetSprite;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = targetSprite;
            }
        }

        private Sprite GetSpriteForSuit(Suits suit)
        {
            switch (suit)
            {
                case Suits.Coins:
                    return coinsSprite;
                case Suits.Crowns:
                    return crownsSprite;
                case Suits.Skulls:
                    return skullsSprite;
                case Suits.Roses:
                    return rosesSprite;
                default:
                    return null;
            }
        }
    }
}
