using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardGame.Cards;
using CardGame.Core;
using CardGame.GameObjects;
using CardGame.Scoring;

namespace CardGame.Managers
{
    /// <summary>
    /// Manages the tutorial flow - guides player through game mechanics step by step
    /// Alternative to RoundManager for tutorial scene
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        [Header("Bubble Windows - Step by Step")]
        [SerializeField] public GameObject bubble1_Introduction;
        [SerializeField] public GameObject bubble2_PointToDeck;
        [SerializeField] public GameObject bubble3_DealCards;
        [SerializeField] public GameObject bubble4_ExplainSpreadZone;
        [SerializeField] public GameObject bubble5_PromptPlaceCard;
        [SerializeField] public GameObject bubble6_CardPlacement;
        [SerializeField] public GameObject bubble7_ExplainGoalNumber;
        [SerializeField] public GameObject bubble8_ExplainSuits;
        [SerializeField] public GameObject bubble9_PromptSpecificCard;
        [SerializeField] public GameObject bubble10_ExplainMultiplier;
        [SerializeField] public GameObject bubble11_ReferToRules;
        [SerializeField] public GameObject bubble12_ExplainDominantSuit;
        [SerializeField] public GameObject bubble13_ExplainGoal;
        [SerializeField] public GameObject bubble14_WaitForCorrectSpread;
        [SerializeField] public GameObject bubble15_ShowPicture;
        [SerializeField] public GameObject bubble16_ShowHint;
        [SerializeField] public GameObject bubble17_FinalAdvice;
        
        [Header("Highlight Objects")]
        [SerializeField] public GameObject highlight_Deck;
        [SerializeField] public GameObject highlight_HandBoard;
        [SerializeField] public GameObject highlight_TargetBoard;
        [SerializeField] public GameObject highlight_FirstCard;
        [SerializeField] public GameObject highlight_CrystalBall;
        [SerializeField] public GameObject highlight_CurrentScore;
        [SerializeField] public GameObject highlight_CardInSpread;
        [SerializeField] public GameObject highlight_CoinCard3;
        [SerializeField] public GameObject highlight_RulesScroll;
        [SerializeField] public GameObject highlight_Crystal;
        [SerializeField] public GameObject highlight_EndTurnButton;
        [SerializeField] public GameObject highlight_Picture;
        [SerializeField] public GameObject highlight_Hint;
        
        [Header("Game References")]
        [SerializeField] private SimpleDeckObject deck;
        [SerializeField] private CardBoard handBoard;
        [SerializeField] private CardBoard targetBoard;
        [SerializeField] private CardScorer scorer;
        
        [Header("UI Elements")]
        [SerializeField] private Button endTurnButton;
        [SerializeField] private GameObject pictureDisplay;
        [SerializeField] private GameObject hintDisplay;

        [Header("Goal Display")]
        [SerializeField] private GameObject goalDisplay;
        [SerializeField] private TMPro.TextMeshProUGUI goalValueText;
        [SerializeField] private TMPro.TextMeshProUGUI goalSuitText;

        [Header("Tutorial Settings")]
        [SerializeField] private int tutorialGoalValue = 5;
        [SerializeField] private Suits tutorialGoalSuit = Suits.Coins;
        
        [Header("Audio")]
        [SerializeField] private AudioClip tutorialCompleteSound;
        
        private int currentStep = 0;
        private bool waitingForInput = false;
        private bool stepInProgress = false;
        private AudioSource audioSource;

        // References to tutorial cards (stored when spawned)
        private SimpleCard tutorialCard_Coin1;
        private SimpleCard tutorialCard_Coin3;
        private SimpleCard tutorialCard_Skull1;
        private SimpleCard tutorialCard_Rose1;
        private SimpleCard tutorialCard_Crown2;
        
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Hide all UI elements initially
            HideAllBubbles();
            HideAllHighlights();
            HideAllUIElements();
            
            // Freeze boards until tutorial allows interaction
            handBoard.SetFreeze(true);
            targetBoard.SetFreeze(true);
            
            // Start tutorial
            StartCoroutine(RunTutorial());
        }
        
        void Update()
        {
            // Allow player to click anywhere to continue when waiting for input
            if (waitingForInput && Input.GetMouseButtonDown(0))
            {
                ContinueTutorial();
            }
        }
        
        /// <summary>
        /// Main tutorial flow coroutine
        /// </summary>
        private IEnumerator RunTutorial()
        {
            // Step 1: Introduction
            yield return Step1_Introduction();
            
            // Step 2: Point to deck
            yield return Step2_PointToDeck();
            
            // Step 3: Deal cards
            yield return Step3_DealCards();
            
            // Step 4: Explain spread zone
            yield return Step4_ExplainSpreadZone();
            
            // Step 5: Prompt to place card
            yield return Step5_PromptPlaceCard();
            
            // Step 6: Wait for card placement
            yield return Step6_WaitForCardPlacement();
            
            // Step 7: Explain goal number
            yield return Step7_ExplainGoalNumber();
            
            // Step 8: Explain suits
            yield return Step8_ExplainSuits();
            
            // Step 9: Prompt specific card
            yield return Step9_PromptSpecificCard();
            
            // Step 10: Wait for second card and explain multiplier
            yield return Step10_ExplainMultiplier();
            
            // Step 11: Refer to rules scroll
            yield return Step11_ReferToRules();
            
            // Step 12: Explain dominant suit
            yield return Step12_ExplainDominantSuit();
            
            // Step 13: Explain goal
            yield return Step13_ExplainGoal();
            
            // Step 14: Wait for correct spread
            yield return Step14_WaitForCorrectSpread();
            
            // Step 15: Show picture
            yield return Step15_ShowPicture();
            
            // Step 16: Show hint
            yield return Step16_ShowHint();
            
            // Step 17: Final advice
            yield return Step17_FinalAdvice();
            
            // Tutorial complete
            CompleteTutorial();
        }
        
        // ====================================================================
        // TUTORIAL STEPS
        // ====================================================================
        
        private IEnumerator Step1_Introduction()
        {
            currentStep = 1;
            ShowBubble(bubble1_Introduction);
            yield return WaitForPlayerClick();
        }
        
        private IEnumerator Step2_PointToDeck()
        {
            currentStep = 2;
            HideAllBubbles();
            ShowHighlight(highlight_Deck);
            ShowBubble(bubble2_PointToDeck);
            yield return WaitForDeckClick();
        }
        
        private IEnumerator Step3_DealCards()
        {
            currentStep = 3;
            HideAllBubbles();
            HideAllHighlights();
            
            // Spawn tutorial cards using deck
            
            handBoard.SetFreeze(false);
            SpawnTutorialCards();
            handBoard.SetFreeze(true);
            
            ShowHighlight(highlight_HandBoard);
            ShowBubble(bubble3_DealCards);
            yield return WaitForPlayerClick();
        }
        
        private IEnumerator Step4_ExplainSpreadZone()
        {
            currentStep = 4;
            HideAllBubbles();
            HideAllHighlights();
            ShowHighlight(highlight_TargetBoard);
            ShowBubble(bubble4_ExplainSpreadZone);
            yield return WaitForPlayerClick();
        }
        
        private IEnumerator Step5_PromptPlaceCard()
        {
            currentStep = 5;
            HideAllBubbles();
            HideAllHighlights();

            // Unfreeze boards to allow card placement
            handBoard.SetFreeze(false);
            targetBoard.SetFreeze(false);

            // Also unfreeze the board where cards actually are (might be different from handBoard)
            UnfreezeCardParentBoard(tutorialCard_Coin1);

            // Freeze all cards except the first one (Coin-1)
            FreezeAllCardsExcept(tutorialCard_Coin1);

            ShowHighlight(highlight_FirstCard);
            ShowBubble(bubble5_PromptPlaceCard);

            // Wait until card is no longer on its original board
            CardBoard originalBoard = tutorialCard_Coin1.GetComponentInParent<CardBoard>();
            while (originalBoard != null && originalBoard.HasCard(tutorialCard_Coin1))
            {
                yield return null;
            }

            // Hide bubble when card starts moving
            HideAllBubbles();
            HideAllHighlights();
        }

        private IEnumerator Step6_WaitForCardPlacement()
        {
            currentStep = 6;

            // Wait until player places the card on target board
            while (targetBoard.CardCount == 0)
            {
                yield return null;
            }

            // Keep cards frozen - will unfreeze specific card in Step 9

            // Show goal display now (before Step 7 highlights it)
            ShowGoalDisplay();

            ShowHighlight(highlight_CrystalBall);
            ShowBubble(bubble6_CardPlacement);
            yield return WaitForPlayerClick();
        }
        
        private IEnumerator Step7_ExplainGoalNumber()
        {
            currentStep = 7;
            HideAllBubbles();
            HideAllHighlights();

            // Goal display is already visible from Step 6
            ShowHighlight(highlight_CurrentScore);
            ShowBubble(bubble7_ExplainGoalNumber);
            yield return WaitForPlayerClick();
        }
        
        private IEnumerator Step8_ExplainSuits()
        {
            currentStep = 8;
            HideAllBubbles();
            HideAllHighlights();
            ShowHighlight(highlight_CardInSpread);
            ShowBubble(bubble8_ExplainSuits);
            yield return WaitForPlayerClick();
        }
        
        private IEnumerator Step9_PromptSpecificCard()
        {
            currentStep = 9;
            HideAllBubbles();
            HideAllHighlights();

            // Unfreeze the board where the card is
            UnfreezeCardParentBoard(tutorialCard_Coin3);

            // Freeze all cards except Coin-3
            FreezeAllCardsExcept(tutorialCard_Coin3);

            ShowHighlight(highlight_CoinCard3);
            ShowBubble(bubble9_PromptSpecificCard);

            // Wait until card is no longer on its original board
            CardBoard originalBoard = tutorialCard_Coin3.GetComponentInParent<CardBoard>();
            while (originalBoard != null && originalBoard.HasCard(tutorialCard_Coin3))
            {
                yield return null;
            }

            // Hide bubble when card starts moving
            HideAllBubbles();
            HideAllHighlights();
        }

        private IEnumerator Step10_ExplainMultiplier()
        {
            currentStep = 10;

            // Wait for player to place the coin card
            int initialCount = targetBoard.CardCount;
            while (targetBoard.CardCount <= initialCount)
            {
                yield return null;
            }

            // Unfreeze all cards after placement
            UnfreezeAllCards();

            // Small delay to let glow effect show
            yield return new WaitForSeconds(0.5f);

            ShowBubble(bubble10_ExplainMultiplier);
            yield return WaitForPlayerClick();
        }
        
        private IEnumerator Step11_ReferToRules()
        {
            currentStep = 11;
            HideAllBubbles();
            HideAllHighlights();
            ShowHighlight(highlight_RulesScroll);
            ShowBubble(bubble11_ReferToRules);
            yield return WaitForPlayerClick();
        }
        
        private IEnumerator Step12_ExplainDominantSuit()
        {
            currentStep = 12;
            HideAllBubbles();
            HideAllHighlights();
            ShowHighlight(highlight_Crystal);
            ShowBubble(bubble12_ExplainDominantSuit);
            yield return WaitForPlayerClick();
        }
        
        private IEnumerator Step13_ExplainGoal()
        {
            currentStep = 13;
            HideAllBubbles();
            HideAllHighlights();
            ShowBubble(bubble13_ExplainGoal);
            yield return WaitForPlayerClick();
        }
        
        private IEnumerator Step14_WaitForCorrectSpread()
        {
            currentStep = 14;
            HideAllBubbles();
            HideAllHighlights();

            // Show the end turn button
            if (endTurnButton != null)
            {
                endTurnButton.gameObject.SetActive(true);
            }

            // Wait for correct spread
            while (!IsSpreadCorrect())
            {
                yield return new WaitForSeconds(0.5f);
            }

            // Goal reached - freeze all cards
            FreezeAllCards();

            // Show bubble and highlight button together
            ShowHighlight(highlight_EndTurnButton);
            ShowBubble(bubble14_WaitForCorrectSpread);

            // Wait for player to click button
            bool buttonClicked = false;
            endTurnButton.onClick.AddListener(() => buttonClicked = true);

            while (!buttonClicked)
            {
                yield return null;
            }
        }
        
        private IEnumerator Step15_ShowPicture()
        {
            currentStep = 15;
            HideAllBubbles();
            HideAllHighlights();
            
            // Show picture
            if (pictureDisplay != null)
            {
                pictureDisplay.SetActive(true);
            }
            
            ShowHighlight(highlight_Picture);
            ShowBubble(bubble15_ShowPicture);
            yield return WaitForPlayerClick();
        }
        
        private IEnumerator Step16_ShowHint()
        {
            currentStep = 16;
            HideAllBubbles();
            HideAllHighlights();
            
            // Show hint
            if (hintDisplay != null)
            {
                hintDisplay.SetActive(true);
            }
            
            ShowHighlight(highlight_Hint);
            ShowBubble(bubble16_ShowHint);
            yield return WaitForPlayerClick();
        }
        
        private IEnumerator Step17_FinalAdvice()
        {
            currentStep = 17;
            HideAllBubbles();
            HideAllHighlights();
            ShowBubble(bubble17_FinalAdvice);
            yield return WaitForPlayerClick();
        }
        
        // ====================================================================
        // HELPER METHODS
        // ====================================================================
        
        private void ShowBubble(GameObject bubble)
        {
            if (bubble != null)
            {
                bubble.SetActive(true);
            }
        }
        
        private void HideBubble(GameObject bubble)
        {
            if (bubble != null)
            {
                bubble.SetActive(false);
            }
        }
        
        private void HideAllBubbles()
        {
            HideBubble(bubble1_Introduction);
            HideBubble(bubble2_PointToDeck);
            HideBubble(bubble3_DealCards);
            HideBubble(bubble4_ExplainSpreadZone);
            HideBubble(bubble5_PromptPlaceCard);
            HideBubble(bubble6_CardPlacement);
            HideBubble(bubble7_ExplainGoalNumber);
            HideBubble(bubble8_ExplainSuits);
            HideBubble(bubble9_PromptSpecificCard);
            HideBubble(bubble10_ExplainMultiplier);
            HideBubble(bubble11_ReferToRules);
            HideBubble(bubble12_ExplainDominantSuit);
            HideBubble(bubble13_ExplainGoal);
            HideBubble(bubble14_WaitForCorrectSpread);
            HideBubble(bubble15_ShowPicture);
            HideBubble(bubble16_ShowHint);
            HideBubble(bubble17_FinalAdvice);
        }
        
        private void ShowHighlight(GameObject highlight)
        {
            if (highlight != null)
            {
                highlight.SetActive(true);

                // Disable raycast on all images so highlight doesn't block card interaction
                foreach (var image in highlight.GetComponentsInChildren<UnityEngine.UI.Image>(true))
                {
                    image.raycastTarget = false;
                }
            }
        }
        
        private void HideHighlight(GameObject highlight)
        {
            if (highlight != null)
            {
                highlight.SetActive(false);
            }
        }
        
        private void HideAllHighlights()
        {
            HideHighlight(highlight_Deck);
            HideHighlight(highlight_HandBoard);
            HideHighlight(highlight_TargetBoard);
            HideHighlight(highlight_FirstCard);
            HideHighlight(highlight_CrystalBall);
            HideHighlight(highlight_CurrentScore);
            HideHighlight(highlight_CardInSpread);
            HideHighlight(highlight_CoinCard3);
            HideHighlight(highlight_RulesScroll);
            HideHighlight(highlight_Crystal);
            HideHighlight(highlight_EndTurnButton);
            HideHighlight(highlight_Picture);
            HideHighlight(highlight_Hint);
        }
        
        private IEnumerator WaitForPlayerClick()
        {
            waitingForInput = true;
            stepInProgress = false;

            while (waitingForInput)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Wait for player to click specifically on the deck
        /// </summary>
        private IEnumerator WaitForDeckClick()
        {
            while (true)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    // Check if click is on the deck using raycast
                    UnityEngine.EventSystems.PointerEventData pointerData =
                        new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
                        {
                            position = Input.mousePosition
                        };

                    List<UnityEngine.EventSystems.RaycastResult> results = new List<UnityEngine.EventSystems.RaycastResult>();
                    UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);

                    foreach (var result in results)
                    {
                        // Check if clicked object is the deck or a child of deck
                        if (deck != null && (result.gameObject == deck.gameObject ||
                            result.gameObject.transform.IsChildOf(deck.transform)))
                        {
                            yield break; // Exit coroutine - deck was clicked
                        }
                    }
                }
                yield return null;
            }
        }
        
        private void ContinueTutorial()
        {
            if (waitingForInput && !stepInProgress)
            {
                waitingForInput = false;
            }
        }
        
        private void HideAllUIElements()
        {
            if (pictureDisplay != null) pictureDisplay.SetActive(false);
            if (hintDisplay != null) hintDisplay.SetActive(false);
            if (endTurnButton != null) endTurnButton.gameObject.SetActive(false);
            if (goalDisplay != null) goalDisplay.SetActive(false);
        }

        /// <summary>
        /// Show goal display and set the tutorial goal values
        /// </summary>
        private void ShowGoalDisplay()
        {
            if (goalDisplay != null)
            {
                goalDisplay.SetActive(true);
            }

            if (goalValueText != null)
            {
                goalValueText.text = tutorialGoalValue.ToString();
            }

            if (goalSuitText != null)
            {
                goalSuitText.text = tutorialGoalSuit.ToString();
            }
        }
        
        /// <summary>
        /// Spawn tutorial cards using deck.SpawnCardOnBoard
        /// Stores references to cards for later use in tutorial steps
        /// </summary>
        private void SpawnTutorialCards()
        {
            // Card 1: Coin-1 (first card to place in step 5)
            tutorialCard_Coin1 = deck.SpawnCardOnBoard(new CardData(Suits.Coins, 1));

            // Card 2: Coin-3 (the one we'll ask player to place in step 9)
            tutorialCard_Coin3 = deck.SpawnCardOnBoard(new CardData(Suits.Coins, 3));

            // Card 3: Skull-1
            tutorialCard_Skull1 = deck.SpawnCardOnBoard(new CardData(Suits.Skulls, 1));

            // Card 4: Rose-1
            tutorialCard_Rose1 = deck.SpawnCardOnBoard(new CardData(Suits.Roses, 1));

            // Card 5: Crown-2
            tutorialCard_Crown2 = deck.SpawnCardOnBoard(new CardData(Suits.Crowns, 2));
        }

        /// <summary>
        /// Freeze all tutorial cards except the specified one
        /// Uses stored card references directly
        /// </summary>
        private void FreezeAllCardsExcept(SimpleCard allowedCard)
        {
            SimpleCard[] allTutorialCards = {
                tutorialCard_Coin1,
                tutorialCard_Coin3,
                tutorialCard_Skull1,
                tutorialCard_Rose1,
                tutorialCard_Crown2
            };

            foreach (SimpleCard card in allTutorialCards)
            {
                if (card != null)
                {
                    card.SetIndividualFreeze(card != allowedCard);
                }
            }
        }

        /// <summary>
        /// Unfreeze all tutorial cards
        /// </summary>
        private void UnfreezeAllCards()
        {
            SimpleCard[] allTutorialCards = {
                tutorialCard_Coin1,
                tutorialCard_Coin3,
                tutorialCard_Skull1,
                tutorialCard_Rose1,
                tutorialCard_Crown2
            };

            foreach (SimpleCard card in allTutorialCards)
            {
                if (card != null)
                {
                    card.SetIndividualFreeze(false);
                }
            }
        }

        /// <summary>
        /// Freeze all tutorial cards
        /// </summary>
        private void FreezeAllCards()
        {
            SimpleCard[] allTutorialCards = {
                tutorialCard_Coin1,
                tutorialCard_Coin3,
                tutorialCard_Skull1,
                tutorialCard_Rose1,
                tutorialCard_Crown2
            };

            foreach (SimpleCard card in allTutorialCards)
            {
                if (card != null)
                {
                    card.SetIndividualFreeze(true);
                }
            }
        }

        /// <summary>
        /// Unfreeze the parent board of a card (in case it's different from handBoard/targetBoard)
        /// </summary>
        private void UnfreezeCardParentBoard(SimpleCard card)
        {
            if (card == null) return;

            CardBoard parentBoard = card.GetComponentInParent<CardBoard>();
            if (parentBoard != null)
            {
                parentBoard.SetFreeze(false);
            }
        }

        /// <summary>
        /// Check if the current spread is correct for tutorial
        /// For tutorial: need exactly value 5 with Coins as dominant suit
        /// </summary>
        private bool IsSpreadCorrect()
        {
            if (targetBoard.CardCount == 0) return false;
            
            // Get current score
            CardLayout layout = new CardLayout();
            foreach (SimpleCard card in targetBoard.GetCards())
            {
                layout.AddCard(card);
            }
            
            Score score = layout.GetScore();
            int totalValue = score.GetFullScore();
            Suits? dominantSuit = score.GetDominantSuit();
            
            // Check if matches tutorial goal
            return totalValue == tutorialGoalValue && dominantSuit == tutorialGoalSuit;
        }
        
        /// <summary>
        /// Complete the tutorial
        /// </summary>
        private void CompleteTutorial()
        {
            HideAllHighlights();
            HideAllBubbles();

            if (tutorialCompleteSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(tutorialCompleteSound);
            }

            Debug.Log("Tutorial complete! Player is ready to play.");

            // Wait for final click then return to main menu
            StartCoroutine(WaitAndReturnToMenu());
        }

        private IEnumerator WaitAndReturnToMenu()
        {
            yield return WaitForPlayerClick();
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}