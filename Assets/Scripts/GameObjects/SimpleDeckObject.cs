using UnityEngine;
using TMPro;
using CardGame.Core;
using CardGame.Cards;
using System.Collections.Generic;

namespace CardGame.GameObjects
{
    /// <summary>
    /// Simple deck that spawns cards onto boards
    /// Uses SpriteRenderer for world-space rendering
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SimpleDeckObject : MonoBehaviour
    {
        [Header("Deck Visual")]
        [SerializeField] private Color deckColor = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private TextMeshProUGUI cardCountText;

        [Header("Deck Sprites")]
        [SerializeField] private Sprite emptyDeckSprite;
        [SerializeField] private Sprite fewCardsSprite;
        [SerializeField] private Sprite mediumCardsSprite;
        [SerializeField] private Sprite manyCardsSprite;

        [Header("Card Settings")]
        [SerializeField] private GameObject simpleCardPrefab;

        [Header("Target Board")]
        [SerializeField] private CardBoard targetBoard;
        [SerializeField] private bool spawnDirectlyOnBoard = true;

        // Card count thresholds for deck sprite changes
        private const int ManyCardsThreshold = 13;
        private const int MediumCardsThreshold = 5;
        private const int FewCardsThreshold = 1;

        private CardDeck deck;
        private SpriteRenderer deckRenderer;

        void Awake()
        {
            deckRenderer = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            InitializeDeck();
            UpdateVisual();
        }

        void InitializeDeck()
        {
            deck = new CardDeck(null);
            deck.Shuffle();
        }

        public List<CardData> PickCardsInDeck(int cardNum)
        {
            return deck.PickCards(cardNum);
        }

        public CardData PickCard()
        {
            CardData pickedCard = deck.Draw();
            return pickedCard;
        }

        public void DrawCardOnBoard()
        {
            if (deck.IsEmpty())
            {
                return;
            }

            CardData cardData = deck.Draw();

            if (cardData != null)
            {
                SpawnCardOnBoard(cardData);
                UpdateVisual();
            }
        }

        public void ShuffleCardIntoDeck(CardData cardData)
        {
            deck.PutCardIntoDeck(cardData);
            deck.Shuffle();
            UpdateVisual();
        }

        public SimpleCard SpawnCardOnBoard(CardData cardData, bool fromDeck = false)
        {
            if (targetBoard == null)
            {
                Debug.LogWarning("Target board is not assigned!");
                UpdateVisual();
                return null;
            }

            if (fromDeck)
            {
                deck.Remove(cardData);
                UpdateVisual();
            }

            // Create card under the board's transform
            GameObject cardObj;

            if (simpleCardPrefab != null)
            {
                cardObj = Instantiate(simpleCardPrefab, targetBoard.transform);
            }
            else
            {
                cardObj = new GameObject("Card");
                cardObj.transform.SetParent(targetBoard.transform);
            }

            // Set initial position at board center (will be repositioned by AddCard)
            cardObj.transform.localPosition = Vector3.zero;

            // Initialize card
            SimpleCard simpleCard = cardObj.GetComponent<SimpleCard>();
            if (simpleCard != null)
            {
                simpleCard.Initialize(cardData);
            }

            // Make draggable with board support
            SimpleDraggableWithBoard draggable = cardObj.GetComponent<SimpleDraggableWithBoard>();
            if (draggable == null)
            {
                draggable = cardObj.AddComponent<SimpleDraggableWithBoard>();
            }

            // Append to rightmost position for consistent left-to-right ordering
            targetBoard.AppendCard(simpleCard);

            return simpleCard;
        }

        void UpdateVisual()
        {
            if (deck == null) return;

            int remainingCards = deck.RemainingCards;

            if (remainingCards >= ManyCardsThreshold)
            {
                deckRenderer.sprite = manyCardsSprite;
            }
            else if (remainingCards >= MediumCardsThreshold)
            {
                deckRenderer.sprite = mediumCardsSprite;
            }
            else if (remainingCards >= FewCardsThreshold)
            {
                deckRenderer.sprite = fewCardsSprite;
            }
            else
            {
                deckRenderer.color = new Color(0, 0, 0, 0);
            }

            if (cardCountText != null)
            {
                cardCountText.text = deck.RemainingCards.ToString();
            }
        }

        public void ResetDeck()
        {
            deck.ResetAndShuffle();
            UpdateVisual();
        }

        public int GetRemainingCards()
        {
            if (deck == null) return 0;
            return deck.RemainingCards;
        }

        public bool IsDeckEmpty()
        {
            if (deck == null) return true;
            return deck.IsEmpty();
        }
    }
}
