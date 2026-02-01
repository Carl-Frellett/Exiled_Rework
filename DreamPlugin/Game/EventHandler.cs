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
            RExiled.Events.Handlers.Server.RoundRestarted += OnRoundRestarting;

            SCPHealthCoroutine = Timing.RunCoroutine(SCPHealth());
        }

        public void UnregisterEvents()
        {
            RExiled.Events.Handlers.Player.ChangedRole -= OnPlayerChangedRole;
            RExiled.Events.Handlers.Player.Joined -= OnPlayerJoined;
            RExiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            RExiled.Events.Handlers.Server.RoundRestarted -= OnRoundRestarting;

            PendingGiveItems.Clear();
        }

        #region 清理系统（尸体 & 物品）
        private bool _isRoundActive = false;
        private float _corpseTimer = 0f;
        private float _itemTimer = 0f;
        private const float CORPSE_CLEAN_INTERVAL = 240f; // 4分钟 = 240秒
        private const float ITEM_CLEAN_INTERVAL = 900f;   // 15分钟 = 900秒
        private CoroutineHandle _cleanupUpdateCoroutine;

        /// <summary>
        /// 回合开始时启动清理系统
        /// </summary>
        private void StartCleanup()
        {
            StopCleanup(); // 确保先停止旧的协程
            _isRoundActive = true;
            _corpseTimer = 0f;
            _itemTimer = 0f;
            _cleanupUpdateCoroutine = Timing.RunCoroutine(CleanupUpdateRoutine(), Segment.LateUpdate);
            Log.Info("[清理系统] 回合开始，启动清理计时器");
        }

        /// <summary>
        /// 回合结束或重启时停止并重置清理系统
        /// </summary>
        private void StopCleanup()
        {
            if (_cleanupUpdateCoroutine.IsRunning)
            {
                Timing.KillCoroutines(_cleanupUpdateCoroutine);
            }
            _isRoundActive = false;
            _corpseTimer = 0f;
            _itemTimer = 0f;
            Log.Info("[清理系统] 回合结束，清理计时器已重置");
        }

        /// <summary>
        /// 每帧精确更新清理计时器
        /// </summary>
        private IEnumerator<float> CleanupUpdateRoutine()
        {
            while (_isRoundActive)
            {
                yield return Timing.WaitForOneFrame;

                if (!_isRoundActive) yield break;

                float deltaTime = Time.deltaTime;

                // 尸体清理（每4分钟）
                _corpseTimer += deltaTime;
                if (_corpseTimer >= CORPSE_CLEAN_INTERVAL)
                {
                    CleanCorpsesNow();
                    _corpseTimer = 0f; // 重置计时器，保证下次准时
                }

                // 物品清理（每15分钟）
                _itemTimer += deltaTime;
                if (_itemTimer >= ITEM_CLEAN_INTERVAL)
                {
                    CleanItemsNow();
                    _itemTimer = 0f; // 重置计时器，保证下次准时
                }
            }
        }

        /// <summary>
        /// 立即清理所有尸体（Ragdoll）
        /// </summary>
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
                    Map.Broadcast(4, $"<size=30>[清理系统] 已清理 {count} 具尸体</size>");
                    Log.Info($"[清理系统] 已清理 {count} 具尸体");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[清理系统] 清理尸体失败: {ex}");
            }
        }

        /// <summary>
        /// 立即清理所有地面物品（Pickup）
        /// </summary>
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
                if (count > 0)
                {
                    Map.Broadcast(4, $"<size=30>[清理系统] 已清理 {count} 个地面物品</size>");
                    Log.Info($"[清理系统] 已清理 {count} 个地面物品");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[清理系统] 清理物品失败: {ex}");
            }
        }
        #endregion

        #region 回合事件绑定
        // 在你的事件注册方法中（如 RegisterEvents），确保绑定：
        // Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
        // Exiled.Events.Handlers.Server.RestartingRound += OnRoundRestarting;

        private void OnRoundStarted()
        {
            StartCleanup();
        }

        private void OnRoundRestarting()
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