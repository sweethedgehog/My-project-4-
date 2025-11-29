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
        public GameObject overlay;
        
        private bool glowable = false;
        
        void Awake()
        {
            // Auto-find components if not assigned
            if (cardBackground == null)
                cardBackground = GetComponent<Image>();
            
            if (valueText == null)
                valueText = GetComponentInChildren<Text>();
        }

        public void SetCardData(CardData cardData)
        {
            suit = cardData.suit;
            cardValue = cardData.cardValue;
        }
        
        public void Initialize(CardData cardData)
        {
            SetCardData(cardData);
            UpdateVisual();
            glowable = true;
            overlay = transform.Find("Overlay").gameObject;
            TurnOffGlow();
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
        
        public void TurnOffGlow()
        {
            if (glowable)
            {
                overlay.SetActive(false);
            }
        }
    
        public void TurnOnGlow()
        {
            if (glowable)
            {
                overlay.SetActive(true);
            }
        }

        public CardData GetCardData() => new CardData(suit, cardValue);
        // Get properties
        public Suits GetSuit() => suit;
        public int GetValue() => cardValue;
    }
}