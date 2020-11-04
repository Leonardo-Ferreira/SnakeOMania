using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SnakeOMania.Library;

namespace SnakeOMania.Server
{
    class Program
    {
        ServerConfigurations _configs;

        List<Player> _activePlayers;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
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

                    var handshakeResult = await Handshake(handler);
                    if (handshakeResult != null)
                    {
                        _activePlayers.Add(handshakeResult);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\n Press any key to continue...");
            Console.ReadKey();
        }

        internal async Task<Player> Handshake(Socket socket)
        {
            var src = new CancellationTokenSource(3000);
            var handshakeData = new Memory<byte>();
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

            return result;
        }
    }
}
