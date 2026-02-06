using DreamPlugin.Badge;
using MEC;
using Mirror;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using RExiled.Events.EventArgs.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace DreamPlugin.Game
{
    public class CommandHandler
    {
        public void RegisterEvents()
        {
            RExiled.Events.Handlers.Player.PlayerCommandExecuting += OnPlayerCommandEnter;
            RExiled.Events.Handlers.Server.ServerCommandExecuting += OnServerEnterCommand;
            RExiled.Events.Handlers.Player.RemoteAdminCommandExecuting += OnRemoteAdminCommandExecuting;
        }

        public void UnregisterEvents()
        {
            RExiled.Events.Handlers.Player.PlayerCommandExecuting -= OnPlayerCommandEnter;
            RExiled.Events.Handlers.Server.ServerCommandExecuting -= OnServerEnterCommand;
            RExiled.Events.Handlers.Player.RemoteAdminCommandExecuting -= OnRemoteAdminCommandExecuting;
        }

        public void OnServerEnterCommand(ServerCommandExecutingEventArgs ev)
        {
            if (ev.Command.StartsWith("restart") == true)
            {
                ev.IsAllowed = false;
                Round.Restart();

                Timing.CallDelayed(1.5f, Application.Quit);
            }
        }
        public void OnRemoteAdminCommandExecuting(RemoteAdminCommandExecutingEventArgs ev)
        {
            string originalCommand = ev.Command;
            string cmd = originalCommand.ToLower().Trim();

            bool handled = false;

            if (cmd == "clean r" || cmd == "cl r")
            {
                CleanCorpsesNow();
                if (ev.Player != null)
                    ev.Player.RemoteAdminMessage("已尝试清理所有尸体。", true);
                handled = true;
            }
            else if (cmd == "clean i" || cmd == "cl i")
            {
                CleanItemsNow();
                if (ev.Player != null)
                    ev.Player.RemoteAdminMessage("已尝试清理所有地面物品。", true);
                handled = true;
            }
            else if (cmd == "iamm t")
            {
                InfiniteAmmo.IsInfiniteAmmoEnabled = true;
                if (ev.Player != null)
                    ev.Player.RemoteAdminMessage("已启动 <b>真·无限子弹</b>", true);
                handled = true;
            }
            else if (cmd == "iamm f")
            {
                InfiniteAmmo.IsInfiniteAmmoEnabled = false;
                if (ev.Player != null)
                    ev.Player.RemoteAdminMessage("已关闭 <b>真·无限子弹</b>", true);
                handled = true;
            }
            else if (cmd.StartsWith("stg "))
            {
                string[] args = cmd.Substring(4).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (args.Length != 2)
                {
                    ev.Player?.RemoteAdminMessage("<color=red>用法: stg <玩家ID> <权限组名称></color>", true);
                    handled = true;
                    return;
                }

                string playerIdStr = args[0];
                string groupName = args[1];

                Player target = null;
                if (int.TryParse(playerIdStr, out int playerId))
                {
                    target = Player.Get(playerId);
                }

                if (target == null)
                {
                    ev.Player?.RemoteAdminMessage($"<color=red>未找到 ID 为 {playerIdStr} 的玩家。</color>", true);
                    handled = true;
                    return;
                }

                var hub = target.ReferenceHub;
                if (hub == null)
                {
                    ev.Player?.RemoteAdminMessage("<color=red>无法获取目标玩家的 Hub。</color>", true);
                    handled = true;
                    return;
                }

                try
                {
                    target?.SetGroup(target?.GameObject, groupName);
                    string successMsg = $"已将玩家 <b>{target.Nickname}</b> 的权限组设置为 <b>{groupName}</b>";
                    ev.Player?.RemoteAdminMessage(successMsg, true);
                    target.RemoteAdminMessage($"你的权限组已被设置为 <b>{groupName}</b>", true);
                }
                catch (Exception ex)
                {
                    ev.Player?.RemoteAdminMessage($"<color=red>设置权限组失败: {ex.Message}</color>", true);
                }

                handled = true;
            }

            if (handled)
            {
                ev.IsAllowed = false;
            }
        }
        public void OnPlayerCommandEnter(PlayerCommandExecutingEventArgs ev)
        {
            ev.IsAllowed = false;
            if (ev?.Player == null || string.IsNullOrEmpty(ev.Command))
            {
                return;
            }

            string cmd = ev.Command.ToLower();

            if (cmd.StartsWith("bc "))
            {
                string msg = cmd.Substring(3);
                if (msg == string.Empty || msg == "" || msg == " ")
                {
                    ev.Player.SendConsoleMessage("不可发送空字符", "red");
                    return;
                }
                BroadcastSystem.BroadcastSystem.ShowGlobal($"[聊天] {ev.Player.Nickname} 说: {msg}",5);
                ev.Player.SendConsoleMessage("聊天消息发送成功", "green");
                return;
            }

            if (cmd.StartsWith("c "))
            {
                string msg = cmd.Substring(2).Trim();
                if (string.IsNullOrEmpty(msg))
                {
                    ev.Player.SendConsoleMessage("不可发送空字符", "red");
                    return;
                }

                LogicalTeam senderTeam = GetLogicalTeam(ev.Player);
                List<Player> receivers;

                if (senderTeam == LogicalTeam.None)
                {
                    receivers = new List<Player> { ev.Player };
                }
                else
                {
                    receivers = new List<Player>();
                    foreach (Player p in Player.List)
                    {
                        if (GetLogicalTeam(p) == senderTeam)
                        {
                            receivers.Add(p);
                        }
                    }
                }

                foreach (Player p in receivers)
                {
                    BroadcastSystem.BroadcastSystem.ShowToPlayer(p, $"[阵营] {ev.Player.Nickname} 说: {msg}", 5);
                }

                ev.Player.SendConsoleMessage("阵营消息发送成功", "green");
                return;
            }

            if (cmd.StartsWith("bag "))
            {
                HandleBadgeCommand(ev);
                return;
            }

            if (cmd.StartsWith("killme") || cmd.StartsWith("kl") || cmd.StartsWith("自杀"))
            {
                if (ev.Player.IsSCP)
                {
                    ev.Player.Kill(DamageTypes.Nuke);
                }
                else
                {
                    ev.Player.Kill(DamageTypes.None);
                }

                var pickups = UnityEngine.Object.FindObjectsOfType<Pickup>();

                foreach (var pickup in pickups)
                {
                    if (pickup == null || pickup.gameObject == null)
                        continue;

                    if (pickup.Networkinfo.itemId == ItemType.Ammo556 || pickup.Networkinfo.itemId == ItemType.Ammo762 || pickup.Networkinfo.itemId == ItemType.Ammo9mm)
                    {
                        NetworkServer.Destroy(pickup.gameObject);
                    }
                }
                return;
            }

            ev.Player.SendConsoleMessage("未知指令!", "red");
        }
        public enum LogicalTeam
        {
            None,
            MtfScientist,
            ChaosDClass,
            Scp
        }
        private LogicalTeam GetLogicalTeam(Player player)
        {
            if (player == null)
                return LogicalTeam.None;

            RoleType role = player.Role;

            if (player == DreamPlugin.Plugin.plugin.SCP073.Scp073CurrentPlayer)
            {
                return LogicalTeam.Scp;
            }

            if (role == RoleType.None || role == RoleType.Spectator || role == RoleType.Tutorial)
            {
                return LogicalTeam.None;
            }

            if (role == RoleType.Scientist ||
                role == RoleType.NtfScientist ||
                role == RoleType.NtfCadet ||
                role == RoleType.NtfLieutenant ||
                role == RoleType.NtfCommander ||
                role == RoleType.FacilityGuard)
            {
                return LogicalTeam.MtfScientist;
            }

            if (role == RoleType.ClassD ||
                role == RoleType.ChaosInsurgency)
            {
                return LogicalTeam.ChaosDClass;
            }

            if (role == RoleType.Scp173 ||
                role == RoleType.Scp106 ||
                role == RoleType.Scp049 ||
                role == RoleType.Scp079 ||
                role == RoleType.Scp096 ||
                role == RoleType.Scp0492 ||
                role == RoleType.Scp93953 ||
                role == RoleType.Scp93989)
            {
                return LogicalTeam.Scp;
            }

            return LogicalTeam.None;
        }
        private void HandleBadgeCommand(PlayerCommandExecutingEventArgs ev)
        {
            var args = ev.Command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var player = ev.Player;

            if (args.Length == 1)
            {
                player.SendConsoleMessage("用法: bag <账号> <密码> 或 bag <子命令>", "red");
                return;
            }

            string subCommand = args[1].ToLower();
            var badgeManager = Plugin.plugin.BadgeManager;
            string adminPassword = Plugin.plugin.Config.AdminPwd;

            switch (subCommand)
            {
                case "reg":
                case "register":
                    HandleRegister(player, args, adminPassword);
                    break;

                case "del":
                case "delete":
                    HandleDelete(player, args, adminPassword);
                    break;

                case "login":
                case "log":
                    if (args.Length >= 4)
                        HandleLogin(player, args[2], args[3]);
                    else
                        player.SendConsoleMessage("用法: bag login <账号> <密码>", "red");
                    break;

                case "info":
                    if (args.Length >= 3)
                        HandleInfo(player, args[2]);
                    else
                        player.SendConsoleMessage("用法: bag info <账号>", "red");
                    break;

                default:
                    if (args.Length >= 3)
                    {
                        HandleLogin(player, args[1], args[2]);
                    }
                    else
                    {
                        player.SendConsoleMessage("用法: bag <账号> <密码> 或 bag <子命令>", "red");
                    }
                    break;
            }
        }

        private void HandleRegister(RExiled.API.Features.Player player, string[] args, string adminPassword)
        {
            if (args.Length < 8)
            {
                string masked = string.IsNullOrEmpty(adminPassword) ? "未设置" :
                    adminPassword.Length <= 2 ? "***" : adminPassword.Substring(0, 2) + new string('*', adminPassword.Length - 2);

                player.SendConsoleMessage(
                    "\n用法: bag reg <管理员密码> <账号> <密码> <类型> <月数> <内容> [颜色]\n" +
                    "类型: sr(单色), rr(彩色), sdr(单色动态), rdr(彩色动态)\n" +
                    $"当前管理员密码: {masked}", "yellow");
                return;
            }

            string inputAdminPwd = args[2];
            string account = args[3];
            string password = args[4];
            string typeStr = args[5].ToLower();
            string monthStr = args[6].ToLower();
            string content = args[7];
            string color = args.Length > 8 ? args[8] : null;

            if (inputAdminPwd != adminPassword)
            {
                player.SendConsoleMessage("管理员密码错误，注册失败", "red");
                return;
            }

            int expirationMonths = 0;
            if (monthStr != "all")
            {
                if (!int.TryParse(monthStr, out expirationMonths) || expirationMonths <= 0)
                {
                    player.SendConsoleMessage("月数必须为正整数或'all'（永久）", "red");
                    return;
                }
            }

            BadgeType badgeType;
            switch (typeStr)
            {
                case "sr": badgeType = BadgeType.Simple; break;
                case "rr": badgeType = BadgeType.Rainbow; break;
                case "sdr": badgeType = BadgeType.SimpleDynamic; break;
                case "rdr": badgeType = BadgeType.RainbowDynamic; break;
                default:
                    player.SendConsoleMessage("无效的称号类型! 可用: sr, rr, sdr, rdr", "red");
                    return;
            }

            if (Plugin.plugin.BadgeManager.RegisterBadge(account, password, badgeType, content, color, expirationMonths))
            {
                string expireInfo = expirationMonths == 0 ? "永久" : $"{expirationMonths}个月";
                player.SendConsoleMessage($"称号账号 {account} 注册成功! 有效期: {expireInfo}", "green");
            }
            else
            {
                player.SendConsoleMessage("注册失败", "red");
            }
        }

        private void HandleDelete(RExiled.API.Features.Player player, string[] args, string adminPassword)
        {
            if (args.Length < 4)
            {
                player.SendConsoleMessage("用法: bag del <管理员密码> <账号>", "red");
                return;
            }

            string inputAdminPwd = args[2];
            string account = args[3];

            if (inputAdminPwd != adminPassword)
            {
                player.SendConsoleMessage("管理员密码错误，删除失败", "red");
                return;
            }

            if (Plugin.plugin.BadgeManager.DeleteBadge(account))
            {
                player.SendConsoleMessage($"称号账号 {account} 删除成功!", "green");
            }
            else
            {
                player.SendConsoleMessage("删除失败，账号不存在", "red");
            }
        }

        private void HandleLogin(RExiled.API.Features.Player player, string account, string password)
        {
            var badge = Plugin.plugin.BadgeManager.Login(player, account, password);
            if (badge != null)
            {
                string expireInfo = badge.ExpirationMonths == 0 ? "永久" : $"{badge.ExpirationMonths}个月";
                player.SendConsoleMessage($"登录成功! 有效期: {expireInfo}。请务必妥善保管账号密码", "green");
            }
            else
            {
                player.SendConsoleMessage("登录失败，账号或密码错误，或账号已过期", "red");
            }
        }

        private void HandleInfo(RExiled.API.Features.Player player, string account)
        {
            string info = Plugin.plugin.BadgeManager.GetAccountExpirationInfo(account);
            player.SendConsoleMessage($"账号 {account} 的状态: {info}", "white");
        }

        private void CleanCorpsesNow()
        {
            try
            {
                var ragdolls = UnityEngine.Object.FindObjectsOfType<Ragdoll>();
                int count = 0;
                foreach (var ragdoll in ragdolls)
                {
                    if (ragdoll != null && ragdoll.gameObject != null)
                    {
                        NetworkServer.Destroy(ragdoll.gameObject);
                        count++;
                    }
                }
                if (count > 0)
                {
                    BroadcastSystem.BroadcastSystem.ShowGlobal($"[手动清理] 已清理 {count} 具尸体");
                    Log.Info($"[手动清理] 已清理 {count} 具尸体");
                }
                else
                {
                    Log.Info("[手动清理] 未发现尸体");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[手动清理] 清理尸体失败: {ex}");
            }
        }

        private void CleanItemsNow()
        {
            try
            {
                var pickups = UnityEngine.Object.FindObjectsOfType<Pickup>();
                int count = 0;
                foreach (var pickup in pickups)
                {
                    if (pickup != null && pickup.gameObject != null)
                    {
                        NetworkServer.Destroy(pickup.gameObject);
                        count++;
                    }
                }
                var ragdolls = UnityEngine.Object.FindObjectsOfType<Ragdoll>();
                foreach (var ragdoll in ragdolls)
                {
                    if (ragdoll != null && ragdoll.gameObject != null)
                    {
                        NetworkServer.Destroy(ragdoll.gameObject);
                    }
                }
                if (count > 0)
                {
                    BroadcastSystem.BroadcastSystem.ShowGlobal($"[手动清理] 已清理 {count} 个地面物品");
                    Log.Info($"[手动清理] 已清理 {count} 个地面物品");
                }
                else
                {
                    Log.Info("[手动清理] 未发现地面物品");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[手动清理] 清理物品失败: {ex}");
            }
        }
    }
}