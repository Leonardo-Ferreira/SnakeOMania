using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SnakeOMania.Library
{
    public class GameSession
    {
        public uint SessionId { get; set; }
        public Board Board { get; set; }
        public Dictionary<Guid, (Player, Snake)> Players { get; set; }
        public GameStatus Status { get; set; }
        public int Difficulty { get; set; } = 200;

        ConcurrentQueue<(ICommand Command, Player Player)> _toBeExecuted;
        ConcurrentDictionary<uint, (string RoomName, List<Player> Players)> chatRoom;

        public GameSession()
        {
            Players = new Dictionary<Guid, (Player, Snake)>();
            Board = new Board() { Size = 10, OpenEdge = false };
            Status = GameStatus.PreGame;
            Difficulty = 250;

            chatRoom = new ConcurrentDictionary<uint, (string RoomName, List<Player> Players)>();
            chatRoom[0] = ("Game Chat", Players.Select(i => i.Value.Item1).ToList());

            var sessionDispatcher = new CommandDispatcher(chatRoom, null);
            _toBeExecuted = sessionDispatcher.ExecutionQueue;
            sessionDispatcher.StartCommandDispatching();
        }

        public void EnqueueCommand(ICommand command, Player target)
        {
            _toBeExecuted.Enqueue((command, target));
        }

        public void CommandReceived(int player, Direction direction)
        {
            /*
            var snake = Players[player];
            if(snake.BodySections[0].Heading != direction)
            {
                int sectionsToMove;
                if (snake)
                {

                }
            }
            */
        }

        public void Join(Player player)
        {
            if (Players.ContainsKey(player.Id))
            {
                return;
            }
            Players.Add(player.Id, (player, new Snake(0, 0)));

            chatRoom[0].Players.Add(player);
        }

        public async Task TakeOver(Player player, Memory<byte> playerBuffer)
        {
            await new PlayerInputHandler(true).Handle(player, _toBeExecuted, playerBuffer);

            player.OnLeftGameSession();
        }
    }
}
