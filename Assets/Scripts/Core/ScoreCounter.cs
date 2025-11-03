//
// using System.Collections.Generic;
//
//
// namespace CardGame.Core
// {
//     public class Score
//     {
//         private Dictionary<Suits, int> suitScores;
//
//         public Score()
//         {
//             suitScores = new Dictionary<Suits, int>();
//             foreach (Suits suit in System.Enum.GetValues(typeof(Suits)))
//             {
//                 suitScores[suit] = 0;
//             }
//         }
//
//         public void AddScore(int value, Suits suit)
//         {
//             suitScores[suit] += value;
//         }
//
//         public int GetFullScore()
//         {
//             return suitScores.Values.Sum();
//         }
//
//         public Suits? GetDominantSuit()
//         {
//             int maxScore = 0;
//             Suits? dominantSuit = null;
//
//             foreach (var kvp in suitScores)
//             {
//                 if (kvp.Value > maxScore)
//                 {
//                     maxScore = kvp.Value;
//                     dominantSuit = kvp.Key;
//                 }
//                 else if (kvp.Value == maxScore && kvp.Value > 0)
//                 {
//                     dominantSuit = null;
//                 }
//             }
//
//             return dominantSuit;
//         }
//
//         // Helper method to get score for a specific suit
//         public int GetSuitScore(Suits suit)
//         {
//             return suitScores[suit];
//         }
//
//         // Debug helper
//         public string GetScoreBreakdown()
//         {
//             string result = "Scores: ";
//             foreach (var kvp in suitScores)
//             {
//                 result += $"{kvp.Key}={kvp.Value} ";
//             }
//
//             return result;
//         }
//     }
//
//     public class ScoreCounter
//     {
//         public List<Card> cards;
//
//         private delegate int MultiplierFunc(int cardNum);
//
//         private Dictionary<Suits, MultiplierFunc> getMultiplier;
//
//         public ScoreCounter(List<Card> cards)
//         {
//             cards = cards;
//
//             getMultiplier = new Dictionary<Suits, MultiplierFunc>
//             {
//                 { Suits.Roses, GetRedMultiplier },
//                 { Suits.Skulls, GetYellowMultiplier },
//                 { Suits.Coins, GetBlueMultiplier },
//                 { Suits.Crowns, GetGreenMultiplier }
//             };
//         }
//
//         public Score GetScore()
//         {
//             Score result = new Score();
//
//             for (int cardNum = 0; cardNum < cards.Count; cardNum++)
//             {
//                 Card card = cards[cardNum];
//                 int multiplier = getMultiplier[card.suit](cardNum);
//                 result.AddScore(card.cardValue * multiplier, card.suit);
//             }
//
//             return result;
//         }
//
//         private bool CheckSameSuitNeighbors(int cardNum, Suits suit)
//         {
//             // Check left neighbor
//             if (cardNum != 0)
//             {
//                 if (cards[cardNum - 1].cardSuit == suit)
//                     return true;
//             }
//
//             // Check right neighbor
//             if (cardNum != cards.Count - 1)
//             {
//                 if (cards[cardNum + 1].cardSuit == suit)
//                     return true;
//             }
//
//             return false;
//         }
//
//         private bool IsBordered(int cardNum, Suits suit)
//         {
//             // Check if all cards from start to current position (inclusive) are the same suit
//             bool allLeftSameSuit = true;
//             for (int i = 0; i <= cardNum; i++)
//             {
//                 if (cards[i].cardSuit != suit)
//                 {
//                     allLeftSameSuit = false;
//                     break;
//                 }
//             }
//
//             if (allLeftSameSuit)
//                 return true;
//
//             // Check if all cards from current position to end are the same suit
//             bool allRightSameSuit = true;
//             for (int i = cardNum; i < cards.Count; i++)
//             {
//                 if (cards[i].cardSuit != suit)
//                 {
//                     allRightSameSuit = false;
//                     break;
//                 }
//             }
//
//             if (allRightSameSuit)
//                 return allRightSameSuit;
//
//             return false;
//         }
//
//         private int GetRedMultiplier(int cardNum)
//         {
//             if (IsBordered(cardNum, Suits.Roses))
//                 return 1;
//             else
//                 return 2;
//         }
//
//         private int GetBlueMultiplier(int cardNum)
//         {
//             if (CheckSameSuitNeighbors(cardNum, Suits.Coins))
//                 return 2;
//             else
//                 return 1;
//         }
//
//         private int GetYellowMultiplier(int cardNum)
//         {
//             if (IsBordered(cardNum, Suits.Skulls))
//                 return 2;
//             else
//                 return 1;
//         }
//
//         private int GetGreenMultiplier(int cardNum)
//         {
//             if (CheckSameSuitNeighbors(cardNum, Suits.Crowns))
//                 return 1;
//             else
//                 return 2;
//         }
//
//         public void AddCard(Card card)
//         {
//             cards.Add(card);
//         }
//     }
// }