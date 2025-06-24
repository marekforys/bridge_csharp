using System.Collections.Generic;
using BridgeGameApp.GameLogic;
using BridgeGameApp.Models;

var playerNames = new List<string> { "North", "East", "South", "West" };
var gameManager = new GameManager(playerNames);
gameManager.StartGame();
foreach (var player in gameManager.Players)
{
    Console.WriteLine($"{player.Name}");
    var suits = new[] { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
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

// After dealing cards, simulate a bidding round
var biddingManager = new BiddingManager(playerNames);
biddingManager.MakeBid(BidType.Number, 1, Suit.Spades); // North: 1S
biddingManager.MakeBid(BidType.Pass);                   // East: Pass
biddingManager.MakeBid(BidType.Number, 2, Suit.Hearts); // South: 2H
biddingManager.MakeBid(BidType.Pass);                   // West: Pass
biddingManager.MakeBid(BidType.Pass);                   // North: Pass
biddingManager.MakeBid(BidType.Pass);                   // East: Pass
Console.WriteLine("Bidding:");
foreach (var bid in biddingManager.Bids)
    Console.WriteLine(bid);

// After bidding, play the game and print scores
Console.WriteLine();
gameManager.PlayGame();
gameManager.PrintScores();
