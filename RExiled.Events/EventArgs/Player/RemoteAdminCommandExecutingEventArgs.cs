using System;
using RExiled.API.Features;

namespace RExiled.Events.EventArgs.Player
{
    /// <summary>
    /// 玩家在管理员面板执行命令时触发的事件
    /// </summary>
    public class RemoteAdminCommandExecutingEventArgs : System.EventArgs
    {
        public RemoteAdminCommandExecutingEventArgs(RExiled.API.Features.Player player, string command, bool isAllowed)
        {
            Player = player;
            Command = command;
            IsAllowed = isAllowed;
        }

        public RExiled.API.Features.Player Player { get; }
        public string Command { get; set; }
        public bool IsAllowed { get; set; }
    }
}