using UnityEngine;
using UnityEngine.Rendering;
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

        [Header("References")]
        public SpriteRenderer cardRenderer;
        public GameObject overlay;

        private bool glowable = false;
        private bool glowActive = false;
        private SpriteRenderer shadowRenderer;
        private SpriteRenderer overlayRenderer;
        private GameObject overlayX2Text;
        private SortingGroup sortingGroup;

        [Header("Glow Pulse")]
        [SerializeField] private float glowLoopDuration = 1f;
        [SerializeField] private float overlayPulseAmplitude = 0.015f;
        [SerializeField] private float x2TextPulseAmplitude = 0.15f;

        // Base scale of the overlay from the prefab
        private Vector3 overlayBaseScale;

        void Awake()
        {
            if (cardRenderer == null)
                cardRenderer = GetComponent<SpriteRenderer>();

            if (cardRenderer == null)
            {
                cardRenderer = gameObject.AddComponent<SpriteRenderer>();
                Debug.LogWarning($"SimpleCard '{name}': SpriteRenderer was missing, added one. Add SpriteRenderer to prefab to fix this.");
            }

            // SortingGroup makes all child renderers sort as one unit
            sortingGroup = GetComponent<SortingGroup>();
            if (sortingGroup == null)
                sortingGroup = gameObject.AddComponent<SortingGroup>();
            sortingGroup.sortingLayerName = "Cards";
            sortingGroup.sortingOrder = 0;

            // Cache shadow renderer for visual updates
            Transform shadowTransform = transform.Find("Shadow");
            if (shadowTransform != null)
                shadowRenderer = shadowTransform.GetComponent<SpriteRenderer>();
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
                overlayRenderer = overlay.GetComponent<SpriteRenderer>();
                overlayBaseScale = overlay.transform.localScale;

                Transform x2 = overlay.transform.Find("x2_text");
                if (x2 != null) overlayX2Text = x2.gameObject;

                glowable = true;

                // Keep overlay GameObject active, hide visuals
                overlay.SetActive(true);
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
        /// Sets sorting order for the entire card (SortingGroup sorts all children as one unit).
        /// </summary>
        public void SetSortingOrder(int order)
        {
            if (sortingGroup != null)
                sortingGroup.sortingOrder = order;
        }

        public int GetSortingOrder()
        {
            return sortingGroup != null ? sortingGroup.sortingOrder : 0;
        }

        public void TurnOffGlow()
        {
            glowActive = false;
            SetGlowVisible(false);
            ResetGlowScales();
        }

        public void TurnOnGlow()
        {
            glowActive = true;
            SetGlowVisible(true);
        }

        private void SetGlowVisible(bool visible)
        {
            if (!glowable) return;

            if (overlayRenderer != null)
                overlayRenderer.enabled = visible;

            if (overlayX2Text != null)
                overlayX2Text.SetActive(visible);
        }

        private void ResetGlowScales()
        {
            if (overlay != null)
                overlay.transform.localScale = overlayBaseScale;
            if (overlayX2Text != null)
                overlayX2Text.transform.localScale = Vector3.one;
        }

        void LateUpdate()
        {
            if (!glowActive) return;

            // Sinusoidal pulse synced to global Time.time
            float t = (Time.time % glowLoopDuration) / glowLoopDuration;
            float pulse = Mathf.Sin(t * Mathf.PI * 2f) * 0.5f + 0.5f; // 0→1→0 over the cycle

            // Overlay border scale
            if (overlay != null)
            {
                float overlayScale = 1f + overlayPulseAmplitude * pulse;
                overlay.transform.localScale = new Vector3(
                    overlayBaseScale.x * overlayScale,
                    overlayBaseScale.y * overlayScale,
                    overlayBaseScale.z);

                // x2 text: own pulse at bigger amplitude, compensate for parent scale
                if (overlayX2Text != null)
                {
                    float x2Scale = (1f + x2TextPulseAmplitude * pulse) / overlayScale;
                    overlayX2Text.transform.localScale = new Vector3(x2Scale, x2Scale, 1f);
                }
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

        }

        public bool IsIndividuallyFrozen() => individualFreeze;
    }
}
