﻿using System;
namespace SnakeOMania.Library
{
    [Flags]
    public enum Direction
    {
        None = 0,
        HorizontalOrder = 0b00000001,
        VerticalOrder = 0b00000010,
        Left = 0b00000101,
        Right = 0b00001001,
        Up = 0b00000110,
        Down = 0b00001010
    }
}
