using Xunit;
using BridgeGameApp.GameLogic;
using BridgeGameApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace BridgeGameApp.Tests
{
    public class GameManagerTrickPlayTests
    {
        [Fact]
        public void PlayGame_Deals13Tricks_TotalTricksIs13()
        {
            var names = new List<string> { "North", "East", "South", "West" };
            var manager = new GameManager(names);
            manager.StartGame();
            var biddingManager = new BiddingManager(names);
            biddingManager.Bids.Add(new Bid(BidType.Number, "North", 1, Suit.Clubs));
            manager.PlayGame(biddingManager);
            int totalTricks = manager.TricksWon.Values.Sum();
            Assert.Equal(13, totalTricks);
        }

        [Fact]
        public void PlayGame_TricksWonKeysMatchPlayers()
        {
            var names = new List<string> { "North", "East", "South", "West" };
            var manager = new GameManager(names);
            manager.StartGame();
            var biddingManager = new BiddingManager(names);
            biddingManager.Bids.Add(new Bid(BidType.Number, "North", 1, Suit.Clubs));
            manager.PlayGame(biddingManager);
            var trickWinners = manager.TricksWon.Keys.OrderBy(x => x).ToList();
            var playerNames = names.OrderBy(x => x).ToList();
            Assert.Equal(playerNames, trickWinners);
        }

        [Fact]
        public void PlayGame_HandIsEmptyAfterPlay()
        {
            var names = new List<string> { "North", "East", "South", "West" };
            var manager = new GameManager(names);
            manager.StartGame();
            var biddingManager = new BiddingManager(names);
            biddingManager.Bids.Add(new Bid(BidType.Number, "North", 1, Suit.Clubs));
            manager.PlayGame(biddingManager);
            Assert.All(manager.Players, p => Assert.Empty(p.Hand));
        }
    }
}
