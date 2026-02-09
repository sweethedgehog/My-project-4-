using UnityEngine;
using UnityEngine.Audio;

namespace CardGame.Managers
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [Header("Mixer")]
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;

        [Header("Music Clips")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private AudioClip victoryMusic;

        private AudioSource musicSource;
        private AudioSource sfxSource;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SetupAudioSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void SetupAudioSources()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.outputAudioMixerGroup = musicGroup;
            musicSource.loop = true;
            musicSource.playOnAwake = false;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.outputAudioMixerGroup = sfxGroup;
            sfxSource.playOnAwake = false;
        }

        // ===== Music =====

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null || musicSource.clip == clip) return;
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void PlayMenuMusic() => PlayMusic(menuMusic);
        public void PlayGameplayMusic() => PlayMusic(gameplayMusic);
        public void PlayVictoryMusic() => PlayMusic(victoryMusic);
        public void StopMusic() => musicSource.Stop();
        public void PauseMusic() => musicSource.Pause();
        public void ResumeMusic() => musicSource.UnPause();
        public bool IsMusicPlaying() => musicSource.isPlaying;

        // ===== SFX =====

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip);
        }

        // ===== Volume (via AudioMixer) =====

        public void SetMusicVolume(float normalized)
        {
            float dB = normalized > 0.001f ? Mathf.Log10(normalized) * 20f : -80f;
            mainMixer.SetFloat("MusicVolume", dB);
        }

        public void SetSFXVolume(float normalized)
        {
            float dB = normalized > 0.001f ? Mathf.Log10(normalized) * 20f : -80f;
            mainMixer.SetFloat("SFXVolume", dB);
        }

        public float GetMusicVolume()
        {
            mainMixer.GetFloat("MusicVolume", out float dB);
            return dB > -79f ? Mathf.Pow(10f, dB / 20f) : 0f;
        }

        public float GetSFXVolume()
        {
            mainMixer.GetFloat("SFXVolume", out float dB);
            return dB > -79f ? Mathf.Pow(10f, dB / 20f) : 0f;
        }
    }
}
