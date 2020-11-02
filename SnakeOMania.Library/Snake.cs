﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SnakeOMania.Library
{
    public class Snake
    {
        //The first Section of the Array represents the head and so on
        public List<SnakeBodySection> BodySections { get; set; }
        private int SectionCount { get; set; }

        public Snake(int top, int left)
        {
            var headSection = new SnakeBodySection()
            {
                Head = new Point(left, top),
                Heading = Direction.Right,
                Tail = new Point(left - 3, top)
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
    }

    public class SnakeBodySection
    {
        public Direction Heading { get; set; }
        public Point Head { get; set; }
        public Point Tail { get; set; }
    }
}
