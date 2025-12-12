using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardGame.UI
{
    /// <summary>
    /// Add this component to any UI element to give it sounds
    /// Automatically plays sounds on hover, click, etc.
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public class UISound : MonoBehaviour, 
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler,
        IPointerDownHandler
    {
        [Header("Sound Settings")]
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip hoverEnterSound;  // Plays when entering
        [SerializeField] private AudioClip hoverLoopSound;   // Loops while hovering
        
        [Header("Volume")]
        [SerializeField] [Range(0f, 1f)] private float clickVolume = 0.8f;
        [SerializeField] [Range(0f, 1f)] private float hoverEnterVolume = 0.5f;
        [SerializeField] [Range(0f, 1f)] private float hoverLoopVolume = 0.3f;
        
        private AudioSource clickAudioSource;
        private AudioSource hoverLoopSource;
        private bool isHovering = false;
        
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
            hoverLoopSource.loop = true;  // This one loops!
            hoverLoopSource.spatialBlend = 0f;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovering = true;
            
            // Play enter sound (one-shot)
            if (hoverEnterSound != null)
            {
                clickAudioSource.PlayOneShot(hoverEnterSound, hoverEnterVolume);
            }
            
            // Start hover loop
            if (hoverLoopSound != null)
            {
                hoverLoopSource.clip = hoverLoopSound;
                hoverLoopSource.volume = hoverLoopVolume;
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
            if (clickSound != null)
            {
                clickAudioSource.PlayOneShot(clickSound, clickVolume);
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            // Optional: different sound on press
        }
        
        void OnDisable()
        {
            // Stop hover loop if object is disabled
            if (hoverLoopSource != null && hoverLoopSource.isPlaying)
            {
                hoverLoopSource.Stop();
            }
        }
    }
}