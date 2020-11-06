using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SnakeOMania.Library;

namespace SnakeOMania.Server.TestClient
{
    class TestClient
    {
        static int chatMessageCount = 3;
        static Socket _mainConnection;

        static uint _currentChatRoom = 0;
        static string switchToChat = null;

        static Dictionary<uint, string> _knownRooms;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting");

            await StartClient();

            await Prepare();

            Console.WriteLine("HANDSHAKE DONE!");
            StartListeningCommands();

            do
            {
                var input = Console.ReadLine();

                ICommand cmd;
                var commandAndParameters = input.Split(" ", 2);
                var baseCommand = commandAndParameters[0].ToLower();

                switch (baseCommand)
                {
                    case "/?":
                        continue;
                        break;
                    case "/join":
                        var jrc = new JoinRoomCommand();
                        jrc.RoomName = commandAndParameters[1];
                        cmd = jrc;
                        switchToChat = jrc.RoomName;
                        break;
                    case "/listchatrooms":
                    case "/listrooms":
                        cmd = new ListChatRoomsCommand();
                        break;
                    case "/leave":
                        var lcrc = new LeaveChatRoomCommand();
                        if (commandAndParameters.Length == 1)
                        {
                            if (_currentChatRoom < 3)
                            {
                                continue;
                            }
                            lcrc.Room = _currentChatRoom;
                        }
                        else
                        {
                            if (commandAndParameters[1] == "Lobby" || commandAndParameters[1] == "Looking For Group" || commandAndParameters[1] == "AFK")
                            {
                                continue;
                            }
                            var roomToLeave = _knownRooms.Where(r => r.Value == commandAndParameters[1]);
                            if (!roomToLeave.Any())
                            {
                                continue;
                            }
                            lcrc.Room = roomToLeave.First().Key;                            
                        }
                        _currentChatRoom = 0;
                        cmd = lcrc;
                        break;
                    default:
                        var auxCmd = new ChatCommand();
                        auxCmd.Message = input;
                        auxCmd.Room = (byte)_currentChatRoom;
                        cmd = auxCmd;
                        break;
                }

                _mainConnection.Send(cmd.Serialize().Span);
            } while (true);
        }

        private static async Task Prepare()
        {
            var cmd = new ListChatRoomsCommand();
            await _mainConnection.SendAsync(cmd.Serialize(), SocketFlags.None);
        }

        private static void StartListeningCommands()
        {
            var receiveTask = Task.Run(async () =>
            {
                byte[] buff = new byte[258];
                var mem = new Memory<byte>(buff);
                while (true)
                {
                    var received = await _mainConnection.ReceiveAsync(mem, SocketFlags.None);
                    var commandType = (CommandId)buff[0];

                    var command = await CommandHelpers.RebuildCommand(mem.Slice(0, received));
                    ExecuteCommand(command);
                }
            });
        }

        private static void ExecuteCommand(ICommand command)
        {
            switch (command.Definition)
            {
                case CommandId.SendChat:
                    PrintChat((ChatCommand)command);
                    break;
                case CommandId.ListChatRooms:
                    var lcr = (ListChatRoomsCommandResponse)command;
                    _knownRooms = lcr.Rooms.ToDictionary(i => i.Id, z => z.Name.Replace("*", ""));
                    if (switchToChat != null)
                    {
                        _currentChatRoom = (byte)lcr.Rooms.First(r => r.Name == switchToChat + "*").Id;
                        switchToChat = null;
                    }
                    else
                    {
                        PrintChatRooms(lcr.Rooms);
                    }
                    break;
                default:
                    break;
            }
        }

        private static void PrintChatRooms(IEnumerable<(uint Id, string Name)> rooms)
        {
            Console.WriteLine(rooms.Count() + " Available Chat Rooms:");
            foreach (var item in rooms)
            {
                Console.WriteLine(item.Name.Replace("*", " (Joined Already)"));
            }
        }

        private static void PrintChat(ChatCommand msg)
        {
            Console.WriteLine($"{msg.By} @ {_knownRooms[msg.Room]}: {msg.Message}");
        }

        public static async Task StartClient()
        {
            byte[] bytes = new byte[1024];
            await Task.Delay(2500);
            try
            {
                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 10000);

                // Create a TCP/IP  socket.    
                Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sender.NoDelay = true;
                sender.Blocking = false;

                // Connect the socket to the remote endpoint. Catch any errors.    
                try
                {
                    // Connect to Remote EndPoint  
                    await sender.ConnectAsync(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    // Encode the data string into a byte array.    
                    byte[] buff = new byte[10];
                    MemoryStream msg = new MemoryStream(buff);
                    msg.Write(BitConverter.GetBytes((ushort)1));
                    msg.Write(Encoding.UTF8.GetBytes("Leonardo"));

                    // Send the data through the socket.    
                    int bytesSent = sender.Send(buff);

                    // Receive the response from the remote device.    
                    int bytesRec = await sender.ReceiveAsync(new Memory<byte>(buff), SocketFlags.None);
                    var resp = (HandshakeFailureReason)BitConverter.ToInt16(buff, 0);

                    if (resp == HandshakeFailureReason.Success)
                    {
                        _mainConnection = sender;
                    }
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
