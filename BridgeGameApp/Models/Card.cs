namespace BridgeGameApp.Models
{
    public enum Suit { Clubs, Diamonds, Hearts, Spades }
    public enum Rank { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }
    public class Card
    {
        public Suit Suit { get; set; }
        public Rank Rank { get; set; }
        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }
        public override string ToString() => $"{Rank} of {Suit}";
    }
}
