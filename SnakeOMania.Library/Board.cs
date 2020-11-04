using System;
using System.Collections.Generic;
using System.Drawing;
using SnakeOMania.Library.Physics;

namespace SnakeOMania.Library
{
    public class Board
    {
        Random _random = new Random();

        public ushort Size { get; set; }
        public bool OpenEdge { get; set; }
        public Point AppleLocation { get; private set; }
        public List<Snake> SnakesPlaying { get; }

        public Board()
        {
            SnakesPlaying = new List<Snake>();
        }

        public void RelocateApple()
        {
            do
            {
                var left = _random.Next(Size);
                var top = _random.Next(Size);
                AppleLocation = new Point(left, top);
            } while (!CheckCollision());
        }

        bool CheckCollision()
        {
            foreach (var item in SnakesPlaying)
            {
                foreach (var section in item.BodySections)
                {
                    if (CollisionChecker.Collided(section.Head, section.Tail, AppleLocation))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
