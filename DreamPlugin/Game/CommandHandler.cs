using DreamPlugin.Badge;
using MEC;
using Mirror;
using RExiled.API.Extensions;
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
            else if (cmd == "setyttt")
            {
                if (ev.Player != null)
                {
                    ev.Player.SetNickname("我是测试大卡尔二二二");
                    ev.Player.RemoteAdminMessage("成功修改", true);
                }
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
            else if (cmd.StartsWith("spi ") || cmd.StartsWith("spim "))
            {
                bool isAtPlayer = cmd.StartsWith("spim ");
                string subCmd = cmd.Substring(isAtPlayer ? 5 : 4).Trim();
                string[] args = subCmd.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                int expectedArgCount = isAtPlayer ? 6 : 8;
                if (args.Length != expectedArgCount)
                {
                    string usage = isAtPlayer
                        ? "<color=red>用法: spim <物品ID> <数量> <玩家ID> <大小X> <大小Y> <大小Z></color>"
                        : "<color=red>用法: spi <物品ID> <数量> <X> <Y> <Z> <大小X> <大小Y> <大小Z></color>";
                    ev.Player?.RemoteAdminMessage(usage, true);
                    handled = true;
                    return;
                }

                if (!int.TryParse(args[0], out int itemId) || !Enum.IsDefined(typeof(ItemType), itemId))
                {
                    ev.Player?.RemoteAdminMessage("<color=red>无效的物品ID。</color>", true);
                    handled = true;
                    return;
                }
                if (!int.TryParse(args[1], out int amount) || amount <= 0)
                {
                    ev.Player?.RemoteAdminMessage("<color=red>数量必须是正整数。</color>", true);
                    handled = true;
                    return;
                }

                Vector3 spawnPosition;
                if (isAtPlayer)
                {
                    if (!int.TryParse(args[2], out int playerId))
                    {
                        ev.Player?.RemoteAdminMessage("<color=red>玩家ID必须是整数。</color>", true);
                        handled = true;
                        return;
                    }

                    Player targetPlayer = Player.Get(playerId);
                    if (targetPlayer == null)
                    {
                        ev.Player?.RemoteAdminMessage($"<color=red>未找到ID为 {playerId} 的有效玩家。</color>", true);
                        handled = true;
                        return;
                    }

                    spawnPosition = targetPlayer.Position;
                }
                else
                {
                    if (!float.TryParse(args[2], out float x) ||
                        !float.TryParse(args[3], out float y) ||
                        !float.TryParse(args[4], out float z))
                    {
                        ev.Player?.RemoteAdminMessage("<color=red>位置坐标必须是有效数字。</color>", true);
                        handled = true;
                        return;
                    }
                    spawnPosition = new Vector3(x, y, z);
                }

                if (!float.TryParse(args[isAtPlayer ? 3 : 5], out float scaleX) ||
                    !float.TryParse(args[isAtPlayer ? 4 : 6], out float scaleY) ||
                    !float.TryParse(args[isAtPlayer ? 5 : 7], out float scaleZ))
                {
                    ev.Player?.RemoteAdminMessage("<color=red>缩放参数必须是有效数字。</color>", true);
                    handled = true;
                    return;
                }

                Vector3 scale = new Vector3(scaleX, scaleY, scaleZ);

                try
                {
                    for (int i = 0; i < amount; i++)
                    {
                        Pickup pickup = ((ItemType)itemId).Spawn(30f, spawnPosition);
                        if (pickup != null)
                        {
                            pickup.transform.localScale = scale;

                            pickup.SyncPickupSize();
                        }
                    }

                    string itemName = Enum.GetName(typeof(ItemType), (ItemType)itemId) ?? "未知物品";
                    string msg = isAtPlayer
                        ? $"已在玩家 <b>{Player.Get(int.Parse(args[2])).Nickname}</b> 位置生成 <b>{amount}x {itemName}</b>（缩放: {scale}）"
                        : $"已在位置 <b>{spawnPosition:0.##}</b> 生成 <b>{amount}x {itemName}</b>（缩放: {scale}）";
                    ev.Player?.RemoteAdminMessage(msg, true);
                }
                catch (Exception ex)
                {
                    ev.Player?.RemoteAdminMessage($"<color=red>生成物品时出错: {ex.Message}</color>", true);
                }

                handled = true;
            }
            else if (cmd.StartsWith("spl "))
            {
                string subCmd = cmd.Substring(4).Trim();
                string[] args = subCmd.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (args.Length != 4)
                {
                    ev.Player?.RemoteAdminMessage("<color=red>用法: spl <玩家ID> <大小X> <大小Y> <大小Z></color>", true);
                    handled = true;
                    return;
                }

                if (!int.TryParse(args[0], out int playerId))
                {
                    ev.Player?.RemoteAdminMessage("<color=red>玩家ID必须是整数。</color>", true);
                    handled = true;
                    return;
                }

                Player targetPlayer = Player.Get(playerId);
                if (targetPlayer == null)
                {
                    ev.Player?.RemoteAdminMessage($"<color=red>未找到ID为 {playerId} 的有效玩家。</color>", true);
                    handled = true;
                    return;
                }

                if (!float.TryParse(args[1], out float x) ||
                    !float.TryParse(args[2], out float y) ||
                    !float.TryParse(args[3], out float z))
                {
                    ev.Player?.RemoteAdminMessage("<color=red>缩放参数必须是有效数字。</color>", true);
                    handled = true;
                    return;
                }

                const float MAX_SCALE = 10f;
                const float MIN_SCALE = 0.1f;

                x = Mathf.Clamp(x, MIN_SCALE, MAX_SCALE);
                y = Mathf.Clamp(y, MIN_SCALE, MAX_SCALE);
                z = Mathf.Clamp(z, MIN_SCALE, MAX_SCALE);

                try
                {
                    targetPlayer.SetScale(x, y, z);
                    ev.Player?.RemoteAdminMessage(
                        $"已将玩家 <b>{targetPlayer.Nickname}</b> 的体积设置为 <b>({x:0.##}, {y:0.##}, {z:0.##})</b>", true);
                }
                catch (Exception ex)
                {
                    ev.Player?.RemoteAdminMessage($"<color=red>设置玩家体积时出错: {ex.Message}</color>", true);
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