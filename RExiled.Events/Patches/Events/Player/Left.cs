using System;

using RExiled.Events.EventArgs;

using HarmonyLib;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;

namespace RExiled.Events.Patches.Events.Player
{
    [HarmonyPatch(typeof(ReferenceHub), nameof(ReferenceHub.OnDestroy))]
    internal static class Left
    {
        private static void Prefix(ReferenceHub __instance)
        {
            try
            {
                RExiled.API.Features.Player player = RExiled.API.Features.Player.Get(__instance.gameObject);

                if (player == null || player.IsHost)
                    return;

                var ev = new LeftEventArgs(player);

                Log.SendRaw($"Player {ev.Player?.Nickname} ({player?.Id}) ({ev.Player?.IPAddress}) disconnected LOGTYPE2");

                Handlers.Player.OnLeft(ev);

                RExiled.API.Features.Player.IdsCache.Remove(player.Id);
                RExiled.API.Features.Player.Dictionary.Remove(player.GameObject);
            }
            catch (Exception exception)
            {
                Log.Error($"Exiled.Events.Patches.Events.Player.Left: {exception}\n{exception.StackTrace}");
            }
        }
    }
}
