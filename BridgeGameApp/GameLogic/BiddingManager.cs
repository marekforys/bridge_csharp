using BridgeGameApp.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace BridgeGameApp.GameLogic
{
    public enum BidType { Pass, Number }

    public class Bid
    {
        public BidType Type { get; set; }
        public int? Level { get; set; }
        public Suit? Suit { get; set; }
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
            var suitChar = Suit!.Value.ToString()[0];
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

        public bool IsValidBid(BidType type, int? level = null, Suit? suit = null)
        {
            if (Bids.Count == 0) return true; // First bid is always valid

            var lastBid = GetLastContractBid();

            return type switch
            {
                BidType.Pass => true,
                BidType.Number => IsHigherBid(level!.Value, suit!.Value, lastBid),
                _ => false
            };
        }

        private bool IsHigherBid(int level, Suit suit, Bid? lastBid)
        {
            if (lastBid == null) return true;

            var newValue = level * 5 + (int)suit;
            var lastValue = lastBid.Level!.Value * 5 + (int)lastBid.Suit!.Value;

            return newValue > lastValue;
        }

        private Bid? GetLastContractBid()
        {
            return Bids.Where(b => b.Type == BidType.Number).LastOrDefault();
        }

        public void MakeBid(BidType type, int? level = null, Suit? suit = null)
        {
            if (!IsValidBid(type, level, suit))
            {
                throw new InvalidOperationException($"Invalid bid: {type} {level} {suit}");
            }

            Bids.Add(new Bid(type, CurrentPlayer, level, suit));
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
        }

        public bool IsBiddingOver()
        {
            if (Bids.Count < 4) return false;
            // Bidding ends when after any non-pass bid, there are three passes in a row
            int passCount = 0;
            for (int i = Bids.Count - 1; i >= 0; i--)
            {
                if (Bids[i].Type == BidType.Pass)
                    passCount++;
                else
                    break;
            }
            // If there was at least one non-pass bid, and the last three bids are passes, bidding is over
            bool anyNonPass = Bids.Any(b => b.Type != BidType.Pass);
            return anyNonPass && passCount >= 3;
        }

        public void PrintBiddingHistory()
        {
            Console.WriteLine("Bidding:");
            foreach (var bid in Bids)
            {
                var playerInitial = bid.Player[0];
                if (bid.Type == BidType.Pass)
                    Console.WriteLine($"{playerInitial}: Pass");
                else
                    Console.WriteLine($"{playerInitial}: {bid.Level}{bid.Suit!.Value.ToString()[0]}");
            }
        }

        // Simple AI bidding
        public void MakeAIBid(Player player)
        {
            // Calculate hand strength
            var points = player.Hand.Sum(card => card.Rank switch
            {
                Rank.Ace => 4,
                Rank.King => 3,
                Rank.Queen => 2,
                Rank.Jack => 1,
                _ => 0
            });

            // Count cards in each suit
            var spades = player.Hand.Count(c => c.Suit == Suit.Spades);
            var hearts = player.Hand.Count(c => c.Suit == Suit.Hearts);
            var diamonds = player.Hand.Count(c => c.Suit == Suit.Diamonds);
            var clubs = player.Hand.Count(c => c.Suit == Suit.Clubs);

            var suitLengths = new[] { (Suit.Spades, spades), (Suit.Hearts, hearts), (Suit.Diamonds, diamonds), (Suit.Clubs, clubs) };
            var longestSuit = suitLengths.OrderByDescending(x => x.Item2).First().Item1;
            var longestLength = suitLengths.OrderByDescending(x => x.Item2).First().Item2;

            // Get the last bid to understand the current situation
            var lastBid = GetLastContractBid();

            // If we have a very weak hand, pass
            if (points < 6)
            {
                MakeBid(BidType.Pass);
                return;
            }

            // If no one has bid yet, bid our longest suit at level 1
            if (lastBid == null)
            {
                MakeBid(BidType.Number, 1, longestSuit);
                return;
            }

            // Check if we can bid higher than the current bid
            var currentLevel = lastBid.Level!.Value;
            var currentSuit = lastBid.Suit!.Value;

            // Try to bid our longest suit at the same level if it's higher
            if (longestSuit > currentSuit && longestLength >= 4)
            {
                MakeBid(BidType.Number, currentLevel, longestSuit);
                return;
            }

            // Try to bid at a higher level
            if (currentLevel < 7)
            {
                // Only bid higher if we have a strong hand
                if (points >= 12 && longestLength >= 5)
                {
                    MakeBid(BidType.Number, currentLevel + 1, longestSuit);
                    return;
                }
            }

            // If we can't bid higher, pass
            MakeBid(BidType.Pass);
        }
    }
}
