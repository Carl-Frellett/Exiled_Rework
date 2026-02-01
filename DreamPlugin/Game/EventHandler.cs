using MEC;
using Mirror;
using RExiled.API.Features;
using RExiled.Events.EventArgs;
using RExiled.Events.EventArgs.Player;
using System.Collections.Generic;
using UnityEngine;

namespace DreamPlugin.Game
{
    public class EventHandler
    {
        private static readonly HashSet<int> PendingGiveItems = new HashSet<int>();
        //SCP回血
        private CoroutineHandle SCPHealthCoroutine;
        private Dictionary<Player, Vector3> LastPos = new Dictionary<Player, Vector3>();
        private Dictionary<Player, float> KeepPosTime = new Dictionary<Player, float>();

        public void RegisterEvents()
        {
            RExiled.Events.Handlers.Player.ChangedRole += OnPlayerChangedRole;
            RExiled.Events.Handlers.Player.Joined += OnPlayerJoined;
            SCPHealthCoroutine = Timing.RunCoroutine(SCPHealth());
            StartCleanup();
        }

        public void UnregisterEvents()
        {
            RExiled.Events.Handlers.Player.ChangedRole -= OnPlayerChangedRole;
            RExiled.Events.Handlers.Player.Joined -= OnPlayerJoined;
            PendingGiveItems.Clear();
            StopCleanup();
        }

        #region 物品清理
        private CoroutineHandle _corpseCoroutine;
        private CoroutineHandle _itemCoroutine;

        public void StartCleanup()
        {
            _corpseCoroutine = Timing.RunCoroutine(CleanCorpsesRoutine(), Segment.Update);
            _itemCoroutine = Timing.RunCoroutine(CleanItemsRoutine(), Segment.Update);
        }

        public void StopCleanup()
        {
            if (_corpseCoroutine.IsRunning)
                Timing.KillCoroutines(_corpseCoroutine);

            if (_itemCoroutine.IsRunning)
                Timing.KillCoroutines(_itemCoroutine);
        }

        private IEnumerator<float> CleanCorpsesRoutine()
        {
            while (true)
            {
                yield return Timing.WaitUntilDone(Timing.CallDelayed(300f, () =>
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
                        Map.Broadcast(4,$"[清理系统] 已清理 {count} 具尸体");
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error($"[清理系统] 清理尸体时出错: {ex}");
                    }
                }));
            }
        }

        private IEnumerator<float> CleanItemsRoutine()
        {
            while (true)
            {
                yield return Timing.WaitUntilDone(Timing.CallDelayed(900f, () =>
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
                        Map.Broadcast(4, $"[清理系统] 已清理 {count} 个地面物品");
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error($"[清理系统] 清理物品时出错: {ex}");
                    }
                }));
            }
        }
        #endregion

        public void OnPlayerJoined(JoinedEventArgs ev)
        {
            ev.Player.Nickname = ev.Player.Nickname;
            Timing.CallDelayed(1.5f, () =>
            {
                if (ev.Player != null)
                    ev.Player.Nickname = ev.Player.Nickname;
            });
        }

        public void OnPlayerChangedRole(ChangedRoleEventArgs ev)
        {
            if (ev.Player == null) return;

            int playerId = ev.Player.Id;
            RoleType newRole = ev.NewRole;

            if (newRole != RoleType.ClassD &&
                newRole != RoleType.ChaosInsurgency &&
                newRole != RoleType.NtfCommander)
            {
                return;
            }

            if (PendingGiveItems.Contains(playerId))
            {
                return;
            }

            PendingGiveItems.Add(playerId);

            Timing.CallDelayed(0.2f, () =>
            {
                PendingGiveItems.Remove(playerId);

                if (ev.Player == null || ev.Player.Role != newRole)
                    return;

                if (newRole == RoleType.ClassD)
                {
                    ev.Player.AddItem(ItemType.KeycardJanitor);
                    ev.Player.AddItem(ItemType.Medkit);
                }
                else if (newRole == RoleType.ChaosInsurgency)
                {
                    ev.Player.AddItem(ItemType.GunUSP);
                    ev.Player.AddItem(ItemType.WeaponManagerTablet);
                }
                else if (newRole == RoleType.NtfCommander)
                {
                    ev.Player.AddItem(ItemType.Medkit);
                }
            });
        }

        private IEnumerator<float> SCPHealth()
        {
            while (true)
            {
                foreach (Player player in Player.List)
                {
                    if (!player.IsSCP)
                        continue;

                    if (!LastPos.ContainsKey(player))
                    {
                        LastPos[player] = player.Position;
                        KeepPosTime[player] = 0f;
                    }
                    else
                    {
                        if (Vector3.Distance(player.Position, LastPos[player]) < 0.1f)
                        {
                            KeepPosTime[player] += 1f;

                            if (KeepPosTime[player] > 5 && player.Health < player.MaxHealth)
                            {
                                player.Health += 3;
                                player.Health = Mathf.Min(player.Health, player.MaxHealth);
                            }
                        }
                        else
                        {
                            LastPos[player] = player.Position;
                            KeepPosTime[player] = 0f;
                        }
                    }
                }
                yield return Timing.WaitForSeconds(1f);
            }
        }
    }
}