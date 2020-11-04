using System;
namespace SnakeOMania.Library
{
    public class ServerConfigurations
    {
        public ServerConfigurations()
        {
        }

        public ushort MinimumClientVersion { get; set; } = 0;

        public ushort MaximumClientVersion { get; set; } = 10;

        public ushort MinimumNumberOfClientPerGame { get; set; } = 1;

        public bool HasLobby { get; set; } = true;

        public bool PlayersMustHaveName { get; set; } = true;

        public short MaximumPlayerNameLength { get; set; } = 15;

        public int DefaultPort { get; set; } = 10000;

    }
}
