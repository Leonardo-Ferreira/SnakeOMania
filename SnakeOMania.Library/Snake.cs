using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SnakeOMania.Library.Physics;

namespace SnakeOMania.Library
{
    public class Snake
    {
        //The first Section of the Array represents the head and so on
        public List<SnakeBodySection> BodySections { get; set; }
        private bool _ateApple { get; set; }

        public Snake(int top, int left)
        {
            var headSection = new SnakeBodySection()
            {
                Head = new Point(left, top),
                Heading = Direction.Right,
                Tail = new Point(left - 6, top)
            };
            BodySections = new List<SnakeBodySection>() {
              headSection
            };
        }

        public void Move(Direction direction)
        {
            var headSection = BodySections.First();
            var tailSection = BodySections.Last();
            var mainDirection = BodySections[0].Heading;

            if (direction == Direction.None)
            {
                direction = headSection.Heading;
            }

            HandleSnakeHead(direction, headSection, mainDirection);

            HandleSnakeTail(tailSection);

            CheckForSelfCollision();
        }

        public void NotifyAteApple()
        {
            _ateApple = true;
        }

        private void HandleSnakeHead(Direction direction, SnakeBodySection headSection, Direction mainDirection)
        {
            int newX;
            int newY;
            if (direction.HasFlag(Direction.HorizontalOrder))
            {
                if (mainDirection.HasFlag(Direction.HorizontalOrder))
                {
                    //No change in direction
                    direction = mainDirection;

                    newX = BodySections[0].Head.X + (direction == Direction.Left ? -1 : 1);
                    newY = BodySections[0].Head.Y;
                    headSection.Head = new Point(newX, newY);
                }
                else
                {
                    var newHead = new SnakeBodySection()
                    {
                        Head = new Point(headSection.Head.X + (direction == Direction.Left ? -1 : 1), headSection.Head.Y),
                        Heading = direction,
                        Tail = headSection.Head
                    };
                    BodySections.Insert(0, newHead);
                }
            }
            else
            {
                if (mainDirection.HasFlag(Direction.VerticalOrder))
                {
                    //No change in direction
                    direction = mainDirection;

                    newX = BodySections[0].Head.X;
                    newY = BodySections[0].Head.Y + (direction == Direction.Down ? 1 : -1);
                    headSection.Head = new Point(newX, newY);
                }
                else
                {
                    var newHead = new SnakeBodySection()
                    {
                        Head = new Point(headSection.Head.X, headSection.Head.Y + (direction == Direction.Down ? 1 : -1)),
                        Heading = direction,
                        Tail = headSection.Head
                    };
                    BodySections.Insert(0, newHead);
                }
            }
        }

        private void HandleSnakeTail(SnakeBodySection tailSection)
        {
            if (_ateApple)
            {
                _ateApple = false;
                return;
            }

            if (tailSection.Heading.HasFlag(Direction.HorizontalOrder))
            {
                tailSection.Tail = new Point(tailSection.Tail.X + (tailSection.Heading == Direction.Left ? -1 : 1), tailSection.Tail.Y);
            }
            else
            {
                tailSection.Tail = new Point(tailSection.Tail.X, tailSection.Tail.Y + (tailSection.Heading == Direction.Down ? 1 : -1));
            }
            if (tailSection.Head == tailSection.Tail)
            {
                BodySections.RemoveAt(BodySections.Count - 1);
            }
        }

        private void CheckForSelfCollision()
        {
            var secCount = BodySections.Count;
            if (secCount < 2)
            {
                //Cant colide with its own section or right next one
                return;
            }
            var head = BodySections[0].Head;
            for (int i = 2; i < BodySections.Count; i++)
            {
                var sec = BodySections[i];
                if (CollisionChecker.Collided(sec.Head, sec.Tail, head))
                    throw new SnakeCollisionException();
            }
        }
    }

    public class SnakeBodySection
    {
        public Direction Heading { get; set; }
        public Point Head { get; set; }
        public Point Tail { get; set; }
    }
}
