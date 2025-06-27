using System.Collections.Generic;
using BridgeGameApp.GameLogic;
using BridgeGameApp.Models;

var playerNames = new List<string> { "North", "East", "South", "West" };
var gameManager = new GameManager(playerNames);
gameManager.StartGame();
void PrintHandVisual(List<Player> players)
{
    var suits = new[] { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
    var north = players.First(p => p.Name == "North");
    var south = players.First(p => p.Name == "South");
    var west = players.First(p => p.Name == "West");
    var east = players.First(p => p.Name == "East");

    string[] FormatHandLines(Player p)
    {
        var sortedHand = p.Hand
            .OrderBy(card => card.Suit)
            .ThenByDescending(card => card.Rank)
            .ToList();
        int maxCardsInSuit = suits.Max(suit => players.Max(pl => pl.Hand.Count(c => c.Suit == suit)));
        return suits.Select(suit =>
        {
            var cards = sortedHand.Where(card => card.Suit == suit).ToList();
            var suitIcon = suit switch
            {
                Suit.Spades => "\u2660", // ♠
                Suit.Hearts => "\u2665", // ♥
                Suit.Diamonds => "\u2666", // ♦
                Suit.Clubs => "\u2663", // ♣
                _ => "?"
            };
            string cardStr = cards.Count == 0 ? "" : string.Join(" ", cards.Select(card => card.ToString().Split(' ')[1]));
            return (suitIcon + (cardStr.Length > 0 ? " " + cardStr : "")).PadRight(2 + maxCardsInSuit * 2);
        }).ToArray();
    }

    var northLines = FormatHandLines(north);
    var southLines = FormatHandLines(south);
    var westLines = FormatHandLines(west);
    var eastLines = FormatHandLines(east);

    int maxLineLength = new[] { northLines, southLines, westLines, eastLines }
        .SelectMany(lines => lines)
        .Where(line => !string.IsNullOrWhiteSpace(line))
        .Select(line => line.Length)
        .DefaultIfEmpty(0)
        .Max();

    string Center(string s, int width) => s.PadLeft((width + s.Length) / 2).PadRight(width);

    int totalWidth = maxLineLength * 2 + 3; // 3 for spacing between W and E
    int centerPad = maxLineLength + 2; // Pad N and S to align with center between W and E
    int eastPad = maxLineLength + 6; // Increase padding to create an empty square at the center
    int centerLabelPad = totalWidth / 2;
    int labelCol = 2 + (maxLineLength > 2 ? 2 : 0); // Column for the second card (after suit icon and first card)

    // N hand
    int nSecondIdx = northLines[0].IndexOf(' ', 2);
    if (nSecondIdx != -1)
    {
        nSecondIdx++;
        while (nSecondIdx < northLines[0].Length && northLines[0][nSecondIdx] == ' ') nSecondIdx++;
    }
    else
    {
        nSecondIdx = northLines[0].Length / 2;
    }
    Console.WriteLine();
    Console.WriteLine(new string(' ', centerPad + nSecondIdx) + "N");
    Console.WriteLine();
    foreach (var line in northLines)
        Console.WriteLine(new string(' ', centerPad) + line);
    Console.WriteLine();
    // W and E hands
    int wSecondIdx = westLines[0].IndexOf(' ', 2);
    if (wSecondIdx != -1)
    {
        wSecondIdx++;
        while (wSecondIdx < westLines[0].Length && westLines[0][wSecondIdx] == ' ') wSecondIdx++;
    }
    else
    {
        wSecondIdx = westLines[0].Length / 2;
    }

    // Find the first suit line for East that has at least 2 cards
    int eLineIdx = 0;
    int eSecondIdx = 0;
    for (int i = 0; i < eastLines.Length; i++)
    {
        if (eastLines[i].Trim().Length > 2)
        {
            eLineIdx = i;
            eSecondIdx = eastLines[i].IndexOf(' ', 2);
            if (eSecondIdx != -1)
            {
                eSecondIdx++;
                while (eSecondIdx < eastLines[i].Length && eastLines[i][eSecondIdx] == ' ') eSecondIdx++;
            }
            else
            {
                eSecondIdx = eastLines[i].Length / 2;
            }
            break;
        }
    }

    // Calculate padding for W and E labels to align with displayed hands
    int wPad = maxLineLength - westLines[0].Length;
    int eBlockStart = maxLineLength + eastPad; // Start of East hand block
    Console.WriteLine(
        new string(' ', wPad + wSecondIdx) + "W" +
        new string(' ', eBlockStart - (wPad + wSecondIdx + 1) + eSecondIdx) + "E");
    Console.WriteLine();
    for (int i = 0; i < 4; i++)
    {
        string w = i < westLines.Length ? westLines[i] : "";
        string e = i < eastLines.Length ? eastLines[i] : "";
        Console.WriteLine($"{w.PadLeft(maxLineLength)}{new string(' ', eastPad)}{e.PadLeft(maxLineLength)}");
    }
    Console.WriteLine();
    // S hand
    int sSecondIdx = southLines[0].IndexOf(' ', 2);
    if (sSecondIdx != -1)
    {
        sSecondIdx++;
        while (sSecondIdx < southLines[0].Length && southLines[0][sSecondIdx] == ' ') sSecondIdx++;
    }
    else
    {
        sSecondIdx = southLines[0].Length / 2;
    }
    Console.WriteLine(new string(' ', centerPad + sSecondIdx) + "S");
    Console.WriteLine();
    foreach (var line in southLines)
        Console.WriteLine(new string(' ', centerPad) + line);
    Console.WriteLine();
}

PrintHandVisual(gameManager.Players);

// Enhanced bidding with AI players
var biddingManager = new BiddingManager(playerNames);

Console.WriteLine("Starting bidding phase...\n");

// Simulate AI bidding until bidding is over
while (!biddingManager.IsBiddingOver())
{
    var currentPlayer = gameManager.Players.First(p => p.Name == biddingManager.CurrentPlayer);

    // Make AI bid
    biddingManager.MakeAIBid(currentPlayer);

    // Print the last bid
    var lastBid = biddingManager.Bids.Last();
    var playerInitial = lastBid.Player[0];

    Console.Write($"{playerInitial}: ");
    switch (lastBid.Type)
    {
        case BidType.Pass:
            Console.WriteLine("Pass");
            break;
        case BidType.Number:
            var suitSymbol = lastBid.Suit!.Value switch
            {
                Suit.Spades => "♠",
                Suit.Hearts => "♥",
                Suit.Diamonds => "♦",
                Suit.Clubs => "♣",
                _ => "?"
            };
            Console.WriteLine($"{lastBid.Level}{suitSymbol}");
            break;
    }

    // Add a small delay to make bidding more readable
    Thread.Sleep(500);
}

Console.WriteLine();
biddingManager.PrintBiddingHistory();

// After bidding, play the game and print scores
Console.WriteLine();
gameManager.PlayGame(biddingManager);
gameManager.PrintScores();
