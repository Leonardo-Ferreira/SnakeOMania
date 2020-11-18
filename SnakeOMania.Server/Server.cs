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

        ConcurrentDictionary<uint, (string RoomName, List<Player> Players)> _chatRooms;
        ConcurrentBag<GameSession> _gameSessions;
        ConcurrentQueue<(ICommand Command, Player Player)> _toBeExecuted;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Server Starting");

            Server p = new Server();
            p._configs = new ServerConfigurations();
            p._chatRooms = new ConcurrentDictionary<uint, (string RoomName, List<Player> Players)>();
            p._chatRooms.TryAdd(0, ("Lobby", new List<Player>()));
            p._chatRooms.TryAdd(1, ("Looking For Group", new List<Player>()));
            p._chatRooms.TryAdd(2, ("AFK", new List<Player>()));

            var serverT = p.StartServer();
            var dispatcher = new CommandDispatcher(p._chatRooms, p._gameSessions);
            p._toBeExecuted = dispatcher.ExecutionQueue;
            var dispatchingT = dispatcher.StartCommandDispatching();

            await serverT;
            await dispatchingT;
        }

        public async Task StartServer()
        {
            _activePlayers = new List<Player>();
            _gameSessions = new ConcurrentBag<GameSession>();
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, _configs.DefaultPort);

            try
            {
                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(10);

                Console.WriteLine("Accepting Connections");

                while (true)
                {
                    var player = await TryAcquirePlayer(listener);
                    if (player == null)
                    {
                        continue;
                    }

                    _activePlayers.Add(player);
                    _chatRooms[0].Players.Add(player);
                    _chatRooms[1].Players.Add(player);

                    var pt = Task.Run(async () =>
                    {
                        await new PlayerInputHandler(false).Handle(player, _toBeExecuted);
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

        private async Task<Player> TryAcquirePlayer(Socket listener)
        {
            Socket handler = await listener.AcceptAsync();
            handler.NoDelay = true;
            handler.Blocking = false;

            var handshakeResult = await Handshake(handler);
            return handshakeResult;
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
