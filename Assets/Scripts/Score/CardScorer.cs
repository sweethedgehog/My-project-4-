using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CardGame.Core;
using CardGame.Cards;
using CardGame.GameObjects;
using TMPro;

namespace CardGame.Scoring
{
    /// <summary>
    /// Displays score for cards on a CardBoard
    /// Automatically updates when cards are added/removed/reordered
    /// Must be a child of CardBoard
    /// </summary>
    public class CardScorer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI totalScoreText;
        [SerializeField] private Text redScoreText;
        [SerializeField] private Text yellowScoreText;
        [SerializeField] private Text blueScoreText;
        [SerializeField] private Text greenScoreText;
        [SerializeField] private TextMeshProUGUI dominantSuitText;
        
        [Header("Display Format")]
        [SerializeField] private string totalScoreFormat = "Total: {0}";
        [SerializeField] private string suitScoreFormat = "{0}: {1}";
        [SerializeField] private string dominantFormat = "Dominant: {0}";
        [SerializeField] private bool showBreakdown = true;
        
        private CardBoard parentBoard;
        private CardLayout cardLayout;
        
        void Awake()
        {
            // Get parent CardBoard
            parentBoard = GetComponentInParent<CardBoard>();
            if (parentBoard == null)
            {
                Debug.LogError("CardScorer must be a child of CardBoard!");
            }
            
            cardLayout = new CardLayout();
        }
        
        void Start()
        {
            // Initial score calculation
            UpdateScore();
        }
        
        void Update()
        {
            // Update score every frame (you can optimize this with events if needed)
            UpdateScore();
        }
        
        /// <summary>
        /// Main function that calculates and displays score
        /// This is your "function_1"
        /// </summary>
        public void UpdateScore()
        {
            if (parentBoard == null) return;
            
            // Get current cards from board
            List<SimpleCard> boardCards = parentBoard.GetCards();
            
            // Convert SimpleCards to Card objects for scoring
            cardLayout.Clear();
            foreach (SimpleCard simpleCard in boardCards)
            {
                Card card = new Card(simpleCard.GetSuit(), simpleCard.GetValue());
                cardLayout.AddCard(card);
            }
            
            // Calculate score using CardLayout
            Score score = cardLayout.GetScore();
            
            // Display results
            DisplayScore(score);
        }
        
        /// <summary>
        /// Display score on UI
        /// </summary>
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
                if (redScoreText != null)
                {
                    int redScore = score.GetSuitScore(Suits.Roses);
                    redScoreText.text = string.Format(suitScoreFormat, "Red", redScore);
                    redScoreText.color = GetSuitColor(Suits.Roses);
                }
                
                if (yellowScoreText != null)
                {
                    int yellowScore = score.GetSuitScore(Suits.Skulls);
                    yellowScoreText.text = string.Format(suitScoreFormat, "Yellow", yellowScore);
                    yellowScoreText.color = GetSuitColor(Suits.Skulls);
                }
                
                if (blueScoreText != null)
                {
                    int blueScore = score.GetSuitScore(Suits.Coins);
                    blueScoreText.text = string.Format(suitScoreFormat, "Blue", blueScore);
                    blueScoreText.color = GetSuitColor(Suits.Coins);
                }
                
                if (greenScoreText != null)
                {
                    int greenScore = score.GetSuitScore(Suits.Crowns);
                    greenScoreText.text = string.Format(suitScoreFormat, "Green", greenScore);
                    greenScoreText.color = GetSuitColor(Suits.Crowns);
                }
            }
            
            // Dominant suit
            if (dominantSuitText != null)
            {
                Suits? dominant = score.GetDominantSuit();
                if (dominant.HasValue)
                {
                    dominantSuitText.text = string.Format(dominantFormat, dominant.Value);
                    dominantSuitText.color = GetSuitColor(dominant.Value);
                }
                else
                {
                    dominantSuitText.text = string.Format(dominantFormat, "Tie");
                    dominantSuitText.color = Color.white;
                }
            }
        }
        
        /// <summary>
        /// Get color for a suit
        /// </summary>
        private Color GetSuitColor(Suits suit)
        {
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
        
        /// <summary>
        /// Force an immediate score update (call this if needed)
        /// </summary>
        public void ForceUpdate()
        {
            UpdateScore();
        }
    }
}