using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        public List<Card> cards;

        private delegate int MultiplierFunc(int cardNum);

        private Dictionary<Suits, MultiplierFunc> getMultiplier;

        public CardLayout()
        {
            cards = new List<Card>();

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
                Card card = cards[cardNum];
                int multiplier = getMultiplier[card.cardSuit](cardNum);
                result.AddScore(card.cardValue * multiplier, card.cardSuit);
            }

            return result;
        }

        // Helper methods for managing cards
        public void Clear()
        {
            cards.Clear();
        }

        public void AddCard(Card card)
        {
            cards.Add(card);
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
                if (cards[cardNum - 1].cardSuit == suit)
                    return true;
            }

            // Check right neighbor
            if (cardNum != cards.Count - 1)
            {
                if (cards[cardNum + 1].cardSuit == suit)
                    return true;
            }

            return false;
        }

        private bool IsBordered(int cardNum, Suits suit)
        {
            if (cardNum == 0 || cardNum == cards.Count - 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int GetRedMultiplier(int cardNum)
        {
            if (IsBordered(cardNum, Suits.Roses))
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
            if (IsBordered(cardNum, Suits.Skulls))
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
}