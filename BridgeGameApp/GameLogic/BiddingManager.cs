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

    public class HandEvaluation
    {
        public int HighCardPoints { get; set; }
        public int DistributionPoints { get; set; }
        public int TotalPoints { get; set; }
        public Dictionary<Suit, int> SuitLengths { get; set; } = new();
        public Suit LongestSuit { get; set; }
        public int LongestSuitLength { get; set; }
        public bool HasStopper(Suit suit) => SuitLengths[suit] >= 3 ||
            SuitLengths[suit] == 2 && HasHighCard(suit) ||
            SuitLengths[suit] == 1 && HasAce(suit);

        private bool HasHighCard(Suit suit) => SuitLengths[suit] > 0 &&
            SuitLengths[suit] <= 2 && HasAce(suit);
        private bool HasAce(Suit suit) => false; // Simplified
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

            // Higher level always wins
            if (level > lastBid.Level!.Value) return true;

            // Same level, higher suit wins
            if (level == lastBid.Level!.Value && suit > lastBid.Suit!.Value) return true;

            return false;
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

        // Enhanced AI bidding
        public void MakeAIBid(Player player)
        {
            var evaluation = EvaluateHand(player.Hand);
            var lastBid = GetLastContractBid();
            var partnerBids = GetPartnerBids(player.Name);
            var opponentsBids = GetOpponentBids(player.Name);

            // Opening bid logic
            if (lastBid == null)
            {
                MakeOpeningBid(evaluation);
                return;
            }

            // Responding to partner's bid
            if (partnerBids.Any())
            {
                MakeRespondingBid(evaluation, partnerBids.Last(), lastBid);
                return;
            }

            // Competing with opponents
            MakeCompetitiveBid(evaluation, lastBid);
        }

        private HandEvaluation EvaluateHand(List<Card> hand)
        {
            var evaluation = new HandEvaluation();

            // Calculate high card points
            evaluation.HighCardPoints = hand.Sum(card => card.Rank switch
            {
                Rank.Ace => 4,
                Rank.King => 3,
                Rank.Queen => 2,
                Rank.Jack => 1,
                _ => 0
            });

            // Calculate distribution points
            var suitLengths = new Dictionary<Suit, int>();
            foreach (Suit suit in Enum.GetValues<Suit>())
            {
                var length = hand.Count(c => c.Suit == suit);
                suitLengths[suit] = length;

                // Add distribution points
                if (length >= 5) evaluation.DistributionPoints += length - 4;
                if (length == 0) evaluation.DistributionPoints += 3; // Void
                if (length == 1) evaluation.DistributionPoints += 2; // Singleton
                if (length == 2) evaluation.DistributionPoints += 1; // Doubleton
            }

            evaluation.SuitLengths = suitLengths;
            evaluation.LongestSuit = suitLengths.OrderByDescending(x => x.Value).First().Key;
            evaluation.LongestSuitLength = suitLengths[evaluation.LongestSuit];
            evaluation.TotalPoints = evaluation.HighCardPoints + evaluation.DistributionPoints;

            return evaluation;
        }

        private void MakeOpeningBid(HandEvaluation evaluation)
        {
            // Pass with very weak hands
            if (evaluation.TotalPoints < 12)
            {
                MakeBid(BidType.Pass);
                return;
            }

            // Strong NT hands (balanced, 15-17 points)
            if (evaluation.TotalPoints >= 15 && evaluation.TotalPoints <= 17 && IsBalanced(evaluation))
            {
                MakeBid(BidType.Number, 1, Suit.NoTrump);
                return;
            }

            // Open longest suit
            if (evaluation.LongestSuitLength >= 5)
            {
                MakeBid(BidType.Number, 1, evaluation.LongestSuit);
                return;
            }

            // Open best 4-card suit
            var bestSuit = GetBestSuit(evaluation);
            MakeBid(BidType.Number, 1, bestSuit);
        }

        private void MakeRespondingBid(HandEvaluation evaluation, Bid partnerBid, Bid lastBid)
        {
            if (partnerBid.Type != BidType.Number)
            {
                // If partner's last bid is not a contract, just pass or make a safe bid
                MakeBid(BidType.Pass);
                return;
            }
            var partnerLevel = partnerBid.Level!.Value;
            var partnerSuit = partnerBid.Suit!.Value;
            var lastLevel = lastBid.Level!.Value;
            var lastSuit = lastBid.Suit!.Value;

            // Support partner's suit
            if (evaluation.SuitLengths[partnerSuit] >= 3 && evaluation.TotalPoints >= 6)
            {
                if (evaluation.TotalPoints >= 13) // Game forcing
                {
                    MakeBid(BidType.Number, 4, partnerSuit);
                }
                else if (evaluation.TotalPoints >= 10) // Invitational
                {
                    MakeBid(BidType.Number, 3, partnerSuit);
                }
                else // Simple raise
                {
                    MakeBid(BidType.Number, 2, partnerSuit);
                }
                return;
            }

            // Bid new suit - must be higher than current bid
            if (evaluation.TotalPoints >= 6)
            {
                var newSuit = GetBestNewSuit(evaluation, partnerSuit);
                if (newSuit != partnerSuit && IsHigherBid(lastLevel, newSuit, lastBid))
                {
                    MakeBid(BidType.Number, lastLevel, newSuit);
                    return;
                }

                // Try bidding at higher level
                if (lastLevel < 7)
                {
                    MakeBid(BidType.Number, lastLevel + 1, newSuit);
                    return;
                }
            }

            // Pass with weak hands
            MakeBid(BidType.Pass);
        }

        private void MakeCompetitiveBid(HandEvaluation evaluation, Bid lastBid)
        {
            var lastLevel = lastBid.Level!.Value;
            var lastSuit = lastBid.Suit!.Value;

            // Double with strong hands
            if (evaluation.TotalPoints >= 15 && HasStoppers(evaluation))
            {
                // For now, just bid higher
                var newSuit = GetBestSuit(evaluation);
                if (newSuit > lastSuit)
                {
                    MakeBid(BidType.Number, lastLevel, newSuit);
                    return;
                }
            }

            // Bid higher if we have a good hand
            if (evaluation.TotalPoints >= 10)
            {
                var newSuit = GetBestSuit(evaluation);
                if (newSuit > lastSuit)
                {
                    MakeBid(BidType.Number, lastLevel, newSuit);
                    return;
                }

                // Try bidding at higher level
                if (lastLevel < 7 && evaluation.SuitLengths[newSuit] >= 5)
                {
                    MakeBid(BidType.Number, lastLevel + 1, newSuit);
                    return;
                }
            }

            // Pass
            MakeBid(BidType.Pass);
        }

        private bool IsBalanced(HandEvaluation evaluation)
        {
            var lengths = evaluation.SuitLengths.Values.OrderBy(x => x).ToArray();
            return lengths.Length == 4 && lengths[0] >= 2 && lengths[3] <= 4;
        }

        private Suit GetBestSuit(HandEvaluation evaluation)
        {
            // Prefer major suits, then longest suit
            var majorSuits = evaluation.SuitLengths.Where(x => x.Key == Suit.Spades || x.Key == Suit.Hearts)
                .OrderByDescending(x => x.Value).ToList();

            if (majorSuits.Any() && majorSuits.First().Value >= 4)
                return majorSuits.First().Key;

            return evaluation.LongestSuit;
        }

        private Suit GetBestNewSuit(HandEvaluation evaluation, Suit partnerSuit)
        {
            var candidates = evaluation.SuitLengths.Where(x => x.Key != partnerSuit && x.Value >= 4)
                .OrderByDescending(x => x.Value).ToList();

            if (candidates.Any())
                return candidates.First().Key;

            return evaluation.LongestSuit;
        }

        private bool HasStoppers(HandEvaluation evaluation)
        {
            return evaluation.SuitLengths.All(x => evaluation.HasStopper(x.Key));
        }

        private List<Bid> GetPartnerBids(string playerName)
        {
            var partnerIndex = (_players.IndexOf(playerName) + 2) % _players.Count;
            var partnerName = _players[partnerIndex];
            return Bids.Where(b => b.Player == partnerName).ToList();
        }

        private List<Bid> GetOpponentBids(string playerName)
        {
            var partnerIndex = (_players.IndexOf(playerName) + 2) % _players.Count;
            var partnerName = _players[partnerIndex];
            return Bids.Where(b => b.Player != playerName && b.Player != partnerName).ToList();
        }
    }
}
