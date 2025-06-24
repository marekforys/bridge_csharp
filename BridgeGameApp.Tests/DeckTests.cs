using Xunit;
using BridgeGameApp.Models;
using System.Linq;

namespace BridgeGameApp.Tests
{
    public class DeckTests
    {
        [Fact]
        public void Deck_InitializesWith52Cards()
        {
            var deck = new Deck();
            Assert.Equal(52, deck.Count);
        }

        [Fact]
        public void Deck_Shuffle_ChangesOrder()
        {
            var deck1 = new Deck();
            var deck2 = new Deck();
            deck2.Shuffle();
            // It's possible (but extremely unlikely) for the order to be the same after shuffling
            var sameOrder = deck1.Deal().ToString() == deck2.Deal().ToString();
            Assert.False(sameOrder && deck1.Count == deck2.Count);
        }

        [Fact]
        public void Deck_Deal_ReducesCount()
        {
            var deck = new Deck();
            var card = deck.Deal();
            Assert.Equal(51, deck.Count);
            Assert.NotNull(card);
        }
    }
}
