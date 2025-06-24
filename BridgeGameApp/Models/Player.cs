using System.Collections.Generic;

namespace BridgeGameApp.Models
{
    public class Player
    {
        public string Name { get; set; }
        public List<Card> Hand { get; set; }
        public Player(string name)
        {
            Name = name;
            Hand = new List<Card>();
        }
    }
}
