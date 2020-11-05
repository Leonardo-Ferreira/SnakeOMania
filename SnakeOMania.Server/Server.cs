using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SnakeOMania.Library;

namespace SnakeOMania.Server
{
    class Server
    {
        ServerConfigurations _configs;

        List<Player> _activePlayers;

        ConcurrentDictionary<int, (string RoomName, List<Player> Players)> _chatRooms;

        ConcurrentQueue<(ICommand Command, Player Player)> _toBeExecuted;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Server Starting");

            Server p = new Server();
            p._configs = new ServerConfigurations();
            p._toBeExecuted = new ConcurrentQueue<(ICommand Command, Player Player)>();

            var serverT = p.StartServer();
            var dispatchingT = p.StartCommandDispatching();

            await serverT;
            await dispatchingT;
        }

        public async Task StartServer()
        {
            _activePlayers = new List<Player>();
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, _configs.DefaultPort);

            try
            {
                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(10);

                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    Socket handler = await listener.AcceptAsync();
                    handler.NoDelay = true;
                    handler.Blocking = false;

                    var handshakeResult = await Handshake(handler);
                    if (handshakeResult == null)
                    {
                        continue;
                    }

                    _activePlayers.Add(handshakeResult);
                    var pt = Task.Run(async () =>
                    {
                        byte[] buff = new byte[258];
                        var mem = new Memory<byte>(buff);
                        while (true)
                        {
                            var received = await handshakeResult.Connection.ReceiveAsync(mem, SocketFlags.None);

                            var command = await CommandHelpers.RebuildCommand(mem.Slice(0, received));
                            _toBeExecuted.Enqueue((command, handshakeResult));
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\n Press any key to continue...");
            Console.ReadKey();
        }

        public async Task StartCommandDispatching()
        {
            while (true)
            {
                if (!_toBeExecuted.TryDequeue(out var item))
                {
                    await Task.Delay(50);
                    continue;
                }

                switch (item.Command.Definition)
                {
                    case CommandId.SendChat:
                        Debug.WriteLine("Echoing: " + item.Command.ToString());
                        ((ChatCommand)item.Command).By = item.Player.Name;
                        var others = _activePlayers;//.Where(p => p.Id != item.Player.Id).ToList();
                        var serializedCommand = item.Command.Serialize();
                        foreach (var player in others)
                        {
                            player.Connection.Send(serializedCommand.Span);
                        }
                        break;
                    default:
                        Debug.WriteLine("unkown command: " + item.Command.ToString());
                        break;
                }
            }
        }

        internal async Task<Player> Handshake(Socket socket)
        {
            var src = new CancellationTokenSource(3000);
            byte[] buff = new byte[100];
            var handshakeData = new Memory<byte>(buff);
            var received = await socket.ReceiveAsync(handshakeData, SocketFlags.None, src.Token);
            if (received == 0)
            {
                //timeout
                socket.Close();
                return null;
            }

            if (received > 2 + _configs.MaximumPlayerNameLength)
            {
                socket.Send(BitConverter.GetBytes((byte)HandshakeFailureReason.PlayerNameIsTooLong));
                socket.Close();
                return null;
            }
            var cliVersion = BitConverter.ToUInt16(handshakeData.Slice(0, 2).Span);
            if (cliVersion < _configs.MinimumClientVersion || cliVersion > _configs.MaximumClientVersion)
            {
                socket.Send(BitConverter.GetBytes((byte)HandshakeFailureReason.UnsupportedClient));
                socket.Close();
                return null;
            }
            var playerName = Encoding.UTF8.GetString(handshakeData.Slice(2, received - 2).Span);

            var result = new Player();
            result.ClientVersion = cliVersion;
            result.Name = playerName;
            result.Connection = socket;

            socket.Send(BitConverter.GetBytes((byte)HandshakeFailureReason.Success));

            return result;
        }
    }
}
