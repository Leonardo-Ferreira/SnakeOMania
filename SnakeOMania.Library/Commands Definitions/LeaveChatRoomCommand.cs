using System;
namespace SnakeOMania.Library
{
    public class LeaveChatRoomCommand : ICommand
    {
        public uint Room { get; set; }

        public CommandId Definition => CommandId.LeaveChatRoom;

        public void Deserialize(Span<byte> data)
        {
            Room = BitConverter.ToUInt32(data.Slice(1));
        }

        public Memory<byte> Serialize()
        {
            byte[] buffer = new byte[5];
            var mem = new Memory<byte>(buffer);
            buffer[0] = (byte)Definition;
            Array.Copy(BitConverter.GetBytes(Room), 0, buffer, 1, 4);
            return mem;
        }
    }
}
