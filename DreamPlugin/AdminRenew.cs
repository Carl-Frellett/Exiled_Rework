using System;
using System.Collections.Generic;
using DreamPlugin;
using RExiled.API.Features;

public class AdminRenew
{
    private  readonly Dictionary<string, TrustedEntry> _cache = new Dictionary<string, TrustedEntry>();
    private  int _currentRoundIndex = -1; 

    public class TrustedEntry
    {
        public string Nickname;
        public int LastSeenRoundIndex;
    }

    public  void RegisterEvents()
    {
        RExiled.Events.Handlers.Player.Joined += OnPlayerJoined;
        RExiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
        RExiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
    }

    public  void UnregisterEvents()
    {
        RExiled.Events.Handlers.Player.Joined -= OnPlayerJoined;
        RExiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
        RExiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;

        _cache.Clear();
        _currentRoundIndex = -1;
    }

    private  void OnRoundStarted()
    {
        _currentRoundIndex++;
    }

    private  void OnRoundEnded()
    {
        if (_currentRoundIndex < 0) return;

        var toRemove = new List<string>();

        foreach (var kvp in _cache)
        {
            var entry = kvp.Value;
            if (_currentRoundIndex - entry.LastSeenRoundIndex > Plugin.plugin.Config.MaxMissedRounds)
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var ip in toRemove)
        {
            _cache.Remove(ip);
        }
    }

    private  void OnPlayerJoined(RExiled.Events.EventArgs.Player.JoinedEventArgs ev)
    {
        var player = ev.Player;
        if (player == null) return;

        string ip = player.IPAddress;
        string nick = player.Nickname;

        if (player.RemoteAdminAccess)
        {
            _cache[ip] = new TrustedEntry
            {
                Nickname = nick,
                LastSeenRoundIndex = _currentRoundIndex
            };
            SetOwnerGroup(player);
            Log.Info($"[AdminRenew] 记录已认证管理员: {nick} ({ip}) @ 回合 {_currentRoundIndex}");
            return;
        }

        if (_cache.TryGetValue(ip, out var entry))
        {
            if (entry.Nickname == nick)
            {
                entry.LastSeenRoundIndex = _currentRoundIndex;
                SetOwnerGroup(player);
                Log.Info($"[AdminRenew] 自动恢复权限: {nick} ({ip})");
            }
            else
            {
            }
        }
    }

    private  void SetOwnerGroup(Player player)
    {
        try
        {
            UserGroup group = ServerStatic.GetPermissionsHandler().GetGroup("owner");
            if (group == null)
            {
                Log.Warn("[AdminRenew] 未找到 'owner' 权限组！");
                return;
            }

            player.Group = group;
        }
        catch (Exception e)
        {
            Log.Error($"[AdminRenew] 赋予 Owner 失败: {e}");
        }
    }
}