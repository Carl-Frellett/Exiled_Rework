namespace RExiled.Events.EventArgs.Player
{
    public class ChangingRoleEventArgs : System.EventArgs
    {
        public RExiled.API.Features.Player Player { get; }
        public RoleType NewRole { get; set; }
        public bool AllowChange { get; set; } = true;

        public ChangingRoleEventArgs(RExiled.API.Features.Player player, RoleType newRole)
        {
            Player = player;
            NewRole = newRole;
        }
    }
}