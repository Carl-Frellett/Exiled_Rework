using HarmonyLib;
using RemoteAdmin;
using RExiled.Events.EventArgs.Player;

namespace RExiled.Events.Patches
{
    [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
    internal class RemoteAdminCommandPatch
    {
        private static bool Prefix(ref string query, CommandSender sender)
        {
            if (query.Contains("REQUEST_DATA PLAYER_LIST SILENT")) return true;

            var player = sender is PlayerCommandSender pcs
                ? API.Features.Player.Get(pcs.SenderId)
                : null;

            var ev = new RemoteAdminCommandExecutingEventArgs(player, query, true);
            Handlers.Player.OnRemoteAdminCommandExecuting(ev);

            if (!ev.IsAllowed)
            {
                sender.RaReply("Command blocked by plugin.", false, true, string.Empty);
                return false;
            }

            query = ev.Command;
            return true;
        }
    }
}