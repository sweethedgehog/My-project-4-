using CardGame.Core;
using UnityEngine;

namespace CardGame.UI
{
    public class MirrorDisplay : MonoBehaviour
    {
        [Header("Suit Sprites")]
        [SerializeField] private Sprite rosesSprite;
        [SerializeField] private Sprite crownsSprite;
        [SerializeField] private Sprite skullsSprite;
        [SerializeField] private Sprite coinsSprite;
        [SerializeField] private Sprite defaultSprite;

        [Header("References")]
        [SerializeField] private SpriteRenderer colorRenderer;
        [SerializeField] private Animator maskAnimator;
        [SerializeField] private Animator flareAnimator;

        private SpriteRenderer previousRenderer;
        private Suits? currentSuit;

        private void Awake()
        {
            if (colorRenderer != null)
            {
                colorRenderer.enabled = false;
                CreatePreviousRenderer();
            }
        }

        private void CreatePreviousRenderer()
        {
            var go = new GameObject("mirror_previous");
            var t = go.transform;
            t.SetParent(colorRenderer.transform.parent);
            t.localPosition = colorRenderer.transform.localPosition;
            t.localScale = colorRenderer.transform.localScale;
            t.localRotation = colorRenderer.transform.localRotation;

            previousRenderer = go.AddComponent<SpriteRenderer>();
            previousRenderer.material = new Material(colorRenderer.sharedMaterial);
            previousRenderer.sortingLayerID = colorRenderer.sortingLayerID;
            previousRenderer.sortingOrder = colorRenderer.sortingOrder;
            previousRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            previousRenderer.enabled = false;
        }

        public void SetSuit(Suits? suit)
        {
            if (suit == currentSuit) return;

            if (colorRenderer == null) return;

            // Show old color on the outside-mask layer
            if (previousRenderer != null)
            {
                previousRenderer.sprite = currentSuit.HasValue
                    ? GetSuitSprite(currentSuit.Value)
                    : defaultSprite;
                previousRenderer.enabled = true;
            }

            // Set new color on the inside-mask layer
            colorRenderer.sprite = suit.HasValue
                ? GetSuitSprite(suit.Value)
                : defaultSprite;
            colorRenderer.enabled = true;

            currentSuit = suit;

            PlayAnimations();
        }

        private void PlayAnimations()
        {
            if (maskAnimator != null)
                maskAnimator.Play("Mirror_mask", 0, 0f);

            if (flareAnimator != null)
                flareAnimator.Play("Mirror_flare", 0, 0f);
        }

        public void ResetMirror()
        {
            currentSuit = null;

            if (colorRenderer != null)
                colorRenderer.enabled = false;

            if (previousRenderer != null)
                previousRenderer.enabled = false;
        }

        private Sprite GetSuitSprite(Suits suit)
        {
            switch (suit)
            {
                case Suits.Roses: return rosesSprite;
                case Suits.Crowns: return crownsSprite;
                case Suits.Skulls: return skullsSprite;
                case Suits.Coins: return coinsSprite;
                default: return defaultSprite;
            }
        }
    }
}
