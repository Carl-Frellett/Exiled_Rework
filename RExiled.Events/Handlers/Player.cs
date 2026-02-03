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

        public static event CustomEventHandler<HurtingEventArgs> Hurting;

        public static event CustomEventHandler<DyingEventArgs> Dying;

        public static event CustomEventHandler<ShootingEventArgs> Shooting;

        public static void OnShooting(ShootingEventArgs ev) => Shooting.InvokeSafely(ev);

        public static void OnHurting(HurtingEventArgs ev) => Hurting.InvokeSafely(ev);

        public static void OnDying(DyingEventArgs ev) => Dying.InvokeSafely(ev);
        public static void OnChangingRole(ChangingRoleEventArgs ev) => ChangingRole.InvokeSafely(ev);

        public static void OnChangedRole(ChangedRoleEventArgs ev) => ChangedRole.InvokeSafely(ev);

        public static void OnRemoteAdminCommandExecuting(RemoteAdminCommandExecutingEventArgs ev) => RemoteAdminCommandExecuting.InvokeSafely(ev);

        public static void OnInGameConsoleCommandExecuting(PlayerCommandExecutingEventArgs ev) => PlayerCommandExecuting.InvokeSafely(ev);

        public static void OnJoined(JoinedEventArgs ev) => Joined.InvokeSafely(ev);

        public static void OnLeft(LeftEventArgs ev) => Left.InvokeSafely(ev);
    }
}
