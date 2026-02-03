using RExiled.API.Enums;
using RExiled.Events.EventArgs.Player;

namespace DreamPlugin.Game
{
    public class InfiniteAmmo
    {
        public void RegisterEvents()
        { 
            RExiled.Events.Handlers.Player.Shooting += OnPlayerShooting;
            RExiled.Events.Handlers.Player.Dying += OnPlayerDying;
        }

        public void UnregisterEvents()
        { 
            RExiled.Events.Handlers.Player.Shooting -= OnPlayerShooting;
            RExiled.Events.Handlers.Player.Dying -= OnPlayerDying;
        }

        public void OnPlayerShooting(ShootingEventArgs ev)
        { 
            if(ev.Shooter == null)
                return;

            ev.Shooter.SetAmmo(AmmoType.Nato556,114514);
            ev.Shooter.SetAmmo(AmmoType.Nato762, 114514);
            ev.Shooter.SetAmmo(AmmoType.Nato9, 114514);
        }

        public void OnPlayerDying(DyingEventArgs ev)
        {
            ev.Target.SetAmmo(AmmoType.Nato556, 0);
            ev.Target.SetAmmo(AmmoType.Nato762, 0);
            ev.Target.SetAmmo(AmmoType.Nato9, 0);
        }
    }
}
