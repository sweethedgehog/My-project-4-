using UnityEngine;
using UnityEngine.EventSystems;
using CardGame.Managers;

namespace CardGame.UI
{
    /// <summary>
    /// Specialized sound component for cards
    /// Add to card GameObjects for card-specific sounds
    /// </summary>
    public class CardSound : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [Header("Card Sounds")]
        [SerializeField] private AudioClip cardHoverEnterSound;  // Plays when entering
        [SerializeField] private AudioClip cardHoverLoopSound;   // Loops while hovering
        [SerializeField] private AudioClip cardClickSound;
        [SerializeField] private AudioClip cardPickupSound;
        [SerializeField] private AudioClip cardDragLoopSound;    // Loops while dragging
        [SerializeField] private AudioClip cardDropSound;
        
        [Header("Volume")]
        [SerializeField] [Range(0f, 1f)] private float clickVolume = 0.7f;
        [SerializeField] [Range(0f, 1f)] private float hoverEnterVolume = 0.5f;
        [SerializeField] [Range(0f, 1f)] private float hoverLoopVolume = 0.3f;
        [SerializeField] [Range(0f, 1f)] private float dragVolume = 0.6f;
        [SerializeField] [Range(0f, 1f)] private float dropVolume = 0.7f;
        
        private AudioSource clickAudioSource;
        private AudioSource hoverLoopSource;
        private AudioSource dragLoopSource;
        private bool isHovering = false;
        private bool isDragging = false;
        
        void Awake()
        {
            // Create audio source for clicks and one-shots
            clickAudioSource = gameObject.AddComponent<AudioSource>();
            clickAudioSource.playOnAwake = false;
            clickAudioSource.loop = false;
            clickAudioSource.spatialBlend = 0f;
            
            // Create dedicated audio source for hover loop
            hoverLoopSource = gameObject.AddComponent<AudioSource>();
            hoverLoopSource.playOnAwake = false;
            hoverLoopSource.loop = true;
            hoverLoopSource.spatialBlend = 0f;
            
            // Create dedicated audio source for drag loop
            dragLoopSource = gameObject.AddComponent<AudioSource>();
            dragLoopSource.playOnAwake = false;
            dragLoopSource.loop = true;
            dragLoopSource.spatialBlend = 0f;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            // Don't play hover sounds if we're currently dragging
            if (isDragging) return;

            isHovering = true;

            // Play enter sound (one-shot)
            if (cardHoverEnterSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCardHover(cardHoverEnterSound);
            }

            // Start hover loop (uses local source but respects SFX volume)
            if (cardHoverLoopSound != null)
            {
                hoverLoopSource.clip = cardHoverLoopSound;
                hoverLoopSource.volume = hoverLoopVolume * (AudioManager.Instance != null ? AudioManager.Instance.GetCardHoverVolume() : 1f);
                hoverLoopSource.Play();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;

            // Stop hover loop immediately
            if (hoverLoopSource.isPlaying)
            {
                hoverLoopSource.Stop();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (cardClickSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCardClick(cardClickSound);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;

            // Stop hover loop when starting to drag
            if (hoverLoopSource.isPlaying)
            {
                hoverLoopSource.Stop();
            }

            // Play pickup sound
            if (cardPickupSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(cardPickupSound);
            }

            // Start drag loop (uses local source but respects SFX volume)
            if (cardDragLoopSound != null)
            {
                dragLoopSource.clip = cardDragLoopSound;
                dragLoopSource.volume = dragVolume * (AudioManager.Instance != null ? AudioManager.Instance.GetSFXVolume() : 1f);
                dragLoopSource.Play();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Keep drag loop playing (handled by loop flag)
            // This event is here for completeness
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;

            // Stop drag loop
            if (dragLoopSource.isPlaying)
            {
                dragLoopSource.Stop();
            }

            // Play drop sound
            if (cardDropSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(cardDropSound);
            }
        }
        
        void OnDisable()
        {
            // Stop all loops if object is disabled
            if (hoverLoopSource != null && hoverLoopSource.isPlaying)
            {
                hoverLoopSource.Stop();
            }
            
            if (dragLoopSource != null && dragLoopSource.isPlaying)
            {
                dragLoopSource.Stop();
            }
            
            isDragging = false;
            isHovering = false;
        }
    }
}