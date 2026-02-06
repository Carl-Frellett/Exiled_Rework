using System;
using HarmonyLib;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using UnityEngine;

namespace RExiled.Events.Patches.Events.Player
{
    [HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.CallCmdMovePlayer))]
    internal static class PocketDimensionEnter
    {
        private static bool Prefix(Scp106PlayerScript __instance, GameObject ply, int t)
        {
            try
            {
                if (ply == null)
                    return false;

                var ccm = ply.GetComponent<CharacterClassManager>();
                if (ccm == null || !ccm.IsHuman() || ccm.GodMode)
                    return false;

                if (ccm.Classes.SafeGet(ccm.CurClass).team == Team.SCP)
                    return false;

                if (!ServerTime.CheckSynchronization(t) ||
                    !__instance.iAm106 ||
                    Vector3.Distance(__instance.GetComponent<PlyMovementSync>().RealModelPosition, ply.transform.position) >= 3f)
                    return false;

                var player = RExiled.API.Features.Player.Get(ply);
                if (player == null)
                    return false;

                var ev = new PocketDimensionEnterEventArgs(player);
                Handlers.Player.OnPocketDimensionEnter(ev);

                if (!ev.IsAllow)
                    return false;

                ply.GetComponent<PlyMovementSync>().OverridePosition(Vector3.down * 1998.5f, 0f, true);

                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"[RExiled] PocketDimensionEnter patch error: {ex}");
                return true;
            }
        }
    }
}