using UnityEngine;
using System.Collections.Generic;

namespace CardGame.Managers
{
    /// <summary>
    /// Manages all audio in the game - music and sound effects
    /// Singleton pattern for easy access from anywhere
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource; // For looping background music
        [SerializeField] private AudioSource sfxSource; // For one-shot sound effects

        [Header("Music Clips")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private AudioClip victoryMusic;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip cardDrawSound;
        [SerializeField] private AudioClip cardPlaceSound;
        [SerializeField] private AudioClip cardFlipSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip winSound;
        [SerializeField] private AudioClip loseSound;
        [SerializeField] private AudioClip scoreSound;

        [Header("Volume Settings")]
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.8f;

        void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Persist across scenes
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Create audio sources if not assigned
            SetupAudioSources();
        }

        void Start()
        {
            // Apply volume settings
            SetMusicVolume(musicVolume);
            SetSFXVolume(sfxVolume);
        }

        /// <summary>
        /// Setup audio sources if not assigned in inspector
        /// </summary>
        private void SetupAudioSources()
        {
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true; // Music loops by default
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.loop = false; // SFX plays once
                sfxSource.playOnAwake = false;
            }
        }

        // ============================================
        // PLAY MUSIC (Looping)
        // ============================================

        /// <summary>
        /// Play music clip in a loop
        /// </summary>
        public void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;

            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        /// <summary>
        /// Play menu music
        /// </summary>
        public void PlayMenuMusic()
        {
            PlayMusic(menuMusic);
        }

        /// <summary>
        /// Play gameplay music
        /// </summary>
        public void PlayGameplayMusic()
        {
            PlayMusic(gameplayMusic);
        }

        /// <summary>
        /// Play victory music
        /// </summary>
        public void PlayVictoryMusic()
        {
            PlayMusic(victoryMusic);
        }

        /// <summary>
        /// Stop music
        /// </summary>
        public void StopMusic()
        {
            musicSource.Stop();
        }

        /// <summary>
        /// Pause music
        /// </summary>
        public void PauseMusic()
        {
            musicSource.Pause();
        }

        /// <summary>
        /// Resume music
        /// </summary>
        public void ResumeMusic()
        {
            musicSource.UnPause();
        }

        // ============================================
        // PLAY SOUND EFFECTS (Once)
        // ============================================

        /// <summary>
        /// Play a sound effect once
        /// </summary>
        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Play sound at specific volume
        /// </summary>
        public void PlaySFX(AudioClip clip, float volume)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, volume);
        }

        // Specific sound effect methods
        public void PlayCardDraw() => PlaySFX(cardDrawSound);
        public void PlayCardPlace() => PlaySFX(cardPlaceSound);
        public void PlayCardFlip() => PlaySFX(cardFlipSound);
        public void PlayButtonClick() => PlaySFX(buttonClickSound);
        public void PlayWin() => PlaySFX(winSound);
        public void PlayLose() => PlaySFX(loseSound);
        public void PlayScore() => PlaySFX(scoreSound);

        // ============================================
        // VOLUME CONTROLS
        // ============================================

        /// <summary>
        /// Set music volume (0 to 1)
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume;
        }

        /// <summary>
        /// Set SFX volume (0 to 1)
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            sfxSource.volume = sfxVolume;
        }

        /// <summary>
        /// Mute/unmute music
        /// </summary>
        public void ToggleMusicMute()
        {
            musicSource.mute = !musicSource.mute;
        }

        /// <summary>
        /// Mute/unmute SFX
        /// </summary>
        public void ToggleSFXMute()
        {
            sfxSource.mute = !sfxSource.mute;
        }

        // ============================================
        // UTILITY
        // ============================================

        /// <summary>
        /// Check if music is playing
        /// </summary>
        public bool IsMusicPlaying()
        {
            return musicSource.isPlaying;
        }

        /// <summary>
        /// Get current music volume
        /// </summary>
        public float GetMusicVolume()
        {
            return musicVolume;
        }

        /// <summary>
        /// Get current SFX volume
        /// </summary>
        public float GetSFXVolume()
        {
            return sfxVolume;
        }
    }
}