using HarmonyLib;
using System;
using UnityEngine;

namespace RExiled.Events.Patches.Events.Player
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdSwitchAWButton))]
    internal static class WarheadPanelInteractPatch
    {
        private static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                if (!__instance._playerInteractRateLimit.CanExecute() ||
                    (__instance._hc.CufferId > 0 && !!__instance.CanDisarmedInteract))
                    return false;

                var panelObj = GameObject.Find("OutsitePanelScript");
                if (panelObj == null || !__instance.ChckDis(panelObj.transform.position))
                    return false;

                var hub = ReferenceHub.GetHub(__instance.gameObject);
                var player = RExiled.API.Features.Player.Get(hub);

                var ev = new RExiled.Events.EventArgs.Player.WarheadPanelInteractingEventArgs(player);
                RExiled.Events.Handlers.Player.OnWarheadPanelInteracting(ev);

                if (ev.IsAllowed)
                {
                    panelObj.GetComponentInParent<AlphaWarheadOutsitePanel>().NetworkkeycardEntered = true;
                }

                __instance.OnInteract();
                return false;
            }
            catch (Exception ex)
            {
                RExiled.API.Features.Log.Error($"[RExiled] WarheadPanelInteractPatch error: {ex}");
                return false;
            }
        }
    }
}