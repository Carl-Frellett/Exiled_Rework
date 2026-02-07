using HarmonyLib;

namespace RExiled.Events.Patches.Events.Player
{
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
                if (lockerId < 0 || lockerId >= lockerManager.lockers.Length) return false;

                var locker = lockerManager.lockers[lockerId];
                if (!__instance.ChckDis(locker.gameObject.transform.position) ||
                    !locker.supportsStandarizedAnimation) return false;

                if (chamberNumber < 0 || chamberNumber >= locker.chambers.Length) return false;
                var chamber = locker.chambers[chamberNumber];
                if (chamber.doorAnimator == null || !chamber.CooldownAtZero()) return false;

                chamber.SetCooldown();

                var hub = ReferenceHub.GetHub(__instance.gameObject);
                var player = RExiled.API.Features.Player.Get(hub);

                var ev = new RExiled.Events.EventArgs.Player.LockerInteractingEventArgs(player, locker, chamber, lockerId, chamberNumber);
                RExiled.Events.Handlers.Player.OnLockerInteracting(ev);

                if (!ev.IsAllowed)
                {
                    lockerManager.RpcChangeMaterial(lockerId, chamberNumber, true);
                    __instance.OnInteract();
                    return false;
                }

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

                if (!string.IsNullOrEmpty(chamber.accessToken))
                {
                    lockerManager.RpcChangeMaterial(lockerId, chamberNumber, false);
                }

                __instance.OnInteract();
                return false;
            }
            catch (System.Exception ex)
            {
                RExiled.API.Features.Log.Error($"[RExiled] LockerInteractPatch error: {ex}");
                return true;
            }
        }
    }
}