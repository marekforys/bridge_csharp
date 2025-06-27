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
    string wPadded = westLines[0].PadLeft(maxLineLength);
    string ePadded = eastLines[0].PadLeft(maxLineLength);
    int wSecondIdx = wPadded.IndexOf(' ', 2);
    if (wSecondIdx != -1)
    {
        wSecondIdx++;
        while (wSecondIdx < wPadded.Length && wPadded[wSecondIdx] == ' ') wSecondIdx++;
    }
    else
    {
        wSecondIdx = wPadded.Length / 2;
    }
    int eSecondIdx = ePadded.IndexOf(' ', 2);
    if (eSecondIdx != -1)
    {
        eSecondIdx++;
        while (eSecondIdx < ePadded.Length && ePadded[eSecondIdx] == ' ') eSecondIdx++;
    }
    else
    {
        eSecondIdx = ePadded.Length / 2;
    }
    Console.WriteLine(
        new string(' ', wSecondIdx) + "W" +
        new string(' ', eastPad + eSecondIdx - wSecondIdx - 1) + "E");
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
{
    var playerInitial = bid.Player[0];
    if (bid.Type == BidType.Pass)
        Console.WriteLine($"{playerInitial}: Pass");
    else
        Console.WriteLine($"{playerInitial}: {bid.Level}{bid.Suit.ToString()[0]}");
}

// After bidding, play the game and print scores
Console.WriteLine();
gameManager.PlayGame(biddingManager);
gameManager.PrintScores();
