using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SnakeOMania.Library
{
    public class Player
    {
        public event Func<object, EventArgs, Task> LeftGameSession;

        public Player()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }

        public ushort ClientVersion { get; set; }

        public string Name { get; set; }

        public Socket Connection { get; set; }

        public async Task OnLeftGameSession()
        {
            Func<object, EventArgs, Task> handler = LeftGameSession;

            if (handler == null)
            {
                return;
            }

            Delegate[] invocationList = handler.GetInvocationList();
            Task[] handlerTasks = new Task[invocationList.Length];

            for (int i = 0; i < invocationList.Length; i++)
            {
                handlerTasks[i] = ((Func<object, EventArgs, Task>)invocationList[i])(this, EventArgs.Empty);
            }

            await Task.WhenAll(handlerTasks);
        }
    }
}
