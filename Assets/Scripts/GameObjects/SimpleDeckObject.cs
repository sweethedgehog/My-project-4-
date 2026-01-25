using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using CardGame.Core;
using CardGame.Cards;

namespace CardGame.GameObjects
{
    /// <summary>
    /// Simple deck that spawns colored cards with numbers
    /// No sprites needed!
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class SimpleDeckObject : MonoBehaviour, IPointerClickHandler
    {
        [Header("Deck Visual")]
        [SerializeField] private Color deckColor = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private TextMeshProUGUI cardCountText;
        
        [Header("Deck Sprites")]
        [SerializeField] private Sprite emptyDeckSprite; // No cards (can be null for no texture)
        [SerializeField] private Sprite fewCardsSprite; // 1-4 cards
        [SerializeField] private Sprite mediumCardsSprite; // 5-12 cards
        [SerializeField] private Sprite manyCardsSprite; // 13+ cards
        
        [Header("Card Settings")]
        [SerializeField] private GameObject simpleCardPrefab;
        [SerializeField] private Canvas parentCanvas;
        [SerializeField] private Vector2 cardSize = new Vector2(80, 120);
        
        [Header("Target Board")]
        [SerializeField] private CardBoard targetBoard;
        [SerializeField] private bool spawnDirectlyOnBoard = true;
        
        private CardDeck deck;
        private Image deckImage;
        
        void Awake()
        {
            deckImage = GetComponent<Image>();
            if (parentCanvas == null)
            {
                parentCanvas = GetComponentInParent<Canvas>();
            }
        }
        
        void Start()
        {
            InitializeDeck();
            UpdateVisual();
        }
        
        void InitializeDeck()
        {
            // Create deck without sprite map (we don't need sprites!)
            deck = new CardDeck(null);
            deck.Shuffle();
            
            Debug.Log($"Simple Deck initialized with {deck.RemainingCards} cards");
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"Deck clicked! SpawnDirectlyOnBoard: {spawnDirectlyOnBoard}, TargetBoard: {(targetBoard != null ? targetBoard.name : "NULL")}");
            // DrawCardUnderMouse();
        }

        public CardData PickCard()
        {
            return deck.Draw();
        }
        
        public void DrawCardOnBoard()
        {
            if (deck.IsEmpty())
            {
                Debug.Log("Deck is empty!");
                return;
            }
            
            CardData cardData = deck.Draw();
            
            if (cardData != null)
            {   
                SpawnCardOnBoard(cardData);
                UpdateVisual();
            }
        }
        
        void DrawCardUnderMouse()
        {
            if (deck.IsEmpty())
            {
                Debug.Log("Deck is empty!");
                return;
            }
            
            CardData cardData = deck.Draw();
            
            if (cardData != null)
            {
                if (spawnDirectlyOnBoard && targetBoard != null)
                {
                    SpawnCardOnBoard(cardData);
                }
                else
                {
                    SpawnCardAtMouse(cardData);
                }
                UpdateVisual();
            }
        }

        public void ShuffleCardIntoDeck(CardData cardData)
        {
            deck.PutCardIntoDeck(cardData);
            deck.Shuffle();
            UpdateVisual();
        }
        
        public void SpawnCardOnBoard(CardData cardData)
        {
            if (targetBoard == null)
            {
                Debug.LogWarning("Target board is not assigned! Spawning under mouse instead.");
                SpawnCardAtMouse(cardData);
                return;
            }
            
            // Create card - spawn it under the board's transform temporarily
            GameObject cardObj;
            
            if (simpleCardPrefab != null)
            {
                cardObj = Instantiate(simpleCardPrefab, targetBoard.transform);
            }
            else
            {
				Debug.Log("createSimpleCard");
                cardObj = CreateSimpleCard();
                cardObj.transform.SetParent(targetBoard.transform);
            }
            
            // Set initial position at board center (will be repositioned by AddCard)
            RectTransform cardRect = cardObj.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                cardRect.anchoredPosition = Vector2.zero;
            }
            
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
            
            // Add directly to target board - this will position it correctly
            targetBoard.AddCard(simpleCard);
            
            Debug.Log($"Drew: {cardData.suit} - Value {cardData.cardValue} → Added to {targetBoard.name} (Remaining: {deck.RemainingCards})");
        }
        
        void SpawnCardAtMouse(CardData cardData)
        {
            // Create card at mouse position
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                Input.mousePosition,
                parentCanvas.worldCamera,
                out mousePos
            );
            
            // Use prefab if assigned, otherwise create from scratch
            GameObject cardObj;
            
            if (simpleCardPrefab != null)
            {
                cardObj = Instantiate(simpleCardPrefab, parentCanvas.transform);
            }
            else
            {
                cardObj = CreateSimpleCard();
            }
            
            // Position card
            RectTransform cardRect = cardObj.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                cardRect.anchoredPosition = mousePos;
            }
            
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
            
            Debug.Log($"Drew: {cardData.suit} - Value {cardData.cardValue} (Remaining: {deck.RemainingCards})");
        }
        
        GameObject CreateSimpleCard()
        {
            // Create card from scratch if no prefab assigned
            GameObject cardObj = new GameObject("Card");
            cardObj.transform.SetParent(parentCanvas.transform);
            
            // Add RectTransform
            RectTransform rect = cardObj.AddComponent<RectTransform>();
            rect.sizeDelta = cardSize;
            
            // Add Image for background - IMPORTANT: Must be raycast target!
            Image bgImage = cardObj.AddComponent<Image>();
            bgImage.color = Color.white;
            bgImage.raycastTarget = true; // Enable raycast!
            
            // Add outline
            Outline outline = cardObj.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, -2);
            
            // Create text child for value
            GameObject textObj = new GameObject("ValueText");
            textObj.transform.SetParent(cardObj.transform);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 48;
            text.color = Color.white;
            text.raycastTarget = false; // Text should NOT block raycasts
            
            // Add SimpleCard component
            SimpleCard cardScript = cardObj.AddComponent<SimpleCard>();
            cardScript.cardBackground = bgImage;
            // Note: valueText in SimpleCard is TextMeshProUGUI, but we're creating basic Text here
            // This works because the field might be null and we check for it
            // If you want TextMeshProUGUI, assign it manually or use a prefab
            
            // Add CanvasGroup for dragging
            CanvasGroup cg = cardObj.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = true; // Initially should block raycasts
            
            return cardObj;
        }
        
        void UpdateVisual()
        {
            if (deck == null) return;

			int remainingCards = deck.RemainingCards;
			
			if (remainingCards >= 13)
			{
				deckImage.sprite = manyCardsSprite;
			}
			else if (remainingCards >= 5)
			{
				deckImage.sprite = mediumCardsSprite;
			}
			else if (remainingCards >= 1)
			{
				deckImage.sprite = fewCardsSprite;
			}
			else
          	{
				deckImage.color = new Color(0, 0, 0, 0);
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
            Debug.Log("Deck reset and shuffled");
        }
        
        // Public methods for external access
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