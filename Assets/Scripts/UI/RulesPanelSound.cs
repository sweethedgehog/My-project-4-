using UnityEngine;
using CardGame.Managers;

namespace CardGame.UI
{
    /// <summary>
    /// Sound component for the rules panel
    /// Plays sounds when opening and closing the panel
    /// </summary>
    [RequireComponent(typeof(RulesPanel))]
    public class RulesPanelSound : MonoBehaviour
    {
        [Header("Panel Sounds")]
        [SerializeField] private AudioClip panelOpenSound;
        [SerializeField] private AudioClip panelCloseSound;
        [SerializeField] private AudioClip panelSlideLoopSound;  // Optional: loops while sliding
        
        [Header("Volume")]
        [SerializeField] [Range(0f, 1f)] private float openVolume = 0.7f;
        [SerializeField] [Range(0f, 1f)] private float closeVolume = 0.7f;
        [SerializeField] [Range(0f, 1f)] private float slideLoopVolume = 0.3f;
        
        private AudioSource effectAudioSource;
        private AudioSource slideLoopSource;
        private RulesPanel rulesPanel;
        private RulesCords currentState;
        private bool isMoving = false;
        
        void Awake()
        {
            rulesPanel = GetComponent<RulesPanel>();
            
            // Create audio source for one-shot sounds
            effectAudioSource = gameObject.AddComponent<AudioSource>();
            effectAudioSource.playOnAwake = false;
            effectAudioSource.loop = false;
            effectAudioSource.spatialBlend = 0f;
            
            // Create dedicated audio source for slide loop
            slideLoopSource = gameObject.AddComponent<AudioSource>();
            slideLoopSource.playOnAwake = false;
            slideLoopSource.loop = true;
            slideLoopSource.spatialBlend = 0f;
        }
        
        void Start()
        {
            // Subscribe to panel state changes
            currentState = RulesCords.Closed;
        }
        
        void Update()
        {
            // Check if panel is currently moving
            bool wasMoving = isMoving;
            isMoving = rulesPanel.IsMoving;
            
            // Start slide loop when movement begins
            if (isMoving && !wasMoving && panelSlideLoopSound != null)
            {
                slideLoopSource.clip = panelSlideLoopSound;
                slideLoopSource.volume = slideLoopVolume;
                slideLoopSource.Play();
            }
            
            // Stop slide loop when movement ends
            if (!isMoving && wasMoving)
            {
                if (slideLoopSource.isPlaying)
                {
                    slideLoopSource.Stop();
                }
            }
        }
        
        /// <summary>
        /// Call this when opening the panel
        /// </summary>
        public void PlayOpenSound()
        {
            if (panelOpenSound != null && currentState != RulesCords.Open)
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayRulesPanel(panelOpenSound);
                currentState = RulesCords.Open;
            }
        }

        /// <summary>
        /// Call this when closing the panel
        /// </summary>
        public void PlayCloseSound()
        {
            if (panelCloseSound != null && currentState != RulesCords.Closed)
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayRulesPanel(panelCloseSound);
                currentState = RulesCords.Closed;
            }
        }
        
        /// <summary>
        /// Automatically plays the appropriate sound based on target position
        /// </summary>
        public void PlaySoundForState(RulesCords targetState)
        {
            if (targetState == RulesCords.Open)
            {
                PlayOpenSound();
            }
            else
            {
                PlayCloseSound();
            }
        }
        
        void OnDisable()
        {
            // Stop slide loop if object is disabled
            if (slideLoopSource != null && slideLoopSource.isPlaying)
            {
                slideLoopSource.Stop();
            }
        }
    }
}