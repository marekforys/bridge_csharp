using BridgeGameApp.Models;
using System.Collections.Generic;

namespace BridgeGameApp.GameLogic
{
    public enum BidType { Pass, Number }
    public class Bid
    {
        public BidType Type { get; set; }
        public int? Level { get; set; } // 1-7 for contract bids
        public Suit? Suit { get; set; } // null for Pass
        public string Player { get; set; }
        public Bid(BidType type, string player, int? level = null, Suit? suit = null)
        {
            Type = type;
            Player = player;
            Level = level;
            Suit = suit;
        }
        public override string ToString()
        {
            if (Type == BidType.Pass)
                return $"{Player}: Pass";
            var suitChar = Suit.HasValue ? Suit.Value.ToString()[0] : '?';
            return $"{Player}: {Level}{suitChar}";
        }
    }

    public class BiddingManager
    {
        public List<Bid> Bids { get; private set; } = new();
        private readonly List<string> _players;
        private int _currentPlayerIndex = 0;
        public BiddingManager(List<string> players)
        {
            _players = players;
        }
        public string CurrentPlayer => _players[_currentPlayerIndex];
        public void MakeBid(BidType type, int? level = null, Suit? suit = null)
        {
            Bids.Add(new Bid(type, CurrentPlayer, level, suit));
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
        }
        public bool IsBiddingOver()
        {
            if (Bids.Count < 4) return false;
            var lastFour = Bids.TakeLast(4).ToList();
            return lastFour.All(b => b.Type == BidType.Pass);
        }
    }
}
