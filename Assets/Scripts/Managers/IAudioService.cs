using UnityEngine;

namespace CardGame.Managers
{
    public interface IAudioService
    {
        void PlaySFX(AudioClip clip);
        void PlayMusic(AudioClip clip);
        void PlayMenuMusic();
        void PlayGameplayMusic();
        void PlayVictoryMusic();
        void StopMusic();
        void PauseMusic();
        void ResumeMusic();
        bool IsMusicPlaying();
        void SetMusicVolume(float normalized);
        void SetSFXVolume(float normalized);
        float GetMusicVolume();
        float GetSFXVolume();
    }
}
