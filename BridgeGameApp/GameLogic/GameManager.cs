using System.Collections.Generic;
using System.Linq;
using BridgeGameApp.Models;

namespace BridgeGameApp.GameLogic
{
    public class GameManager
    {
        public List<Player> Players { get; private set; }
        public Deck Deck { get; private set; }
        public Dictionary<string, int> TricksWon { get; private set; } = new();

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

        public void PlayGame()
        {
            TricksWon = Players.ToDictionary(p => p.Name, p => 0);
            int leaderIndex = 0; // Start with North (index 0)
            for (int trick = 0; trick < 13; trick++)
            {
                var trickCards = new List<(string Player, Card Card)>();
                for (int i = 0; i < Players.Count; i++)
                {
                    var player = Players[(leaderIndex + i) % Players.Count];
                    var card = player.Hand[0];
                    player.Hand.RemoveAt(0);
                    trickCards.Add((player.Name, card));
                }
                var leadSuit = trickCards[0].Card.Suit;
                var winningCard = trickCards
                    .Where(tc => tc.Card.Suit == leadSuit)
                    .OrderByDescending(tc => tc.Card.Rank)
                    .First();
                TricksWon[winningCard.Player]++;
                // Print trick in one line with order and winner
                var trickLine = string.Join(" ", trickCards.Select(tc => $"{tc.Player[0]}: {tc.Card}"));
                Console.WriteLine($"Trick {trick + 1}: {trickLine}  Winner: {winningCard.Player[0]}");
                // Set leaderIndex for next trick
                leaderIndex = Players.FindIndex(p => p.Name == winningCard.Player);
            }
        }

        public void PlayGame(BiddingManager biddingManager)
        {
            TricksWon = Players.ToDictionary(p => p.Name, p => 0);
            // Find the final contract (last non-pass bid)
            var contractBid = biddingManager.Bids.LastOrDefault(b => b.Type == BidType.Number);
            if (contractBid == null)
            {
                Console.WriteLine("No contract bid. No play phase.");
                return;
            }
            // Find the declarer: first player who bid the contract suit at the contract level
            var declarerBid = biddingManager.Bids.FirstOrDefault(b => b.Type == BidType.Number && b.Suit == contractBid.Suit);
            var declarerIndex = Players.FindIndex(p => p.Name == declarerBid.Player);
            // The player after declarer starts the play
            int leaderIndex = (declarerIndex + 1) % Players.Count;
            for (int trick = 0; trick < 13; trick++)
            {
                var trickCards = new List<(string Player, Card Card)>();
                for (int i = 0; i < Players.Count; i++)
                {
                    var player = Players[(leaderIndex + i) % Players.Count];
                    var card = player.Hand[0];
                    player.Hand.RemoveAt(0);
                    trickCards.Add((player.Name, card));
                }
                var leadSuit = trickCards[0].Card.Suit;
                var winningCard = trickCards
                    .Where(tc => tc.Card.Suit == leadSuit)
                    .OrderByDescending(tc => tc.Card.Rank)
                    .First();
                TricksWon[winningCard.Player]++;
                var trickLine = string.Join(" ", trickCards.Select(tc => $"{tc.Player[0]}: {tc.Card}"));
                Console.WriteLine($"Trick {trick + 1}: {trickLine}  Winner: {winningCard.Player[0]}");
                leaderIndex = Players.FindIndex(p => p.Name == winningCard.Player);
            }
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
