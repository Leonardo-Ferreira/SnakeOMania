using System;
using System.Threading.Tasks;

namespace SnakeOMania.Library
{
    public class CommandHelpers
    {
        public CommandHelpers()
        {
        }

        public static async Task<ICommand> RebuildCommand(CommandId type, Memory<byte> data)
        {
            ICommand result;
            switch (type)
            {
                case CommandId.SendChat:
                    result = (ChatCommand)Activator.CreateInstance(typeof(ChatCommand));
                    result.Deserialize(data.Span);
                    break;
                case CommandId.JoinChatRoom:
                    result = (JoinRoomCommand)Activator.CreateInstance(typeof(JoinRoomCommand));
                    result.Deserialize(data.Span);
                    break;
                case CommandId.ListChatRooms:
                    result = (ListChatRoomsCommandResponse)Activator.CreateInstance(typeof(ListChatRoomsCommandResponse));
                    result.Deserialize(data.Span);
                    break;
                case CommandId.LeaveChatRoom:
                    result = (LeaveChatRoomCommand)Activator.CreateInstance(typeof(LeaveChatRoomCommand));
                    result.Deserialize(data.Span);
                    break;
                default:
                    result = null;
                    break;
            }
            return result;
        }

        public static async Task<ICommand> RebuildCommand(Memory<byte> data)
        {
            var commandType = (CommandId)data.Span[0];

            return await RebuildCommand(commandType, data);
        }
    }
}
