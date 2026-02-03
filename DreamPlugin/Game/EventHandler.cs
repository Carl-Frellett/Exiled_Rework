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
            RExiled.Events.Handlers.Player.Dying += OnPlayerDying;
            RExiled.Events.Handlers.Player.Hurting += OnPlayerHurting;

            SCPHealthCoroutine = Timing.RunCoroutine(SCPHealth());
        }

        public void UnregisterEvents()
        {
            RExiled.Events.Handlers.Player.ChangedRole -= OnPlayerChangedRole;
            RExiled.Events.Handlers.Player.Joined -= OnPlayerJoined;
            RExiled.Events.Handlers.Player.Left -= OnPlayerLeft;
            RExiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            RExiled.Events.Handlers.Server.RoundRestarted -= OnRoundRestarting;
            RExiled.Events.Handlers.Player.Dying -= OnPlayerDying;
            RExiled.Events.Handlers.Player.Hurting -= OnPlayerHurting;

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

        private void OnRoundStarted()
        {
            StartCleanup();

            Timing.CallDelayed(1.5f, () =>
            {
                string scpName = string.Empty;
                int scpCount = 0;
                foreach (var item in Player.List)
                {
                    if (item.IsSCP)
                    {
                        scpName += $"{item.Role.ToString()} ";
                        scpCount++;
                    }
                }
                Map.Broadcast(4,$"<size=30>此次收容失效 <color=yellow>共有{scpCount}个SCP突破收容: </color><color=red>{scpName}</color></size>");
            });
        }
        public void OnPlayerHurting(HurtingEventArgs ev)
        {
            if (ev.HitInfo.GetDamageType() == DamageTypes.Scp207)
            {
                ev.HitInfo.Amount = 0.2f;
            }
        }
        private void OnRoundRestarting()
        {
            StopCleanup();
        }
        public void OnPlayerDying(DyingEventArgs ev)
        {
            if (ev.Target.IsSCP && ev.Attacker != null && !ev.Attacker.IsSCP)
            {
                Map.Broadcast(4,$"<size=30><color=yellow>玩家{ev.Attacker.Nickname}</color>收容了<color=red>{ev.Target.Role.ToString()}</color></size>");
            }
            CleanAmmoPickups();
        }
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

            Map.Broadcast(4, $"<size=30>欢迎<color=green>{ev.Player.Nickname}</color>加入<color=blue>*梦时镜·怀旧服*</color>\n欢迎加入Q群: 801888832\n当前服务器人数: {Player.List?.Count()}</size>", true);

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
            if (ev.Player == null)
                return;

            int playerId = ev.Player.Id;
            RoleType newRole = ev.NewRole;

            if (PendingGiveItems.Contains(playerId))
                return;

            PendingGiveItems.Add(playerId);

            Timing.CallDelayed(0.2f, () =>
            {
                PendingGiveItems.Remove(playerId);

                if (ev.Player == null || ev.Player.Role != newRole)
                    return;

                var player = ev.Player;

                switch (newRole)
                {
                    case RoleType.ClassD:
                        player.AddItem(ItemType.KeycardJanitor);
                        player.AddItem(ItemType.Adrenaline);
                        break;

                    case RoleType.Scientist:
                        player.AddItem(ItemType.Adrenaline);
                        break;

                    case RoleType.ChaosInsurgency:
                        player.AddItem(ItemType.GunUSP);
                        player.AddItem(ItemType.WeaponManagerTablet);
                        break;

                    case RoleType.NtfCommander:
                        player.AddItem(ItemType.Medkit);
                        break;

                    case RoleType.NtfCadet:
                        var seniorCard = player.Inventory.items.FirstOrDefault(i => i.id == ItemType.KeycardSeniorGuard);
                        if (seniorCard != null)
                            player.RemoveItem(seniorCard);

                        player.AddItem(ItemType.KeycardNTFLieutenant);
                        player.AddItem(ItemType.Adrenaline);
                        player.AddItem(ItemType.GrenadeFlash);
                        break;

                    case RoleType.NtfLieutenant:
                        player.AddItem(ItemType.GunUSP);
                        player.AddItem(ItemType.GrenadeFrag);
                        break;

                    case RoleType.NtfScientist:
                        player.AddItem(ItemType.GunUSP);
                        player.AddItem(ItemType.GrenadeFrag);
                        break;

                    case RoleType.FacilityGuard:
                        player.AddItem(ItemType.GrenadeFrag);
                        break;
                }
            });
        }
        private void CleanAmmoPickups()
        {
            try
            {
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
            }
            catch (System.Exception ex)
            {
                Log.Error($"[子弹清理] 清理失败: {ex}");
            }
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