using HarmonyLib;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using System;

namespace RExiled.Events.Patches.Events.Player
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SetClassIDAdv))]
    internal static class ChangedRolePatch
    {
        private static void Prefix(CharacterClassManager __instance, ref RoleType id, bool lite, bool escape, out RoleType __state)
        {
            __state = default;

            try
            {
                if (__instance == null || __instance.gameObject == null)
                    return;

                var hub = ReferenceHub.GetHub(__instance.gameObject);
                if (hub == null)
                    return;

                var player = RExiled.API.Features.Player.Get(hub);
                if (player == null)
                    return;

                __state = player.Role;
            }
            catch (Exception ex)
            {
                Log.Error($"[RExiled] ChangedRolePatch Prefix error: {ex}");
                __state = RoleType.None;
            }
        }
        private static void Postfix(CharacterClassManager __instance, RoleType id, bool lite, bool escape, RoleType __state)
        {
            try
            {
                if (__state == default(RoleType))
                    return;

                if (__instance == null || __instance.gameObject == null)
                    return;

                var hub = ReferenceHub.GetHub(__instance.gameObject);
                if (hub == null)
                    return;

                var player = RExiled.API.Features.Player.Get(hub);
                if (player == null)
                    return;

                var ev = new ChangedRoleEventArgs(player, __state, id);
                Handlers.Player.OnChangedRole(ev);
            }
            catch (Exception ex)
            {
                Log.Error($"[RExiled] ChangedRolePatch Postfix error: {ex}");
            }
        }
    }
}