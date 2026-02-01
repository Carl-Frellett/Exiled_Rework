using MEC;
using Mirror;
using RExiled.API.Features;
using RExiled.Events.EventArgs;
using RExiled.Events.EventArgs.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DreamPlugin.Game
{
    public class EventHandler
    {
        private static readonly HashSet<int> PendingGiveItems = new HashSet<int>();

        private CoroutineHandle SCPHealthCoroutine;

        private Dictionary<Player, Vector3> LastPos = new Dictionary<Player, Vector3>();
        private Dictionary<Player, float> KeepPosTime = new Dictionary<Player, float>();

        public void RegisterEvents()
        {
            RExiled.Events.Handlers.Player.ChangedRole += OnPlayerChangedRole;
            RExiled.Events.Handlers.Player.Joined += OnPlayerJoined;
            RExiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            RExiled.Events.Handlers.Server.RoundRestarted += RoundRestarted;

            SCPHealthCoroutine = Timing.RunCoroutine(SCPHealth());
        }

        public void UnregisterEvents()
        {
            RExiled.Events.Handlers.Player.ChangedRole -= OnPlayerChangedRole;
            RExiled.Events.Handlers.Player.Joined -= OnPlayerJoined;
            RExiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            RExiled.Events.Handlers.Server.RoundRestarted -= RoundRestarted;

            PendingGiveItems.Clear();
        }

        #region 清理系统（尸体 & 物品）

        private CoroutineHandle _corpseCoroutine;
        private CoroutineHandle _itemCoroutine;

        private void StartCleanup()
        {
            StopCleanup();

            _corpseCoroutine = Timing.RunCoroutine(CleanCorpsesRoutine(), Segment.Update);
            _itemCoroutine = Timing.RunCoroutine(CleanItemsRoutine(), Segment.Update);
        }

        private void StopCleanup()
        {
            if (_corpseCoroutine.IsRunning) Timing.KillCoroutines(_corpseCoroutine);
            if (_itemCoroutine.IsRunning) Timing.KillCoroutines(_itemCoroutine);
        }

        private IEnumerator<float> CleanCorpsesRoutine()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(240f);

                if (!Round.IsStarted) break;

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
                        Map.Broadcast(4, $"<size=30>[清理系统] 已清理 {count} 具尸体</size>");
                        Log.Info($"[清理系统] 已清理 {count} 具尸体");
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Error($"[清理系统] 清理尸体时出错: {ex}");
                }
            }
        }

        private IEnumerator<float> CleanItemsRoutine()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(900f);

                if (!Round.IsStarted) break;

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
                    if (count > 0)
                    {
                        Map.Broadcast(4, $"<size=30>[清理系统] 已清理 {count} 个地面物品</size>");
                        Log.Info($"[清理系统] 已清理 {count} 个地面物品");
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Error($"[清理系统] 清理物品时出错: {ex}");
                }
            }
        }
        #endregion

        #region 回合事件

        private void OnRoundStarted()
        {
            StartCleanup();
        }

        private void RoundRestarted()
        {
            StopCleanup();
        }

        #endregion
        public void OnPlayerJoined(JoinedEventArgs ev)
        {
            Map.Broadcast(4, $"<size=30>欢迎<color=green>{ev.Player.Nickname}</color>加入<color=blue>*鸟之诗.梦时镜怀旧服*</color>\n欢迎加入Q群: 801888832\n当前服务器人数: {Player.List?.Count()}</size>",true);
            ev.Player.RankName = "_";
            ev.Player.RankName = string.Empty;
            Timing.CallDelayed(0.2f, () =>
            {
                ev.Player.RankName = string.Empty;
            });
        }

        public void OnPlayerChangedRole(ChangedRoleEventArgs ev)
        {
            if (ev.Player == null) return;
            int playerId = ev.Player.Id;
            RoleType newRole = ev.NewRole;

            if (newRole != RoleType.ClassD && newRole != RoleType.ChaosInsurgency && newRole != RoleType.NtfCommander)
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
                if (ev.Player == null || ev.Player.Role != newRole) return;

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
                else if (newRole == RoleType.NtfCadet)
                {
                    List<ItemType> items = new List<ItemType>()
                    {
                        ItemType.KeycardNTFLieutenant,
                        ItemType.GunProject90,
                        ItemType.WeaponManagerTablet,
                        ItemType.Disarmer,
                        ItemType.Radio,
                        ItemType.Medkit,
                        ItemType.Adrenaline
                    };
                    ev.Player.ResetInventory(items);
                }
                else if (newRole == RoleType.NtfLieutenant)
                {
                    List<ItemType> items = new List<ItemType>()
                    {
                        ItemType.KeycardNTFLieutenant,
                        ItemType.GunE11SR,
                        ItemType.GunUSP,
                        ItemType.GrenadeFrag,
                        ItemType.WeaponManagerTablet,
                        ItemType.Disarmer,
                        ItemType.Radio,
                        ItemType.Medkit
                    };
                }
                else if (newRole == RoleType.FacilityGuard)
                {
                    ev.Player.AddItem(ItemType.GrenadeFrag);
                }
            });
        }

        private IEnumerator<float> SCPHealth()
        {
            while (true)
            {
                foreach (Player player in Player.List)
                {
                    if (!player.IsSCP) continue;

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