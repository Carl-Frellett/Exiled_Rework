using System;

namespace RExiled.Events.EventArgs.Player
{
    public class ChangingRoleEventArgs : System.EventArgs
    {
        public ChangingRoleEventArgs(RExiled.API.Features.Player player, RoleType newRole, bool allowOverride = true)
        {
            Player = player;
            NewRole = newRole;
            AllowOverride = allowOverride;
            IsAllowed = true;
        }

        public RExiled.API.Features.Player Player { get; }
        public RoleType NewRole { get; set; }
        public bool AllowOverride { get; }
        public bool IsAllowed { get; set; }
    }
}