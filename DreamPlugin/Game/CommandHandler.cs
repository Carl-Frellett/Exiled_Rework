using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using System;
using System.Linq;
using UnityEngine;

namespace DreamPlugin.Game
{
    public class CommandHandler
    {
        public void RegisterEvents()
        {
            RExiled.Events.Handlers.Player.PlayerCommandExecuting += OnPlayerCommandEnter;
            RExiled.Events.Handlers.Player.RemoteAdminCommandExecuting += OnPlayerRACommandEnter;
        }

        public void UnregisterEvents()
        {
            RExiled.Events.Handlers.Player.PlayerCommandExecuting -= OnPlayerCommandEnter;
            RExiled.Events.Handlers.Player.RemoteAdminCommandExecuting -= OnPlayerRACommandEnter;
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

            ev.Player.SendConsoleMessage("未知指令!", "red");
        }

        public void OnPlayerRACommandEnter(RemoteAdminCommandExecutingEventArgs ev)
        {
            if (ev.Player == null)
                return;

            string cmd = ev.Command;

            if (cmd.StartsWith("SetScale ", StringComparison.OrdinalIgnoreCase) ||
                cmd.StartsWith("sps ", StringComparison.OrdinalIgnoreCase))
            {
                ev.IsAllowed = true;

                var args = cmd.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (args.Length < 5)
                {
                    ev.Player.SendConsoleMessage("用法: sps <目标> <X> <Y> <Z>", "red");
                    return;
                }

                string targetSpec = args[1];
                if (!float.TryParse(args[2], out float x) ||
                    !float.TryParse(args[3], out float y) ||
                    !float.TryParse(args[4], out float z))
                {
                    ev.Player.SendConsoleMessage("错误: X/Y/Z 必须为数字！", "red");
                    return;
                }

                x = Mathf.Clamp(x, 0.1f, 100f);
                y = Mathf.Clamp(y, 0.1f, 100f);
                z = Mathf.Clamp(z, 0.1f, 100f);

                var targets = new System.Collections.Generic.List<Player>();

                if (targetSpec.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    targets.AddRange(Player.List.Where(p => p.IsAlive));
                }
                else if (targetSpec.Contains("."))
                {
                    foreach (var idStr in targetSpec.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (int.TryParse(idStr, out int id) && Player.Get(id) is var p && p != null && p.IsAlive)
                            targets.Add(p);
                    }
                }
                else if (int.TryParse(targetSpec, out int singleId) && Player.Get(singleId) is var p2 && p2 != null && p2.IsAlive)
                {
                    targets.Add(p2);
                }

                if (targets.Count == 0)
                {
                    ev.Player.SendConsoleMessage("未找到有效目标。", "red");
                    return;
                }

                foreach (var p in targets)
                {
                    try
                    {
                        p.ReferenceHub.transform.localScale = new UnityEngine.Vector3(x, y, z);
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error($"[SetScale] 应用缩放失败: {ex}");
                    }
                }

                ev.Player.SendConsoleMessage($"已设置 {targets.Count} 名玩家缩放为 ({x:F2}, {y:F2}, {z:F2})", "green");
                Log.Info($"[SetScale] {ev.Player.Nickname} 设置了 {targets.Count} 名玩家的缩放");
            }
        }
    }
}