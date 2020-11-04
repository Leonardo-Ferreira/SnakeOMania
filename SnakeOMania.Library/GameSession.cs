using System;
using System.Collections.Generic;

namespace SnakeOMania.Library
{
    public class GameSession
    {
        public int SessionId { get; set; }
        public Board Board { get; set; }
        public Dictionary<int, Snake> Players { get; set; }
        public bool Running { get; set; } = false;
        public int Difficulty { get; set; } = 200;

        public void CommandReceived(int player, Direction direction)
        {
            /*
            var snake = Players[player];
            if(snake.BodySections[0].Heading != direction)
            {
                int sectionsToMove;
                if (snake)
                {

                }
            }
            */
        }
    }
}
