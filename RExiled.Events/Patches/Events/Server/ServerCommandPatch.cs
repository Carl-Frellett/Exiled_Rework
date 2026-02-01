using HarmonyLib;
using GameCore;
using RExiled.Events.EventArgs.Server;

namespace RExiled.Events.Patches
{
    [HarmonyPatch(typeof(Console), nameof(Console.TypeCommand))]
    internal class ServerCommandPatch
    {
        private static bool Prefix(ref string cmd)
        {
            var ev = new ServerCommandExecutingEventArgs(cmd, true);
            Handlers.Server.OnTerminalCommandExecuting(ev);

            return ev.IsAllowed;
        }
    }
}