namespace RExiled.Events.EventArgs.Player
{
    /// <summary>
    /// 玩家在控制台执行指令时触发的事件
    /// </summary>
    public class PlayerConsoleCommandExecutingEventArgs : System.EventArgs
    {
        public PlayerConsoleCommandExecutingEventArgs(RExiled.API.Features.Player player, string command, bool isAllowed)
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