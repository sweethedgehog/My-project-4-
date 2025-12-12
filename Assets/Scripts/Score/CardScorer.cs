using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using CardGame.Core;
using CardGame.Cards;
using CardGame.GameObjects;
using CardGame.Managers;
using TMPro;
using UnityEngine.Serialization;

namespace CardGame.Scoring
{
    public class CardScorer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI totalScoreText;
        [SerializeField] private TextMeshProUGUI roseScoreText;
        [SerializeField] private TextMeshProUGUI crownScoreText;
        [SerializeField] private TextMeshProUGUI skullScoreText;
        [SerializeField] private TextMeshProUGUI coinsScoreText;
        [SerializeField] private TextMeshProUGUI dominantSuitText;
        [SerializeField] private CryLogic crystal;
        
        [Header("Display Format")]
        [SerializeField] private string totalScoreFormat = "Total: {0}";
        [SerializeField] private string suitScoreFormat = "{0}: {1}";
        [SerializeField] private string dominantFormat = "Dominant: {0}";
        [SerializeField] private bool showBreakdown = true;
        
        [Header("Goal Completion Sounds")]
        [SerializeField] private AudioClip valueGoalCompleteSound;
        [SerializeField] private AudioClip roseSuitCompleteSound;
        [SerializeField] private AudioClip crownSuitCompleteSound;
        [SerializeField] private AudioClip skullSuitCompleteSound;
        [SerializeField] private AudioClip coinSuitCompleteSound;
        
        [Header("Sound Settings")]
        [SerializeField] [Range(0f, 1f)] private float soundVolume = 0.7f;
        [SerializeField] private float dualGoalDelay = 0.5f; // Delay between sounds when both goals complete
        
        public AudioManager audioManager;
        private AudioSource audioSource;
        
        private Suits goalSuit;
        private int goalValue;
        
        private bool valueGoalComplete = false;
        private bool suitGoalComplete = false;
        
        void Awake()
        {
            // Create audio source for goal completion sounds
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
        }

        public void SetGoal(Suits _goalSuit, int _goalValue)
        {
            goalSuit = _goalSuit;
            goalValue = _goalValue;
            
            // Reset goal completion flags when new goal is set
            valueGoalComplete = false;
            suitGoalComplete = false;
        }
        
        public void UpdateScore(Score score)
        {
            int currentValue = score.GetFullScore();
            Suits? dominantSuit = score.GetDominantSuit();
            
            // Check if value goal is currently met (EXACT MATCH ONLY)
            bool valueGoalMet = currentValue == goalValue;
            
            // Check if value goal was just completed
            bool valueJustCompleted = !valueGoalComplete && valueGoalMet;
            
            // Check if suit goal was just completed (BUT ONLY IF VALUE IS ALSO MET)
            bool suitJustCompleted = !suitGoalComplete && dominantSuit == goalSuit && goalSuit != null && valueGoalMet;
            
            // Handle sound playback based on what completed
            if (valueJustCompleted && suitJustCompleted)
            {
                // Both goals completed at the same time - play with delay
                StartCoroutine(PlayBothGoalSounds());
                valueGoalComplete = true;
                suitGoalComplete = true;
            }
            else if (valueJustCompleted)
            {
                // Only value goal completed
                PlayValueGoalSound();
                valueGoalComplete = true;
            }
            else if (suitJustCompleted)
            {
                // Only suit goal completed (value was already complete)
                PlaySuitGoalSound(goalSuit);
                suitGoalComplete = true;
            }
            
            // Reset flags if goals are no longer met
            if (!valueGoalMet)
            {
                valueGoalComplete = false;
                suitGoalComplete = false; // Also reset suit if value is not met
            }
            else if (dominantSuit != goalSuit)
            {
                suitGoalComplete = false;
            }

            DisplayScore(score);
        }
        
        /// <summary>
        /// Play sound when value goal is completed
        /// </summary>
        private void PlayValueGoalSound()
        {
            if (valueGoalCompleteSound != null)
            {
                audioSource.PlayOneShot(valueGoalCompleteSound, soundVolume);
                Debug.Log($"Value goal completed! Target: {goalValue}");
            }
        }
        
        /// <summary>
        /// Play sound when suit goal is completed (suit-specific)
        /// </summary>
        private void PlaySuitGoalSound(Suits suit)
        {
            AudioClip suitSound = GetSuitSound(suit);
            
            if (suitSound != null)
            {
                audioSource.PlayOneShot(suitSound, soundVolume);
                Debug.Log($"Suit goal completed! Target suit: {suit}");
            }
            
            // Alternative: Use AudioManager if you prefer
            // if (audioManager != null)
            // {
            //     audioManager.ActivateSuitSound(suit);
            // }
        }
        
        /// <summary>
        /// Get the appropriate sound clip for a suit
        /// </summary>
        private AudioClip GetSuitSound(Suits suit)
        {
            switch (suit)
            {
                case Suits.Roses:
                    return roseSuitCompleteSound;
                case Suits.Crowns:
                    return crownSuitCompleteSound;
                case Suits.Skulls:
                    return skullSuitCompleteSound;
                case Suits.Coins:
                    return coinSuitCompleteSound;
                default:
                    return null;
            }
        }
        
        /// <summary>
        /// Play both goal completion sounds with a delay
        /// </summary>
        private IEnumerator PlayBothGoalSounds()
        {
            // Play value goal sound first
            PlayValueGoalSound();
            
            // Wait for delay
            yield return new WaitForSeconds(dualGoalDelay);
            
            // Play suit goal sound
            PlaySuitGoalSound(goalSuit);
            
            Debug.Log($"Both goals completed! Value: {goalValue}, Suit: {goalSuit}");
        }

        private void DisplayScore(Score score)
        {
            // Total score
            if (totalScoreText != null)
            {
                totalScoreText.text = string.Format(totalScoreFormat, score.GetFullScore());
            }
            
            // Individual suit scores
            if (showBreakdown)
            {
                if (roseScoreText != null)
                {
                    int redScore = score.GetSuitScore(Suits.Roses);
                    roseScoreText.text = score.GetSuitScore(Suits.Roses).ToString();
                }
                
                if (crownScoreText != null)
                {
                    int yellowScore = score.GetSuitScore(Suits.Crowns);
                    crownScoreText.text = score.GetSuitScore(Suits.Crowns).ToString();
                }
                
                if (skullScoreText != null)
                {
                    int blueScore = score.GetSuitScore(Suits.Skulls);
                    skullScoreText.text = score.GetSuitScore(Suits.Skulls).ToString();
                }
                
                if (coinsScoreText != null)
                {
                    int greenScore = score.GetSuitScore(Suits.Coins);
                    coinsScoreText.text = score.GetSuitScore(Suits.Coins).ToString();
                }
            }
            
            if (crystal != null) crystal.setTexture(score.GetDominantSuit());
        }
    }
}