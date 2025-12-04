using UnityEngine;
using UnityEngine.UI;
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
        public AudioManager audioManager;
        private Suits goalSuit;
        private bool suitGoalComplete = false;

        public void SetSuitGoal(Suits suit)
        {
            goalSuit = suit;
        }
        
        public void UpdateScore(Score score)
        {
            Suits? dominantSuit = score.GetDominantSuit();
            if (dominantSuit == goalSuit && goalSuit != null)
            {
                if (suitGoalComplete == false)
                {
                    audioManager.ActivateSuitSound(goalSuit);
                    suitGoalComplete = true;
                }
            }
            else
            {
                suitGoalComplete = false;
            }

            DisplayScore(score);
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