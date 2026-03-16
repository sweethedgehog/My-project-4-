using UnityEngine;
using CardGame.Core;
using CardGame.Managers;
using CardGame.GameObjects;
using TMPro;

namespace CardGame.Cards
{
    /// <summary>
    /// Simple card that displays a suit sprite and dynamically rendered value number.
    /// Uses SpriteRenderer for world-space rendering, TextMeshPro for value text.
    /// </summary>
    public class SimpleCard : MonoBehaviour
    {
        [Header("Card Properties")]
        public Suits suit;
        public int cardValue;

        [Header("Suit Sprites (one per suit)")]
        public Sprite roseSprite;
        public Sprite skullSprite;
        public Sprite crownSprite;
        public Sprite coinSprite;

        [Header("Value Text")]
        [SerializeField] private TextMeshPro topValueText;
        [SerializeField] private TextMeshPro bottomValueText;
        [SerializeField] private float valueFontSize = 3.5f;
        [SerializeField] private Vector2 topValuePosition = new Vector2(-0.33f, 0.70f);
        [SerializeField] private Vector2 bottomValuePosition = new Vector2(0.33f, -0.70f);

        [Header("References")]
        public SpriteRenderer cardRenderer;
        public GameObject overlay;

        private bool glowable = false;
        private SpriteRenderer shadowRenderer;

        void Awake()
        {
            if (cardRenderer == null)
                cardRenderer = GetComponent<SpriteRenderer>();

            if (cardRenderer == null)
            {
                cardRenderer = gameObject.AddComponent<SpriteRenderer>();
                Debug.LogWarning($"SimpleCard '{name}': SpriteRenderer was missing, added one. Add SpriteRenderer to prefab to fix this.");
            }

            cardRenderer.sortingLayerName = "Cards";
            cardRenderer.sortingOrder = 0;

            // Cache shadow renderer for visual updates
            Transform shadowTransform = transform.Find("Shadow");
            if (shadowTransform != null)
                shadowRenderer = shadowTransform.GetComponent<SpriteRenderer>();

            EnsureValueTexts();
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
            Transform overlayTransform = transform.Find("Overlay");
            if (overlayTransform != null)
            {
                overlay = overlayTransform.gameObject;
                glowable = true;
            }
            TurnOffGlow();
        }

        void UpdateVisual()
        {
            Sprite suitSprite = suit switch
            {
                Suits.Roses => roseSprite,
                Suits.Skulls => skullSprite,
                Suits.Coins => coinSprite,
                Suits.Crowns => crownSprite,
                _ => null
            };

            if (suitSprite == null)
            {
                Debug.LogError($"[SimpleCard] {name}: No sprite assigned for suit {suit}!");
                return;
            }

            cardRenderer.sprite = suitSprite;

            // Update shadow silhouette to match current suit
            if (shadowRenderer != null)
                shadowRenderer.sprite = suitSprite;

            // Set value text on both corners
            string valueStr = cardValue.ToString();
            if (topValueText != null) topValueText.text = valueStr;
            if (bottomValueText != null) bottomValueText.text = valueStr;
        }

        /// <summary>
        /// Creates TextMeshPro children for value display if not already assigned.
        /// Top-left and bottom-right (rotated 180°) like a playing card.
        /// </summary>
        private void EnsureValueTexts()
        {
            if (topValueText == null)
                topValueText = CreateValueText("TopValue", topValuePosition, 0f);

            if (bottomValueText == null)
                bottomValueText = CreateValueText("BottomValue", bottomValuePosition, 180f);
        }

        private TextMeshPro CreateValueText(string objName, Vector2 localPos, float zRotation)
        {
            GameObject textObj = new GameObject(objName);
            textObj.transform.SetParent(transform, false);
            textObj.transform.localPosition = new Vector3(localPos.x, localPos.y, -0.01f);
            textObj.transform.localRotation = Quaternion.Euler(0, 0, zRotation);

            TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
            tmp.fontSize = valueFontSize;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;

            // Match the card's sorting layer, one order above the card sprite
            MeshRenderer meshRenderer = textObj.GetComponent<MeshRenderer>();
            meshRenderer.sortingLayerName = "Cards";
            meshRenderer.sortingOrder = cardRenderer.sortingOrder + 1;

            // Small rect to contain a single digit
            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0.5f, 0.5f);

            return tmp;
        }

        /// <summary>
        /// Sets sorting order for all card visuals (sprite + value text).
        /// Called by SimpleDraggableWithBoard during drag.
        /// </summary>
        public void SetSortingOrder(int order)
        {
            cardRenderer.sortingOrder = order;

            int textOrder = order + 1;
            if (topValueText != null)
                topValueText.GetComponent<MeshRenderer>().sortingOrder = textOrder;
            if (bottomValueText != null)
                bottomValueText.GetComponent<MeshRenderer>().sortingOrder = textOrder;
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
            if (RoundManager.inGameMenu || TutorialManager.inGameMenu) return false;

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

            // Dim value text to match
            Color textColor = frozen ? new Color(0.3f, 0.3f, 0.3f, 1f) : Color.black;
            if (topValueText != null) topValueText.color = textColor;
            if (bottomValueText != null) bottomValueText.color = textColor;
        }

        public bool IsIndividuallyFrozen() => individualFreeze;
    }
}
