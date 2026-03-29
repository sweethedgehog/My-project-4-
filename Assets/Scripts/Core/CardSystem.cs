using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CardGame.Cards;

namespace CardGame.Core
{
    [System.Serializable]
    public class Card
    {
        public Suits cardSuit;
        public int cardValue;

        public Card(Suits suit, int value)
        {
            cardSuit = suit;
            cardValue = value;
        }
    }

    public class Score
    {
        private Dictionary<Suits, int> suitScores;
        private List<bool> multipliers = new List<bool>();

        public Score()
        {
            suitScores = new Dictionary<Suits, int>();
            foreach (Suits suit in Enum.GetValues(typeof(Suits)))
            {
                suitScores[suit] = 0;
            }
        }

        public void AddScore(int value, Suits suit)
        {
            suitScores[suit] += value;
        }

        public int GetFullScore()
        {
            return suitScores.Values.Sum();
        }

        public Suits? GetDominantSuit()
        {
            int maxScore = 0;
            Suits? dominantSuit = null;

            foreach (var kvp in suitScores)
            {
                if (kvp.Value > maxScore)
                {
                    maxScore = kvp.Value;
                    dominantSuit = kvp.Key;
                }
                else if (kvp.Value == maxScore && kvp.Value > 0)
                {
                    dominantSuit = null;
                }
            }

            return dominantSuit;
        }

        // Helper method to get score for a specific suit
        public int GetSuitScore(Suits suit)
        {
            return suitScores[suit];
        }

        public void AddMultiplier(bool multiplier)
        {
            multipliers.Add(multiplier);
        }
        
        public List<bool> GetMultipliers()
        {
            return multipliers;
        }

        // Debug helper
        public string GetScoreBreakdown()
        {
            string result = "Scores: ";
            foreach (var kvp in suitScores)
            {
                result += $"{kvp.Key}={kvp.Value} ";
            }

            return result;
        }
    }

    public class CardLayout
    {
        private List<CardData> cards;

        private delegate int MultiplierFunc(int cardNum);

        private Dictionary<Suits, MultiplierFunc> getMultiplier;

        public CardLayout()
        {
            cards = new List<CardData>();

            getMultiplier = new Dictionary<Suits, MultiplierFunc>
            {
                { Suits.Roses, GetRedMultiplier },
                { Suits.Skulls, GetYellowMultiplier },
                { Suits.Coins, GetBlueMultiplier },
                { Suits.Crowns, GetGreenMultiplier }
            };
        }

        public Score GetScore()
        {
            Score result = new Score();

            for (int cardNum = 0; cardNum < cards.Count; cardNum++)
            {
                CardData card = cards[cardNum];
                int multiplier = getMultiplier[card.suit](cardNum);
                if (multiplier > 1)
                {
                    result.AddMultiplier(true);
                }
                else
                {
                    result.AddMultiplier(false);
                }
                result.AddScore(card.cardValue * multiplier, card.suit);
            }

            return result;
        }

        // Helper methods for managing cards
        public void Clear()
        {
            cards.Clear();
        }

        // Add card using CardData directly
        public void AddCard(CardData card)
        {
            cards.Add(card);
        }

        // Overload for SimpleCard - extracts CardData
        public void AddCard(SimpleCard card)
        {
            cards.Add(card.GetCardData());
        }

        public int CardCount()
        {
            return cards.Count;
        }

        private bool CheckSameSuitNeighbors(int cardNum, Suits suit)
        {
            // Check left neighbor
            if (cardNum != 0)
            {
                if (cards[cardNum - 1].suit == suit)
                    return true;
            }

            // Check right neighbor
            if (cardNum != cards.Count - 1)
            {
                if (cards[cardNum + 1].suit == suit)
                    return true;
            }

            return false;
        }

        private bool IsBordered(int cardNum)
        {
            return cardNum == 0 || cardNum == cards.Count - 1;
        }

        private int GetRedMultiplier(int cardNum)
        {
            if (IsBordered(cardNum))
                return 1;
            else
                return 2;
        }

        private int GetBlueMultiplier(int cardNum)
        {
            if (CheckSameSuitNeighbors(cardNum, Suits.Coins))
                return 2;
            else
                return 1;
        }

        private int GetYellowMultiplier(int cardNum)
        {
            if (IsBordered(cardNum))
                return 2;
            else
                return 1;
        }

        private int GetGreenMultiplier(int cardNum)
        {
            if (CheckSameSuitNeighbors(cardNum, Suits.Crowns))
                return 1;
            else
                return 2;
        }

    }
    
    public static class CardCombinations
    {
        // Reusable layout to avoid GC pressure during combination checking
        private static readonly CardLayout sharedLayout = new CardLayout();

        /// <summary>
        /// Generate all ordered subsets of a collection and check for goal match.
        /// Returns: 0 (no match), 1 (score match only), or 2 (perfect match with suit)
        /// </summary>
        public static int AllOrderedSubsets(List<CardData> cards, int goalValue, Suits goalSuit)
        {
            int n = cards.Count;

            // Early exit: if max possible score (all values × 2) can't reach goal
            int maxPossibleScore = 0;
            for (int i = 0; i < n; i++)
                maxPossibleScore += cards[i].cardValue * 2;
            if (maxPossibleScore < goalValue)
                return 0;

            int bestMatch = 0;

            // Skip size 0 (score is always 0) — start from size 1
            for (int size = 1; size <= n; size++)
            {
                foreach (var combo in GetCombinations(cards, size))
                {
                    // Early skip: if sum of values * 2 < goal, no permutation can reach it
                    int sumValues = 0;
                    for (int i = 0; i < combo.Count; i++)
                        sumValues += combo[i].cardValue;
                    if (sumValues * 2 < goalValue)
                        continue;

                    foreach (var perm in GetPermutations(combo))
                    {
                        sharedLayout.Clear();
                        for (int i = 0; i < perm.Count; i++)
                        {
                            sharedLayout.AddCard(perm[i]);
                        }

                        Score score = sharedLayout.GetScore();

                        if (score.GetFullScore() == goalValue)
                        {
                            bestMatch = 1;

                            Suits? dominantSuit = score.GetDominantSuit();
                            if (dominantSuit.HasValue && dominantSuit.Value == goalSuit)
                            {
                                return 2; // Perfect match!
                            }
                        }
                    }
                }
            }

            return bestMatch;
        }

        /// <summary>
        /// Generate all combinations of a specific size from a list
        /// </summary>
        private static IEnumerable<List<T>> GetCombinations<T>(List<T> list, int size)
        {
            if (size == 0)
            {
                yield return new List<T>();
                yield break;
            }

            if (list.Count == 0)
                yield break;

            // Include first element
            T first = list[0];
            List<T> rest = list.Skip(1).ToList();

            foreach (var combo in GetCombinations(rest, size - 1))
            {
                List<T> newCombo = new List<T> { first };
                newCombo.AddRange(combo);
                yield return newCombo;
            }

            // Exclude first element
            foreach (var combo in GetCombinations(rest, size))
            {
                yield return combo;
            }
        }

        /// <summary>
        /// Generate all permutations of a list
        /// </summary>
        private static IEnumerable<List<T>> GetPermutations<T>(List<T> list)
        {
            if (list.Count <= 1)
            {
                yield return new List<T>(list);
                yield break;
            }

            for (int i = 0; i < list.Count; i++)
            {
                T current = list[i];
                List<T> remaining = new List<T>(list);
                remaining.RemoveAt(i);

                foreach (var perm in GetPermutations(remaining))
                {
                    List<T> result = new List<T> { current };
                    result.AddRange(perm);
                    yield return result;
                }
            }
        }
    }

}