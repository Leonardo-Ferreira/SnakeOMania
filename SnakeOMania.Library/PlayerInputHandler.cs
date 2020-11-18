using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SnakeOMania.Library
{
    public class PlayerInputHandler
    {
        ConcurrentQueue<(ICommand, Player)> _dispatcherQueue;
        bool _isGameSessionHandler;

        public PlayerInputHandler(bool isGameSession)
        {
            _isGameSessionHandler = isGameSession;
        }

        public async Task Handle(Player player, ConcurrentQueue<(ICommand, Player)> dispatcherQueue)
        {
            byte[] buff = new byte[258];
            var mem = new Memory<byte>(buff);
            await Handle(player, dispatcherQueue, mem);
        }

        public async Task Handle(Player player, ConcurrentQueue<(ICommand, Player)> dispatcherQueue, Memory<byte> playerBuffer)
        {
            var loop = true;
            while (loop)
            {
                var received = await player.Connection.ReceiveAsync(playerBuffer, SocketFlags.None);

                var command = await CommandHelpers.RebuildCommand(playerBuffer.Slice(0, received));

                if (_isGameSessionHandler && !CanBeExecutedOnGameSession(command))
                {
                    continue;
                }

                if (command.Definition == CommandId.CreateGame)
                {
                    ((CreateGameCommand)command).CurrentPlayerBuffer = playerBuffer;
                    player.LeftGameSession += Player_LeftGameSession;
                    loop = false;
                }

                dispatcherQueue.Enqueue((command, player));
            }
        }

        private bool CanBeExecutedOnGameSession(ICommand command)
        {
            if (command is CreateGameCommand || command is JoinRoomCommand || command is LeaveChatRoomCommand)
            {
                return false;
            }

            return true;
        }

        private Task Player_LeftGameSession(object player, EventArgs args)
        {
            return Handle((Player)player, _dispatcherQueue);
        }
    }
}
