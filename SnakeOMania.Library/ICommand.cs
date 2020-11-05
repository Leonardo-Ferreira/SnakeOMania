using System;
namespace SnakeOMania.Library
{
    public interface ICommand
    {
        CommandId Definition { get; }
        Memory<byte> Serialize();
    }
}
