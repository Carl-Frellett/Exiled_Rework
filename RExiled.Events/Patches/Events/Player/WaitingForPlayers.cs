using GameCore;
using HarmonyLib;
using System;

namespace RExiled.Events.Patches.Events.Player
{
    using RExiled.API.Features;


    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.AddLog))]
    public class WaitingForPlayersEvent
    {
        private static bool hasTriggered = false;

        public static void Prefix(ref string q)
        {
            try
            {
                if (q == "Waiting for players.." && !hasTriggered)
                {
                    RExiled.Events.Handlers.Server.OnWaitingForPlayers();

                    hasTriggered = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[RExiled] WaitingForPlayersEvent error: {ex}");
            }
        }
        public static void Reset() => hasTriggered = false;
    }
}
