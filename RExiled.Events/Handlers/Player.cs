using RExiled.Events.EventArgs;
using RExiled.Events.EventArgs.Player;
using RExiled.Events.Extensions;
using static RExiled.Events.Events;

namespace RExiled.Events.Handlers
{
    public static class Player
    {
        public static event CustomEventHandler<JoinedEventArgs> Joined;

        public static event CustomEventHandler<LeftEventArgs> Left;

        public static event CustomEventHandler<PlayerCommandExecutingEventArgs> PlayerCommandExecuting;

        public static event CustomEventHandler<RemoteAdminCommandExecutingEventArgs> RemoteAdminCommandExecuting;

        public static event CustomEventHandler<ChangingRoleEventArgs> ChangingRole;

        public static event CustomEventHandler<ChangedRoleEventArgs> ChangedRole;

        public static void OnChangingRole(ChangingRoleEventArgs ev) => ChangingRole.InvokeSafely(ev);

        public static void OnChangedRole(ChangedRoleEventArgs ev) => ChangedRole.InvokeSafely(ev);

        public static void OnRemoteAdminCommandExecuting(RemoteAdminCommandExecutingEventArgs ev) => RemoteAdminCommandExecuting.InvokeSafely(ev);

        public static void OnInGameConsoleCommandExecuting(PlayerCommandExecutingEventArgs ev) => PlayerCommandExecuting.InvokeSafely(ev);

        public static void OnJoined(JoinedEventArgs ev) => Joined.InvokeSafely(ev);

        public static void OnLeft(LeftEventArgs ev) => Left.InvokeSafely(ev);
    }
}
