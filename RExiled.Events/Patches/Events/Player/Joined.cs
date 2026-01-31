using System;
using RExiled.Events.EventArgs;
using HarmonyLib;
using MEC;

namespace RExiled.Events.Patches.Events.Player
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.NetworkIsVerified), MethodType.Setter)]
    internal static class Joined
    {
        private static void Prefix(CharacterClassManager __instance, bool value)
        {
            try
            {
                if (!value || string.IsNullOrEmpty(__instance?.UserId))
                    return;

                if (!API.Features.Player.Dictionary.TryGetValue(__instance.gameObject, out API.Features.Player player))
                {
                    player = new API.Features.Player(ReferenceHub.GetHub(__instance.gameObject));

                    API.Features.Player.Dictionary.Add(__instance.gameObject, player);
                }

                API.Features.Log.SendRaw($"Player {player?.Nickname} ({player?.Id}) connected with the IP: {player?.IPAddress} LOGTYPE2");

                if (PlayerManager.players.Count >= CustomNetworkManager.slots)
                    API.Features.Log.Debug($"Server is full!");

                Timing.CallDelayed(0.25f, () =>
                {
                    if (player != null && player.IsMuted)
                        player.ReferenceHub.characterClassManager.SetDirtyBit(1UL);
                });

                var ev = new JoinedEventArgs(API.Features.Player.Get(__instance.gameObject));

                RExiled.Events.Handlers.Player.OnJoined(ev);
            }
            catch (Exception exception)
            {
                API.Features.Log.Error($"Exiled.Events.Patches.Events.Player.Joined: {exception}\n{exception.StackTrace}");
            }
        }
    }
}