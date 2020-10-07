using System;
using System.Drawing;

namespace SnakeOMania.Library
{
    public class Board
    {
        public ushort Size { get; set; }
        public bool OpenEdge { get; set; }
        public Point AppleLocation { get; set; }
    }
}
