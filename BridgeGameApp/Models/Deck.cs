using System;
using System.Collections.Generic;
using System.Linq;

namespace BridgeGameApp.Models
{
    public class Deck
    {
        private List<Card> cards;
        public Deck()
        {
            cards = new List<Card>();
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                    cards.Add(new Card(suit, rank));
        }
        public void Shuffle()
        {
            var rng = new Random();
            cards = cards.OrderBy(_ => rng.Next()).ToList();
        }
        public Card Deal()
        {
            var card = cards[0];
            cards.RemoveAt(0);
            return card;
        }
        public int Count => cards.Count;
    }
}
