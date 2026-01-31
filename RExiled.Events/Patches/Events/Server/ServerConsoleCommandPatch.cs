using HarmonyLib;
using RemoteAdmin;
using RExiled.Events.EventArgs.Server;

namespace RExiled.Events.Patches
{
    [HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.ProcessGameConsoleQuery))]
    internal class ServerConsoleCommandPatch
    {
        private static bool Prefix(QueryProcessor __instance, ref string query, ref bool encrypted)
        {
            var ev = new ServerConsoleCommandExecutingEventArgs(query, encrypted, true);
            Handlers.Server.OnServerConsoleCommandExecuting(ev);

            if (!ev.IsAllowed)
                return false;

            if (!string.IsNullOrEmpty(ev.Response))
            {
                __instance.GCT.SendToClient(__instance.connectionToClient, ev.Response, ev.Color);
                return false;
            }

            query = ev.Command;
            encrypted = ev.Encrypted;
            return true;
        }
    }
}