using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SnakeOMania.Library
{
    public class PlayerInputHandler
    {
        ConcurrentQueue<(ICommand, Player)> _dispatcherQueue;
        public PlayerInputHandler()
        {
        }

        public async Task Handle(Player player, ConcurrentQueue<(ICommand, Player)> dispatcherQueue)
        {
            byte[] buff = new byte[258];
            var mem = new Memory<byte>(buff);
            var loop = true;
            while (loop)
            {
                var received = await player.Connection.ReceiveAsync(mem, SocketFlags.None);

                var command = await CommandHelpers.RebuildCommand(mem.Slice(0, received));

                if (command.Definition == CommandId.CreateGame)
                {
                    ((CreateGameCommand)command).CurrentPlayerBuffer = mem;
                    player.LeftGameSession += Player_LeftGameSession;
                    loop = false;
                }

                dispatcherQueue.Enqueue((command, player));
            }
        }

        private Task Player_LeftGameSession(object player, EventArgs args)
        {
            return Handle((Player)player, _dispatcherQueue);
        }
    }
}
