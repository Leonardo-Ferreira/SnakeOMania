using System;
using System.Text;

namespace SnakeOMania.Library
{
    public class ChatCommand : ICommand
    {
        public byte Room { get; set; } = 0; //Room zero is the lobby

        public string Message { get; set; }

        public string By { get; set; }

        //Do not serialize this
        public string RoomName { get; set; }

        public CommandId Definition { get; set; } = CommandId.SendChat;

        public Memory<byte> Serialize()
        {
            var total = Message.Length + 4 + (By?.Length ?? 0);
            byte[] buff = new byte[total];
            Span<byte> spn = new Span<byte>(buff);
            spn[0] = (byte)Definition;
            spn[1] = (byte)(Message.Length + 1);
            spn[2] = Room;
            var bytes = Encoding.UTF8.GetBytes(Message + By);
            Array.Copy(bytes, 0, buff, 3, bytes.Length);
            return new Memory<byte>(buff);
        }

        public void Deserialize(Span<byte> data)
        {
            Room = data[2];
            var rawMessage = Encoding.UTF8.GetString(data.Slice(3));
            Message = rawMessage.Substring(0, data[1] - 1);
            By = rawMessage.Substring(data[1] - 1);
            By = By == "\0" ? null : By;
        }

        public override string ToString()
        {
            return $"{By} @ {Room}: {Message}";
        }
    }
}
