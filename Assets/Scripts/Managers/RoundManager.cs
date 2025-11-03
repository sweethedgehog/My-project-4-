using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using CardGame.Core;
using CardGame.GameObjects;

namespace CardGame.Managers
{
    /// <summary>
    /// Manages game rounds - drawing goal cards and dealing cards to board
    /// </summary>
    public class RoundManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SimpleDeckObject deck;
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
        
        private int currentRound = 0;
        private int currentGoalValue;
        private Suits currentGoalSuit;
        private bool isDealing = false;
        
        void Start()
        {
            // Setup button
            if (startRoundButton != null)
            {
                startRoundButton.onClick.AddListener(StartRound);
            }
            
            UpdateRoundDisplay();
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
            
            if (deck == null || targetBoard == null)
            {
                Debug.LogError("Deck or Board not assigned!");
                return;
            }
            
            currentRound++;
            
            // Generate random goal
            GenerateGoal();
            
            // Deal cards to board
            StartCoroutine(DealCardsToBoard());
            
            UpdateRoundDisplay();
        }
        
        /// <summary>
        /// Generate random goal value and suit
        /// </summary>
        private void GenerateGoal()
        {
            // Random value between min and max (inclusive)
            currentGoalValue = Random.Range(minGoalValue, maxGoalValue + 1);
            
            // Random suit
            Suits[] allSuits = (Suits[])System.Enum.GetValues(typeof(Suits));
            currentGoalSuit = allSuits[Random.Range(0, allSuits.Length)];
            
            Debug.Log($"Round {currentRound} Goal: {currentGoalSuit} - Value {currentGoalValue}");
            
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
            int currentCards = targetBoard.CardCount;
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
            currentGoalValue = 0;
            targetBoard.ClearBoard();
            UpdateRoundDisplay();
            
            if (goalValueText != null)
                goalValueText.text = "?";
            if (goalSuitText != null)
                goalSuitText.text = "Press Start";
            
            Debug.Log("Game reset!");
        }
    }
}