using System;
using HarmonyLib;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;

namespace RExiled.Events.Patches.Events.Player
{
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.CallCmdDropItem))]
    internal static class DroppingItem
    {
        private static bool Prefix(Inventory __instance, int itemInventoryIndex)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(true) ||
                    itemInventoryIndex < 0 ||
                    itemInventoryIndex >= __instance.items.Count)
                {
                    return false;
                }

                var syncItemInfo = __instance.items[itemInventoryIndex];

                if (__instance.items[itemInventoryIndex].id != syncItemInfo.id)
                {
                    return false;
                }

                var hub = __instance.GetComponent<ReferenceHub>();
                if (hub == null)
                    return true;

                var player = RExiled.API.Features.Player.Get(hub);
                if (player == null)
                    return true;

                var droppingEv = new DroppingItemEventArgs(
                    player,
                    syncItemInfo.id,
                    syncItemInfo.durability,
                    itemInventoryIndex);

                Handlers.Player.OnDroppingItem(droppingEv);

                if (!droppingEv.IsAllowed)
                {
                    return false; // 取消丢弃
                }

                // 执行实际丢弃逻辑（复制自原 CallCmdDropItem 实现）
                Pickup dropped = __instance.SetPickup(
                    syncItemInfo.id,
                    syncItemInfo.durability,
                    __instance.transform.position,
                    __instance.camera.transform.rotation,
                    syncItemInfo.modSight,
                    syncItemInfo.modBarrel,
                    syncItemInfo.modOther);

                __instance.items.RemoveAt(itemInventoryIndex);

                // 触发“已丢弃”事件（只读通知）
                if (dropped != null)
                {
                    var droppedEv = new ItemDroppedEventArgs(
                        player,
                        dropped,
                        syncItemInfo.id,
                        syncItemInfo.durability);

                    Handlers.Player.OnItemDropped(droppedEv);
                }

                return false; // 阻止原始方法执行
            }
            catch (Exception ex)
            {
                Log.Error($"[RExiled] DroppingItem patch error: {ex}");
                return true; // 允许原始方法以防崩溃（尽管它可能无效）
            }
        }
    }
}