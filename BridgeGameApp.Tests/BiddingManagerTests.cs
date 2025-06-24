using Xunit;
using BridgeGameApp.GameLogic;
using BridgeGameApp.Models;
using System.Collections.Generic;

namespace BridgeGameApp.Tests
{
    public class BiddingManagerTests
    {
        [Fact]
        public void Bid_ToString_PassAndNumber()
        {
            var passBid = new Bid(BidType.Pass, "North");
            var numberBid = new Bid(BidType.Number, "East", 1, Suit.Spades);
            Assert.Equal("North: Pass", passBid.ToString());
            Assert.Equal("East: 1S", numberBid.ToString());
        }

        [Fact]
        public void BiddingManager_TracksTurnOrder()
        {
            var players = new List<string> { "North", "East", "South", "West" };
            var manager = new BiddingManager(players);
            Assert.Equal("North", manager.CurrentPlayer);
            manager.MakeBid(BidType.Pass);
            Assert.Equal("East", manager.CurrentPlayer);
            manager.MakeBid(BidType.Pass);
            Assert.Equal("South", manager.CurrentPlayer);
        }

        [Fact]
        public void BiddingManager_DetectsBiddingOver()
        {
            var players = new List<string> { "North", "East", "South", "West" };
            var manager = new BiddingManager(players);
            manager.MakeBid(BidType.Pass);
            manager.MakeBid(BidType.Pass);
            manager.MakeBid(BidType.Pass);
            manager.MakeBid(BidType.Pass);
            Assert.True(manager.IsBiddingOver());
        }

        [Fact]
        public void BiddingManager_BiddingNotOverWithLessThanFourBids()
        {
            var players = new List<string> { "North", "East", "South", "West" };
            var manager = new BiddingManager(players);
            manager.MakeBid(BidType.Pass);
            manager.MakeBid(BidType.Pass);
            manager.MakeBid(BidType.Pass);
            Assert.False(manager.IsBiddingOver());
        }
    }
}
