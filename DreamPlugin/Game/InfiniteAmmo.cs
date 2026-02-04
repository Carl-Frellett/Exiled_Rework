using RExiled.API.Enums;
using RExiled.API.Extensions;
using RExiled.Events.EventArgs.Player;

namespace DreamPlugin.Game
{
    public class InfiniteAmmo
    {
        public static bool IsInfiniteAmmoEnabled { get; set; } = false;

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
            if (ev.Shooter == null)
                return;

            if (IsInfiniteAmmoEnabled)
            {
                ev.Shooter.SetWeaponAmmo(ev.Shooter.CurrentItem, 65535);
            }
            else
            {
                ev.Shooter.SetAmmo(AmmoType.Nato9, 114514);
                ev.Shooter.SetAmmo(AmmoType.Nato762, 114514);
                ev.Shooter.SetAmmo(AmmoType.Nato556, 114514);
            }
        }
    }
}
