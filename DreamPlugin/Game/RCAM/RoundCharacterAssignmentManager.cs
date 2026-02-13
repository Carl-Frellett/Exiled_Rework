using DreamPlugin.Game.RCAM.Profile;
using MEC;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DreamPlugin.Game.RCAM
{
    public class RoundCharacterAssignmentManager
    {
        private readonly Dictionary<string, float> _lastProcessedTime = new Dictionary<string, float>();
        private readonly HashSet<string> _processedPlayers = new HashSet<string>();

        private const float DEBOUNCE_WINDOW = 0.1f;
        private const float LATE_JOIN_WINDOW_START = 5f;
        private const float LATE_JOIN_WINDOW_DURATION = 60f;

        private float _roundStartTime = -1f;

        public void OnRoundStarted()
        {
            _roundStartTime = Time.time;
            _processedPlayers.Clear();
            Timing.CallDelayed(0.5f, () =>
            {
                var initialPlayers = Player.List.ToList();
                if (initialPlayers.Count > 0)
                {
                    AssignBaseRoles(initialPlayers, initialPlayers.Count);
                }
            });
        }

        public void OnPlayerJoined(JoinedEventArgs ev)
        {
            if (_roundStartTime == -1f || ev.Player == null)
                return;

            float currentTime = Time.time;
            float elapsed = currentTime - _roundStartTime;

            if (elapsed >= LATE_JOIN_WINDOW_START && elapsed < LATE_JOIN_WINDOW_START + LATE_JOIN_WINDOW_DURATION)
            {
                string userId = ev.Player.UserId;
                if (_processedPlayers.Contains(userId))
                    return;

                _processedPlayers.Add(userId);

                Timing.CallDelayed(1.5f, () =>
                {
                    if (ev.Player == null || !Player.List.Contains(ev.Player))
                        return;

                    if (ev.Player.Role == RoleType.Spectator || ev.Player.Role == RoleType.None)
                    {
                        ev.Player.SetRole(RoleType.ClassD);
                    }
                });
            }
        }

        public void OnRoundEnded()
        {
            _roundStartTime = -1f;
            _processedPlayers.Clear();
            _lastProcessedTime.Clear();
        }

        public void OnChangedRole(ChangedRoleEventArgs ev)
        {
            if (ev.Player == null)
                return;

            string playerId = ev.Player.UserId;
            float currentTime = Time.time;

            if (_lastProcessedTime.TryGetValue(playerId, out float lastTime))
            {
                if (currentTime - lastTime < DEBOUNCE_WINDOW)
                    return;
            }

            _lastProcessedTime[playerId] = currentTime;

            Timing.CallDelayed(0.5f, () =>
            {
                if (ev.Player == null || ev.Player.Role != ev.NewRole)
                    return;

                var profile = RoleProfileRegistry.GetProfile(ev.NewRole);
                if (profile == null)
                    return;

                var player = ev.Player;
                player.Health = profile.Health;
                player.MaxHealth = profile.MaxHealth;

                if (profile.StartingItems.Count > 0)
                {
                    player.ResetInventory(profile.StartingItems);
                }
                else
                {
                    player.ClearInventory();
                }

                if (profile.SpawnPosition.HasValue)
                {
                    player.Position = profile.SpawnPosition.Value + new Vector3(0, 4f, 0);
                }
            });
        }

        private void AssignBaseRoles(List<Player> pool, int totalCount)
        {
            if (totalCount <= 0 || pool.Count == 0)
                return;

            // 定义完整的 SCP 池（不含 Scp0492）
            var fullScpPool = new List<RoleType>
    {
        RoleType.Scp173,
        RoleType.Scp106,
        RoleType.Scp049,
        RoleType.Scp096,
        RoleType.Scp93953,
        RoleType.Scp93989,
        RoleType.Scp079  // 注意：079 现在始终包含在池中（当需要时）
    };

            int scpCount, scientistCount, guardCount, dCount;

            if (totalCount == 1)
            {
                dCount = 1;
                scpCount = scientistCount = guardCount = 0;
            }
            else if (totalCount == 2)
            {
                dCount = 1;
                scpCount = 1;
                scientistCount = guardCount = 0;
            }
            else if (totalCount >= 3 && totalCount <= 6)
            {
                scpCount = scientistCount = guardCount = 1;
                dCount = totalCount - 3;
                if (dCount < 1) dCount = 1;
            }
            else if (totalCount >= 7 && totalCount <= 12)
            {
                scpCount = scientistCount = guardCount = 2;
                dCount = totalCount - 6;
                if (dCount <= scientistCount)
                {
                    dCount = scientistCount + 1;
                    int totalAssigned = scpCount + scientistCount + guardCount + dCount;
                    if (totalAssigned > totalCount)
                    {
                        int overflow = totalAssigned - totalCount;
                        dCount -= overflow;
                        if (dCount < 1) dCount = 1;
                    }
                }
            }
            else
            {
                // totalCount >= 13
                guardCount = totalCount / 5;
                scientistCount = guardCount;

                // === 核心逻辑：根据人数确定 SCP 数量 ===
                if (totalCount >= 20)
                {
                    scpCount = fullScpPool.Count; // 7 个 SCP 全部分配
                }
                else // 13 <= totalCount < 20
                {
                    scpCount = 5; // 必须分配 5 个不同的 SCP
                }

                // 计算 D 级数量
                dCount = totalCount - (scpCount + scientistCount + guardCount);
                if (dCount <= scientistCount)
                {
                    dCount = scientistCount + 1;
                    int adjust = (scpCount + scientistCount + guardCount + dCount) - totalCount;
                    if (adjust > 0)
                    {
                        int half = adjust / 2;
                        guardCount = Mathf.Max(0, guardCount - half);
                        scientistCount = Mathf.Max(0, scientistCount - (adjust - half));
                        // 重新计算 dCount 以确保总和正确
                        dCount = totalCount - (scpCount + scientistCount + guardCount);
                        if (dCount < 1) dCount = 1;
                    }
                }
            }

            // Safety: ensure we don't assign more roles than players
            int totalNeeded = scpCount + scientistCount + guardCount + dCount;
            if (pool.Count < totalNeeded)
            {
                // 极端情况：玩家太少，优先保证 D 级
                dCount = pool.Count;
                scpCount = scientistCount = guardCount = 0;
            }

            // === 构建实际要分配的 SCP 列表 ===
            List<RoleType> selectedScps = new List<RoleType>();

            if (totalCount >= 13)
            {
                // 使用完整池
                var tempScpPool = new List<RoleType>(fullScpPool);

                if (totalCount >= 20)
                {
                    // 分配全部 7 个
                    selectedScps = new List<RoleType>(tempScpPool);
                }
                else
                {
                    // 随机选择 5 个不同的 SCP
                    for (int i = 0; i < 5 && tempScpPool.Count > 0; i++)
                    {
                        int idx = Random.Range(0, tempScpPool.Count);
                        selectedScps.Add(tempScpPool[idx]);
                        tempScpPool.RemoveAt(idx);
                    }
                }
            }
            else
            {
                // 小局逻辑：使用原始 SCP 池（不含 079）
                var smallScpPool = new List<RoleType>
        {
            RoleType.Scp173,
            RoleType.Scp106,
            RoleType.Scp049,
            RoleType.Scp096,
            RoleType.Scp93953,
            RoleType.Scp93989
        };

                var tempScpPool = new List<RoleType>(smallScpPool);
                for (int i = 0; i < scpCount && tempScpPool.Count > 0; i++)
                {
                    int idx = Random.Range(0, tempScpPool.Count);
                    selectedScps.Add(tempScpPool[idx]);
                    tempScpPool.RemoveAt(idx);
                }
            }

            // === 执行分配 ===
            foreach (var scp in selectedScps)
            {
                if (pool.Count == 0) break;
                AssignOne(pool, scp);
            }

            for (int i = 0; i < scientistCount && pool.Count > 0; i++)
                AssignOne(pool, RoleType.Scientist);

            for (int i = 0; i < guardCount && pool.Count > 0; i++)
                AssignOne(pool, RoleType.FacilityGuard);

            for (int i = 0; i < dCount && pool.Count > 0; i++)
                AssignOne(pool, RoleType.ClassD);

            // Fallback: any remaining players become Class-D
            foreach (var p in pool)
            {
                p.SetRole(RoleType.ClassD, true);
            }
        }

        private void AssignOne(List<Player> pool, RoleType role)
        {
            if (pool.Count == 0) return;
            int idx = Random.Range(0, pool.Count);
            pool[idx].SetRole(role, true);
            pool.RemoveAt(idx);
        }
    }
}