using System;
using RExiled.API.Features;

namespace RExiled.Events.EventArgs.Player
{
    public class ChangedRoleEventArgs : System.EventArgs
    {
        public RExiled.API.Features.Player Player { get; }
        public RoleType OldRole { get; }
        public RoleType NewRole { get; }

        public ChangedRoleEventArgs(RExiled.API.Features.Player player, RoleType oldRole, RoleType newRole)
        {
            Player = player;
            OldRole = oldRole;
            NewRole = newRole;
        }
    }
}