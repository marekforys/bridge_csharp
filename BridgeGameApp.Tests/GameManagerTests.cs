using Xunit;
using BridgeGameApp.GameLogic;
using System.Collections.Generic;
using System.Linq;

namespace BridgeGameApp.Tests
{
    public class GameManagerTests
    {
        [Fact]
        public void GameManager_InitializesWithFourPlayers()
        {
            var names = new List<string> { "North", "East", "South", "West" };
            var manager = new GameManager(names);
            Assert.Equal(4, manager.Players.Count);
            Assert.All(manager.Players, p => Assert.Contains(p.Name, names));
        }

        [Fact]
        public void GameManager_StartGame_Deals13CardsToEachPlayer()
        {
            var names = new List<string> { "North", "East", "South", "West" };
            var manager = new GameManager(names);
            manager.StartGame();
            Assert.All(manager.Players, p => Assert.Equal(13, p.Hand.Count));
            Assert.Equal(0, manager.Deck.Count);
        }
    }
}
