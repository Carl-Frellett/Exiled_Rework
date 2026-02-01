using System;
using HarmonyLib;
using RExiled.API.Features;
using RExiled.Events.EventArgs;

namespace RExiled.Events.Patches.Events.Player
{
    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.SetNick))]
    internal static class Joined
    {
        private static void Postfix(NicknameSync __instance)
        {
            try
            {
                if (__instance == null || __instance.gameObject == null)
                    return;

                var hub = ReferenceHub.GetHub(__instance.gameObject);
                if (hub == null || string.IsNullOrEmpty(hub.nicknameSync.Network_myNickSync))
                    return;

                if (hub.nicknameSync.Network_myNickSync == "Dedicated Server")
                    return;

                if (RExiled.API.Features.Player.Dictionary.ContainsKey(__instance.gameObject))
                    return;

                var player = new RExiled.API.Features.Player(hub);
                RExiled.API.Features.Player.Dictionary[__instance.gameObject] = player;
                RExiled.API.Features.Player.IdsCache[player.Id] = player;

                var ev = new JoinedEventArgs(player);
                Handlers.Player.OnJoined(ev);

                Log.SendRaw($"Player {player.Nickname} ({player.Id}) ({player.IPAddress}) joined LOGTYPE2");
            }
            catch (Exception ex)
            {
                Log.Error($"[RExiled] Joined patch error: {ex}");
            }
        }
    }
}