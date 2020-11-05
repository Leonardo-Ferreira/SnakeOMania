using System;
using System.Net.Sockets;

namespace SnakeOMania.Library
{
    public class Player
    {
        public Player()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }

        public ushort ClientVersion { get; set; }

        public string Name { get; set; }

        public Socket Connection { get; set; }
    }
}
