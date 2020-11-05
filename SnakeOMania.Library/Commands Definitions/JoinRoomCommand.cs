using System;
using System.Text;

namespace SnakeOMania.Library
{
    public class JoinRoomCommand : ICommand
    {
        public CommandId Definition => CommandId.JoinChatRoom;

        public string RoomName { get; set; }

        public void Deserialize(Span<byte> data)
        {
            RoomName = Encoding.UTF8.GetString(data.Slice(1));
        }

        public Memory<byte> Serialize()
        {
            var total = RoomName.Length + 3;
            byte[] buff = new byte[total];
            Span<byte> spn = new Span<byte>(buff);
            spn[0] = (byte)Definition;
            spn[1] = (byte)(RoomName.Length + 1);
            var bytes = Encoding.UTF8.GetBytes(RoomName);
            Array.Copy(bytes, 0, buff, 3, bytes.Length);
            return new Memory<byte>(buff);
        }

        public override string ToString()
        {
            return "Join Room " + RoomName;
        }
    }
}
