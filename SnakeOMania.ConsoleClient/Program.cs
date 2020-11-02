using System;
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
            _mainSnake = new Snake(0, 0);

            DrawEmptyBoard();
            Console.ReadKey();
            _gameRunning = true;

            Task.Run(async () =>
            {
                while (_gameRunning)
                {
                    Tick();
                    await Task.Delay(500);
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
            Console.SetCursorPosition(snake.BodySections[0].Head.X + 1, snake.BodySections[0].Head.Y + 1);
            Console.Write("H");
            switch (snake.BodySections[0].Heading)
            {
                case Direction.Up:
                    {
                        Console.CursorLeft--;
                        Console.CursorTop--;
                        break;
                    }
                case Direction.Down:
                    {
                        Console.CursorLeft--;
                        Console.CursorTop++;
                        break;
                    }
                case Direction.Left:
                    {
                        Console.CursorLeft--;
                        Console.CursorLeft--;
                        break;
                    }
                default:
                    break;
            }
            Console.Write("X");
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
                //Tick();

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

            RenderApple();

            _mainSnake.Turn(_lastOrder);

            RenderSnake(_mainSnake);
        }
    }
}