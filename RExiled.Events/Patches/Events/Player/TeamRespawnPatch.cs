using HarmonyLib;
using System.Linq;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;

namespace RExiled.Events.Patches.Events.Player
{
    [HarmonyPatch(typeof(MTFRespawn), nameof(MTFRespawn.RespawnDeadPlayers))]
    internal static class TeamRespawnPatch
    {
        private static bool Prefix(MTFRespawn __instance)
        {
            try
            {
                var allHubs = ReferenceHub.Hubs.Values.ToList();

                var deadPlayers = allHubs
                    .Where(hub => hub != null &&
                                  hub.gameObject != null &&
                                  !hub.serverRoles.OverwatchEnabled &&
                                  hub.characterClassManager.NetworkCurClass == RoleType.Spectator)
                    .ToList();

                bool isChaos = __instance.nextWaveIsCI;
                int maxRespawn = isChaos ? __instance.maxCIRespawnAmount : __instance.maxMTFRespawnAmount;

                var playersToRespawnHubs = deadPlayers.Take(maxRespawn).ToList();

                var playersToRespawn = playersToRespawnHubs
                    .Select(hub => RExiled.API.Features.Player.Get(hub))
                    .Where(p => p != null)
                    .ToList();

                var spawningEv = new SpawningTeamEventArgs(isChaos, maxRespawn, playersToRespawn);
                Handlers.Player.OnSpawningTeam(spawningEv);

                if (!spawningEv.IsAllowed || spawningEv.Players.Count == 0)
                {
                    playersToRespawnHubs.Clear();
                }
                else
                {
                    playersToRespawnHubs = spawningEv.Players.Select(p => p.ReferenceHub).ToList();
                    isChaos = spawningEv.IsChaos;
                    maxRespawn = spawningEv.MaxRespawnAmount;
                }

                int num = 0;
                __instance.playersToNTF.Clear();

                foreach (var hub in playersToRespawnHubs)
                {
                    if (num >= maxRespawn) break;
                    if (hub == null) continue;

                    num++;

                    if (isChaos)
                    {
                        __instance.GetComponent<CharacterClassManager>()
                                 .SetPlayersClass(RoleType.ChaosInsurgency, hub.gameObject);

                        ServerLogs.AddLog(ServerLogs.Modules.ClassChange,
                            $"{hub.nicknameSync.Network_myNickSync} respawned as Chaos Insurgency agent.",
                            ServerLogs.ServerLogType.GameEvent);
                    }
                    else
                    {
                        __instance.playersToNTF.Add(hub.gameObject);
                    }
                }
                if (num > 0)
                {
                    var spawnedPlayers = playersToRespawnHubs.Take(num)
                        .Select(hub => RExiled.API.Features.Player.Get(hub))
                        .Where(p => p != null)
                        .ToList();

                    var spawnedEv = new SpawnedTeamEventArgs(isChaos, spawnedPlayers);
                    Handlers.Player.OnSpawnedTeam(spawnedEv);

                    ServerLogs.AddLog(ServerLogs.Modules.ClassChange,
                        (isChaos ? "Chaos Insurgency" : "MTF") + " respawned!",
                        ServerLogs.ServerLogType.GameEvent);

                    if (isChaos)
                        __instance.Invoke("CmdDelayCIAnnounc", 1f);
                }

                __instance.SummonNTF();

                return false;
            }
            catch (System.Exception ex)
            {
                Log.Error($"TeamRespawnPatch error: {ex}");
                return true;
            }
        }
    }
}