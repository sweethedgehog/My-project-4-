using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CardGame.Core;
using CardGame.Cards;
using CardGame.GameObjects;
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
        
        public void UpdateScore(Score score)
        {
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
                    roseScoreText.color = GetSuitColor(Suits.Roses);
                }
                
                if (crownScoreText != null)
                {
                    int yellowScore = score.GetSuitScore(Suits.Crowns);
                    crownScoreText.text = score.GetSuitScore(Suits.Crowns).ToString();
                    crownScoreText.color = GetSuitColor(Suits.Crowns);
                }
                
                if (skullScoreText != null)
                {
                    int blueScore = score.GetSuitScore(Suits.Skulls);
                    skullScoreText.text = score.GetSuitScore(Suits.Skulls).ToString();
                    skullScoreText.color = GetSuitColor(Suits.Skulls);
                }
                
                if (coinsScoreText != null)
                {
                    int greenScore = score.GetSuitScore(Suits.Coins);
                    coinsScoreText.text = score.GetSuitScore(Suits.Coins).ToString();
                    coinsScoreText.color = GetSuitColor(Suits.Coins);
                }
            }
            
            if (crystal != null) crystal.setTexture(score.GetDominantSuit());
        }
        
        private Color GetSuitColor(Suits suit)
        {
            return Color.black;
            switch (suit)
            {
                case Suits.Roses:
                    return new Color(0.9f, 0.2f, 0.2f);
                case Suits.Skulls:
                    return new Color(1f, 0.9f, 0.2f);
                case Suits.Coins:
                    return new Color(0.2f, 0.5f, 1f);
                case Suits.Crowns:
                    return new Color(0.2f, 0.8f, 0.3f);
                default:
                    return Color.white;
            }
        }
    }
}