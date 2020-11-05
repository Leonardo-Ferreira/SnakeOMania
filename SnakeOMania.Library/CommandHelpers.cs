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
                default:
                    result = null;
                    break;
            }
            return result;
        }
    }
}
