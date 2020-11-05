using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SnakeOMania.Library;

namespace SnakeOMania.Server.TestClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting");

            await StartClient();
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

                // Connect the socket to the remote endpoint. Catch any errors.    
                try
                {
                    // Connect to Remote EndPoint  
                    await sender.ConnectAsync(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    // Encode the data string into a byte array.    
                    byte[] buff = new byte[18];
                    MemoryStream msg = new MemoryStream(buff);
                    msg.Write(BitConverter.GetBytes((ushort)1));
                    msg.Write(Encoding.UTF8.GetBytes("Leonardo"));

                    // Send the data through the socket.    
                    int bytesSent = sender.Send(buff);

                    // Receive the response from the remote device.    
                    int bytesRec = await sender.ReceiveAsync(new Memory<byte>(buff), SocketFlags.None);
                    var resp = (HandshakeFailureReason)BitConverter.ToInt16(buff, 0);


                    // Release the socket.    
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

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
