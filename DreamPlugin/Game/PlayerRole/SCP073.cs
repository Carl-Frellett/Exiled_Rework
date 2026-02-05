using MEC;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using System.Collections.Generic;
using System.Linq;

namespace DreamPlugin.Game.PlayerRole
{
    public class SCP073
    {
        public Player Scp073CurrentPlayer = null;

        public void RegisterEvents()
        { 
            RExiled.Events.Handlers.Player.RemoteAdminCommandExecuting += OnRAsp073;
            RExiled.Events.Handlers.Player.SpawnedTeam += OnSpawnedTeam;
            RExiled.Events.Handlers.Player.Hurting += OnHurting;
            RExiled.Events.Handlers.Player.Died += OnDied;
            RExiled.Events.Handlers.Player.Left += OnPlayerLeft;
        }
        public void UnregisterEvents()
        {
            RExiled.Events.Handlers.Player.RemoteAdminCommandExecuting -= OnRAsp073;
            RExiled.Events.Handlers.Player.SpawnedTeam -= OnSpawnedTeam;
            RExiled.Events.Handlers.Player.Hurting -= OnHurting;
            RExiled.Events.Handlers.Player.Died -= OnDied;
            RExiled.Events.Handlers.Player.Left -= OnPlayerLeft;
        }

        public void OnRAsp073(RemoteAdminCommandExecutingEventArgs ev)
        {
            if (ev.Command.StartsWith("sp073"))
            {
                if (Scp073CurrentPlayer != null)
                {
                    ev.Player.RemoteAdminMessage($"场内已经有了一名SCP073，你不得再次刷新 \nID:{Scp073CurrentPlayer.Id} | Name:{Scp073CurrentPlayer.Nickname}" ,true);
                    ev.IsAllowed = false;
                    return;
                }
                ev.IsAllowed = false;
                SpawnScp073(ev.Player);
                ev.Player.RemoteAdminMessage($"成功刷新Scp073 \nID:{Scp073CurrentPlayer.Id} | Name:{Scp073CurrentPlayer.Nickname}");

                ev.IsAllowed = false;
            }
        }

        public void SpawnScp073(Player ply)
        {
            if (Scp073CurrentPlayer != null)
            {
                return;
            }
            Scp073CurrentPlayer = ply;
            Scp073CurrentPlayer.SetRole(RoleType.NtfCommander,true);
            Scp073CurrentPlayer.MaxHealth = 200;
            Scp073CurrentPlayer.Health = 200;
            Scp073CurrentPlayer.AdrenalineHealth += 50;
            List<ItemType> Scp073Items = new List<ItemType>()
            {
            ItemType.GunLogicer,
            ItemType.KeycardO5,
            ItemType.SCP500,
            ItemType.Medkit,
            ItemType.Adrenaline,
            ItemType.WeaponManagerTablet,
            ItemType.Radio,
            };
            Timing.CallDelayed(0.2f, () =>
            {
                Scp073CurrentPlayer.ResetInventory(Scp073Items);
            });
            BroadcastSystem.BroadcastSystem.ShowToPlayer(Scp073CurrentPlayer, "[个人消息] 你是<color=blue>SCP-073</color> <i>该隐</i> <i>对伤害有抗性, 对攻击有增强</i>", 5);

            string currentRank = Scp073CurrentPlayer.RankName?.Trim() ?? "";
            if (string.IsNullOrEmpty(currentRank))
            {
                Scp073CurrentPlayer.RankName = "SCP-073";
            }
            else
            {
                Scp073CurrentPlayer.RankName += " | SCP-073";
            }
            RExiled.Events.Handlers.Player.ChangedRole += OnChangeRole;
        }

        public void OnPlayerLeft(LeftEventArgs ev)
        {
            if (ev.Player == null || Scp073CurrentPlayer == null)
                return;

            if (ev.Player == Scp073CurrentPlayer)
            {
                string currentRank = ev.Player.RankName ?? "";
                const string scp073Tag = "SCP-073";
                const string separatorTag = " | SCP-073";
                string newRank = currentRank;

                if (currentRank.Contains(separatorTag))
                {
                    newRank = currentRank.Replace(separatorTag, "");
                }
                else if (currentRank == scp073Tag)
                {
                    newRank = "";
                }

                newRank = newRank.Trim();
                if (newRank.EndsWith(" |"))
                    newRank = newRank.Substring(0, newRank.Length - 2).Trim();

                ev.Player.RankName = newRank;

                Scp073CurrentPlayer = null;
                RExiled.Events.Handlers.Player.ChangedRole -= OnChangeRole;
            }
        }

        public void OnSpawnedTeam(SpawnedTeamEventArgs ev)
        {
            Timing.CallDelayed(0.5f, () =>
            {
                if (ev.IsChaos == false && Scp073CurrentPlayer == null)
                {
                    if (ev.Players.Count() >= 5)
                    {
                        var spPlayers = ev.Players.Where(p => p.Role == RoleType.NtfCadet).ToList();

                        if (spPlayers.Count > 0)
                        {
                            var random073 = spPlayers[UnityEngine.Random.Range(0, spPlayers.Count)];
                            SpawnScp073(random073);
                        }
                    }
                }
            });
        }

        public void OnHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker == null || ev.Target == null) return;

            if (ev.Attacker == Scp073CurrentPlayer)
            {
                ev.Amount = ev.Amount + (ev.Amount * 0.3f);
            }

            if (ev.Target == Scp073CurrentPlayer)
            {
                ev.Amount *= 0.4f;
            }
        }

        public void OnChangeRole(ChangedRoleEventArgs ev)
        {
            if (ev.Player == null || Scp073CurrentPlayer == null)
                return;

            if (ev.Player == Scp073CurrentPlayer)
            {
                string currentRank = ev.Player.RankName ?? "";
                const string scp073Tag = "SCP-073";
                const string separatorTag = " | SCP-073";
                string newRank = currentRank;

                if (currentRank.Contains(separatorTag))
                {
                    newRank = currentRank.Replace(separatorTag, "");
                }
                else if (currentRank == scp073Tag)
                {
                    newRank = "";
                }

                newRank = newRank.Trim();
                if (newRank.EndsWith(" |"))
                    newRank = newRank.Substring(0, newRank.Length - 2).Trim();

                ev.Player.RankName = newRank;

                Scp073CurrentPlayer = null;
                RExiled.Events.Handlers.Player.ChangedRole -= OnChangeRole;
            }
        }

        public void OnDied(DiedEventArgs ev)
        {
            if (ev.Target == null || Scp073CurrentPlayer == null)
                return;

            if (ev.Target == Scp073CurrentPlayer)
            {
                // 安全清除称号：使用 ev.Target 而非 Scp073CurrentPlayer
                string currentRank = ev.Target.RankName ?? "";
                const string scp073Tag = "SCP-073";
                const string separatorTag = " | SCP-073";
                string newRank = currentRank;

                if (currentRank.Contains(separatorTag))
                {
                    newRank = currentRank.Replace(separatorTag, "");
                }
                else if (currentRank == scp073Tag)
                {
                    newRank = "";
                }

                newRank = newRank.Trim();
                if (newRank.EndsWith(" |"))
                    newRank = newRank.Substring(0, newRank.Length - 2).Trim();

                ev.Target.RankName = newRank;

                // 重置状态
                Scp073CurrentPlayer = null;
                RExiled.Events.Handlers.Player.ChangedRole -= OnChangeRole;

                BroadcastSystem.BroadcastSystem.ShowGlobal("<color=blue>SCP-073</color>已被收容！", 5);
            }
        }
    }
}