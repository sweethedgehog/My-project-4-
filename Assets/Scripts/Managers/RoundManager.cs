using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using CardGame.Core;
using CardGame.GameObjects;
using CardGame.Scoring;

namespace CardGame.Managers
{
    /// <summary>
    /// Manages game rounds - drawing goal cards and dealing cards to board
    /// </summary>
    public class RoundManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SimpleDeckObject deck;
        [SerializeField] private CardBoard handBoard;
        [SerializeField] private CardBoard targetBoard;
        [SerializeField] private Button startRoundButton;
        
        [Header("Goal Display")]
        [SerializeField] private TextMeshProUGUI goalValueText;
        [SerializeField] private TextMeshProUGUI goalSuitText;
        [SerializeField] private Image goalCardImage; // Optional: visual card display
        
        [Header("Round Settings")]
        [SerializeField] private int minGoalValue = 8;
        [SerializeField] private int maxGoalValue = 14;
        [SerializeField] private int cardsPerRound = 5;
        [SerializeField] private float dealDelay = 0.3f; // Delay between dealing cards
        
        [Header("Round Info")]
        [SerializeField] private TextMeshProUGUI roundNumberText; // Optional: show current round
        [SerializeField] private TextMeshProUGUI scoreHistoryText; // Shows score list: "1, 0.5, 1"
        
        [Header("End Round")]
        [SerializeField] private Button endRoundButton;
        [SerializeField] private TextMeshProUGUI resultText; // Shows round result
        [SerializeField] private float resultDisplayTime = 2f; // How long to show result
        
        [Header("Game Rules")]
        [SerializeField] private int maxRounds = 6;
        [SerializeField] private int maxSameSuitOccurrences = 2;
        
        private int currentRound = 0;
        private List<float> scoreHistory = new List<float>(); // List of scores per round
        private Dictionary<Suits, int> suitUsageCount = new Dictionary<Suits, int>(); // Track suit usage
        private int currentGoalValue;
        private Suits currentGoalSuit;
        private bool isDealing = false;
        private bool roundActive = false; // Track if round is in progress
        
        void Start()
        {
            // Setup buttons
            if (startRoundButton != null)
            {
                startRoundButton.onClick.AddListener(StartRound);
            }
            
            if (endRoundButton != null)
            {
                endRoundButton.onClick.AddListener(EndRound);
                endRoundButton.interactable = false; // Disabled until round starts
            }
            
            // Initialize suit usage counter
            foreach (Suits suit in System.Enum.GetValues(typeof(Suits)))
            {
                suitUsageCount[suit] = 0;
            }
            
            UpdateRoundDisplay();
            UpdateScoreHistoryDisplay();
            
            if (resultText != null)
            {
                resultText.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Start a new round
        /// </summary>
        public void StartRound()
        {
            if (isDealing)
            {
                Debug.Log("Already dealing cards!");
                return;
            }
            
            if (roundActive)
            {
                Debug.Log("Round already in progress! End current round first.");
                return;
            }
            
            if (currentRound >= maxRounds)
            {
                Debug.Log("Game Over! Maximum rounds reached.");
                if (startRoundButton != null)
                {
                    startRoundButton.interactable = false;
                }
                ShowGameOver();
                return;
            }
            
            if (deck == null || targetBoard == null || handBoard == null)
            {
                Debug.LogError("Deck or Board not assigned!");
                return;
            }
            
            currentRound++;
            roundActive = true;
            
            // Generate random goal
            GenerateGoal();
            
            // Deal cards to board
            StartCoroutine(DealCardsToBoard());
            
            UpdateRoundDisplay();
            
            // Enable end round button
            if (endRoundButton != null)
            {
                endRoundButton.interactable = true;
            }
        }
        
        /// <summary>
        /// End the current round and calculate score
        /// </summary>
        public void EndRound()
        {
            if (!roundActive)
            {
                Debug.Log("No active round to end!");
                return;
            }
            
            // Check if at least one card is on the board
            if (targetBoard.CardCount == 0)
            {
                Debug.Log("Cannot end round - at least one card required on board!");
                if (resultText != null)
                {
                    StartCoroutine(ShowTemporaryMessage("Need at least 1 card!", Color.red));
                }
                return;
            }
            
            // Get the scorer from the board
            CardScorer scorer = targetBoard.GetComponentInChildren<CardScorer>();
            
            if (scorer == null)
            {
                Debug.LogError("No CardScorer found on board!");
                return;
            }
            
            // Calculate round score
            float roundScore = CalculateRoundScore(scorer);
            scoreHistory.Add(roundScore);
            
            // Show result
            StartCoroutine(ShowRoundResult(roundScore));
            
            // Clear the board
            ClearBoard();
            
            // Mark round as inactive
            roundActive = false;
            
            // Disable end round button
            if (endRoundButton != null)
            {
                endRoundButton.interactable = false;
            }
            
            UpdateScoreHistoryDisplay();
            
            // Check if game is over
            if (currentRound >= maxRounds)
            {
                if (startRoundButton != null)
                {
                    startRoundButton.interactable = false;
                }
            }
        }
        
        /// <summary>
        /// Generate random goal value and suit
        /// Ensures no suit appears more than maxSameSuitOccurrences times
        /// </summary>
        private void GenerateGoal()
        {
            // Random value between min and max (inclusive)
            currentGoalValue = Random.Range(minGoalValue, maxGoalValue + 1);
            
            // Get available suits (those that haven't reached the limit)
            List<Suits> availableSuits = new List<Suits>();
            foreach (Suits suit in System.Enum.GetValues(typeof(Suits)))
            {
                if (suitUsageCount[suit] < maxSameSuitOccurrences)
                {
                    availableSuits.Add(suit);
                }
            }
            
            if (availableSuits.Count == 0)
            {
                Debug.LogWarning("All suits have reached max usage! Resetting suit counters.");
                // Reset if somehow all suits are exhausted (shouldn't happen with proper limits)
                foreach (Suits suit in System.Enum.GetValues(typeof(Suits)))
                {
                    suitUsageCount[suit] = 0;
                    availableSuits.Add(suit);
                }
            }
            
            // Random suit from available ones
            currentGoalSuit = availableSuits[Random.Range(0, availableSuits.Count)];
            suitUsageCount[currentGoalSuit]++;
            
            Debug.Log($"Round {currentRound} Goal: {currentGoalSuit} - Value {currentGoalValue} (Suit usage: {suitUsageCount[currentGoalSuit]}/{maxSameSuitOccurrences})");
            
            // Update display
            UpdateGoalDisplay();
        }
        
        /// <summary>
        /// Deal cards from deck to board
        /// </summary>
        private IEnumerator DealCardsToBoard()
        {
            isDealing = true;
            
            // Disable button during dealing
            if (startRoundButton != null)
            {
                startRoundButton.interactable = false;
            }
            
            // Calculate how many cards to deal
            int currentCards = handBoard.CardCount;
            int cardsToDeal = Mathf.Max(0, cardsPerRound - currentCards);
            
            Debug.Log($"Board has {currentCards} cards. Dealing {cardsToDeal} more cards.");
            
            // Deal cards one by one with delay
            for (int i = 0; i < cardsToDeal; i++)
            {
                if (deck.IsDeckEmpty())
                {
                    Debug.Log("Deck is empty! Cannot deal more cards.");
                    break;
                }
                
                // Trigger deck click to draw card
                UnityEngine.EventSystems.PointerEventData pointerData = 
                    new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                deck.OnPointerClick(pointerData);
                
                // Wait before dealing next card
                if (i < cardsToDeal - 1) // Don't wait after last card
                {
                    yield return new WaitForSeconds(dealDelay);
                }
            }
            
            isDealing = false;
            
            // Re-enable button
            if (startRoundButton != null)
            {
                startRoundButton.interactable = true;
            }
            
            Debug.Log($"Round {currentRound} started! Board now has {targetBoard.CardCount} cards.");
        }
        
        /// <summary>
        /// Update the goal display UI
        /// </summary>
        private void UpdateGoalDisplay()
        {
            // Update value text
            if (goalValueText != null)
            {
                goalValueText.text = currentGoalValue.ToString();
            }
            
            // Update suit text
            if (goalSuitText != null)
            {
                goalSuitText.text = currentGoalSuit.ToString();
                goalSuitText.color = GetSuitColor(currentGoalSuit);
            }
            
            // Update goal card image (if using visual card)
            if (goalCardImage != null)
            {
                goalCardImage.color = GetSuitColor(currentGoalSuit);
            }
        }
        
        /// <summary>
        /// Calculate score based on goal matching
        /// Returns 0, 0.5, or 1.0
        /// </summary>
        private float CalculateRoundScore(CardScorer scorer)
        {
            // Need to get the score from the scorer
            // We'll need to expose a method to get the current score
            Score currentScore = GetScoreFromScorer(scorer);
            
            int achievedScore = currentScore.GetFullScore();
            Suits? dominantSuit = currentScore.GetDominantSuit();
            
            Debug.Log($"Goal: {currentGoalSuit} {currentGoalValue} | Achieved: {(dominantSuit.HasValue ? dominantSuit.Value.ToString() : "None")} {achievedScore}");
            
            // Check if score matches goal
            bool scoreMatches = (achievedScore == currentGoalValue);
            bool suitMatches = dominantSuit.HasValue && (dominantSuit.Value == currentGoalSuit);
            
            if (!scoreMatches)
            {
                // Score doesn't match goal
                Debug.Log("Round Result: MISS - Score doesn't match goal (0 points)");
                return 0f;
            }
            else if (scoreMatches && !suitMatches)
            {
                // Score matches but suit doesn't
                Debug.Log("Round Result: PARTIAL - Score matches but wrong suit (0.5 points)");
                return 0.5f;
            }
            else
            {
                // Perfect match!
                Debug.Log("Round Result: PERFECT - Exact match! (1 point)");
                return 1f;
            }
        }
        
        /// <summary>
        /// Get Score object from CardScorer
        /// This is a helper that uses reflection to get the private score
        /// </summary>
        private Score GetScoreFromScorer(CardScorer scorer)
        {
            // We need to calculate the score by getting cards from board
            var cards = targetBoard.GetCards();
            
            CardLayout layout = new CardLayout();
            foreach (var simpleCard in cards)
            {
                Card card = new Card(simpleCard.GetSuit(), simpleCard.GetValue());
                layout.AddCard(card);
            }
            
            return layout.GetScore();
        }
        
        /// <summary>
        /// Show round result to player
        /// </summary>
        private IEnumerator ShowRoundResult(float roundScore)
        {
            if (resultText != null)
            {
                resultText.gameObject.SetActive(true);
                
                string resultMessage = "";
                Color resultColor = Color.white;
                
                if (roundScore == 0f)
                {
                    resultMessage = "MISS!\nScore doesn't match\n+0 points";
                    resultColor = new Color(1f, 0.3f, 0.3f); // Red
                }
                else if (roundScore == 0.5f)
                {
                    resultMessage = "PARTIAL!\nRight score, wrong suit\n+0.5 points";
                    resultColor = new Color(1f, 0.8f, 0.2f); // Orange/Yellow
                }
                else
                {
                    resultMessage = "PERFECT!\nExact match!\n+1 point";
                    resultColor = new Color(0.3f, 1f, 0.3f); // Green
                }
                
                resultText.text = resultMessage;
                resultText.color = resultColor;
                
                yield return new WaitForSeconds(resultDisplayTime);
                
                resultText.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Clear all cards from the board
        /// </summary>
        private void ClearBoard()
        {
            var cards = targetBoard.GetCards();
            
            // Destroy all card GameObjects
            foreach (var card in cards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            
            // Clear the board's internal list
            targetBoard.ClearBoard();
            
            Debug.Log("Board cleared for next round");
        }
        
        /// <summary>
        /// Show temporary message
        /// </summary>
        private IEnumerator ShowTemporaryMessage(string message, Color color)
        {
            if (resultText != null)
            {
                resultText.gameObject.SetActive(true);
                resultText.text = message;
                resultText.color = color;
                
                yield return new WaitForSeconds(1f);
                
                resultText.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Show game over screen
        /// </summary>
        private void ShowGameOver()
        {
            float totalScore = 0f;
            foreach (float score in scoreHistory)
            {
                totalScore += score;
            }
            
            if (resultText != null)
            {
                resultText.gameObject.SetActive(true);
                resultText.text = $"GAME OVER!\n\nFinal Score: {totalScore:F1}/{maxRounds}\n\n{GetScoreHistoryString()}";
                resultText.color = Color.white;
                resultText.fontSize = 36;
            }
            
            Debug.Log($"Game Over! Final Score: {totalScore}/{maxRounds}");
        }
        
        /// <summary>
        /// Update total score display
        /// </summary>
        private void UpdateScoreHistoryDisplay()
        {
            if (scoreHistoryText != null)
            {
                scoreHistoryText.text = "Scores: " + GetScoreHistoryString();
            }
        }
        
        /// <summary>
        /// Get score history as a formatted string
        /// </summary>
        private string GetScoreHistoryString()
        {
            if (scoreHistory.Count == 0)
            {
                return "-";
            }
            
            List<string> scoreStrings = new List<string>();
            foreach (float score in scoreHistory)
            {
                if (score == 0f)
                    scoreStrings.Add("0");
                else if (score == 0.5f)
                    scoreStrings.Add("1/2");
                else
                    scoreStrings.Add("1");
            }
            
            return string.Join(", ", scoreStrings);
        }
        
        /// <summary>
        /// Update round number display
        /// </summary>
        private void UpdateRoundDisplay()
        {
            if (roundNumberText != null)
            {
                roundNumberText.text = $"Round: {currentRound}";
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
                    return new Color(0.9f, 0.2f, 0.2f); // Red
                case Suits.Skulls:
                    return new Color(1f, 0.9f, 0.2f); // Yellow
                case Suits.Coins:
                    return new Color(0.2f, 0.5f, 1f); // Blue
                case Suits.Crowns:
                    return new Color(0.2f, 0.8f, 0.3f); // Green
                default:
                    return Color.white;
            }
        }
        
        /// <summary>
        /// Get current goal for other systems to check
        /// </summary>
        public int GetGoalValue() => currentGoalValue;
        public Suits GetGoalSuit() => currentGoalSuit;
        public int GetCurrentRound() => currentRound;
        
        /// <summary>
        /// Reset the game
        /// </summary>
        public void ResetGame()
        {
            currentRound = 0;
            scoreHistory = new List<float> {};
            currentGoalValue = 0;
            roundActive = false;
            
            ClearBoard();
            
            UpdateRoundDisplay();
            UpdateScoreHistoryDisplay();
            
            if (goalValueText != null)
                goalValueText.text = "?";
            if (goalSuitText != null)
                goalSuitText.text = "Press Start";
            if (endRoundButton != null)
                endRoundButton.interactable = false;
            
            Debug.Log("Game reset!");
        }
    }
}