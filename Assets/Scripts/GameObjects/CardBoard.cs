using System.Collections.Generic;
using UnityEngine;
using CardGame.Cards;
using CardGame.Scoring;
using CardGame.Core;


namespace CardGame.GameObjects
{
    /// <summary>
    /// Magnetic card board that organizes cards in a row
    /// Cards automatically arrange themselves when added/removed
    /// Works with world-space Transform system
    /// </summary>
    public class CardBoard : MonoBehaviour
    {
        [Header("Board Settings")] [SerializeField]
        private int maxCards = 5;

        [SerializeField] private float boardWidth = 5.92f;
        [SerializeField] private float boardHeight = 1.58f;
        [SerializeField] private float yPosition = 0f;
        [SerializeField] private float edgeExtension = 1.0f;

        [Header("Animation")] [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private bool smoothMovement = true;

        [Header("Visual Feedback")] [SerializeField]
        private Color boardColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        [SerializeField] private bool showBoardVisual = true;

        [Header("Interaction Control")] public bool freeze = false;
        [SerializeField] private float frozenAlpha = 0.6f;

        private List<SimpleCard> cards = new List<SimpleCard>();
        public CardScorer scorer;
        public bool neverGlow;

        void Awake()
        {
            if (showBoardVisual)
            {
                SetupBoardVisual();
            }
        }

        public void SetGoal(Suits suit, int value)
        {
            scorer.SetGoal(suit, value);
        }

        void SetupBoardVisual()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = gameObject.AddComponent<SpriteRenderer>();
            }

            sr.color = boardColor;
            sr.sortingLayerName = "Gameplay";
        }

        /// <summary>
        /// Set freeze state and update card interactability
        /// </summary>
        public void SetFreeze(bool frozen)
        {
            if (freeze == frozen) return;

            freeze = frozen;
            UpdateCardInteractability();
            RebaseAllCards();
        }

        /// <summary>
        /// Update interactability of all cards on this board
        /// </summary>
        private void UpdateCardInteractability()
        {
            foreach (SimpleCard card in cards)
            {
                if (card != null)
                {
                    SetCardInteractable(card, !freeze);
                }
            }
        }

        /// <summary>
        /// Set whether a single card is interactable
        /// </summary>
        private void SetCardInteractable(SimpleCard card, bool interactable)
        {
            SpriteRenderer sr = card.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = interactable ? Color.white : new Color(1f, 1f, 1f, frozenAlpha);
            }

            BoxCollider2D col = card.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.enabled = interactable;
            }
        }

        /// <summary>
        /// Check if this board accepts interactions
        /// </summary>
        public bool IsInteractable()
        {
            return !freeze;
        }

        private void UpdateScore()
        {
            if (scorer == null) return;

            CardLayout cardLayout = new CardLayout();
            foreach (SimpleCard simpleCard in cards)
            {
                cardLayout.AddCard(simpleCard);
            }

            // Calculate score using CardLayout
            Score score = cardLayout.GetScore();

            int i = 0;
            foreach (bool hasMultiplier in score.GetMultipliers())
            {
                if (hasMultiplier)
                {
                    cards[i].TurnOnGlow();
                }
                else
                {
                    cards[i].TurnOffGlow();
                }

                i++;
            }

            scorer.UpdateScore(score);
        }

        public void AddCard(SimpleCard card)
        {
            AddCardAtPosition(card, -1);
        }

        /// <summary>
        /// Add a card at a specific index (left-to-right order)
        /// Use index -1 for position-based insertion (drag-drop behavior)
        /// Use index >= 0 to insert at specific position (0 = leftmost)
        /// Use index >= cards.Count to append at rightmost position
        /// </summary>
        public void AddCardAtPosition(SimpleCard card, int index)
        {
            if (freeze)
            {
                return;
            }

            if (card == null) return;

            if (neverGlow)
            {
                card.TurnOffGlow();
            }

            if (index < 0)
            {
                // Auto-detect position based on card's current location
                Vector2 localCardPos = transform.InverseTransformPoint(card.transform.position);
                float cardX = localCardPos.x;

                if (cards.Count == 0)
                {
                    cards.Add(card);
                }
                else
                {
                    int bestIndex = 0;
                    float minDistance = float.MaxValue;

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
            }
            else
            {
                int insertIndex = Mathf.Clamp(index, 0, cards.Count);
                cards.Insert(insertIndex, card);
            }

            card.transform.SetParent(transform);

            SetCardInteractable(card, !freeze);

            RebaseAllCards();

            UpdateScore();
        }

        /// <summary>
        /// Append a card to the rightmost position (left-to-right order)
        /// </summary>
        public void AppendCard(SimpleCard card)
        {
            AddCardAtPosition(card, cards.Count);
        }

        /// <summary>
        /// Remove a card from the board
        /// </summary>
        public void RemoveCard(SimpleCard card)
        {
            if (freeze)
            {
                return;
            }

            if (cards.Remove(card))
            {
                RebaseAllCards();
            }

            card.TurnOffGlow();
            UpdateScore();
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

            UpdateScore();
        }

        /// <summary>
        /// Calculate X position for card at index
        /// Uses serialized board width for adaptive sizing
        /// </summary>
        private float GetCardXPosition(int index)
        {
            if (cards.Count <= 1)
            {
                return 0f;
            }

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
                card.transform.localPosition = new Vector3(targetPos.x, targetPos.y, 0f);
            }
        }

        /// <summary>
        /// Check if a position is within the board's detection area
        /// Converts screen position to world-space for comparison
        /// </summary>
        public bool IsPositionNearBoard(Vector2 screenPosition)
        {
            if (freeze) return false;

            Camera cam = Camera.main;
            Vector3 worldPos = cam.ScreenToWorldPoint(
                new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(cam.transform.position.z)));
            Vector2 localPos = transform.InverseTransformPoint(worldPos);

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

        public List<CardData> GetCardsData()
        {
            List<CardData> result = new List<CardData>();
            foreach (SimpleCard simpleCard in GetCards())
            {
                result.Add(simpleCard.GetCardData());
            }

            return result;
        }

    /// <summary>
        /// Clear all cards from board
        /// </summary>
        public void ClearBoard()
        {
            cards.Clear();
        }

        /// <summary>
        /// Swap two cards on the board
        /// </summary>
        public void SwapCards(int index1, int index2)
        {
            if (freeze)
            {
                return;
            }

            if (index1 < 0 || index1 >= cards.Count || index2 < 0 || index2 >= cards.Count)
                return;

            SimpleCard temp = cards[index1];
            cards[index1] = cards[index2];
            cards[index2] = temp;
            RebaseAllCards();
            UpdateScore();
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
    /// Uses Transform.localPosition for world-space positioning
    /// </summary>
    public class SmoothCardMover : MonoBehaviour
    {
        private Vector2 targetPosition;
        private float speed = 10f;
        private bool isMoving = false;

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

            Vector2 currentPos = (Vector2)transform.localPosition;
            Vector2 newPos = Vector2.Lerp(currentPos, targetPosition, Time.deltaTime * speed);
            transform.localPosition = new Vector3(newPos.x, newPos.y, transform.localPosition.z);

            if (Vector2.Distance(newPos, targetPosition) < 0.01f)
            {
                transform.localPosition = new Vector3(targetPosition.x, targetPosition.y, transform.localPosition.z);
                isMoving = false;
            }
        }
    }
}
