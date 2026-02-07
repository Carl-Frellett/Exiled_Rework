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

        public static event CustomEventHandler<ShootingEventArgs> Shooting;

        public static event CustomEventHandler<ChangedRoleEventArgs> ChangedRole;

        public static event CustomEventHandler<SpawningTeamEventArgs> SpawningTeam;

        public static event CustomEventHandler<SpawnedTeamEventArgs> SpawnedTeam;

        public static event CustomEventHandler<HurtingEventArgs> Hurting;

        public static event CustomEventHandler<HurtEventArgs> Hurt;

        public static event CustomEventHandler<DiedEventArgs> Died;

        public static event CustomEventHandler<PickingUpItemEventArgs> PickingUpItem;

        public static event CustomEventHandler<DroppingItemEventArgs> DroppingItem;

        public static event CustomEventHandler<ItemDroppedEventArgs> ItemDropped;

        public static event CustomEventHandler<PocketDimensionEnterEventArgs> PocketDimensionEnter;

        public static event CustomEventHandler<LockerInteractingEventArgs> LockerInteracting;

        public static event CustomEventHandler<WarheadPanelInteractingEventArgs> WarheadPanelInteracting;

        public static event CustomEventHandler<GeneratorDoorOpeningEventArgs> GeneratorDoorOpening;

        public static event CustomEventHandler<GeneratorDoorClosingEventArgs> GeneratorDoorClosing;

        public static event CustomEventHandler<DoorInteractingEventArgs> DoorInteracting;

        public static void OnLockerInteracting(LockerInteractingEventArgs ev) => LockerInteracting?.InvokeSafely(ev);

        public static void OnWarheadPanelInteracting(WarheadPanelInteractingEventArgs ev) => WarheadPanelInteracting?.InvokeSafely(ev);

        public static void OnGeneratorDoorOpening(GeneratorDoorOpeningEventArgs ev) => GeneratorDoorOpening?.InvokeSafely(ev);

        public static void OnGeneratorDoorClosing(GeneratorDoorClosingEventArgs ev) => GeneratorDoorClosing?.InvokeSafely(ev);

        public static void OnDoorInteracting(DoorInteractingEventArgs ev) => DoorInteracting?.InvokeSafely(ev);

        public static void OnPocketDimensionEnter(PocketDimensionEnterEventArgs ev) => PocketDimensionEnter.InvokeSafely(ev);

        public static void OnItemDropped(ItemDroppedEventArgs ev) => ItemDropped?.InvokeSafely(ev);

        public static void OnDroppingItem(DroppingItemEventArgs ev) => DroppingItem?.InvokeSafely(ev);

        public static void OnPickingUpItem(PickingUpItemEventArgs ev) => PickingUpItem?.InvokeSafely(ev);

        internal static void OnHurting(HurtingEventArgs ev) => Hurting?.Invoke(ev);

        internal static void OnHurt(HurtEventArgs ev) => Hurt?.Invoke(ev);

        internal static void OnDied(DiedEventArgs ev) => Died?.Invoke(ev);
        public static void OnSpawningTeam(SpawningTeamEventArgs ev) => SpawningTeam.InvokeSafely(ev);

        public static void OnSpawnedTeam(SpawnedTeamEventArgs ev) => SpawnedTeam.InvokeSafely(ev);

        public static void OnChangedRole(ChangedRoleEventArgs ev) => ChangedRole.InvokeSafely(ev);

        public static void OnShooting(ShootingEventArgs ev) => Shooting.InvokeSafely(ev);

        public static void OnRemoteAdminCommandExecuting(RemoteAdminCommandExecutingEventArgs ev) => RemoteAdminCommandExecuting.InvokeSafely(ev);

        public static void OnInGameConsoleCommandExecuting(PlayerCommandExecutingEventArgs ev) => PlayerCommandExecuting.InvokeSafely(ev);

        public static void OnJoined(JoinedEventArgs ev) => Joined.InvokeSafely(ev);

        public static void OnLeft(LeftEventArgs ev) => Left.InvokeSafely(ev);
    }
}
