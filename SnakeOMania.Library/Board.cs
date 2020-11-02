using System;
using System.Drawing;

namespace SnakeOMania.Library
{
    public class Board
    {
        Random _random = new Random();

        public ushort Size { get; set; }
        public bool OpenEdge { get; set; }
        public Point AppleLocation { get; private set; }

        public void RelocateApple()
        {
            var left = _random.Next(Size);
            var top = _random.Next(Size);
            AppleLocation = new Point(left, top);
        }
    }
}
