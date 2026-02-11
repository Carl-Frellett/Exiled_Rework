using RExiled.API.Enums;
using RExiled.API.Features;
using RExiled.Events.EventArgs;
using RExiled.Events.EventArgs.Player;

namespace DreamPlugin.Game
{
    class InventoryAccess
    {
        public void RegisterEvents()
        {
            RExiled.Events.Handlers.Player.DoorInteracting += OnPlayerDoorInteract;
            RExiled.Events.Handlers.Player.LockerInteracting += OnPlayerLockerInteract;
        }

        public void UnregisterEvents()
        {
            RExiled.Events.Handlers.Player.DoorInteracting -= OnPlayerDoorInteract;
            RExiled.Events.Handlers.Player.LockerInteracting -= OnPlayerLockerInteract;
        }
        public void OnPlayerDoorInteract(DoorInteractingEventArgs ev)
        {
            if (ev.Player == null) return;

            if (ev.Player.Side == Side.SCP) return;
            if (ev.IsAllowed) return;
            ev.IsAllowed = HasPermission(ev.Player, ev.Door.permissionLevel);
        }

        public void OnPlayerLockerInteract(LockerInteractingEventArgs ev)
        {
            if (ev.Player == null) return;

            if (ev.Player.Side == Side.SCP) return;
            if (ev.IsAllowed) return;

            ev.IsAllowed = HasPermission(ev.Player, ev.Chamber.accessToken);
        }

        private bool HasPermission(Player player, string requested)
        {
            if (requested == "")
            {
                return true;
            }

            foreach (var item in player.Inventory.items)
            {
                foreach (var permission in player.Inventory.GetItemByID(item.id).permissions)
                {
                    if (requested.Contains(permission))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
