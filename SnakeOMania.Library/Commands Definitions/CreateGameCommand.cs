using System;
namespace SnakeOMania.Library
{
    public class CreateGameCommand : ICommand
    {
        public CommandId Definition => CommandId.CreateGame;

        public void Deserialize(Span<byte> data)
        {

        }

        public Memory<byte> Serialize()
        {
            return new Memory<byte>(new byte[1] { (byte)Definition });
        }

        //DO not serialize this
        public Memory<byte> CurrentPlayerBuffer { get; set; }
    }
}
