using MEC;
using RExiled.API.Extensions;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets._Scripts.Dissonance;
using RExiled.API.Enums;

namespace DreamPlugin.Game.PlayerRole
{
    public class SCP6000
    {
        public Player SCP6000CurrentPlayer = null;
        private CoroutineHandle addcoin;

        public void RegisterEvents()
        {
            RExiled.Events.Handlers.Player.RemoteAdminCommandExecuting += OnRAsp6000;
            RExiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            RExiled.Events.Handlers.Player.Died += OnDied;
            RExiled.Events.Handlers.Player.Left += OnPlayerLeft;
            RExiled.Events.Handlers.Player.PickingUpItem += OnPickItem;
            RExiled.Events.Handlers.Player.ItemDropped += OnDropItem;
        }
        public void UnregisterEvents()
        {
            RExiled.Events.Handlers.Player.RemoteAdminCommandExecuting -= OnRAsp6000;
            RExiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            RExiled.Events.Handlers.Player.Died -= OnDied;
            RExiled.Events.Handlers.Player.Left -= OnPlayerLeft;
            RExiled.Events.Handlers.Player.PickingUpItem -= OnPickItem;
            RExiled.Events.Handlers.Player.ItemDropped -= OnDropItem;
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
            SCP6000CurrentPlayer.SetRole(RoleType.ClassD,true);
            List<ItemType> SCP6000Items = new List<ItemType>()
            {
            ItemType.KeycardJanitor,
            ItemType.Adrenaline,
            ItemType.Coin,
            ItemType.Coin,
            ItemType.Coin
            };
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
            addcoin = Timing.RunCoroutine(CoinGivingCoroutine());
            Timing.CallDelayed(0.3f, () =>
            {
                SCP6000CurrentPlayer.ResetInventory(SCP6000Items);
            });
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

                Timing.KillCoroutines(addcoin);
                SCP6000CurrentPlayer = null;
                RExiled.Events.Handlers.Player.ChangedRole -= OnChangeRole;
            }
        }

        public void OnRoundStart()
        {
            Timing.CallDelayed(1.7f, () =>
            {
                if (Player.List.Count() >= 5 && SCP6000CurrentPlayer == null)
                {
                    var scp550Player = Plugin.plugin.SCP550.Scp550CurrentPlayer;

                    var eligibleClassDs = Player.List
                        .Where(p => p.Role == RoleType.ClassD && p != scp550Player)
                        .ToList();

                    if (eligibleClassDs.Count > 0)
                    {
                        var selectedPlayer = eligibleClassDs[UnityEngine.Random.Range(0, eligibleClassDs.Count)];
                        SpawnSCP6000(selectedPlayer);
                    }
                    else
                    {
                        Log.Warn("没有符合条件的 ClassD 玩家可选为 SCP-6000");
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
        public void OnDropItem(ItemDroppedEventArgs ev)
        {
            if (ev.Player == null)
                return;

            if (ev.Player == SCP6000CurrentPlayer && ev.ItemId == ItemType.Coin)
            {
                if (Warhead.IsDetonated == true)
                {
                    TeleportPlayerToRandomPlayer();
                }
                else
                {
                    TeleportToRandomRoom();
                }
            }
        }

        private IEnumerator<float> CoinGivingCoroutine()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(120f);

                if (SCP6000CurrentPlayer == null || !SCP6000CurrentPlayer.IsAlive)
                {
                    yield break;
                }

                SCP6000CurrentPlayer.AddItem(ItemType.Coin);
                BroadcastSystem.BroadcastSystem.ShowToPlayer(SCP6000CurrentPlayer, "[个人消息] <color=yellow>获得一枚硬币！</color>", 4);
            }
        }

        public void TeleportPlayerToRandomPlayer()
        {
            if (SCP6000CurrentPlayer == null)
            {
                return;
            }

            var candidates = Player.List
                .Where(p => p != SCP6000CurrentPlayer && p.IsAlive)
                .ToList();

            if (candidates.Count == 0)
            {
                return;
            }

            Player randomPlayer = candidates[Random.Range(0, candidates.Count)];

            Vector3 offset = new Vector3(0f, 0f, 0f);
            SCP6000CurrentPlayer.Position = randomPlayer.Position + offset;
        }

        public void TeleportToRandomRoom()
        {
            if (SCP6000CurrentPlayer == null)
            {
                return;
            }

            var rooms = Map.Rooms;
            if (rooms == null || rooms.Count == 0)
            {
                return;
            }

            var validRooms = rooms.Where(r => r.Type != RoomType.Unknown && r.Type != RoomType.Surface && r.Type != RoomType.EzShelter).ToList();

            if (validRooms.Count == 0)
            {
                validRooms = rooms.ToList();
            }

            Room randomRoom = validRooms[UnityEngine.Random.Range(0, validRooms.Count)];

            Vector3 spawnPos = randomRoom.Position;

            spawnPos.y += 3.5f;

            SCP6000CurrentPlayer.Position = spawnPos;
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
                Timing.KillCoroutines(addcoin);
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

                Timing.KillCoroutines(addcoin);
                SCP6000CurrentPlayer = null;
                BroadcastSystem.BroadcastSystem.ShowGlobal("<color=red>SCP-6000</color>已被收容!", 5);
                RExiled.Events.Handlers.Player.ChangedRole -= OnChangeRole;
            }
        }
    }
}
