using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SnakeOMania.Library
{
    public class ListChatRoomsCommand : ICommand
    {
        public ListChatRoomsCommand()
        {
        }

        public CommandId Definition => CommandId.ListChatRooms;

        public void Deserialize(Span<byte> data)
        {

        }

        public Memory<byte> Serialize()
        {
            return new Memory<byte>(new byte[] { (byte)Definition });
        }

        public override string ToString()
        {
            return "List Chat Rooms";
        }
    }

    public class ListChatRoomsCommandResponse : ICommand
    {
        public IEnumerable<(int Id, string Name)> Rooms { get; private set; }

        public CommandId Definition => CommandId.ListChatRooms;

        public ListChatRoomsCommandResponse(List<(int, string)> rooms)
        {
            Rooms = rooms;
        }

        public ListChatRoomsCommandResponse()
        {
        }

        public Memory<byte> Serialize()
        {
            var totLength = Rooms.Sum(i => i.Name.Length + 6); //int is 4 bytes and a extra byte for the ";" and one for the header
            var buff = new byte[totLength];
            var ms = new MemoryStream(buff);
            ms.WriteByte((byte)Definition);
            foreach (var item in Rooms)
            {
                ms.Write(BitConverter.GetBytes(item.Id));
                ms.Write(Encoding.UTF8.GetBytes(item.Name + ";"));
            }
            return buff;
        }

        public void Deserialize(Span<byte> data)
        {
            var result = new List<(int, string)>();
            int index = 1;
            while (index < data.Length - 1)//-1 because it ends in ";"
            {
                var sub = data.Slice(index);

                var length = sub.IndexOf((byte)0x3b);
                var rawItem = data.Slice(index, length);

                var item = (BitConverter.ToInt32(rawItem.Slice(0, 4)), Encoding.UTF8.GetString(rawItem.Slice(4)));

                result.Add(item);

                index += length + 1; //+1 because we must ignore the trailing ";
            }

            Rooms = result;
        }
    }
}
