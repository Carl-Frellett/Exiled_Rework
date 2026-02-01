using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using System.Linq;

namespace DreamPlugin.Chat
{
    public class ChatEventCommand
    {
        public void RegisterEvents()
        {
            RExiled.Events.Handlers.Player.PlayerCommandExecuting += OnPlayerCommandEnter;
        }

        public void UnregisterEvents()
        {
            RExiled.Events.Handlers.Player.PlayerCommandExecuting -= OnPlayerCommandEnter;
        }

        public void OnPlayerCommandEnter(PlayerCommandExecutingEventArgs ev)
        {
            ev.IsAllowed = false;

            if (ev?.Player == null || string.IsNullOrEmpty(ev.Command))
            {
                return;
            }

            string cmd = ev.Command;

            if (cmd.StartsWith("bc "))
            {
                string msg = cmd.Substring(3);
                Map.Broadcast(5, $"<size=30>[聊天] {ev.Player.Nickname} 说: {msg}</size>");
                ev.Player.SendConsoleMessage("聊天消息发送成功", "green");
                return;
            }

            if (cmd.StartsWith("c "))
            {
                string msg = cmd.Substring(2);
                var teammates = Player.List.Where(p => p.Team == ev.Player.Team).ToList();
                foreach (var p in teammates)
                {
                    p.Broadcast(5, $"<size=30>[阵营] {ev.Player.Nickname} 说: {msg}</size>");
                }
                ev.Player.SendConsoleMessage("阵营消息发送成功", "green");
                return;
            }

            ev.Player.SendConsoleMessage("未知指令!", "red");
        }
    }
}