using System.Collections.Generic;
using RExiled.API.Enums;
using RExiled.API.Extensions;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;

namespace DreamPlugin.Game
{
    public class InfiniteAmmo
    {
        public static bool IsInfiniteAmmoEnabled { get; set; } = false;

        private static readonly HashSet<int> TempInfinitePlayers = new HashSet<int>();

        public void RegisterEvents()
        {
            RExiled.Events.Handlers.Player.Shooting += OnPlayerShooting;
        }

        public void UnregisterEvents()
        {
            RExiled.Events.Handlers.Player.Shooting -= OnPlayerShooting;
        }

        public void OnPlayerShooting(ShootingEventArgs ev)
        {
            if (ev.Shooter == null || ev.Shooter.CurrentItem == null)
                return;

            if (TempInfinitePlayers.Contains(ev.Shooter.Id))
            {
                if (ev.Shooter.CurrentItem.GetWeaponAmmo() <= 0)
                {
                    ev.Shooter.SetWeaponAmmo(ev.Shooter.CurrentItem, int.MaxValue);
                }
                return;
            }

            if (IsInfiniteAmmoEnabled)
            {
                if (ev.Shooter.CurrentItem.GetWeaponAmmo() <= 0)
                {
                    ev.Shooter.SetWeaponAmmo(ev.Shooter.CurrentItem, int.MaxValue);
                }
            }
            else
            {
                const int desiredAmmo = 114514;
                const int threshold = 100;
                foreach (var ammoType in new[] { AmmoType.Nato9, AmmoType.Nato762, AmmoType.Nato556 })
                {
                    int currentAmmo = ev.Shooter.GetAmmo(ammoType);
                    if (currentAmmo < threshold)
                    {
                        ev.Shooter.SetAmmo(ammoType, desiredAmmo);
                    }
                }
            }
        }

        public static void EnableForPlayer(Player player)
        {
            if (player != null)
                TempInfinitePlayers.Add(player.Id);
        }

        public static void DisableForPlayer(Player player)
        {
            if (player != null)
                TempInfinitePlayers.Remove(player.Id);
        }

        public static void ClearAllTemporary()
        {
            TempInfinitePlayers.Clear();
        }

        public static bool HasTemporaryAmmo(Player player)
        {
            return player != null && TempInfinitePlayers.Contains(player.Id);
        }
    }
}