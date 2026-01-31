using HarmonyLib;
using RExiled.Events.Handlers;

namespace RExiled.Events.Patches.Server
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.Roundrestart))]
    internal class RoundRestartedPatch
    {
        private static void Prefix()
        {
            RExiled.Events.Handlers.Server.OnRoundRestarted();
        }
    }
}