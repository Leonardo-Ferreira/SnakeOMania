using System;
namespace SnakeOMania.Library
{
    [Flags]
    public enum Direction
    {
        None = 0,
        HorizontalOrder = 0b00000001,
        VerticalOrder = 0b00000010,
        Left = 0b00000101,
        Right = 0b00001001,
        Up = 0b00000110,
        Down = 0b00001010
    }

    public enum HandshakeFailureReason
    {
        Success = 0,
        UnsupportedClient = 1,
        PlayerNameIsTooLong = 2
    }

    public enum CommandId : byte
    {
        Quit = 0,
        ListLobby = 1,
        SendChat = 2,
        CreateGame = 3,
        StartGame = 4,
        ReadyCheck = 5,
        MoveSnake = 6,
        Ping = 7,
        JoinChatRoom = 8,
        LeaveChatRoom = 9,
        ListChatRooms = 10
    }
}
