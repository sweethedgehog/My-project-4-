using System.Collections.Generic;
using CardGame.Cards;
using DefaultNamespace.Tiles;

namespace CardGame.Core
{
    /// <summary>
    /// Static facade for score calculation logic.
    /// Wraps CardLayout creation and goal evaluation.
    /// </summary>
    public static class ScoreCalculator
    {
        /// <summary>
        /// Calculate score from a list of cards.
        /// </summary>
        public static Score CalculateScore(IReadOnlyList<CardData> cards)
        {
            CardLayout layout = new CardLayout();
            for (int i = 0; i < cards.Count; i++)
            {
                layout.AddCard(cards[i]);
            }

            return layout.GetScore();
        }

        /// <summary>
        /// Calculate score from a list of SimpleCards.
        /// </summary>
        public static Score CalculateScore(IReadOnlyList<SimpleCard> cards)
        {
            CardLayout layout = new CardLayout();
            for (int i = 0; i < cards.Count; i++)
            {
                layout.AddCard(cards[i]);
            }

            return layout.GetScore();
        }

        /// <summary>
        /// Evaluate whether a card layout matches the goal.
        /// </summary>
        public static SuccessCodes EvaluateGoal(Score score, int goalValue, Suits goalSuit)
        {
            int achievedScore = score.GetFullScore();
            Suits? dominantSuit = score.GetDominantSuit();

            bool scoreMatches = achievedScore == goalValue;
            bool suitMatches = dominantSuit.HasValue && dominantSuit.Value == goalSuit;

            if (!scoreMatches) return SuccessCodes.Failer;
            if (!suitMatches) return SuccessCodes.Partial;
            return SuccessCodes.Success;
        }
    }
}
