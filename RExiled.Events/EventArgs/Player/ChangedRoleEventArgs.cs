namespace RExiled.Events.EventArgs.Player
{
    public class ChangedRoleEventArgs : System.EventArgs
    {
        public ChangedRoleEventArgs(RExiled.API.Features.Player player, RoleType oldRole, RoleType newRole)
        {
            Player = player;
            OldRole = oldRole;
            NewRole = newRole;
        }

        public RExiled.API.Features.Player Player { get; }
        public RoleType OldRole { get; }
        public RoleType NewRole { get; }
    }
}