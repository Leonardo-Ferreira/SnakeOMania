using System;
using System.Drawing;

namespace SnakeOMania.Library
{
    public class Snake
    {
        //The first Section of the Array represents the head and so on
        public SnakeBodySection[] BodySections { get; set; }
    }

    public struct SnakeBodySection
    {
        public Point Head { get; set; }
        public Point Tail { get; set; }
    }
}
