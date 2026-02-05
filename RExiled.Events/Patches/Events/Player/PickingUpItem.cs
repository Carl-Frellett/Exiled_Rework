using HarmonyLib;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using System;
using UnityEngine;

namespace RExiled.Events.Patches.Events.Player
{
    [HarmonyPatch(typeof(Searching), nameof(Searching.CallCmdPickupItem))]
    internal static class PickingUpItem
    {
        private static bool Prefix(Searching __instance, GameObject t)
        {
            try
            {
                if (t == null || __instance == null)
                    return true;

                var hub = __instance.hub;
                if (hub == null)
                    return true;

                var player = RExiled.API.Features.Player.Get(hub);
                if (player == null)
                    return true;

                var pickup = t.GetComponent<Pickup>();
                if (pickup == null)
                    return true;

                var ev = new PickingUpItemEventArgs(player, pickup);
                Handlers.Player.OnPickingUpItem(ev);

                return ev.IsAllowed;
            }
            catch (Exception ex)
            {
                Log.Error($"[RExiled] PickingUpItem patch error: {ex}");
                return true;
            }
        }
    }
}