using DreamPlugin.Badge;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using System;
using System.Linq;

namespace DreamPlugin.Game
{
    public class CommandHandler
    {
        public void RegisterEvents()
        {
            RExiled.Events.Handlers.Player.PlayerCommandExecuting += OnPlayerCommandEnter;
        }

        public void UnregisterEvents()
        {
            RExiled.Events.Handlers.Player.PlayerCommandExecuting -= OnPlayerCommandEnter;
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
                Map.Broadcast(5, $"<size=30>[聊天] {ev.Player.Nickname} 说: {msg}</size>");
                ev.Player.SendConsoleMessage("聊天消息发送成功", "green");
                return;
            }

            if (cmd.StartsWith("c "))
            {
                string msg = cmd.Substring(2);
                if (msg == string.Empty || msg == "" || msg == " ")
                {
                    ev.Player.SendConsoleMessage("不可发送空字符", "red");
                    return;
                }
                var teammates = Player.List.Where(p => p.Team == ev.Player.Team).ToList();
                foreach (var p in teammates)
                {
                    p.Broadcast(5, $"<size=30>[阵营] {ev.Player.Nickname} 说: {msg}</size>");
                }
                ev.Player.SendConsoleMessage("阵营消息发送成功", "green");
                return;
            }

            if (cmd.StartsWith("bag "))
            {
                HandleBadgeCommand(ev);
                return;
            }

            ev.Player.SendConsoleMessage("未知指令!", "red");
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
                    // 尝试直接登录：bag <账号> <密码>
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
    }
}