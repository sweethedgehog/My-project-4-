using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using CardGame.Cards;
using CardGame.Core;
using CardGame.GameObjects;
using CardGame.Scoring;
using CardGame.UI;
using DefaultNamespace.Tiles;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;


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
        [SerializeField] private TilesManager tilesManager;
        [SerializeField] private RulesPanel rulesPanel;
        [SerializeField] private bool onlyPossibleSetsMode = false;

        [Header("UI - Goal Display")]
        [SerializeField] private TextMeshProUGUI goalValueText;
        [SerializeField] private TextMeshProUGUI goalSuitText;
        [SerializeField] private Image goalCardImage;

        [Header("UI - Round Info")]
        [SerializeField] private TextMeshProUGUI roundNumberText;
        [SerializeField] private TextMeshProUGUI scoreHistoryText;
        [SerializeField] private TextMeshProUGUI availabilityText;
        [SerializeField] private TextMeshProUGUI resultText;

        [Header("UI - Buttons")]
        [SerializeField] private Button startRoundButton;
        [SerializeField] private Button endRoundButton;
        [SerializeField] private Button rerollSuitButton;
        [SerializeField] private Button rerollCardsButton;

        [Header("Cat Animation")]
        [SerializeField] private CatAnimationController catAnimationController;
        [SerializeField] private float catTalkDuration = 3f;

        [Header("Round Settings")]
        [SerializeField] private int minGoalValue = 8;
        [SerializeField] private int maxGoalValue = 14;
        [SerializeField] private int cardsPerRound = 5;
        [SerializeField] private float dealDelay = 0.3f;
        [SerializeField] private float resultDisplayTime = 2f;

        [Header("Game Rules")]
        [SerializeField] private int maxRounds = 6;
        [SerializeField] private int maxSameSuitOccurrences = 2;

        [Header("Audio")]
        [SerializeField] private AudioClip cardsShuffle;

        // Runtime state
        private AudioSource audioSource;
        private int currentRound = 0;
        private int currentGoalValue;
        private Suits currentGoalSuit;
        private List<SuccessCodes> scoreHistory = new List<SuccessCodes>();
        private Dictionary<Suits, int> suitUsageCount = new Dictionary<Suits, int>();
        private bool isDealing = false;
        private bool isRoundActive = false;
        private bool isRulesOpened = false;
        private bool isWaitingToDeal = true;

        public static bool inGameMenu = false;

        // Availability check result codes
        private const int AVAILABILITY_FULL_MATCH = 2;
        private const int AVAILABILITY_VALUE_ONLY = 1;
        private const int AVAILABILITY_NO_MATCH = 0;

        // Retry limits for card set generation
        private const int MAX_CARD_REROLL_ATTEMPTS = 5;
        private const int MAX_SUIT_REROLL_ATTEMPTS = 5;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            inGameMenu = false;
            Time.timeScale = 1f;

            if (startRoundButton != null)
                startRoundButton.onClick.AddListener(OnStartButtonClicked);
            if (endRoundButton != null)
                endRoundButton.onClick.AddListener(OnEndButtonClicked);
            if (rerollSuitButton != null)
                rerollSuitButton.onClick.AddListener(RerollSuitGoal);
            if (rerollCardsButton != null)
                rerollCardsButton.onClick.AddListener(RerollCards);

            foreach (Suits suit in System.Enum.GetValues(typeof(Suits)))
            {
                suitUsageCount[suit] = 0;
            }

            UpdateRoundDisplay();
            UpdateScoreHistoryDisplay();
            UpdateButtonStates();

            if (resultText != null)
            {
                resultText.gameObject.SetActive(false);
            }
            isRulesOpened = false;
        }

        private void OnStartButtonClicked()
        {
            if (currentRound >= maxRounds)
            {
                StartPostdiction();
                return;
            }
            StartRound();
        }

        private void OnEndButtonClicked()
        {
            EndRound();
        }
        
        private void Update()
        {
            if (inGameMenu) return;
            if (isRulesOpened && Input.GetMouseButton(0)) RulesToggle();
            if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene("GameMenu", LoadSceneMode.Additive);

            // Continuously update end button state based on card count (user can drag cards)
            if (!isWaitingToDeal && endRoundButton != null)
            {
                endRoundButton.interactable = targetBoard.CardCount > 0;
            }
        }

        private void UpdateButtonStates()
        {
            // Start button: active only between rounds
            if (startRoundButton != null)
                startRoundButton.interactable = isWaitingToDeal;

            // End button: active during round AND when targetBoard has cards
            if (endRoundButton != null)
                endRoundButton.interactable = !isWaitingToDeal && targetBoard.CardCount > 0;
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

            if (isRoundActive)
            {
                Debug.Log("Round already in progress! End current round first.");
                return;
            }

            if (deck == null || targetBoard == null || handBoard == null)
            {
                Debug.LogError("Deck or Board not assigned!");
                return;
            }

            // Clear cards from previous round
            ClearPreviousRoundCards();

            // Unfreeze both boards for the new round
            handBoard.SetFreeze(false);
            targetBoard.SetFreeze(false);

            currentRound++;
            isRoundActive = true;
            isWaitingToDeal = false;
            audioSource.PlayOneShot(cardsShuffle);

            GenerateGoal();
            StartCoroutine(DealCardsToBoard());

            UpdateRoundDisplay();
            UpdateButtonStates();
        }

        private void ClearPreviousRoundCards()
        {
            var targetCards = targetBoard.GetCards();
            foreach (var card in targetCards)
            {
                if (card != null) Destroy(card.gameObject);
            }
            targetBoard.ClearBoard();
        }


        private int CheckAvailability(List<CardData> cardSet)
        {
            return CardCombinations.AllOrderedSubsets(cardSet, currentGoalValue, currentGoalSuit);
        }
        
        private void UpdateAvailabilityField()
        {
            if (availabilityText == null) return;

            List<SimpleCard> currentCards = GetActiveCards();
            List<CardData> cardsData = new List<CardData>();
            foreach (var card in currentCards)
            {
                cardsData.Add(card.GetCardData());
            }

            int result = CheckAvailability(cardsData);
            switch (result)
            {
                case AVAILABILITY_FULL_MATCH:
                    availabilityText.text = "Full";
                    break;
                case AVAILABILITY_VALUE_ONLY:
                    availabilityText.text = "Only value";
                    break;
                case AVAILABILITY_NO_MATCH:
                    availabilityText.text = "Nothing Here";
                    break;
            }
        }
        
        /// <summary>
        /// End the current round and calculate score
        /// </summary>
        public bool EndRound()
        {
            if (!isRoundActive)
            {
                Debug.Log("No active round to end!");
                return false;
            }

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

            // Freeze both boards - cards stay visible but can't be moved
            handBoard.SetFreeze(true);
            targetBoard.SetFreeze(true);

            // Calculate round score
            SuccessCodes roundScore = CalculateRoundScore();
            scoreHistory.Add(roundScore);
            if (tilesManager != null && tilesManager.isActive) tilesManager.setVisibility(roundScore);

            // Show result
            StartCoroutine(ShowRoundResult(roundScore));

            // Mark round as inactive, ready for next round
            isRoundActive = false;
            isWaitingToDeal = true;

            UpdateScoreHistoryDisplay();
            UpdateButtonStates();

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

        private List<CardData> GetPossibleSetOfCards(int cardsNum)
        {
            List<CardData> newSetOfCards = GetNewSetOfCards(cardsNum);
            List<CardData> currentCardSet = handBoard.GetCardsData();

            for (int suitAttempt = 0; suitAttempt < MAX_SUIT_REROLL_ATTEMPTS; suitAttempt++)
            {
                for (int cardAttempt = 0; cardAttempt < MAX_CARD_REROLL_ATTEMPTS; cardAttempt++)
                {
                    newSetOfCards = GetNewSetOfCards(cardsNum);
                    if (CheckAvailability(newSetOfCards.Concat(currentCardSet).ToList()) == AVAILABILITY_FULL_MATCH)
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
        /// </summary>
        private SuccessCodes CalculateRoundScore()
        {
            Score currentScore = CalculateScoreFromBoard();
            
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
        
        private Score CalculateScoreFromBoard()
        {
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
            isRoundActive = false;
            
            ClearBoard();
            
            UpdateRoundDisplay();
            UpdateScoreHistoryDisplay();
            
            if (goalValueText != null)
                goalValueText.text = "?";
            if (goalSuitText != null)
                goalSuitText.text = "Press Start";
            if (catAnimationController != null)
                catAnimationController.CatIdle();

            Debug.Log("Game reset!");
        }
    }
}