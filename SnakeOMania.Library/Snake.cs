using System;
using System.Drawing;

namespace SnakeOMania.Library
{
    public class Snake
    {
        //The first Section of the Array represents the head and so on
        public SnakeBodySection[] BodySections { get; set; }
        private int SectionCount { get; set; }

        public Snake(int top, int left)
        {
            BodySections = new SnakeBodySection[] {
                new SnakeBodySection()
            {
                Head = new Point(left,top),
                Heading = Direction.None
            }
            };
        }

        public void Turn(Direction direction)
        {
            if (direction == Direction.None)
            {
                return;
            }
            var mainDirection = BodySections[0].Heading;
            int newX;
            int newY;
            if (direction.HasFlag(Direction.HorizontalOrder))
            {
                if (mainDirection.HasFlag(Direction.HorizontalOrder))
                {
                    direction = mainDirection;
                }
                newX = BodySections[0].Head.X + (direction == Direction.Left ? -1 : 1);
                newY = BodySections[0].Head.Y;
                BodySections[0].Heading = direction;
            }
            else
            {
                if (mainDirection.HasFlag(Direction.VerticalOrder))
                {
                    direction = mainDirection;
                }
                newX = BodySections[0].Head.X;
                newY = BodySections[0].Head.Y + (direction == Direction.Down ? 1 : -1);
                BodySections[0].Heading = direction;
            }

            BodySections[0].Head = new Point(newX, newY);
        }
    }

    public struct SnakeBodySection
    {
        public Direction Heading { get; set; }
        public Point Head { get; set; }
        public Point? Tail { get; set; }
    }
}
