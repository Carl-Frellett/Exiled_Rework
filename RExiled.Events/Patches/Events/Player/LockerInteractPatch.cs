// LockerInteractPatch.cs

namespace RExiled.Events.Patches.Events.Player
{
    using HarmonyLib;
    using RExiled.API.Features;
    using RExiled.Events.EventArgs.Player;
    using System;

    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdUseLocker))]
    internal static class LockerInteractPatch
    {
        private static bool Prefix(PlayerInteract __instance, int lockerId, int chamberNumber)
        {
            try
            {
                if (!__instance._playerInteractRateLimit.CanExecute(true) ||
                    (__instance._hc.CufferId > 0 && !__instance.CanDisarmedInteract))
                    return false;

                var lockerManager = LockerManager.singleton;
                if (lockerId < 0 || lockerId >= lockerManager.lockers.Length)
                    return false;

                var locker = lockerManager.lockers[lockerId];
                if (!__instance.ChckDis(locker.gameObject.transform.position) ||
                    !locker.supportsStandarizedAnimation)
                    return false;

                if (chamberNumber < 0 || chamberNumber >= locker.chambers.Length)
                    return false;

                var chamber = locker.chambers[chamberNumber];
                if (chamber.doorAnimator == null || !chamber.CooldownAtZero())
                    return false;

                chamber.SetCooldown();

                string accessToken = chamber.accessToken;
                var inventory = __instance._inv;
                var currentItem = inventory.GetItemByID(inventory.curItem);
                bool hasPermission = string.IsNullOrEmpty(accessToken) ||
                                     (currentItem != null && currentItem.permissions.Contains(accessToken)) ||
                                     __instance._sr.BypassMode;

                var hub = ReferenceHub.GetHub(__instance.gameObject);
                var player = RExiled.API.Features.Player.Get(hub);
                if (player == null)
                    return false;

                var ev = new LockerInteractingEventArgs(player, locker, chamber, lockerId, chamberNumber, hasPermission);
                RExiled.Events.Handlers.Player.OnLockerInteracting(ev);

                bool allow = ev.IsAllowed;

                if (allow)
                {
                    bool willOpen = (lockerManager.openLockers[lockerId] & (1 << chamberNumber)) == 0;
                    lockerManager.ModifyOpen(lockerId, chamberNumber, willOpen);
                    lockerManager.RpcDoSound(lockerId, chamberNumber, willOpen);

                    bool allClosed = true;
                    for (int i = 0; i < locker.chambers.Length; i++)
                    {
                        if ((lockerManager.openLockers[lockerId] & (1 << i)) != 0)
                        {
                            allClosed = false;
                            break;
                        }
                    }
                    locker.LockPickups(allClosed);

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        lockerManager.RpcChangeMaterial(lockerId, chamberNumber, false);
                    }
                }
                else
                {
                    // 拒绝时高亮材质（红色）
                    lockerManager.RpcChangeMaterial(lockerId, chamberNumber, true);
                }

                __instance.OnInteract();
                return false;
            }
            catch (Exception ex)
            {
                RExiled.API.Features.Log.Error($"[RExiled] LockerInteractPatch error: {ex}");
                return true;
            }
        }
    }
}