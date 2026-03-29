using UnityEngine;

namespace CardGame.Core
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "CardGame/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Round Settings")]
        [SerializeField] private int minGoalValue = 8;
        [SerializeField] private int maxGoalValue = 14;
        [SerializeField] private int cardsPerRound = 5;
        [SerializeField] private float dealDelay = 0.3f;
        [SerializeField] private float resultDisplayTime = 2f;

        [Header("Game Rules")]
        [SerializeField] private int maxRounds = 6;
        [SerializeField] private int maxSameSuitOccurrences = 2;

        [Header("Cat Animation")]
        [SerializeField] private float catTalkDuration = 3f;

        public int MinGoalValue => minGoalValue;
        public int MaxGoalValue => maxGoalValue;
        public int CardsPerRound => cardsPerRound;
        public float DealDelay => dealDelay;
        public float ResultDisplayTime => resultDisplayTime;
        public int MaxRounds => maxRounds;
        public int MaxSameSuitOccurrences => maxSameSuitOccurrences;
        public float CatTalkDuration => catTalkDuration;
    }
}
