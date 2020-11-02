﻿using System;
using System.Threading.Tasks;
using SnakeOMania.Library;

namespace SnakeOMania.ConsoleClient
{
    class Program
    {
        Snake _mainSnake;
        Board _gameBoard;
        bool _gameRunning;
        Direction _lastOrder;

        static async Task Main(string[] args)
        {
            Program p = new Program();
            p.StartAndRun();
        }

        public void StartAndRun()
        {
            _gameBoard = new Board() { Size = 13 };
            _mainSnake = new Snake(4, 4);

            DrawEmptyBoard();
            Console.ReadKey();
            _gameRunning = true;

            Console.CursorVisible = false;

            Task.Run(async () =>
            {
                while (_gameRunning)
                {
                    Tick();
                    await Task.Delay(100);
                }
            });

            Loop();
        }

        void DrawEmptyBoard()
        {
            Console.Clear();
            for (int i = 0; i < _gameBoard.Size + 2; i++)
            {
                Console.Write("=");
            }
            Console.SetCursorPosition(0, _gameBoard.Size + 1);
            for (int i = 0; i < _gameBoard.Size + 2; i++)
            {
                Console.Write("=");
            }
            for (int i = 1; i < _gameBoard.Size + 1; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write("║");
            }
            for (int i = 1; i < _gameBoard.Size + 1; i++)
            {
                Console.SetCursorPosition(_gameBoard.Size + 1, i);
                Console.Write("║");
            }
            Console.SetCursorPosition(_gameBoard.Size, _gameBoard.Size);
        }

        void RenderSnake(Snake snake)
        {
            for (int i = 0; i < snake.BodySections.Count; i++)
            {
                var currentPos = snake.BodySections[i].Head;
                Direction currentDirection = snake.BodySections[i].Heading;
                do
                {
                    Console.SetCursorPosition(currentPos.X + 1, currentPos.Y + 1);
                    if (currentPos == snake.BodySections[0].Head)
                    {
                        Console.Write("H");
                    }
                    else
                    {
                        Console.Write("o");
                    }
                    switch (currentDirection)
                    {
                        case Direction.Up:
                            {
                                currentPos.Y++;
                                break;
                            }
                        case Direction.Down:
                            {
                                currentPos.Y--;
                                break;
                            }
                        case Direction.Left:
                            {
                                currentPos.X++;
                                break;
                            }
                        case Direction.Right:
                            {
                                currentPos.X--;
                                break;
                            }
                        default:
                            break;
                    }
                } while (currentPos != snake.BodySections[i].Tail);
            }
        }

        void RenderApple()
        {
            Console.SetCursorPosition(_gameBoard.AppleLocation.X + 1, _gameBoard.AppleLocation.Y + 1);
            Console.Write("A");
        }

        void Loop()
        {
            while (true)
            {
                var keyRead = Console.ReadKey(true);
                if (keyRead.Key == ConsoleKey.Escape)
                {
                    _gameRunning = false;
                    break;
                }

                if (keyRead.Key == ConsoleKey.UpArrow)
                {
                    _lastOrder = Direction.Up;
                }
                else if (keyRead.Key == ConsoleKey.DownArrow)
                {
                    _lastOrder = Direction.Down;
                }
                else if (keyRead.Key == ConsoleKey.LeftArrow)
                {
                    _lastOrder = Direction.Left;
                }
                else if (keyRead.Key == ConsoleKey.RightArrow)
                {
                    _lastOrder = Direction.Right;
                }
                else if (keyRead.Key == ConsoleKey.R)
                {
                    _gameBoard.RelocateApple();
                }
            }
        }

        void Tick()
        {
            if (!_gameRunning)
            {
                return;
            }

            DrawEmptyBoard();

            _mainSnake.Move(_lastOrder);

            RenderSnake(_mainSnake);

            if (_mainSnake.BodySections[0].Head == _gameBoard.AppleLocation)
            {
                _mainSnake.NotifyAteApple();
                while (_mainSnake.BodySections[0].Head == _gameBoard.AppleLocation)
                {
                    _gameBoard.RelocateApple();
                }                
            }

            RenderApple();

            Console.SetCursorPosition(_gameBoard.Size + 3, _gameBoard.Size + 3);
        }
    }
}