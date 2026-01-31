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

        public static event CustomEventHandler<PlayerConsoleCommandExecutingEventArgs> PlayerConsoleCommandExecuting;

        public static event CustomEventHandler<RemoteAdminCommandExecutingEventArgs> RemoteAdminCommandExecuting;

        public static void OnRemoteAdminCommandExecuting(RemoteAdminCommandExecutingEventArgs ev) => RemoteAdminCommandExecuting.InvokeSafely(ev);

        public static void OnPlayerConsoleCommandExecuting(PlayerConsoleCommandExecutingEventArgs ev) => PlayerConsoleCommandExecuting.InvokeSafely(ev);

        public static void OnJoined(JoinedEventArgs ev) => Joined.InvokeSafely(ev);

        public static void OnLeft(LeftEventArgs ev) => Left.InvokeSafely(ev);
    }
}
