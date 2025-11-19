using System.Collections;
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
        [SerializeField] private float edgeExtension = 100f;
        
        [Header("Animation")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private bool smoothMovement = true;
        
        [Header("Visual Feedback")]
        [SerializeField] private Color boardColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private bool showBoardVisual = true;
        [SerializeField] private bool showDebugGizmos = false;
        
        private List<SimpleCard> cards = new List<SimpleCard>();
        private RectTransform rectTransform;
        public bool neverGlow;
        
        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }
            
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
            img.raycastTarget = false;
        }
        
        /// <summary>
        /// Add a card to the board, automatically positioning it
        /// </summary>
        public void AddCard(SimpleCard card)
        {
            if (card == null) return;

            if (neverGlow)
            {
                card.TurnOffGlow();
            }
            
            RectTransform cardRect = card.GetComponent<RectTransform>();
            
            // Get card position in board's local space
            Vector2 localCardPos = transform.InverseTransformPoint(cardRect.position);
            float cardX = localCardPos.x;
            
            if (cards.Count == 0)
            {
                cards.Add(card);
            }
            else
            {
                int bestIndex = 0;
                float minDistance = float.MaxValue;
                
                // Check all possible insertion positions
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
                
                cards.Insert(bestIndex, card);
            }
            
            card.transform.SetParent(transform);
            RebaseAllCards();
            
            Debug.Log($"Card added to board at position {cards.IndexOf(card)}. Total cards: {cards.Count}");
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
        /// Uses actual board width for adaptive sizing
        /// </summary>
        private float GetCardXPosition(int index)
        {
            if (cards.Count <= 1)
            {
                return 0f;
            }
            
            float boardWidth = rectTransform.rect.width;
            float usableWidth = boardWidth * 0.8f;
            float spreadFactor = Mathf.Min(1f, (float)cards.Count / maxCards);
            float actualSpread = usableWidth * spreadFactor;
            
            float leftEdge = -actualSpread / 2f;
            float rightEdge = actualSpread / 2f;
            
            return Map(index, 0, cards.Count - 1, leftEdge, rightEdge);
        }
        
        /// <summary>
        /// Get the X position where a card would be if inserted at index
        /// </summary>
        private float GetInsertionXPosition(int index)
        {
            if (cards.Count == 0)
            {
                return 0f;
            }
            
            int futureCardCount = cards.Count + 1;
            float boardWidth = rectTransform.rect.width;
            float usableWidth = boardWidth * 0.8f;
            float spreadFactor = Mathf.Min(1f, (float)futureCardCount / maxCards);
            float actualSpread = usableWidth * spreadFactor;
            
            float leftEdge = -actualSpread / 2f;
            float rightEdge = actualSpread / 2f;
            
            return Map(index, 0, futureCardCount - 1, leftEdge, rightEdge);
        }
        
        /// <summary>
        /// Set target position for a card
        /// </summary>
        private void SetCardTargetPosition(SimpleCard card, Vector2 targetPos)
        {
            RectTransform cardRect = card.GetComponent<RectTransform>();
            if (cardRect == null) return;
            
            if (smoothMovement)
            {
                SmoothCardMover mover = card.GetComponent<SmoothCardMover>();
                if (mover == null)
                {
                    mover = card.gameObject.AddComponent<SmoothCardMover>();
                }
                mover.SetTarget(targetPos, moveSpeed);
            }
            else
            {
                cardRect.anchoredPosition = targetPos;
            }
        }
        
        /// <summary>
        /// Check if a position is within the board's detection area
        /// </summary>
        public bool IsPositionNearBoard(Vector2 screenPosition)
        {
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                screenPosition,
                null,
                out localPos
            );
            
            float boardWidth = rectTransform.rect.width;
            float boardHeight = rectTransform.rect.height;
            
            float leftEdge = -boardWidth / 2f - edgeExtension;
            float rightEdge = boardWidth / 2f + edgeExtension;
            float topEdge = boardHeight / 2f + boardHeight * 0.2f;
            float bottomEdge = -boardHeight / 2f - boardHeight * 0.2f;
            
            bool inHorizontalRange = localPos.x >= leftEdge && localPos.x <= rightEdge;
            bool inVerticalRange = localPos.y >= bottomEdge && localPos.y <= topEdge;
            
            return inHorizontalRange && inVerticalRange;
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
        /// Manually reorder a card
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
        
        void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos || rectTransform == null) return;
            
            float boardWidth = rectTransform.rect.width;
            float boardHeight = rectTransform.rect.height;
            
            float leftEdge = -boardWidth / 2f - edgeExtension;
            float rightEdge = boardWidth / 2f + edgeExtension;
            float topEdge = boardHeight / 2f + boardHeight * 0.2f;
            float bottomEdge = -boardHeight / 2f - boardHeight * 0.2f;
            
            Gizmos.color = Color.green;
            Vector3[] corners = new Vector3[4];
            corners[0] = transform.TransformPoint(new Vector3(-boardWidth / 2f, bottomEdge + boardHeight * 0.2f, 0));
            corners[1] = transform.TransformPoint(new Vector3(boardWidth / 2f, bottomEdge + boardHeight * 0.2f, 0));
            corners[2] = transform.TransformPoint(new Vector3(boardWidth / 2f, topEdge - boardHeight * 0.2f, 0));
            corners[3] = transform.TransformPoint(new Vector3(-boardWidth / 2f, topEdge - boardHeight * 0.2f, 0));
            
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
            }
            
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

        public void Stop()
        {
            isMoving = false;
        }
        
        void Update()
        {
            if (!isMoving) return;
            
            Vector2 currentPos = rectTransform.anchoredPosition;
            Vector2 newPos = Vector2.Lerp(currentPos, targetPosition, Time.deltaTime * speed);
            rectTransform.anchoredPosition = newPos;
            
            if (Vector2.Distance(newPos, targetPosition) < 0.1f)
            {
                rectTransform.anchoredPosition = targetPosition;
                isMoving = false;
            }
        }
    }
}