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
    
		[Header("Edge Detection")]
    	[SerializeField] private float edgeExtension = 100f;
    	[SerializeField] private float boardHeight = 150f;
        
        [Header("Animation")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private bool smoothMovement = true;
        
        [Header("Visual Feedback")]
        [SerializeField] private Color boardColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private bool showBoardVisual = true;
        [SerializeField] private bool showDebugGizmos = false;
        
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
        
        void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos || rectTransform == null) return;
            
            // Draw the board detection area
            Vector3 boardPos = transform.position;
            
            // Convert local board coordinates to world space for gizmos
            float leftEdge = minX - edgeExtension;
            float rightEdge = maxX + edgeExtension;
            float topEdge = boardHeight / 2f;
            float bottomEdge = -boardHeight / 2f;
            
            // Draw board rectangle (green)
            Gizmos.color = Color.green;
            Vector3[] corners = new Vector3[4];
            corners[0] = transform.TransformPoint(new Vector3(minX, bottomEdge, 0));
            corners[1] = transform.TransformPoint(new Vector3(maxX, bottomEdge, 0));
            corners[2] = transform.TransformPoint(new Vector3(maxX, topEdge, 0));
            corners[3] = transform.TransformPoint(new Vector3(minX, topEdge, 0));
            
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
            }
            
            // Draw extended detection area (yellow)
            Gizmos.color = Color.yellow;
            Vector3[] extendedCorners = new Vector3[4];
            extendedCorners[0] = transform.TransformPoint(new Vector3(leftEdge, bottomEdge, 0));
            extendedCorners[1] = transform.TransformPoint(new Vector3(rightEdge, bottomEdge, 0));
            extendedCorners[2] = transform.TransformPoint(new Vector3(rightEdge, topEdge, 0));
            extendedCorners[3] = transform.TransformPoint(new Vector3(leftEdge, topEdge, 0));
            
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(extendedCorners[i], extendedCorners[(i + 1) % 4]);
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
            
            RectTransform cardRect = card.GetComponent<RectTransform>();
            float cardX = cardRect.anchoredPosition.x;
            float cardY = cardRect.anchoredPosition.y;
            
            // Find the best position to insert the card
            if (cards.Count == 0)
            {
                // First card - just add it
                cards.Add(card);
            }
            else
            {
                // Find insertion point by checking distance to all positions (including beyond edges)
                int bestIndex = 0;
                float minDistance = float.MaxValue;
                
                // Check distance to each possible position (including edge extensions)
                for (int i = 0; i <= cards.Count; i++)
                {
                    float targetX = GetInsertionXPosition(i);
                    float distance = Mathf.Abs(cardX - targetX);
                    
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestIndex = i;
                    }
                }
                
                // Insert at the best position
                cards.Insert(bestIndex, card);
            }
            
            // Update card's parent
            card.transform.SetParent(transform);
            
            // Rearrange all cards
            RebaseAllCards();
            
            Debug.Log($"Card added to board at position {cards.IndexOf(card)}. Total cards: {cards.Count}");
        }
        
        /// <summary>
        /// Check if a position is within the board's detection area
        /// Now includes edges and vertical range
        /// </summary>
        public bool IsPositionNearBoard(Vector2 position)
        {
            // Get board position in local space
            Vector2 boardCenter = rectTransform.anchoredPosition;
            
            // Check horizontal range (with edge extension)
            float leftEdge = boardCenter.x + minX - edgeExtension;
            float rightEdge = boardCenter.x + maxX + edgeExtension;
            
            // Check vertical range
            float topEdge = boardCenter.y + boardHeight / 2f;
            float bottomEdge = boardCenter.y - boardHeight / 2f;
            
            bool inHorizontalRange = position.x >= leftEdge && position.x <= rightEdge;
            bool inVerticalRange = position.y >= bottomEdge && position.y <= topEdge;
            
            return inHorizontalRange && inVerticalRange;
        }
        
        /// <summary>
        /// Get the X position where a card would be if inserted at index
        /// Used to find the best insertion point
        /// </summary>
        private float GetInsertionXPosition(int index)
        {
            if (cards.Count == 0)
            {
                return (minX + maxX) / 2f;
            }
            
            // Calculate what the X would be with one more card
            int futureCardCount = cards.Count + 1;
            
            float mid = minX + (maxX - minX) / 2f;
            float offset = mid - minX;
            float spread = (float)futureCardCount / maxCards;
            
            float rangeMin = mid - offset * spread;
            float rangeMax = mid + offset * spread;
            
            return Map(index, 0, futureCardCount - 1, rangeMin, rangeMax);
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
        
        /// <summary>
        /// Manually reorder a card (for shuffling)
        /// </summary>
        public void MoveCard(SimpleCard card, int newIndex)
        {
            if (!cards.Contains(card)) return;
            
            cards.Remove(card);
            newIndex = Mathf.Clamp(newIndex, 0, cards.Count);
            cards.Insert(newIndex, card);
            RebaseAllCards();
        }
        
        /// <summary>
        /// Swap two cards on the board
        /// </summary>
        public void SwapCards(int index1, int index2)
        {
            if (index1 < 0 || index1 >= cards.Count || index2 < 0 || index2 >= cards.Count)
                return;
            
            SimpleCard temp = cards[index1];
            cards[index1] = cards[index2];
            cards[index2] = temp;
            RebaseAllCards();
        }
        
        /// <summary>
        /// Get card at index
        /// </summary>
        public SimpleCard GetCardAt(int index)
        {
            if (index < 0 || index >= cards.Count)
                return null;
            return cards[index];
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