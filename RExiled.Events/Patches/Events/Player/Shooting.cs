using HarmonyLib;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using UnityEngine;

namespace RExiled.Events.Patches.Events.Player
{
    [HarmonyPatch(typeof(WeaponManager), nameof(WeaponManager.CallCmdShoot))]
    internal static class Shooting
    {
        public static bool Prefix(
            WeaponManager __instance,
            GameObject target,
            string hitboxType,
            Vector3 dir,
            Vector3 sourcePos,
            Vector3 targetPos)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(true))
                    return false;

                int itemIndex = __instance.hub.inventory.GetItemIndex();
                if (itemIndex < 0 ||
                    itemIndex >= __instance.hub.inventory.items.Count ||
                    __instance.curWeapon < 0 ||
                    ((__instance.reloadCooldown > 0.0 || __instance.fireCooldown > 0.0) && !__instance.isLocalPlayer) ||
                    (__instance.hub.inventory.curItem != __instance.weapons[__instance.curWeapon].inventoryID ||
                     __instance.hub.inventory.items[itemIndex].durability <= 0.0))
                {
                    return false;
                }

                var player = RExiled.API.Features.Player.Get(__instance.hub);
                if (player == null)
                    return false;

                var ev = new ShootingEventArgs(player, target, ref targetPos);
                Handlers.Player.OnShooting(ev);

                if (!ev.IsAllowed)
                    return false;

                target = ev.Target;
                targetPos = ev.TargetPosition;

                return true;
            }
            catch (System.Exception ex)
            {
                Log.Error($"[RExiled] Shooting patch error: {ex}");
                return true;
            }
        }
    }
}