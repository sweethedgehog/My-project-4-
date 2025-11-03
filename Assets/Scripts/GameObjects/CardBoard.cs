using System;
using System.Collections.Generic;
using UnityEngine;
using CardGame.Cards;

namespace CardGame.GameObjects
{
    /// <summary>
    /// Magnetic card board that organizes cards in a row
    /// Cards automatically arrange themselves when added/removed
    /// Works with UI RectTransform system
    /// </summary>
    public class CardBoard : MonoBehaviour
    {
        [Header("Board Settings")]
        [SerializeField] private int maxCards = 5;
        [SerializeField] private float minX = -300f;
        [SerializeField] private float maxX = 300f;
        [SerializeField] private float yPosition = 0f;
        
        [Header("Animation")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private bool smoothMovement = true;
        
        [Header("Visual Feedback")]
        [SerializeField] private Color boardColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private bool showBoardVisual = true;
        
        private List<SimpleCard> cards = new List<SimpleCard>();
        private RectTransform rectTransform;
        
        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }
            
            // Setup visual if needed
            if (showBoardVisual)
            {
                SetupBoardVisual();
            }
        }
        
        void SetupBoardVisual()
        {
            UnityEngine.UI.Image img = GetComponent<UnityEngine.UI.Image>();
            if (img == null)
            {
                img = gameObject.AddComponent<UnityEngine.UI.Image>();
            }
            img.color = boardColor;
            img.raycastTarget = false; // Don't block card clicks
        }
        
        /// <summary>
        /// Add a card to the board, automatically positioning it
        /// </summary>
        public void AddCard(SimpleCard card)
        {
            if (card == null) return;
            
            // Find the best position to insert the card
            if (cards.Count == 0)
            {
                cards.Add(card);
            }
            else
            {
                float minDif = -1f;
                int index = 0;
                
                RectTransform cardRect = card.GetComponent<RectTransform>();
                float cardX = cardRect.anchoredPosition.x;
                
                for (int i = 0; i < cards.Count; i++)
                {
                    float targetX = GetCardXPosition(i);
                    float buf = Mathf.Abs(cardX - targetX);
                    
                    if (buf < minDif || minDif <= -0.5f)
                    {
                        minDif = buf;
                        index = i;
                    }
                    else break;
                }
                
                // Check if card should be inserted after this position
                RectTransform indexCardRect = cards[index].GetComponent<RectTransform>();
                if (indexCardRect.anchoredPosition.x < cardX)
                {
                    index++;
                }
                
                cards.Insert(index, card);
            }
            
            // Update card's parent
            card.transform.SetParent(transform);
            
            // Rearrange all cards
            RebaseAllCards();
            
            Debug.Log($"Card added to board. Total cards: {cards.Count}");
        }
        
        /// <summary>
        /// Remove a card from the board
        /// </summary>
        public void RemoveCard(SimpleCard card)
        {
            if (cards.Remove(card))
            {
                RebaseAllCards();
                Debug.Log($"Card removed from board. Remaining cards: {cards.Count}");
            }
        }
        
        /// <summary>
        /// Check if a card is on this board
        /// </summary>
        public bool HasCard(SimpleCard card)
        {
            return cards.Contains(card);
        }
        
        /// <summary>
        /// Rearrange all cards to their proper positions
        /// </summary>
        private void RebaseAllCards()
        {
            for (int i = 0; i < cards.Count; i++)
            {
                Vector2 targetPos = new Vector2(GetCardXPosition(i), yPosition);
                SetCardTargetPosition(cards[i], targetPos);
            }
        }
        
        /// <summary>
        /// Calculate X position for card at index
        /// </summary>
        private float GetCardXPosition(int index)
        {
            float mid = minX + (maxX - minX) / 2f;
            float offset = mid - minX;
            float spread = (float)cards.Count / maxCards;
            
            float rangeMin = mid - offset * spread;
            float rangeMax = mid + offset * spread;
            
            return Map(index, 0, cards.Count - 1, rangeMin, rangeMax);
        }
        
        /// <summary>
        /// Set target position for a card (with optional smooth movement)
        /// </summary>
        private void SetCardTargetPosition(SimpleCard card, Vector2 targetPos)
        {
            RectTransform cardRect = card.GetComponent<RectTransform>();
            if (cardRect == null) return;
            
            if (smoothMovement)
            {
                // Add smooth movement component if not present
                SmoothCardMover mover = card.GetComponent<SmoothCardMover>();
                if (mover == null)
                {
                    mover = card.gameObject.AddComponent<SmoothCardMover>();
                }
                mover.SetTarget(targetPos, moveSpeed);
            }
            else
            {
                // Instant positioning
                cardRect.anchoredPosition = targetPos;
            }
        }
        
        /// <summary>
        /// Map value from one range to another
        /// </summary>
        private float Map(float x, float inMin, float inMax, float outMin, float outMax)
        {
            if (inMin == inMax) return outMin + (outMax - outMin) / 2f;
            return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }
        
        /// <summary>
        /// Get number of cards on board
        /// </summary>
        public int CardCount => cards.Count;
        
        /// <summary>
        /// Get all cards on the board
        /// </summary>
        public List<SimpleCard> GetCards() => new List<SimpleCard>(cards);
        
        /// <summary>
        /// Clear all cards from board
        /// </summary>
        public void ClearBoard()
        {
            cards.Clear();
            Debug.Log("Board cleared");
        }
    }
    
    /// <summary>
    /// Component that smoothly moves a card to its target position
    /// </summary>
    public class SmoothCardMover : MonoBehaviour
    {
        private Vector2 targetPosition;
        private float speed = 10f;
        private RectTransform rectTransform;
        private bool isMoving = false;
        
        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }
        
        public void SetTarget(Vector2 target, float moveSpeed)
        {
            targetPosition = target;
            speed = moveSpeed;
            isMoving = true;
        }
        
        void Update()
        {
            if (!isMoving) return;
            
            // Smoothly move towards target
            Vector2 currentPos = rectTransform.anchoredPosition;
            Vector2 newPos = Vector2.Lerp(currentPos, targetPosition, Time.deltaTime * speed);
            rectTransform.anchoredPosition = newPos;
            
            // Stop when close enough
            if (Vector2.Distance(newPos, targetPosition) < 0.1f)
            {
                rectTransform.anchoredPosition = targetPosition;
                isMoving = false;
            }
        }
    }
}