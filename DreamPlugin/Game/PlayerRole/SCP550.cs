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
    public class SCP550
    {
        public Player Scp550CurrentPlayer = null;

        public void RegisterEvents()
        {
            RExiled.Events.Handlers.Player.RemoteAdminCommandExecuting += OnRAsp550;
            RExiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            RExiled.Events.Handlers.Player.Hurting += OnHurting;
            RExiled.Events.Handlers.Player.Died += OnDied;
            RExiled.Events.Handlers.Player.Left += OnPlayerLeft;
            RExiled.Events.Handlers.Player.PickingUpItem += OnPickItem;
            RExiled.Events.Handlers.Player.PocketDimensionEnter += OnEnterPocket;
        }
        public void UnregisterEvents()
        {
            RExiled.Events.Handlers.Player.RemoteAdminCommandExecuting -= OnRAsp550;
            RExiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            RExiled.Events.Handlers.Player.Hurting -= OnHurting;
            RExiled.Events.Handlers.Player.Died -= OnDied;
            RExiled.Events.Handlers.Player.Left -= OnPlayerLeft;
            RExiled.Events.Handlers.Player.PickingUpItem -= OnPickItem;
            RExiled.Events.Handlers.Player.PocketDimensionEnter -= OnEnterPocket;
        }

        public void OnRAsp550(RemoteAdminCommandExecutingEventArgs ev)
        {
            if (ev.Command.StartsWith("sp550"))
            {
                if (Scp550CurrentPlayer != null)
                {
                    ev.Player.RemoteAdminMessage($"场内已经有了一名SCP550，你不得再次刷新 \nID:{Scp550CurrentPlayer.Id} | Name:{Scp550CurrentPlayer.Nickname}", true);
                    ev.IsAllowed = false;
                    return;
                }
                ev.IsAllowed = false;
                SpawnScp550(ev.Player);
                ev.Player.RemoteAdminMessage($"成功刷新SCP550 \nID:{Scp550CurrentPlayer.Id} | Name:{Scp550CurrentPlayer.Nickname}");

                ev.IsAllowed = false;
            }
        }

        public void SpawnScp550(Player ply)
        {
            if (Scp550CurrentPlayer != null)
            {
                return;
            }

            Scp550CurrentPlayer = ply;
            Scp550CurrentPlayer.SetRole(RoleType.Tutorial, true);
            Scp550CurrentPlayer.MaxHealth = 950;
            Scp550CurrentPlayer.Health = 300;
            List<ItemType> Scp550Items = new List<ItemType>()
            {
            ItemType.GunProject90,
            ItemType.GrenadeFlash,
            ItemType.KeycardNTFLieutenant
            };
            Timing.CallDelayed(0.3f, () =>
            {
                Scp550CurrentPlayer.ResetInventory(Scp550Items);
                Scp550CurrentPlayer.Position = Map.GetRandomSpawnPoint(RoleType.ChaosInsurgency);
            });
            BroadcastSystem.BroadcastSystem.ShowToPlayer(Scp550CurrentPlayer, "[个人消息] 你是<color=red>SCP-550</color> <i>击杀回血</i>", 5);
            string currentRank = Scp550CurrentPlayer.RankName?.Trim() ?? "";
            if (string.IsNullOrEmpty(currentRank))
            {
                Scp550CurrentPlayer.RankName = "SCP-550";
            }
            else
            {
                Scp550CurrentPlayer.RankName += " | SCP-550";
            }
            RExiled.Events.Handlers.Player.ChangedRole += OnChangeRole;
        }
        public void OnPlayerLeft(LeftEventArgs ev)
        {
            if (ev.Player == Scp550CurrentPlayer)
            {
                string currentRank = ev.Player.RankName ?? "";
                const string scp550Tag = "SCP-550";
                const string separatorTag = " | SCP-550";
                string newRank = currentRank;

                if (currentRank.Contains(separatorTag))
                {
                    newRank = currentRank.Replace(separatorTag, "");
                }
                else if (currentRank == scp550Tag)
                {
                    newRank = "";
                }

                newRank = newRank.Trim();
                if (newRank.EndsWith(" |"))
                    newRank = newRank.Substring(0, newRank.Length - 2).Trim();

                ev.Player.RankName = newRank;

                Scp550CurrentPlayer = null;
                RExiled.Events.Handlers.Player.ChangedRole -= OnChangeRole;
            }
        }

        public void OnRoundStart()
        {
            Timing.CallDelayed(1.5f, () =>
            {
                if (Player.List.Count() >= 10 && Scp550CurrentPlayer == null)
                {
                    var scp550s = Player.List.Where(p => p.Role == RoleType.ClassD).ToList();

                    if (scp550s.Count > 0)
                    {
                        var scp550 = scp550s[UnityEngine.Random.Range(0, scp550s.Count)];
                        SpawnScp550(scp550);
                    }
                }
            });
        }
        public void OnPickItem(PickingUpItemEventArgs ev)
        {
            if (ev.Player == null)
                return;

            if (ev.Player == Scp550CurrentPlayer && ev.Pickup.ItemId.IsMedical())
            {
                ev.IsAllowed = false;
                BroadcastSystem.BroadcastSystem.ShowToPlayer(Scp550CurrentPlayer, "[个人消息] 你不可以拾取医疗物品");
            }
            if (ev.Player == Scp550CurrentPlayer && ev.Pickup.ItemId == ItemType.MicroHID)
            {
                ev.IsAllowed = false;
                BroadcastSystem.BroadcastSystem.ShowToPlayer(Scp550CurrentPlayer, "[个人消息] 你不可以拾取此物品");
            }
        }
        public void OnEnterPocket(PocketDimensionEnterEventArgs ev)
        {
            if (ev.Player == null)
                return;

            if (ev.Player == Scp550CurrentPlayer)
            {
                ev.IsAllow = false;
            }
            else
            {
                ev.IsAllow = true;
            }
        }

        public void OnHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker == null || ev.Target == null) return;
            if (ev.Attacker.IsSCP && ev.Target == Scp550CurrentPlayer)
            {
                ev.IsAllowed = false;
            }
            if (ev.Target.IsSCP && ev.Attacker == Scp550CurrentPlayer)
            {
                ev.IsAllowed = false;
            }
            if (ev.Target == Scp550CurrentPlayer && ev.DamageType == DamageTypes.Falldown)
            {
                ev.IsAllowed = false;
            }
        }

        public void OnChangeRole(ChangedRoleEventArgs ev)
        {
            if (ev.Player == Scp550CurrentPlayer)
            {
                string currentRank = ev.Player.RankName ?? "";
                const string scp550Tag = "SCP-550";
                const string separatorTag = " | SCP-550";
                string newRank = currentRank;

                if (currentRank.Contains(separatorTag))
                {
                    newRank = currentRank.Replace(separatorTag, "");
                }
                else if (currentRank == scp550Tag)
                {
                    newRank = "";
                }

                newRank = newRank.Trim();
                if (newRank.EndsWith(" |"))
                    newRank = newRank.Substring(0, newRank.Length - 2).Trim();

                ev.Player.RankName = newRank;
                Scp550CurrentPlayer = null;
                RExiled.Events.Handlers.Player.ChangedRole -= OnChangeRole;
            }
        }

        public void OnDied(DiedEventArgs ev)
        {
            var target = ev.Target;
            if (target == null || Scp550CurrentPlayer == null)
                return;

            if (target == Scp550CurrentPlayer)
            {
                BroadcastSystem.BroadcastSystem.ShowGlobal("<color=red>SCP-550</color>已被收容!", 5);
                Cassie.CassieMessage("S C P 5 5 0 successfully terminated by work", false,true);
                string currentRank = Scp550CurrentPlayer.RankName ?? "";
                const string scp550Tag = "SCP-550";
                const string separatorTag = " | SCP-550";
                string newRank = currentRank;

                if (currentRank.Contains(separatorTag))
                {
                    newRank = currentRank.Replace(separatorTag, "");
                }
                else if (currentRank == scp550Tag)
                {
                    newRank = "";
                }

                newRank = newRank.Trim();
                if (newRank.EndsWith(" |"))
                    newRank = newRank.Substring(0, newRank.Length - 2).Trim();

                target.RankName = newRank;

                Scp550CurrentPlayer = null;
                RExiled.Events.Handlers.Player.ChangedRole -= OnChangeRole;
            }

            if (ev.Killer != null && ev.Killer == Scp550CurrentPlayer)
            {
                int health = 30;
                ev.Killer.Health = Mathf.Min(ev.Killer.Health + health, 950);
                BroadcastSystem.BroadcastSystem.ShowToPlayer(Scp550CurrentPlayer, $"[个人消息] 击杀玩家 <color=green>+{health}HP</color>");
            }
        }
    }
}
