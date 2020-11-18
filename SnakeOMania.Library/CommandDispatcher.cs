using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SnakeOMania.Library
{
    public class CommandDispatcher
    {
        ConcurrentQueue<(ICommand Command, Player Player)> _toBeExecuted;
        ConcurrentDictionary<uint, (string RoomName, List<Player> Players)> _chatRooms;
        ConcurrentBag<GameSession> _gameSessions;

        public ConcurrentQueue<(ICommand Command, Player Player)> ExecutionQueue => _toBeExecuted;

        public CommandDispatcher(ConcurrentDictionary<uint, (string RoomName, List<Player> Players)> chatRooms, ConcurrentBag<GameSession> gameSessions)
        {
            _toBeExecuted = new ConcurrentQueue<(ICommand Command, Player Player)>();
            _chatRooms = chatRooms;
            _gameSessions = gameSessions;
        }

        public async Task StartCommandDispatching()
        {
            while (true)
            {
                if (!_toBeExecuted.TryDequeue(out var item))
                {
                    await Task.Delay(50);
                    continue;
                }

                switch (item.Command.Definition)
                {
                    case CommandId.SendChat:
                        DispatchChatCommand((ChatCommand)item.Command, item.Player);
                        break;
                    case CommandId.ListChatRooms:
                        ExecuteListChatRoomsCommand(item.Player);
                        break;
                    case CommandId.JoinChatRoom:
                        ExecuteJoinChatRoomCommand((JoinRoomCommand)item.Command, item.Player);
                        break;
                    case CommandId.LeaveChatRoom:
                        ExecuteLeaveChatRoomCommand((LeaveChatRoomCommand)item.Command, item.Player);
                        break;
                    case CommandId.CreateGame:
                        OpenNewGameSession((CreateGameCommand)item.Command, item.Player);
                        break;
                    default:
                        Debug.WriteLine("unkown command: " + item.Command.ToString());
                        break;
                }
            }
        }

        private GameSession OpenNewGameSession(CreateGameCommand command, Player player)
        {
            var session = new GameSession();
            session.Board = new Board();
            session.SessionId = _gameSessions.Any() ? _gameSessions.Max(s => s.SessionId + 1) : 1;
            session.Join(player);

            session.EnqueueCommand(new ListChatRoomsCommand(), player); //Refresh client chat list
            _gameSessions.Add(session);

            session.TakeOver(player, command.CurrentPlayerBuffer);

            return session;
        }

        private void ExecuteLeaveChatRoomCommand(LeaveChatRoomCommand command, Player player)
        {
            var entry = _chatRooms[command.Room];

            entry.Players.RemoveAll(p => p.Id == player.Id);

            _toBeExecuted.Enqueue((new ListChatRoomsCommand(), player));
        }

        private void ExecuteJoinChatRoomCommand(JoinRoomCommand jcr, Player player)
        {
            var entry = _chatRooms.FirstOrDefault(i => i.Value.RoomName == jcr.RoomName).Value;
            if (entry == default)
            {
                var key = _chatRooms.Keys.Max() + 1;
                _chatRooms.TryAdd(key, (jcr.RoomName, new List<Player>() { player }));
            }
            else
            {
                if (entry.Players.Any(p => p.Id == player.Id))
                {
                    return;
                }
                entry.Players.Add(player);
            }
            _toBeExecuted.Enqueue((new ListChatRoomsCommand(), player));
        }

        private void ExecuteListChatRoomsCommand(Player player)
        {
            var rooms = _chatRooms.Select(i => (i.Key, i.Value.RoomName + (i.Value.Players.Any(p => p.Id == player.Id) ? "*" : ""))).ToList();
            var resp = new ListChatRoomsCommandResponse(rooms);
            var buff = resp.Serialize();
            player.Connection.Send(buff.Span);
        }

        private void DispatchChatCommand(ChatCommand cc, Player player)
        {
            cc.By = player.Name;
            var others = _chatRooms[cc.Room].Players;//.Where(p => p.Id != item.Player.Id).ToList();
            var serializedCommand = cc.Serialize();
            foreach (var oplayer in others)
            {
                oplayer.Connection.Send(serializedCommand.Span);
            }
        }
    }
}
