using System.Collections.Generic;
using BridgeGameApp.GameLogic;
using BridgeGameApp.Models;

var playerNames = new List<string> { "North", "East", "South", "West" };
var gameManager = new GameManager(playerNames);
gameManager.StartGame();
foreach (var player in gameManager.Players)
{
    Console.WriteLine($"{player.Name}'s hand:");
    var suits = new[] { Suit.Clubs, Suit.Diamonds, Suit.Hearts, Suit.Spades };
    var sortedHand = player.Hand
        .OrderBy(card => card.Suit)
        .ThenByDescending(card => card.Rank)
        .ToList();
    foreach (var suit in suits)
    {
        var cardsInSuit = sortedHand.Where(card => card.Suit == suit).ToList();
        if (cardsInSuit.Count > 0)
        {
            Console.Write($"{cardsInSuit[0].ToString().Split(' ')[0]}: "); // Print suit icon
            foreach (var card in cardsInSuit)
                Console.Write($"{card.ToString().Split(' ')[1]} "); // Print rank symbol
            Console.WriteLine();
        }
    }
    Console.WriteLine();
}
