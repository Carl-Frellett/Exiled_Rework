using HarmonyLib;
using System;
using UnityEngine;

namespace RExiled.Events.Patches.Events.Player
{
    [HarmonyPatch(typeof(Generator079), nameof(Generator079.Interact))]
    internal static class GeneratorDoorInteractionPatch
    {
        private static bool Prefix(Generator079 __instance, GameObject person, string command)
        {
            try
            {
                // 仅处理 EPS_DOOR 命令（开门/关门）
                if (!command.StartsWith("EPS_DOOR"))
                    return false; // 忽略其他命令（插入/弹出等）

                var inventory = person.GetComponent<Inventory>();
                if (inventory == null || __instance.doorAnimationCooldown > 0.0 || __instance.deniedCooldown > 0.0)
                    return false;

                // 如果门未解锁，先尝试解锁（但不触发事件，因不属于“开启/关闭”）
                if (!__instance.isDoorUnlocked)
                {
                    bool canUnlock = person.GetComponent<ServerRoles>().BypassMode;
                    if (inventory.curItem > ItemType.KeycardJanitor)
                    {
                        var item = inventory.GetItemByID(inventory.curItem);
                        if (item != null && item.permissions.Contains("ARMORY_LVL_2"))
                            canUnlock = true;
                    }

                    if (!canUnlock)
                    {
                        __instance.RpcDenied();
                        return false;
                    }

                    __instance.NetworkisDoorUnlocked = true;
                    __instance.doorAnimationCooldown = 0.5f;
                    return false;
                }

                var hub = ReferenceHub.GetHub(person);
                var player = RExiled.API.Features.Player.Get(hub);

                if (!__instance.NetworkisDoorOpen)
                {
                    // Opening
                    var ev = new RExiled.Events.EventArgs.Player.GeneratorDoorOpeningEventArgs(player, __instance);
                    RExiled.Events.Handlers.Player.OnGeneratorDoorOpening(ev);

                    if (!ev.IsAllowed)
                    {
                        __instance.RpcDenied();
                        return false;
                    }
                }
                else
                {
                    // Closing
                    var ev = new RExiled.Events.EventArgs.Player.GeneratorDoorClosingEventArgs(player, __instance);
                    RExiled.Events.Handlers.Player.OnGeneratorDoorClosing(ev);

                    if (!ev.IsAllowed)
                    {
                        __instance.RpcDenied();
                        return false;
                    }
                }

                __instance.doorAnimationCooldown = 1.5f;
                __instance.NetworkisDoorOpen = !__instance.isDoorOpen;
                __instance.RpcDoSound(__instance.isDoorOpen);
                return false;
            }
            catch (System.Exception ex)
            {
                RExiled.API.Features.Log.Error($"[RExiled] GeneratorDoorInteractionPatch error: {ex}");
                return true;
            }
        }
    }
}