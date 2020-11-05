using System;
using System.Text;

namespace SnakeOMania.Library
{
    public class ChatCommand : ICommand
    {
        public ChatCommand()
        {
        }

        public byte Room { get; set; } = 0; //Room zero is the lobby

        public string Message { get; set; }

        public CommandId Definition { get; set; } = CommandId.SendChat;

        public Memory<byte> Serialize()
        {
            var total = Message.Length + 1;
            byte[] buff = new byte[total];
            Span<byte> spn = new Span<byte>(buff);
            spn[0] = Room;
            Encoding.UTF8.GetBytes(Message.AsSpan(), spn);
            return new Memory<byte>(buff);
        }
    }
}
