using System.Collections.Generic;
using BridgeGameApp.GameLogic;

var playerNames = new List<string> { "North", "East", "South", "West" };
var gameManager = new GameManager(playerNames);
gameManager.StartGame();
foreach (var player in gameManager.Players)
{
    Console.WriteLine($"{player.Name}'s hand:");
    foreach (var card in player.Hand)
        Console.WriteLine(card);
    Console.WriteLine();
}
