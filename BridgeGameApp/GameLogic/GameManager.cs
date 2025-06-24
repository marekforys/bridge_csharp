using System.Collections.Generic;
using System.Linq;
using BridgeGameApp.Models;

namespace BridgeGameApp.GameLogic
{
    public class GameManager
    {
        public List<Player> Players { get; private set; }
        public Deck Deck { get; private set; }
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
    }
}
