using System;
using System.Threading.Tasks;
using SnakeOMania.Library;

namespace SnakeOMania.ConsoleClient
{
    class Program
    {
        Snake _mainSnake;
        Board _gameBoard;
        Direction _lastOrder;
        GameSession _session;

        static async Task Main(string[] args)
        {
            Program p = new Program();
            while (p.StartAndRun())
            {
                // do nothing
            }
        }

        private void DrawMenu()
        {
            Console.Clear();
            Console.WriteLine("Snake O'Mania");
            Console.WriteLine("");
            Console.WriteLine("Options:");
            Console.WriteLine("1 - New Game");
            Console.WriteLine("2 - Difficulty Settings");
            Console.WriteLine("Q - Quit");
            Console.WriteLine("");
            Console.WriteLine("Take your pick:");
        }

        public bool StartAndRun()
        {
            DrawMenu();

            var pick = Console.ReadKey();

            if (pick.Key == ConsoleKey.Q)
            {
                return false;
            }

            if (_session == null)
            {
                _session = new GameSession();
            }

            if (pick.Key == ConsoleKey.D2)
            {
                SetDifficultySettings();
                return true;
            }

            _gameBoard = new Board() { Size = 13 };
            _gameBoard.SnakesPlaying.Add(new Snake(4, 4));
            _mainSnake = _gameBoard.SnakesPlaying[0];

            DrawEmptyBoard();
            DrawMainMessage("To Start");
            Console.ReadKey();
            _session.Status = GameStatus.Running;

            Console.CursorVisible = false;

            Task.Run(async () =>
            {
                try
                {
                    while (_session.Status == GameStatus.Running)
                    {
                        Tick();
                        await Task.Delay(_session.Difficulty);
                    }
                }
                catch
                {
                    _session.Status = GameStatus.PostGame;
                    DrawMainMessage("Game Over");
                }

            });

            Loop();
            return true;
        }

        void SetDifficultySettings()
        {
            do
            {
                Console.Clear();
                Console.WriteLine($"Current difficulty is: {_session.Difficulty}");
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("Tip: The smaller the number, faster the snake moves. Maximum difficulty is 25, which means 40 blocks per second.");
                Console.SetCursorPosition(0, 2);
                Console.Write("Enter a new difficulty setting: ");

                var input = Console.ReadLine();
                if (int.TryParse(input,out int set))
                {
                    if (set >= 25)
                    {
                        _session.Difficulty = set;
                        break;
                    }
                }
            } while (true);
        }

        void DrawMainMessage(string msg)
        {
            Console.SetCursorPosition(0, (_gameBoard.Size / 2) - 1);
            Console.WriteLine(" ");
            for (int i = 0; i < (_gameBoard.Size / 2) - (int)Math.Round((decimal)msg.Length / 2, MidpointRounding.AwayFromZero) + 2; i++)
            {
                Console.Write(" ");
            }
            Console.Write(msg);
            for (int i = 0; i < (_gameBoard.Size / 2); i++)
            {
                Console.Write(" ");
            }
            Console.WriteLine();
            for (int i = 0; i < (_gameBoard.Size / 2) - 5; i++)
            {
                Console.Write(" ");
            }
            Console.Write("Press any key");
            for (int i = 0; i < (_gameBoard.Size / 2); i++)
            {
                Console.Write(" ");
            }
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
            while (_session.Status == GameStatus.Running)
            {
                var keyRead = Console.ReadKey(true);
                if (keyRead.Key == ConsoleKey.Escape)
                {
                    _session.Status = GameStatus.PostGame;
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
            }
        }

        void Tick()
        {
            if (_session.Status != GameStatus.Running)
            {
                return;
            }

            DrawEmptyBoard();

            try
            {
                _mainSnake.Move(_lastOrder);
            }
            finally
            {
                RenderSnake(_mainSnake);
            }
            CheckForCollisionAgainstBorders(_mainSnake);

            if (_mainSnake.BodySections[0].Head == _gameBoard.AppleLocation)
            {
                _mainSnake.NotifyAteApple();

                _gameBoard.RelocateApple();
            }

            RenderApple();

            Console.SetCursorPosition(_gameBoard.Size + 3, _gameBoard.Size + 3);
        }

        private void CheckForCollisionAgainstBorders(Snake mainSnake)
        {
            var head = mainSnake.BodySections[0].Head;
            if (head.X == -1 || head.Y == -1)
            {
                // collision with top or left borders
                throw new SnakeCollisionException();
            }
            if (head.X == _gameBoard.Size || head.Y == _gameBoard.Size)
            {
                // collision with bottom or right borders
                throw new SnakeCollisionException();
            }
        }
    }
}