using HarmonyLib;
using RemoteAdmin;
using RExiled.API.Extensions;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;

namespace RExiled.Events.Patches
{
    [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
    internal class RemoteAdminCommandPatch
    {
        private static bool Prefix(ref string q, CommandSender sender)
        {
            if (q.Contains("REQUEST_DATA PLAYER_LIST SILENT"))
                return true;

            if (sender is PlayerCommandSender pcs)
            {
                if (q.ToLower().StartsWith("gban-kick") && !pcs.SR.RaEverywhere)
                {
                    sender.RaReply("GBAN-KICK# Permission denied.", false, true, string.Empty);
                    Log.Error($"GBAN-KICK blocked for {sender.Nickname}");
                    return false;
                }

                Player player = pcs.Processor.connectionToClient.GetRExiledPlayer();
                var ev = new RemoteAdminCommandExecutingEventArgs(player, q, true);
                Handlers.Player.OnRemoteAdminCommandExecuting(ev);

                if (!ev.IsAllowed)
                {
                    sender?.RaReply("Command blocked by plugin.", false, true, string.Empty);
                    return false;
                }

                q = ev.Command;
                return true;
            }

            var ev2 = new RemoteAdminCommandExecutingEventArgs(null, q, true);
            Handlers.Player.OnRemoteAdminCommandExecuting(ev2);

            if (!ev2.IsAllowed)
            {
                sender?.RaReply("Command blocked by plugin.", false, true, string.Empty);
                return false;
            }

            q = ev2.Command;
            return true;
        }
    }
}