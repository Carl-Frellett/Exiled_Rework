using System.Collections.Generic;
using System.Text;
using DreamPlugin.BroadcastSystem.Manager;
using MEC;
using RExiled.API.Features;

namespace DreamPlugin.BroadcastSystem
{
    public static class BroadcastSystem
    {
        private static readonly Dictionary<Player, PlayerBroadcastManager> PlayerManagers = new Dictionary<Player, PlayerBroadcastManager>();
        private static readonly GlobalBroadcastManager GlobalManager = new GlobalBroadcastManager();

        private const float UpdateInterval = 0.8f;

        static BroadcastSystem()
        {
            Timing.RunCoroutine(UpdateLoop());
        }

        private static IEnumerator<float> UpdateLoop()
        {
            while (true)
            {
                try
                {
                    var validGlobalMessages = new List<BroadcastMessage>();
                    foreach (var msg in GlobalManager.GetActiveMessages())
                    {
                        validGlobalMessages.Add(msg);
                    }

                    foreach (var player in Player.List)
                    {
                        if (player == null) continue;

                        PlayerManagers.TryGetValue(player, out var playerManager);

                        var validPlayerMessages = playerManager?.GetActiveMessages() ?? new List<BroadcastMessage>();

                        if (validGlobalMessages.Count == 0 && validPlayerMessages.Count == 0)
                        {
                            if (playerManager != null && !playerManager.HasMessages)
                                PlayerManagers.Remove(player);
                            continue;
                        }

                        var allMessages = new List<BroadcastMessage>(validGlobalMessages);
                        allMessages.AddRange(validPlayerMessages);

                        if (allMessages.Count == 0)
                            continue;

                        var sb = new StringBuilder("<size=30>");
                        bool hasValid = false;
                        foreach (var msg in allMessages)
                        {
                            if (!msg.IsExpired)
                            {
                                sb.Append($"<size=15>[{msg.RemainingSeconds}]</size> {msg.Text}\n");
                                hasValid = true;
                            }
                        }

                        if (hasValid)
                        {
                            if (sb.Length > 11) sb.Length -= 1;
                            sb.Append("</size>");
                            player.Broadcast(1, sb.ToString(), false);
                        }

                        if (playerManager != null && !playerManager.HasMessages)
                        {
                            PlayerManagers.Remove(player);
                        }
                    }

                    GlobalManager.CleanupExpired();
                }
                catch (System.Exception ex)
                {
                    Log.Error($"[DreamPlugin.BroadcastSystem] Error in UpdateLoop: {ex}");
                }

                yield return Timing.WaitForSeconds(UpdateInterval);
            }
        }

        public static void ShowGlobal(string message, float duration = 5f)
        {
            GlobalManager.AddMessage(message, duration);
        }

        public static void ShowToPlayer(Player player, string message, float duration = 5f)
        {
            if (player == null) return;

            if (!PlayerManagers.TryGetValue(player, out var manager))
            {
                manager = new PlayerBroadcastManager(player);
                PlayerManagers[player] = manager;
            }

            manager.AddMessage(message, duration);
        }

        public static void ClearGlobal() => GlobalManager.Clear();

        public static void ClearPlayer(Player player)
        {
            if (player != null)
                PlayerManagers.Remove(player);
        }
    }
}