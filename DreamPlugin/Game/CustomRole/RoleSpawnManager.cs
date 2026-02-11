using DreamPlugin.Game.CustomRole.Extensions;
using MEC;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using System.Collections.Generic;
using System.Linq;

namespace DreamPlugin.Game.CustomRole
{
    public static class RoleSpawnManager
    {
        private static readonly Dictionary<string, CustomRole> ActiveRoles = new Dictionary<string, CustomRole>();
        private static readonly List<CustomRole> RegisteredRoles = new List<CustomRole>();

        public static void Register(CustomRole role)
        {
            RegisteredRoles.Add(role);
        }

        public static void UnregisterAll()
        {
            foreach (var role in ActiveRoles.Values)
                role.Destroy();
            ActiveRoles.Clear();
            RegisteredRoles.Clear();
        }

        public static bool TrySpawnByCommand(string roleName, Player player)
        {
            var role = RegisteredRoles.FirstOrDefault(r =>
                r.Name.Equals(roleName, System.StringComparison.OrdinalIgnoreCase));

            if (role == null || role.SpawnCondition != SpawnConditionType.CommandOnly)
                return false;

            if (ActiveRoles.ContainsKey(role.Name))
            {
                player.RemoteAdminMessage($"场内已有 {role.Name}，无法重复刷新。", true);
                return false;
            }

            role.AssignTo(player);
            ActiveRoles[role.Name] = role;
            player.RemoteAdminMessage($"成功刷新 {role.Name}！", false);
            return true;
        }

        public static void OnRoundStarted()
        {
            Timing.CallDelayed(1.2f, () =>
            {
                var allPlayers = Player.List.ToList();
                var alivePlayers = allPlayers.Where(p => p.Role != RoleType.Spectator).ToList();
                if (alivePlayers.Count == 0) return;

                int total = alivePlayers.Count;
                var unassigned = new List<Player>(alivePlayers);

                AssignBaseRoles(unassigned, total);

                AssignCustomRoles(unassigned, total);
            });
        }

        public static void OnSpawnedTeam(SpawnedTeamEventArgs ev)
        {
            if (ev.Players == null || !ev.Players.Any()) return;

            var players = ev.Players.ToList();
            int total = Player.List.Count();
            AssignTeamCustomRoles(players, total, ev.IsChaos);
        }

        private static void AssignBaseRoles(List<Player> pool, int totalCount)
        {
            if (totalCount == 1)
            {
                AssignFromPool(pool, RoleType.ClassD, 1);
                return;
            }

            if (totalCount == 2)
            {
                AssignFromPool(pool, RoleType.ClassD, 1);
                AssignFromPool(pool, GetRandomScp(exclude079: true), 1);
                return;
            }

            if (totalCount >= 3 && totalCount <= 5)
            {
                int d = 2;
                int scp = 1;
                int ntf = (totalCount >= 4) ? 1 : 0;
                int sci = (totalCount >= 5) ? 1 : 0;

                AssignFromPool(pool, GetRandomScp(exclude079: true), scp);
                AssignFromPool(pool, RoleType.Scientist, sci);
                AssignFromPool(pool, RoleType.NtfCadet, ntf);
                AssignFromPool(pool, RoleType.ClassD, d);
                return;
            }

            if (totalCount >= 6 && totalCount <= 10)
            {
                AssignFromPool(pool, GetRandomScp(exclude079: true), 2);
                AssignFromPool(pool, RoleType.Scientist, 2);
                AssignFromPool(pool, RoleType.NtfCadet, 2);
                AssignFromPool(pool, RoleType.ClassD, 4);
                return;
            }

            if (totalCount >= 11 && totalCount <= 20)
            {
                AssignFromPool(pool, GetRandomScp(exclude079: false), 4);
                AssignFromPool(pool, RoleType.Scientist, 4);
                AssignFromPool(pool, RoleType.NtfCadet, 4);
                AssignFromPool(pool, RoleType.ClassD, 8);
                return;
            }

            if (totalCount > 20)
            {
                AssignFromPool(pool, GetRandomScp(exclude079: false), 7);
                AssignFromPool(pool, RoleType.Scientist, 2);
                AssignFromPool(pool, RoleType.NtfCadet, 5);
                AssignFromPool(pool, RoleType.ClassD, 16);
            }

            foreach (var p in pool)
                p.SetRole(RoleType.ClassD, true);
        }

        private static void AssignFromPool(List<Player> pool, RoleType role, int count)
        {
            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int idx = UnityEngine.Random.Range(0, pool.Count);
                pool[idx].SetRole(role, true);
                pool.RemoveAt(idx);
            }
        }

        private static RoleType GetRandomScp(bool exclude079)
        {
            var scps = new List<RoleType>
            {
                RoleType.Scp173, RoleType.Scp106, RoleType.Scp049,
                RoleType.Scp096, RoleType.Scp93953, RoleType.Scp93989, RoleType.Scp0492
            };
            if (!exclude079) scps.Add(RoleType.Scp079);
            return scps[UnityEngine.Random.Range(0, scps.Count)];
        }

        private static void AssignCustomRoles(List<Player> candidates, int totalPlayers)
        {
            var classDCount = Player.List.Count(p => p.Role == RoleType.ClassD);
            var scientistCount = Player.List.Count(p => p.Role == RoleType.Scientist);
            var scpCount = Player.List.Count(p => p.Role.IsScp());

            foreach (var role in RegisteredRoles)
            {
                if (ActiveRoles.ContainsKey(role.Name)) continue;
                if (!role.IsJoinSpawnQueue) continue;
                if (role.SpawnCondition != SpawnConditionType.RoundStart) continue;
                if (totalPlayers < role.SpawnCapacityLimit) continue;
                if (!role.SpawnRoleType.IsAllowedInRoundStart()) continue;

                int poolSize = 0;
                if (role.SpawnRoleType == RoleType.ClassD) poolSize = classDCount;
                else if (role.SpawnRoleType == RoleType.Scientist) poolSize = scientistCount;
                else if (role.SpawnRoleType.IsScp()) poolSize = scpCount;
                else poolSize = Player.List.Count(p => p.Role == role.SpawnRoleType);

                if (poolSize < role.SpawnRoleCapacityLimit) continue;

                var eligible = candidates.Where(p => p.Role == role.SpawnRoleType).ToList();
                if (eligible.Count == 0) continue;

                var selected = eligible[UnityEngine.Random.Range(0, eligible.Count)];
                candidates.Remove(selected);

                role.AssignTo(selected);
                ActiveRoles[role.Name] = role;
            }
        }

        private static void AssignTeamCustomRoles(List<Player> teamPlayers, int totalPlayers, bool isChaos)
        {
            foreach (var role in RegisteredRoles)
            {
                if (ActiveRoles.ContainsKey(role.Name)) continue;
                if (!role.IsJoinSpawnQueue) continue;
                if (role.SpawnCondition != SpawnConditionType.TeamSpawn) continue;
                if (totalPlayers < role.SpawnCapacityLimit) continue;

                if (isChaos && !role.SpawnRoleType.IsChaos()) continue;
                if (!isChaos && !role.SpawnRoleType.IsNTF()) continue;

                int poolSize = Player.List.Count(p => p.Role == role.SpawnRoleType);
                if (poolSize < role.SpawnRoleCapacityLimit) continue;

                var eligible = teamPlayers.Where(p => p.Role == role.SpawnRoleType).ToList();
                if (eligible.Count == 0) continue;

                var selected = eligible[UnityEngine.Random.Range(0, eligible.Count)];
                role.AssignTo(selected);
                ActiveRoles[role.Name] = role;
            }
        }
    }
}