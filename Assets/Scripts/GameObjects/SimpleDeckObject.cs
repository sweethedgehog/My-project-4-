using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
        [SerializeField] private Text cardCountText;
        
        [Header("Card Settings")]
        [SerializeField] private GameObject simpleCardPrefab;
        [SerializeField] private Canvas parentCanvas;
        [SerializeField] private Vector2 cardSize = new Vector2(80, 120);
        
        private CardDeck deck;
        private Image deckImage;
        
        void Awake()
        {
            deckImage = GetComponent<Image>();
            deckImage.color = deckColor;
            
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
            DrawCardUnderMouse();
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
                SpawnCardAtMouse(cardData);
                UpdateVisual();
            }
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
            
            Debug.Log($"Drew: {cardData.suit} - Value {cardData.value} (Remaining: {deck.RemainingCards})");
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
            cardScript.valueText = text;
            
            // Add CanvasGroup for dragging
            CanvasGroup cg = cardObj.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = true; // Initially should block raycasts
            
            return cardObj;
        }
        
        void UpdateVisual()
        {
            if (cardCountText != null)
            {
                cardCountText.text = deck.RemainingCards.ToString();
            }
            
            // Darken deck when empty
            if (deckImage != null)
            {
                deckImage.color = deck.IsEmpty() ? 
                    new Color(0.2f, 0.2f, 0.2f) : deckColor;
            }
        }
        
        public void ResetDeck()
        {
            deck.ResetAndShuffle();
            UpdateVisual();
            Debug.Log("Deck reset and shuffled");
        }
    }
    
    /// <summary>
    /// Simple draggable component for cards
    /// </summary>
    public class SimpleDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        private RectTransform rectTransform;
        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private bool isDragging = false;
        private Vector2 offset;
        
        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            // Make sure the card can receive events
            Image img = GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = true;
            }
            
            Debug.Log($"SimpleDraggable initialized on {gameObject.name}");
        }
        
        public void StartDragging()
        {
            isDragging = true;
            canvasGroup.blocksRaycasts = false;
            
            // Bring card to front
            transform.SetAsLastSibling();
            
            Debug.Log("Card started dragging");
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log($"Card CLICKED! Position: {eventData.position}");
            
            // Calculate offset from card center to click position
            Vector2 clickPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out clickPos
            );
            
            offset = rectTransform.anchoredPosition - clickPos;
            
            // Bring to front when clicked
            transform.SetAsLastSibling();
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log("Card pointer UP");
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("OnBeginDrag called!");
            isDragging = true;
            canvasGroup.blocksRaycasts = false;
            transform.SetAsLastSibling();
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) 
            {
                Debug.LogWarning("OnDrag called but isDragging is false!");
                return;
            }
            
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out mousePos
            );
            
            // Apply offset so card doesn't jump
            rectTransform.anchoredPosition = mousePos + offset;
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log($"OnEndDrag called! Card dropped at: {rectTransform.anchoredPosition}");
            isDragging = false;
            canvasGroup.blocksRaycasts = true;
        }
    }
}