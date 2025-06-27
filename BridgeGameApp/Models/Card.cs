namespace BridgeGameApp.Models
{
    public enum Suit { Clubs, Diamonds, Hearts, Spades, NoTrump }
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
        public override string ToString()
        {
            string suitIcon = Suit switch
            {
                Suit.Clubs => "\u2663",    // ♣
                Suit.Diamonds => "\u2666", // ♦
                Suit.Hearts => "\u2665",   // ♥
                Suit.Spades => "\u2660",   // ♠
                Suit.NoTrump => "NT",
                _ => "?"
            };
            string rankSymbol = Rank switch
            {
                Rank.Ace => "A",
                Rank.King => "K",
                Rank.Queen => "Q",
                Rank.Jack => "J",
                Rank.Ten => "T",
                Rank.Nine => "9",
                Rank.Eight => "8",
                Rank.Seven => "7",
                Rank.Six => "6",
                Rank.Five => "5",
                Rank.Four => "4",
                Rank.Three => "3",
                Rank.Two => "2",
                _ => "?"
            };
            return $"{suitIcon} {rankSymbol}";
        }
    }
}
