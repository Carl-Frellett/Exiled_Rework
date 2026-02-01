using HarmonyLib;
using RemoteAdmin;
using RExiled.API.Extensions;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;

namespace RExiled.Events.Patches
{
    [HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.ProcessGameConsoleQuery))]
    internal class PlayerCommandPatch
    {
        private static bool Prefix(QueryProcessor __instance, ref string query, ref bool encrypted)
        {
            Player player = __instance.connectionToClient.GetRExiledPlayer();

            var ev = new PlayerCommandExecutingEventArgs(player, query, true);
            Handlers.Player.OnInGameConsoleCommandExecuting(ev);

            if (!ev.IsAllowed) return false;
            query = ev.Command;
            return true;
        }
    }
}