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

        private static readonly HashSet<int> _allocatedIds = new HashSet<int>();
        private static readonly object _idLock = new object();

        public void RegisterEvents()
        {
            RExiled.Events.Handlers.Player.ChangedRole += OnPlayerChangedRole;
            RExiled.Events.Handlers.Player.Joined += OnPlayerJoined;
            RExiled.Events.Handlers.Player.Left += OnPlayerLeft;
            RExiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            RExiled.Events.Handlers.Server.RoundRestarted += OnRoundRestarting;

            SCPHealthCoroutine = Timing.RunCoroutine(SCPHealth());
        }

        public void UnregisterEvents()
        {
            RExiled.Events.Handlers.Player.ChangedRole -= OnPlayerChangedRole;
            RExiled.Events.Handlers.Player.Joined -= OnPlayerJoined;
            RExiled.Events.Handlers.Player.Left -= OnPlayerLeft;
            RExiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            RExiled.Events.Handlers.Server.RoundRestarted -= OnRoundRestarting;

            PendingGiveItems.Clear();
        }

        #region 清理
        private bool _isRoundActive = false;
        private float _corpseTimer = 0f;
        private float _itemTimer = 0f;
        private const float CORPSE_CLEAN_INTERVAL = 240f;
        private const float ITEM_CLEAN_INTERVAL = 900f;
        private CoroutineHandle _cleanupUpdateCoroutine;

        private void StartCleanup()
        {
            StopCleanup(); 
            _isRoundActive = true;
            _corpseTimer = 0f;
            _itemTimer = 0f;
            _cleanupUpdateCoroutine = Timing.RunCoroutine(CleanupUpdateRoutine(), Segment.LateUpdate);
            Log.Info("[清理系统] 回合开始，启动清理计时器");
        }

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

        private IEnumerator<float> CleanupUpdateRoutine()
        {
            while (_isRoundActive)
            {
                yield return Timing.WaitForOneFrame;

                if (!_isRoundActive) yield break;

                float deltaTime = Time.deltaTime;

                _corpseTimer += deltaTime;
                if (_corpseTimer >= CORPSE_CLEAN_INTERVAL)
                {
                    CleanCorpsesNow();
                    _corpseTimer = 0f;
                }

                _itemTimer += deltaTime;
                if (_itemTimer >= ITEM_CLEAN_INTERVAL)
                {
                    CleanItemsNow();
                    _itemTimer = 0f;
                }
            }
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
                    Map.Broadcast(4, $"<size=30>[清理系统] 已清理 {count} 具尸体</size>");
                    Log.Info($"[清理系统] 已清理 {count} 具尸体");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[清理系统] 清理尸体失败: {ex}");
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

        #region 回合事件

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
            int newId;

            lock (_idLock)
            {
                newId = 2;
                while (_allocatedIds.Contains(newId))
                {
                    newId++;
                }

                _allocatedIds.Add(newId);
            }

            ev.Player.ReferenceHub.queryProcessor.NetworkPlayerId = newId;

            Map.Broadcast(4, $"<size=30>欢迎<color=green>{ev.Player.Nickname}</color>加入<color=blue>*鸟之诗.梦时镜怀旧服*</color>\n欢迎加入Q群: 801888832\n当前服务器人数: {Player.List?.Count()}</size>", true);

            ev.Player.RankName = string.Empty;
            Timing.CallDelayed(0.2f, () =>
            {
                ev.Player.RankName = string.Empty;
            });
        }
        public void OnPlayerLeft(LeftEventArgs ev)
        {
            lock (_idLock)
            {
                _allocatedIds.Remove(ev.Player.Id);
            }
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
                    ev.Player.AddItem(ItemType.Adrenaline);
                    ev.Player.AddItem(ItemType.GrenadeFlash);
                }
                else if (newRole == RoleType.NtfLieutenant)
                {
                    ev.Player.AddItem(ItemType.GunUSP);
                    ev.Player.AddItem(ItemType.GrenadeFrag);
                }
                else if (newRole == RoleType.NtfScientist)
                {
                    ev.Player.AddItem(ItemType.GunUSP);
                    ev.Player.AddItem(ItemType.GrenadeFrag);
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
                                float healAmount = player.Role == RoleType.Scp106 ? 1f : 3f;
                                player.Health = Mathf.Min(player.Health + healAmount, player.MaxHealth);
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