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

        [Header("References")]
        public SpriteRenderer cardRenderer;
        public GameObject overlay;

        private bool glowable = false;
        private bool glowActive = false;
        private SpriteRenderer shadowRenderer;
        private SpriteRenderer overlayRenderer;
        private GameObject overlayX2Text;

        // Glow pulse parameters (match the original animation curves)
        private const float GlowLoopDuration = 1f;
        private const float OverlayPulseAmplitude = 0.015f;
        private const float TextPulseAmplitude = 0.15f;

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

            cardRenderer.sortingLayerName = "Cards";
            cardRenderer.sortingOrder = 0;

            // Cache shadow renderer for visual updates
            Transform shadowTransform = transform.Find("Shadow");
            if (shadowTransform != null)
                shadowRenderer = shadowTransform.GetComponent<SpriteRenderer>();

            ConfigureValueTextSorting();
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

                // Disable the prefab Animator — scale is now driven by code
                Animator overlayAnimator = overlay.GetComponent<Animator>();
                if (overlayAnimator != null) overlayAnimator.enabled = false;

                glowable = true;

                // Keep overlay GameObject active (Animator-free), hide visuals
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
        /// Ensures TMP value text renderers are on the same sorting layer as the card,
        /// one order above so they render on top. Needed for both prefab-wired and
        /// programmatically created TMP objects.
        /// </summary>
        private void ConfigureValueTextSorting()
        {
            int textOrder = cardRenderer.sortingOrder + 1;
            SetTmpSorting(topValueText, "Cards", textOrder);
            SetTmpSorting(bottomValueText, "Cards", textOrder);
        }

        private static void SetTmpSorting(TextMeshPro tmp, string layerName, int order)
        {
            if (tmp == null) return;
            MeshRenderer mr = tmp.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sortingLayerName = layerName;
                mr.sortingOrder = order;
            }
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

            if (topValueText != null)
                topValueText.transform.localScale = Vector3.one;
            if (bottomValueText != null)
                bottomValueText.transform.localScale = Vector3.one;
        }

        void LateUpdate()
        {
            if (!glowActive) return;

            // Sinusoidal pulse synced to global Time.time
            float t = (Time.time % GlowLoopDuration) / GlowLoopDuration;
            float pulse = Mathf.Sin(t * Mathf.PI * 2f) * 0.5f + 0.5f; // 0→1→0 over the cycle

            // Overlay border scale
            if (overlay != null)
            {
                float overlayScale = 1f + OverlayPulseAmplitude * pulse;
                overlay.transform.localScale = new Vector3(
                    overlayBaseScale.x * overlayScale,
                    overlayBaseScale.y * overlayScale,
                    overlayBaseScale.z);
            }

            // Text scale
            float textScale = 1f + TextPulseAmplitude * pulse;
            Vector3 ts = new Vector3(textScale, textScale, 1f);
            if (topValueText != null)
                topValueText.transform.localScale = ts;
            if (bottomValueText != null)
                bottomValueText.transform.localScale = ts;
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
