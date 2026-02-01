using System;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using CardGame.Cards;
using CardGame.Core;
using CardGame.GameObjects;
using CardGame.Scoring;
using CardGame.UI;
using DefaultNamespace.Tiles;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;
using System.Linq;


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
        [SerializeField] private bool onlyPossibleSetsMode = false;
        
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
        [SerializeField] private TextMeshProUGUI scoreHistoryText;
        [SerializeField] private TextMeshProUGUI availabilityText; 
        
        [Header("Cat Animation")]
        [SerializeField] private CatAnimationController catAnimationController;
        [SerializeField] private float catTalkDuration = 3f;

        [Header("End Round")]
        [SerializeField] private Button endRoundButton;
        [SerializeField] private Button rerollSuitButton;
        [SerializeField] private Button rerollCardsButton;
        [SerializeField] private Sprite postdictionSprite;
        [SerializeField] private Sprite startSprite;
        [SerializeField] private Sprite endSprite;
        [SerializeField] private TextMeshProUGUI resultText; // Shows round result
        [SerializeField] private float resultDisplayTime = 2f; // How long to show result
        
        [Header("Game Rules")]
        [SerializeField] private int maxRounds = 6;
        [SerializeField] private int maxSameSuitOccurrences = 2;
        
        public AudioClip cardsShuffle;
        
        private AudioSource audioSource;
        private int currentRound = 0;
        private List<SuccessCodes> scoreHistory = new List<SuccessCodes>(); // List of scores per round
        [SerializeField] private TilesManager tilesManager;
        private Dictionary<Suits, int> suitUsageCount = new Dictionary<Suits, int>(); // Track suit usage
        private int currentGoalValue;
        private Suits currentGoalSuit;
        private bool isDealing = false;
        private bool roundActive = false; // Track if round is in progress
        private bool isRulesOpened = false;
        [SerializeField] private RulesPanel rulesPanel;
        public static bool inGameMenu = false;
        private bool inDealState = true;
        
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            inGameMenu = false;
            Time.timeScale = 1f;
            if (endRoundButton != null)
            {
                endRoundButton.onClick.AddListener(listener);
                // endRoundButton.interactable = false; // Disabled until round starts
            }
            
            rerollSuitButton.onClick.AddListener(RerollSuitGoal);
            rerollCardsButton.onClick.AddListener(RerollCards);
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
            isRulesOpened = false;
        }

        private void listener()
        {
            if (currentRound >= maxRounds && inDealState)
            {
                StartPostdiction();
                return;
            }
            Image buttonImage = endRoundButton.GetComponent<Image>();
            if (inDealState) StartRound();
            else inDealState = !EndRound();
            
            buttonImage.sprite = inDealState ? endSprite : startSprite;
            inDealState = !inDealState;
            if (currentRound >= maxRounds && inDealState)
            {
                buttonImage.sprite = postdictionSprite;
            }
        }
        
        private void Update()
        {
            if (inGameMenu) return;
            if (isRulesOpened && Input.GetMouseButton(0)) RulesToggle();
            if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene("GameMenu", LoadSceneMode.Additive);
        }

        public void RulesToggle()
        {
            if (rulesPanel == null) return;
            isRulesOpened = !isRulesOpened;
            rulesPanel.Toggle();
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
            
            handBoard.SetFreeze(false);
            
            if (roundActive)
            {
                Debug.Log("Round already in progress! End current round first.");
                return;
            }
            
            if (deck == null || targetBoard == null || handBoard == null)
            {
                Debug.LogError("Deck or Board not assigned!");
                return;
            }
            
            currentRound++;
            roundActive = true;
            audioSource.PlayOneShot(cardsShuffle);
            
            // Generate random goal
            GenerateGoal();
            
            // Deal cards to board
            StartCoroutine(DealCardsToBoard());
            
            UpdateRoundDisplay();
        }


        private int CheckAvailability(List<CardData> cardSet)
        {
            return CardCombinations.AllOrderedSubsets(cardSet, currentGoalValue, currentGoalSuit);
        }
        
        private void UpdateAvailabilityField()
        {
            List<SimpleCard> currentCards = GetActiveCards();
            List<CardData> cardsData = new List<CardData>();
            foreach (var card in currentCards)
            {
                cardsData.Add(card.GetCardData());
            }
            
            int result = CheckAvailability(cardsData);
            if (result == 2)
            {
                availabilityText.text = "Full";
            }
            if (result == 1)
            {
                availabilityText.text = "Only value";
            }
            if (result == 0)
            {
                availabilityText.text = "Nothing Here";
            }
        }
        
        /// <summary>
        /// End the current round and calculate score
        /// </summary>
        public bool EndRound()
        {
            if (!roundActive)
            {
                Debug.Log("No active round to end!");
                return false;
            }
            
            handBoard.SetFreeze(true);
            
            // Check if at least one card is on the board
            if (targetBoard.CardCount == 0)
            {
                Debug.Log("Cannot end round - at least one card required on board!");
                if (resultText != null)
                {
                    StartCoroutine(ShowTemporaryMessage("Need at least 1 card!", Color.red));
                }
                return false;
            }
            
            // Get the scorer from the board
            CardScorer scorer = targetBoard.GetComponentInChildren<CardScorer>();
            
            if (scorer == null)
            {
                Debug.LogError("No CardScorer found on board!");
                return false;
            }
            
            // Calculate round score
            SuccessCodes roundScore = CalculateRoundScore(scorer);
            scoreHistory.Add(roundScore);
            if (tilesManager != null && tilesManager.isActive) tilesManager.setVisibility(roundScore);
            
            // Show result
            StartCoroutine(ShowRoundResult(roundScore));
            
            // Clear the board
            ClearBoard();
            
            // Mark round as inactive
            roundActive = false;
            
            // Disable end round button
            if (endRoundButton != null)
            {
                // if (currentRound < maxRounds - 1) endRoundButton.interactable = false;
            }
            
            UpdateScoreHistoryDisplay();

            return true;
        }

        private void StartPostdiction() => SceneManager.LoadScene("PostDictionScene", LoadSceneMode.Additive);
        
        private void GenerateGoal()
        {
            // Random value between min and max (inclusive)
            currentGoalValue = Random.Range(minGoalValue, maxGoalValue + 1);
            
            // Get available suits (those that haven't reached the limit)
            RollNewSuitGoal();
            
            // Update display
            UpdateGoalDisplay();
        }

		private void RerollSuitGoal()
		{
			suitUsageCount[currentGoalSuit]--;
			RollNewSuitGoal();
            UpdateGoalDisplay();
		}       

		private void RollNewSuitGoal()
		{
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
                foreach (Suits suit in System.Enum.GetValues(typeof(Suits)))
                {
                    suitUsageCount[suit] = 0;
                    availableSuits.Add(suit);
                }
            }
            
            // Random suit from available ones
            currentGoalSuit = availableSuits[Random.Range(0, availableSuits.Count)];
            suitUsageCount[currentGoalSuit]++;
		}

        private void RerollCards()
        {
            StartCoroutine(GetAnotherSetOfCards());
        }

        private List<SimpleCard> GetActiveCards()
        {
            List<SimpleCard> result = new List<SimpleCard>();
            result.AddRange(handBoard.GetCards());
            result.AddRange(targetBoard.GetCards());
            return result;
        }
        
        private IEnumerator GetAnotherSetOfCards()
        {
            List<SimpleCard> currentCards = GetActiveCards();
    
            foreach (SimpleCard card in currentCards)
            {
                deck.ShuffleCardIntoDeck(card.GetCardData());
                Destroy(card.gameObject);
                yield return new WaitForSeconds(dealDelay);
            }
            handBoard.ClearBoard();
            targetBoard.ClearBoard();
            
            yield return StartCoroutine(DealCardsToBoard());
        }
        private IEnumerator DrawCardsToBoard(List<CardData> cardsToDraw)
        {
            isDealing = true;

            foreach (CardData cardData in cardsToDraw)
            {
                deck.SpawnCardOnBoard(cardData, true);
                yield return new WaitForSeconds(dealDelay);
            }
            
            isDealing = false;
            
            Debug.Log($"Round {currentRound} started! Board now has {targetBoard.CardCount} cards.");
            UpdateAvailabilityField();
        }

        private IEnumerator DealCardsToBoard()
        {
            // Calculate how many cards to deal
            int currentCards = handBoard.CardCount + targetBoard.CardCount;
            int cardsToDeal = cardsPerRound - currentCards;

            List<CardData> newSetOfCards = GetNewSetOfCards(cardsToDeal);
            if (onlyPossibleSetsMode)
            {
                newSetOfCards = GetPossibleSetOfCards(cardsToDeal);
            }
            
            yield return StartCoroutine(DrawCardsToBoard(newSetOfCards));
        }

        private void ReturnCards(List<CardData> cardsToReturn)
        {
            foreach (CardData cardData in cardsToReturn)
            {
                deck.ShuffleCardIntoDeck(cardData);
            }
        }
        
        private List<CardData> GetPossibleSetOfCards(int cardsNum)
        {
            List<CardData> newSetOfCards = GetNewSetOfCards(cardsNum);
            List<CardData> currentCardSet = handBoard.GetCardsData();
            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 5; i++)
                {
                    newSetOfCards = GetNewSetOfCards(cardsNum);
                    if (CheckAvailability(newSetOfCards.Concat(currentCardSet).ToList()) == 2)
                    {
                        return newSetOfCards;
                    }
                }
                RerollSuitGoal();
            }
            return newSetOfCards;
        }
        
        private List<CardData> GetNewSetOfCards(int cardsNum)
        {
            return deck.PickCardsInDeck(cardsNum);
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
            
            // Update suit text (cat replica) and trigger cat talk animation
            SetCatReplica(RoundTips.replica[currentRound]);
            
            // Update goal card image (if using visual card)
            if (goalCardImage != null)
            {
                goalCardImage.color = GetSuitColor(currentGoalSuit);
            }
            
            targetBoard.SetGoal(currentGoalSuit, currentGoalValue);
            UpdateAvailabilityField();
        }
        
        /// <summary>
        /// Calculate score based on goal matching
        /// Returns 0, 0.5, or 1.0
        /// </summary>
        private SuccessCodes CalculateRoundScore(CardScorer scorer)
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
                return SuccessCodes.Failer;
            }
            else if (scoreMatches && !suitMatches)
            {
                // Score matches but suit doesn't
                Debug.Log("Round Result: PARTIAL - Score matches but wrong suit (0.5 points)");
                return SuccessCodes.Patrial;
            }
            else
            {
                // Perfect match!
                Debug.Log("Round Result: PERFECT - Exact match! (1 point)");
                return SuccessCodes.Success;
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
                layout.AddCard(simpleCard);
            }
            
            return layout.GetScore();
        }
        
        /// <summary>
        /// Show round result to player
        /// </summary>
        private IEnumerator ShowRoundResult(SuccessCodes roundScore)
        {
            if (resultText != null)
            {
                resultText.gameObject.SetActive(true);
                
                string resultMessage = "";
                Color resultColor = Color.white;
                
                if (roundScore == SuccessCodes.Failer)
                {
                    resultMessage = "MISS!\nScore doesn't match\n+0 points";
                    resultColor = new Color(1f, 0.3f, 0.3f); // Red
                }
                else if (roundScore == SuccessCodes.Patrial)
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
            foreach (SuccessCodes score in scoreHistory)
            {
                totalScore += (float)score / 2;
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
            foreach (SuccessCodes score in scoreHistory)
            {
                if (score == SuccessCodes.Failer)
                    scoreStrings.Add("0");
                else if (score == SuccessCodes.Patrial)
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
        /// Sets the cat's replica text and triggers talk animation
        /// </summary>
        private void SetCatReplica(string text)
        {
            if (goalSuitText != null)
            {
                goalSuitText.color = Color.black;
                goalSuitText.text = text;
            }

            if (catAnimationController != null)
            {
                catAnimationController.CatTalkForDuration(catTalkDuration);
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
                    return new Color(0.92f, 0.11f, 0.14f); 
                case Suits.Skulls:
                    return new Color(0.95f, 0.84f, 0.40f); 
                case Suits.Coins:
                    return new Color(0.22f, 0.46f, 0.27f); 
                case Suits.Crowns:
                    return new Color(0.74f, 0.45f, 0.28f); 
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
            scoreHistory = new List<SuccessCodes> {};
            currentGoalValue = 0;
            roundActive = false;
            
            ClearBoard();
            
            UpdateRoundDisplay();
            UpdateScoreHistoryDisplay();
            
            if (goalValueText != null)
                goalValueText.text = "?";
            if (goalSuitText != null)
                goalSuitText.text = "Press Start";
            if (catAnimationController != null)
                catAnimationController.CatIdle();
            // if (endRoundButton != null)
            //     endRoundButton.interactable = false;
            
            Debug.Log("Game reset!");
        }
    }
}