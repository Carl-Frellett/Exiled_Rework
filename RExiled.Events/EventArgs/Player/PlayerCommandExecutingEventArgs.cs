namespace RExiled.Events.EventArgs.Player
{
    public class PlayerCommandExecutingEventArgs : System.EventArgs
    {
        public PlayerCommandExecutingEventArgs(RExiled.API.Features.Player player, string command, bool isAllowed)
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