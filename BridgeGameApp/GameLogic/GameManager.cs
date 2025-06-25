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
            // Initialize tricks won
            TricksWon = Players.ToDictionary(p => p.Name, p => 0);
            // Each player plays one card per trick, 13 tricks in total
            for (int trick = 0; trick < 13; trick++)
            {
                Console.WriteLine($"\nTrick {trick + 1}:");
                var trickCards = new List<(string Player, Card Card)>();
                for (int i = 0; i < Players.Count; i++)
                {
                    var player = Players[i];
                    var card = player.Hand[0];
                    player.Hand.RemoveAt(0);
                    trickCards.Add((player.Name, card));
                    Console.WriteLine($"  {player.Name[0]}: {card}");
                }
                var leadSuit = trickCards[0].Card.Suit;
                var winningCard = trickCards
                    .Where(tc => tc.Card.Suit == leadSuit)
                    .OrderByDescending(tc => tc.Card.Rank)
                    .First();
                TricksWon[winningCard.Player]++;
                Console.WriteLine($"  Winner: {winningCard.Player[0]} with {winningCard.Card}");
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
