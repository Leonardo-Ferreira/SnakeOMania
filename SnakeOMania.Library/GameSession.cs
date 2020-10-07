using System;
using System.Collections.Generic;

namespace SnakeOMania.Library
{
    public class GameSession
    {
        public int SessionId { get; set; }
        public Board Board { get; set; }
        public List<Snake> Players { get; set; }
        public bool Running { get; set; }
    }
}
