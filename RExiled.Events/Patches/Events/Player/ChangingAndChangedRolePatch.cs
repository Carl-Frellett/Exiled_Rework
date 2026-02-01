using System;
using System.Collections.Generic;
using HarmonyLib;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using Mirror;

namespace RExiled.Events.Patches.Events.Player
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SetClassIDAdv))]
    internal static class ChangingAndChangedRolePatch
    {
        private static readonly Dictionary<CharacterClassManager, RoleType> OldRoles = new Dictionary<CharacterClassManager, RoleType>();

        private static bool Prefix(CharacterClassManager __instance, ref RoleType id)
        {
            try
            {
                if (__instance == null || __instance.gameObject == null || !NetworkServer.active)
                    return true;

                var hub = ReferenceHub.GetHub(__instance.gameObject);
                if (hub == null)
                    return true;

                var player = RExiled.API.Features.Player.Get(hub);
                if (player == null)
                    return true;

                OldRoles[__instance] = player.Role;

                var ev = new ChangingRoleEventArgs(player, id);
                Handlers.Player.OnChangingRole(ev);

                if (!ev.AllowChange)
                    return false;

                id = ev.NewRole;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[RExiled] ChangingRole prefix error: {ex}");
                return true;
            }
        }

        private static void Postfix(CharacterClassManager __instance)
        {
            try
            {
                if (__instance == null || __instance.gameObject == null || !NetworkServer.active)
                    return;

                RoleType oldRole = RoleType.None;
                if (OldRoles.TryGetValue(__instance, out var storedOld))
                {
                    oldRole = storedOld;
                    OldRoles.Remove(__instance);
                }

                var hub = ReferenceHub.GetHub(__instance.gameObject);
                if (hub == null)
                    return;

                var player = RExiled.API.Features.Player.Get(hub);
                if (player == null)
                    return;

                RoleType newRole = player.Role;

                if (oldRole == newRole)
                    return;

                var ev = new ChangedRoleEventArgs(player, oldRole, newRole);
                Handlers.Player.OnChangedRole(ev);
            }
            catch (Exception ex)
            {
                Log.Error($"[RExiled] ChangedRole postfix error: {ex}");
            }
        }
    }
}