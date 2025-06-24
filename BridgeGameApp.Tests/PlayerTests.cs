using Xunit;
using BridgeGameApp.Models;

namespace BridgeGameApp.Tests
{
    public class PlayerTests
    {
        [Fact]
        public void Player_InitializesWithNameAndEmptyHand()
        {
            var player = new Player("TestPlayer");
            Assert.Equal("TestPlayer", player.Name);
            Assert.Empty(player.Hand);
        }

        [Fact]
        public void Player_CanAddCardToHand()
        {
            var player = new Player("TestPlayer");
            var card = new Card(Suit.Spades, Rank.Ace);
            player.Hand.Add(card);
            Assert.Single(player.Hand);
            Assert.Equal(card, player.Hand[0]);
        }
    }
}
