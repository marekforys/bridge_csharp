using System.Collections.Generic;
using System.Linq;
using BridgeGameApp.Models;

namespace BridgeGameApp.GameLogic
{
    public class Contract
    {
        public int Level { get; set; }
        public Suit Suit { get; set; }
        public string Declarer { get; set; } = "";
        public bool Doubled { get; set; }
        public bool Redoubled { get; set; }

        public override string ToString()
        {
            var result = $"{Level}{GetSuitSymbol(Suit)} by {Declarer}";
            if (Redoubled) result += " (Redoubled)";
            else if (Doubled) result += " (Doubled)";
            return result;
        }

        private static string GetSuitSymbol(Suit suit) => suit switch
        {
            Suit.Spades => "♠",
            Suit.Hearts => "♥",
            Suit.Diamonds => "♦",
            Suit.Clubs => "♣",
            _ => "?"
        };
    }

    public class GameManager
    {
        public List<Player> Players { get; private set; }
        public Deck Deck { get; private set; }
        public Dictionary<string, int> TricksWon { get; private set; } = new();
        public Contract? CurrentContract { get; private set; }
        public bool IsVulnerable { get; set; } = false; // Simplified vulnerability
        public string Declarer { get; private set; } = "";
        public string Dummy { get; private set; } = "";

        public GameManager(List<string> playerNames)
        {
            Players = playerNames.Select(name => new Player(name)).ToList();
            Deck = new Deck();
        }

        public void StartGame()
        {
            Deck.Shuffle();
            int playerIndex = 0;
            while (Deck.Count > 0)
            {
                Players[playerIndex % Players.Count].Hand.Add(Deck.Deal());
                playerIndex++;
            }
        }

        public void PlayGame(BiddingManager biddingManager)
        {
            TricksWon = Players.ToDictionary(p => p.Name, p => 0);

            // Determine contract and declarer
            var contractBid = biddingManager.Bids.LastOrDefault(b => b.Type == BidType.Number);
            if (contractBid == null)
            {
                Console.WriteLine("No contract bid. No play phase.");
                return;
            }

            // Find the declarer (first player to bid this suit at this level)
            var declarerBid = biddingManager.Bids.FirstOrDefault(b => b.Type == BidType.Number &&
                b.Level == contractBid.Level && b.Suit == contractBid.Suit);

            if (declarerBid == null)
            {
                Console.WriteLine("Error: Could not determine declarer.");
                return;
            }

            Declarer = declarerBid.Player;
            var declarerIndex = Players.FindIndex(p => p.Name == Declarer);
            Dummy = Players[(declarerIndex + 2) % Players.Count].Name; // Partner of declarer

            CurrentContract = new Contract
            {
                Level = contractBid.Level!.Value,
                Suit = contractBid.Suit!.Value,
                Declarer = Declarer
            };

            Console.WriteLine($"\nContract: {CurrentContract}");
            Console.WriteLine($"Declarer: {Declarer}, Dummy: {Dummy}");
            Console.WriteLine();

            // The player after declarer leads
            int leaderIndex = (declarerIndex + 1) % Players.Count;

            for (int trick = 0; trick < 13; trick++)
            {
                var trickCards = new List<(string Player, Card Card)>();
                var leadSuit = Suit.Clubs; // Will be set by first card
                var trumpSuit = CurrentContract.Suit;

                // Play the trick
                for (int i = 0; i < Players.Count; i++)
                {
                    var player = Players[(leaderIndex + i) % Players.Count];
                    Card card;

                    if (player.Name == Dummy)
                    {
                        // Dummy plays declarer's choice (simplified - just play first valid card)
                        card = GetAICard(player, leadSuit, trumpSuit, trickCards, true);
                    }
                    else
                    {
                        // AI or human plays
                        card = GetAICard(player, leadSuit, trumpSuit, trickCards, false);
                    }

                    player.Hand.Remove(card);
                    trickCards.Add((player.Name, card));

                    if (i == 0) leadSuit = card.Suit; // Set lead suit
                }

                // Determine winner
                var winningCard = DetermineWinner(trickCards, leadSuit, trumpSuit);
                var winner = trickCards.First(tc => tc.Card == winningCard).Player;
                TricksWon[winner]++;

                // Print trick
                var trickLine = string.Join(" ", trickCards.Select(tc => $"{tc.Player[0]}: {tc.Card}"));
                Console.WriteLine($"Trick {trick + 1}: {trickLine}  Winner: {winner[0]}");

                // Set leader for next trick
                leaderIndex = Players.FindIndex(p => p.Name == winner);
            }

            // Evaluate contract
            EvaluateContract();
        }

        private Card GetAICard(Player player, Suit leadSuit, Suit trumpSuit, List<(string Player, Card Card)> trickCards, bool isDummy)
        {
            var validCards = GetValidCards(player.Hand, leadSuit, trickCards.Count == 0);

            if (validCards.Count == 1) return validCards[0];

            // Simple AI logic
            if (trickCards.Count == 0)
            {
                // Leading - play highest card in longest suit
                var longestSuit = player.Hand.GroupBy(c => c.Suit)
                    .OrderByDescending(g => g.Count())
                    .First().Key;
                return player.Hand.Where(c => c.Suit == longestSuit)
                    .OrderByDescending(c => c.Rank)
                    .First();
            }
            else
            {
                // Following - try to win if possible, otherwise play low
                var winningCards = validCards.Where(c => CanWinTrick(c, trickCards, leadSuit, trumpSuit)).ToList();

                if (winningCards.Any())
                {
                    return winningCards.OrderByDescending(c => c.Rank).First();
                }
                else
                {
                    return validCards.OrderBy(c => c.Rank).First(); // Play lowest
                }
            }
        }

        private List<Card> GetValidCards(List<Card> hand, Suit leadSuit, bool isLeading)
        {
            if (isLeading) return hand;

            var cardsInLeadSuit = hand.Where(c => c.Suit == leadSuit).ToList();
            return cardsInLeadSuit.Any() ? cardsInLeadSuit : hand;
        }

        private bool CanWinTrick(Card card, List<(string Player, Card Card)> trickCards, Suit leadSuit, Suit trumpSuit)
        {
            var currentWinner = DetermineWinner(trickCards, leadSuit, trumpSuit);

            // In NoTrump, only follow suit matters
            if (trumpSuit == Suit.NoTrump)
            {
                // If we're not following suit, we can't win
                if (card.Suit != leadSuit) return false;

                // Compare ranks in same suit
                return card.Rank > currentWinner.Rank;
            }

            // If we're not following suit, we can't win unless we have trump
            if (card.Suit != leadSuit && card.Suit != trumpSuit) return false;

            // If current winner is trump and we're not trump, we can't win
            if (currentWinner.Suit == trumpSuit && card.Suit != trumpSuit) return false;

            // If we're trump and current winner isn't, we win
            if (card.Suit == trumpSuit && currentWinner.Suit != trumpSuit) return true;

            // Compare ranks in same suit
            return card.Rank > currentWinner.Rank;
        }

        private Card DetermineWinner(List<(string Player, Card Card)> trickCards, Suit leadSuit, Suit trumpSuit)
        {
            Card winner = trickCards[0].Card;

            foreach (var (player, card) in trickCards.Skip(1))
            {
                // In NoTrump, only follow suit matters
                if (trumpSuit == Suit.NoTrump)
                {
                    if (card.Suit == winner.Suit)
                    {
                        // Same suit - higher rank wins
                        if (card.Rank > winner.Rank)
                        {
                            winner = card;
                        }
                    }
                    // If card doesn't follow suit, it can't win in NoTrump
                }
                else
                {
                    // Trump always beats non-trump
                    if (card.Suit == trumpSuit && winner.Suit != trumpSuit)
                    {
                        winner = card;
                    }
                    else if (winner.Suit == trumpSuit && card.Suit != trumpSuit)
                    {
                        // Current winner is trump, non-trump can't beat it
                        continue;
                    }
                    else if (card.Suit == winner.Suit)
                    {
                        // Same suit - higher rank wins
                        if (card.Rank > winner.Rank)
                        {
                            winner = card;
                        }
                    }
                    // If card doesn't follow suit, it can't win
                }
            }

            return winner;
        }

        private void EvaluateContract()
        {
            if (CurrentContract == null) return;

            var declarerTricks = TricksWon[Declarer] + TricksWon[Dummy];
            var requiredTricks = CurrentContract.Level + 6; // 6 tricks base + contract level

            Console.WriteLine($"\nContract Evaluation:");
            Console.WriteLine($"Declarer needed: {requiredTricks} tricks");
            Console.WriteLine($"Declarer made: {declarerTricks} tricks");

            if (declarerTricks >= requiredTricks)
            {
                var overtricks = declarerTricks - requiredTricks;
                Console.WriteLine($"✓ Contract made! (+{overtricks} overtricks)");

                // Calculate score (simplified)
                var baseScore = CalculateBaseScore(CurrentContract.Level, CurrentContract.Suit);
                var overtrickScore = overtricks * 30; // Simplified overtrick scoring
                var totalScore = baseScore + overtrickScore;

                Console.WriteLine($"Score: {totalScore} points");
            }
            else
            {
                var undertricks = requiredTricks - declarerTricks;
                Console.WriteLine($"✗ Contract failed! (-{undertricks} undertricks)");

                // Calculate penalty (simplified)
                var penalty = undertricks * 50; // Simplified penalty scoring
                Console.WriteLine($"Penalty: {penalty} points");
            }
        }

        private int CalculateBaseScore(int level, Suit suit)
        {
            var baseScore = level * 30; // Simplified scoring

            // Major suits (Spades, Hearts) score more
            if (suit == Suit.Spades || suit == Suit.Hearts)
            {
                baseScore = level * 30;
            }
            else
            {
                baseScore = level * 20; // Minor suits
            }

            return baseScore;
        }

        public void PrintScores()
        {
            Console.WriteLine();
            Console.WriteLine("Tricks won:");
            foreach (var kvp in TricksWon)
                Console.WriteLine($"{kvp.Key[0]}: {kvp.Value}");
        }
    }
}
