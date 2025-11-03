using UnityEngine;
using System.Collections.Generic;


namespace CardGame.Core
{
    public class CardDeck
    {
        private List<CardData> deck;
        private Dictionary<Suits, Dictionary<int, Sprite>> spriteMap;
        
        public int RemainingCards => deck.Count;
        
        public CardDeck(Dictionary<Suits, Dictionary<int, Sprite>> sprites = null)
        {
            spriteMap = sprites;
            deck = new List<CardData>();
            InitializeDeck();
        }
        
        private void InitializeDeck()
        {
            deck.Clear();
            
            foreach (Suits suit in System.Enum.GetValues(typeof(Suits)))
            {
                // 3 copies of value 1
                for (int i = 0; i < 3; i++)
                {
                    deck.Add(CreateCard(suit, 1));
                }
                
                // 2 copies of value 2
                for (int i = 0; i < 2; i++)
                {
                    deck.Add(CreateCard(suit, 2));
                }
                
                // 1 copy of value 3
                deck.Add(CreateCard(suit, 3));
            }
            
            Shuffle();
            Debug.Log($"Deck initialized with {deck.Count} cards");
			
        }
        
        private CardData CreateCard(Suits suit, int value)
        {
            Sprite sprite = null;
            
            if (spriteMap != null && 
                spriteMap.ContainsKey(suit) && 
                spriteMap[suit].ContainsKey(value))
            {
                sprite = spriteMap[suit][value];
            }
            
            return new CardData(suit, value, sprite);
        }
     
        public void Shuffle()
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                CardData temp = deck[i];
                deck[i] = deck[randomIndex];
                deck[randomIndex] = temp;
            }
            
            Debug.Log("Deck shuffled");
        }
        
        public CardData Draw()
        {
            if (deck.Count == 0)
            {
                Debug.LogWarning("Cannot draw from empty deck!");
                return null;
            }
            
            // Take the last card (most efficient)
            CardData drawnCard = deck[deck.Count - 1];
            deck.RemoveAt(deck.Count - 1);
            
            return drawnCard;
        }
        
        public CardData Peek()
        {
            if (deck.Count == 0)
            {
                Debug.LogWarning("Cannot peek at empty deck!");
                return null;
            }
            
            return deck[deck.Count - 1];
        }
        
        public void Reset()
        {
            InitializeDeck();
        }
        
        public void ResetAndShuffle()
        {
            Reset();
            Shuffle();
        }
        
        // Check if deck is empty
        public bool IsEmpty()
        {
            return deck.Count == 0;
        }
        
        // Get count of specific card type remaining
        public int CountCard(Suits suit, int value)
        {
            int count = 0;
            foreach (var card in deck)
            {
                if (card.suit == suit && card.value == value)
                    count++;
            }
            return count;
        }
        
        // Get count of all cards of a specific suit
        public int CountSuit(Suits suit)
        {
            int count = 0;
            foreach (var card in deck)
            {
                if (card.suit == suit)
                    count++;
            }
            return count;
        }
    }
}

