using System;
using System.Net.Sockets;

namespace SnakeOMania.Library
{
    public class Player
    {
        public Player()
        {
        }

        public ushort ClientVersion { get; set; }

        public string Name { get; set; }

        public Socket Connection { get; set; }
    }
}
