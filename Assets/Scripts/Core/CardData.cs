using UnityEngine;

namespace CardGame.Core
{
    [System.Serializable]
    public class CardData
    {
        public Suits suit;
        public int value;
        public Sprite sprite;

        public CardData(Suits suit, int value, Sprite sprite = null)
        {
            this.suit = suit;
            this.value = value;
            this.sprite = sprite;
        }
    }
}
