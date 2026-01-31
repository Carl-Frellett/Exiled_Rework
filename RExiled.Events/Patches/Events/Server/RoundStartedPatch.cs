using HarmonyLib;

namespace RExiled.Events.Patches.Events.Server
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.CmdStartRound))]
    internal class RoundStartedPatch
    {
        private static void Postfix()
        {
            RExiled.Events.Handlers.Server.OnRoundStarted();
        }
    }
}