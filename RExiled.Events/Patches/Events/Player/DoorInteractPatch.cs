using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace RExiled.Events.Patches.Events.Player
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdOpenDoor), typeof(GameObject))]
    internal static class DoorInteractPatch
    {
        private static bool Prefix(PlayerInteract __instance, GameObject doorId)
        {
            try
            {
                if (!__instance._playerInteractRateLimit.CanExecute() ||
                    (__instance._hc.CufferId > 0 && !__instance.CanDisarmedInteract) ||
                    doorId == null ||
                    __instance._ccm.CurClass == RoleType.None ||
                    __instance._ccm.CurClass == RoleType.Spectator)
                    return false;

                var doorComp = doorId.GetComponent<Door>();
                if (doorComp == null)
                    return false;

                bool inRange = doorComp.buttons.Count == 0
                    ? __instance.ChckDis(doorId.transform.position)
                    : doorComp.buttons.Any(button => __instance.ChckDis(button.transform.position));
                if (!inRange) return false;

                var scp096 = __instance.GetComponent<Scp096PlayerScript>();
                if (doorComp.destroyedPrefab != null && (!doorComp.isOpen || doorComp.curCooldown > 0.0) &&
                    scp096 != null && scp096.iAm096 && scp096.enraged == Scp096PlayerScript.RageState.Enraged)
                {
                    if (!__instance._096DestroyLockedDoors && doorComp.locked && !__instance._sr.BypassMode)
                        return false;
                    doorComp.DestroyDoor(true);
                    return false;
                }

                __instance.OnInteract();

                var hub = ReferenceHub.GetHub(__instance.gameObject);
                var player = RExiled.API.Features.Player.Get(hub);

                bool isAllowed;
                if (__instance._sr.BypassMode)
                {
                    isAllowed = true;
                }
                else if (string.Equals(doorComp.permissionLevel, "CHCKPOINT_ACC", System.StringComparison.OrdinalIgnoreCase) &&
                         __instance._ccm.Classes.SafeGet(__instance._ccm.CurClass).team == Team.SCP)
                {
                    isAllowed = true;
                }
                else
                {
                    var item = __instance._inv.GetItemByID(__instance._inv.curItem);
                    if (string.IsNullOrEmpty(doorComp.permissionLevel))
                    {
                        isAllowed = !doorComp.locked;
                    }
                    else if (item != null && item.permissions.Contains(doorComp.permissionLevel))
                    {
                        isAllowed = !doorComp.locked;
                    }
                    else
                    {
                        isAllowed = false;
                    }
                }

                var ev = new RExiled.Events.EventArgs.Player.DoorInteractingEventArgs(player, doorComp)
                {
                    IsAllowed = isAllowed
                };

                RExiled.Events.Handlers.Player.OnDoorInteracting(ev);

                if (!ev.IsAllowed)
                {
                    __instance.RpcDenied(doorId);
                    return false;
                }

                doorComp.ChangeState();
                return false;
            }
            catch (System.Exception ex)
            {
                RExiled.API.Features.Log.Error($"[RExiled] DoorInteractPatch error: {ex}");
                return false;
            }
        }
    }
}