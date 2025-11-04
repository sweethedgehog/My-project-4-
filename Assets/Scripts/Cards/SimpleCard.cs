using UnityEngine;
using UnityEngine.UI;
using CardGame.Core;
using System.Collections.Generic;

namespace CardGame.Cards
{
    /// <summary>
    /// Simple card that displays suit color and value number
    /// No sprites needed - just colors and text
    /// </summary>
    public class SimpleCard : MonoBehaviour
    {
        [Header("Card Properties")]
        public Suits suit;
        public int cardValue;

        public List<Sprite> RoseSprites;
        public List<Sprite> SkullSprites;
        public List<Sprite> CrownSprites;
        public List<Sprite> CoinSprites;
        
        [Header("UI References")]
        public Image cardBackground;
        public Text valueText;
        
        void Awake()
        {
            // Auto-find components if not assigned
            if (cardBackground == null)
                cardBackground = GetComponent<Image>();
            
            if (valueText == null)
                valueText = GetComponentInChildren<Text>();
        }
        
        public void Initialize(CardData cardData)
        {
            suit = cardData.suit;
            cardValue = cardData.cardValue;
            UpdateVisual();
        }
        
        void UpdateVisual()
        {
            List<Sprite> card_set = new List<Sprite> {} ;
            
            switch (suit)
            {
                case Suits.Roses:
                    card_set = RoseSprites;
                    break;
                case Suits.Skulls:
                    card_set = SkullSprites;
                    break;
                case Suits.Coins:
                    card_set = CoinSprites;
                    break;
                case Suits.Crowns:
                    card_set = CrownSprites;
                    break;
            }

            cardBackground.sprite = card_set[cardValue - 1];
        }
        
        Color GetSuitColor(Suits suit)
        {
            switch (suit)
            {
                case Suits.Roses:
                    return new Color(0.9f, 0.2f, 0.2f); // Bright red
                case Suits.Skulls:
                    return new Color(1f, 0.9f, 0.2f); // Bright yellow
                case Suits.Coins:
                    return new Color(0.2f, 0.5f, 1f); // Bright blue
                case Suits.Crowns:
                    return new Color(0.2f, 0.8f, 0.3f); // Bright green
                default:
                    return Color.white;
            }
        }
        
        // Get properties
        public Suits GetSuit() => suit;
        public int GetValue() => cardValue;
    }
}