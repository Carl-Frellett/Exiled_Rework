using MEC;
using RExiled.API.Extensions;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets._Scripts.Dissonance;

namespace DreamPlugin.Game.PlayerRole
{
    public class SCP6000
    {
        public Player SCP6000CurrentPlayer = null;

        public void RegisterEvents()
        {
            RExiled.Events.Handlers.Player.RemoteAdminCommandExecuting += OnRAsp6000;
            RExiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            RExiled.Events.Handlers.Player.Died += OnDied;
            RExiled.Events.Handlers.Player.Left += OnPlayerLeft;
            RExiled.Events.Handlers.Player.PickingUpItem += OnPickItem;
        }
        public void UnregisterEvents()
        {
            RExiled.Events.Handlers.Player.RemoteAdminCommandExecuting -= OnRAsp6000;
            RExiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            RExiled.Events.Handlers.Player.Died -= OnDied;
            RExiled.Events.Handlers.Player.Left -= OnPlayerLeft;
            RExiled.Events.Handlers.Player.PickingUpItem -= OnPickItem;
        }

        public void OnRAsp6000(RemoteAdminCommandExecutingEventArgs ev)
        {
            if (ev.Command.StartsWith("sp6000"))
            {
                if (SCP6000CurrentPlayer != null)
                {
                    ev.Player.RemoteAdminMessage($"场内已经有了一名SCP6000，你不得再次刷新 \nID:{SCP6000CurrentPlayer.Id} | Name:{SCP6000CurrentPlayer.Nickname}", true);
                    ev.IsAllowed = false;
                    return;
                }
                ev.IsAllowed = false;
                SpawnSCP6000(ev.Player);
                ev.Player.RemoteAdminMessage($"成功刷新SCP6000 \nID:{SCP6000CurrentPlayer.Id} | Name:{SCP6000CurrentPlayer.Nickname}");

                ev.IsAllowed = false;
            }
        }

        public void SpawnSCP6000(Player ply)
        {
            if (SCP6000CurrentPlayer != null)
            {
                return;
            }

            SCP6000CurrentPlayer = ply;
            List<ItemType> SCP6000Items = new List<ItemType>()
            {
            ItemType.KeycardJanitor,
            ItemType.Adrenaline,
            ItemType.Coin,
            ItemType.Coin,
            ItemType.Coin
            };
            Timing.CallDelayed(0.3f, () =>
            {
                SCP6000CurrentPlayer.ResetInventory(SCP6000Items);
            });
            BroadcastSystem.BroadcastSystem.ShowToPlayer(SCP6000CurrentPlayer, "[个人消息] 你是<color=red>SCP-6000</color> <i>丢弃硬币可传送</i>", 5);
            string currentRank = SCP6000CurrentPlayer.RankName?.Trim() ?? "";
            if (string.IsNullOrEmpty(currentRank))
            {
                SCP6000CurrentPlayer.RankName = "SCP-6000";
            }
            else
            {
                SCP6000CurrentPlayer.RankName += " | SCP-6000";
            }
            RExiled.Events.Handlers.Player.ChangedRole += OnChangeRole;
        }
        public void OnPlayerLeft(LeftEventArgs ev)
        {
            if (ev.Player == SCP6000CurrentPlayer)
            {
                string currentRank = ev.Player.RankName ?? "";
                const string SCP6000Tag = "SCP-6000";
                const string separatorTag = " | SCP-6000";
                string newRank = currentRank;

                if (currentRank.Contains(separatorTag))
                {
                    newRank = currentRank.Replace(separatorTag, "");
                }
                else if (currentRank == SCP6000Tag)
                {
                    newRank = "";
                }

                newRank = newRank.Trim();
                if (newRank.EndsWith(" |"))
                    newRank = newRank.Substring(0, newRank.Length - 2).Trim();

                ev.Player.RankName = newRank;

                SCP6000CurrentPlayer = null;
                RExiled.Events.Handlers.Player.ChangedRole -= OnChangeRole;
            }
        }

        public void OnRoundStart()
        {
            Timing.CallDelayed(1.5f, () =>
            {
                if (Player.List.Count() >= 10 && SCP6000CurrentPlayer == null)
                {
                    var SCP6000s = Player.List.Where(p => p.Role == RoleType.ClassD).ToList();

                    if (SCP6000s.Count > 0)
                    {
                        var SCP6000 = SCP6000s[UnityEngine.Random.Range(0, SCP6000s.Count)];
                        SpawnSCP6000(SCP6000);
                    }
                }
            });
        }
        public void OnPickItem(PickingUpItemEventArgs ev)
        {
            if (ev.Player == null || ev.Pickup == null)
                return;

            if (ev.Player == SCP6000CurrentPlayer && ev.Pickup.ItemId == ItemType.Coin)
            {
                ev.IsAllowed = false;
                BroadcastSystem.BroadcastSystem.ShowToPlayer(SCP6000CurrentPlayer, "[个人消息] 你不可以拾取硬币");
            }
        }
        public void OnDropItem(DroppingItemEventArgs ev)
        {
            if (ev.Player == null)
                return;

            if (ev.Player == SCP6000CurrentPlayer)
            { 
            }
        }
        public void OnChangeRole(ChangedRoleEventArgs ev)
        {
            if (ev.Player == SCP6000CurrentPlayer)
            {
                string currentRank = ev.Player.RankName ?? "";
                const string SCP6000Tag = "SCP-6000";
                const string separatorTag = " | SCP-6000";
                string newRank = currentRank;

                if (currentRank.Contains(separatorTag))
                {
                    newRank = currentRank.Replace(separatorTag, "");
                }
                else if (currentRank == SCP6000Tag)
                {
                    newRank = "";
                }

                newRank = newRank.Trim();
                if (newRank.EndsWith(" |"))
                    newRank = newRank.Substring(0, newRank.Length - 2).Trim();

                ev.Player.RankName = newRank;
                SCP6000CurrentPlayer = null;
                RExiled.Events.Handlers.Player.ChangedRole -= OnChangeRole;
            }
        }

        public void OnDied(DiedEventArgs ev)
        {
            var target = ev.Target;
            if (target == null || SCP6000CurrentPlayer == null)
                return;

            if (target == SCP6000CurrentPlayer)
            {
                string currentRank = SCP6000CurrentPlayer.RankName ?? "";
                const string SCP6000Tag = "SCP-6000";
                const string separatorTag = " | SCP-6000";
                string newRank = currentRank;

                if (currentRank.Contains(separatorTag))
                {
                    newRank = currentRank.Replace(separatorTag, "");
                }
                else if (currentRank == SCP6000Tag)
                {
                    newRank = "";
                }

                newRank = newRank.Trim();
                if (newRank.EndsWith(" |"))
                    newRank = newRank.Substring(0, newRank.Length - 2).Trim();

                target.RankName = newRank;

                SCP6000CurrentPlayer = null;
                BroadcastSystem.BroadcastSystem.ShowGlobal("<color=red>SCP-6000</color>已被收容!", 5);
                RExiled.Events.Handlers.Player.ChangedRole -= OnChangeRole;
            }
        }
    }
}
