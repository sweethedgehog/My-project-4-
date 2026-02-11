using UnityEngine;
using CardGame.Core;
using System.Collections.Generic;
using CardGame.GameObjects;

namespace CardGame.Cards
{
    /// <summary>
    /// Simple card that displays suit color and value number
    /// Uses SpriteRenderer for world-space rendering
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

        [Header("References")]
        public SpriteRenderer cardRenderer;
        public GameObject overlay;

        private bool glowable = false;

        void Awake()
        {
            if (cardRenderer == null)
                cardRenderer = GetComponent<SpriteRenderer>();
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
            List<Sprite> cardSet = new List<Sprite> {} ;

            switch (suit)
            {
                case Suits.Roses:
                    cardSet = RoseSprites;
                    break;
                case Suits.Skulls:
                    cardSet = SkullSprites;
                    break;
                case Suits.Coins:
                    cardSet = CoinSprites;
                    break;
                case Suits.Crowns:
                    cardSet = CrownSprites;
                    break;
            }

            cardRenderer.sprite = cardSet[cardValue - 1];
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

        private bool individualFreeze = false;

        public bool CanInteract()
        {
            if (individualFreeze) return false;

            CardBoard board = GetComponentInParent<CardBoard>();
            if (board != null)
            {
                return board.IsInteractable();
            }
            return true;
        }

        /// <summary>
        /// Freeze or unfreeze this specific card (independent of board state)
        /// </summary>
        public void SetIndividualFreeze(bool frozen)
        {
            individualFreeze = frozen;

            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.enabled = !frozen;
            }

            // Darken the card when frozen (no transparency)
            if (cardRenderer != null)
            {
                cardRenderer.color = frozen ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;
            }
        }

        public bool IsIndividuallyFrozen() => individualFreeze;
    }
}
