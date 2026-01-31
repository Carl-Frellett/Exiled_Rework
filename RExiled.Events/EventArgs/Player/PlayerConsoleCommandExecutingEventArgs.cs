namespace RExiled.Events.EventArgs.Player
{
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