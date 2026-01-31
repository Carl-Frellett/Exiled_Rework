// RExiled.Events.Patches.PlayerConsoleCommandPatch.cs
using HarmonyLib;
using GameCore;
using RExiled.Events.EventArgs.Player;

namespace RExiled.Events.Patches
{
    [HarmonyPatch(typeof(Console), nameof(Console.TypeCommand))]
    internal class PlayerConsoleCommandPatch
    {
        private static bool Prefix(ref string cmd)
        {
            var sender = Console._ccs;
            var player = sender == null ? null : API.Features.Player.Get(sender.Nickname);

            var ev = new PlayerConsoleCommandExecutingEventArgs(player, cmd, true);
            Handlers.Player.OnPlayerConsoleCommandExecuting(ev);

            if (!ev.IsAllowed) return false;
            cmd = ev.Command;
            return true;
        }
    }
}