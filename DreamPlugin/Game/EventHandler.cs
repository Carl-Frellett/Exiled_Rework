using MEC;
using Mirror;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using System.Collections.Generic;
using UnityEngine;

namespace DreamPlugin.Game
{
    public class EventHandler
    {
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
            RExiled.Events.Handlers.Player.Hurting += OnPlayerHurting;
            RExiled.Events.Handlers.Player.Died += OnPlayerDied;

            SCPHealthCoroutine = Timing.RunCoroutine(SCPHealth());
        }

        public void UnregisterEvents()
        {
            RExiled.Events.Handlers.Player.ChangedRole -= OnPlayerChangedRole;
            RExiled.Events.Handlers.Player.Joined -= OnPlayerJoined;
            RExiled.Events.Handlers.Player.Left -= OnPlayerLeft;
            RExiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            RExiled.Events.Handlers.Server.RoundRestarted -= OnRoundRestarting;
            RExiled.Events.Handlers.Player.Hurting -= OnPlayerHurting;
            RExiled.Events.Handlers.Player.Died -= OnPlayerDied;
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
                    BroadcastSystem.BroadcastSystem.ShowGlobal($"[清理系统] 已清理 {count} 具尸体");
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
                    BroadcastSystem.BroadcastSystem.ShowGlobal($"[清理系统] 已清理 {count} 个地面物品");
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
                BroadcastSystem.BroadcastSystem.ShowGlobal($"此次收容失效 <color=yellow>共有{scpCount}个SCP突破收容: </color><color=red>{scpName}</color>");
            });
        }
        private void OnRoundRestarting()
        {
            StopCleanup();
        }
        public void OnPlayerHurting(HurtingEventArgs ev)
        {
            if (ev.DamageType == DamageTypes.Scp207)
            {
                ev.Amount = 0.03f;
            }

            if (ev.DamageType == DamageTypes.Usp && ev.Target.Team != Team.SCP && ev.Target != Plugin.plugin.SCP073.Scp073CurrentPlayer)
            {
                ev.Amount += new System.Random().Next(50, 150);
            }
            if (ev.DamageType == DamageTypes.MicroHid)
            {
                ev.Amount += new System.Random().Next(300, 800);
            }
        }
        public void OnPlayerDied(DiedEventArgs ev)
        {
            if (ev.Target.IsSCP)
            {
                BroadcastSystem.BroadcastSystem.ShowGlobal($"<color=yellow>玩家{ev.Killer.Nickname}</color>收容了<color=red>{ev?.Target?.Role.ToString()}</color>");
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

            BroadcastSystem.BroadcastSystem.ShowGlobal($"欢迎<color=green>{ev.Player.Nickname}</color>加入<color=blue>*梦时镜·怀旧服*</color>");

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
            List<ItemType> Cld = new List<ItemType>()
            {
                ItemType.KeycardJanitor,
                ItemType.Adrenaline
            };

            List<ItemType> Slt = new List<ItemType>()
            {
                ItemType.KeycardScientist,
                ItemType.Adrenaline,
                ItemType.Painkillers
            };

            List<ItemType> CI = new List<ItemType>()
            {
                ItemType.GunLogicer,
                ItemType.GunUSP,
                ItemType.KeycardChaosInsurgency,
                ItemType.WeaponManagerTablet,
                ItemType.Medkit,
                ItemType.Painkillers,
                ItemType.GrenadeFrag,
                ItemType.GrenadeFlash,
            };

            List<ItemType> Ntc = new List<ItemType>()
            {
                ItemType.GunE11SR,
                ItemType.GunUSP,
                ItemType.KeycardNTFCommander,
                ItemType.WeaponManagerTablet,
                ItemType.Radio,
                ItemType.Disarmer,
                ItemType.Medkit,
                ItemType.GrenadeFrag,
            };

            List<ItemType> Ntct = new List<ItemType>()
            {
                ItemType.GunProject90,
                ItemType.GunCOM15,
                ItemType.KeycardNTFLieutenant,
                ItemType.WeaponManagerTablet,
                ItemType.Radio,
                ItemType.Disarmer,
                ItemType.Medkit,
            };

            List<ItemType> ntl = new List<ItemType>()
            {
                ItemType.GunE11SR,
                ItemType.GunUSP,
                ItemType.KeycardNTFLieutenant,
                ItemType.WeaponManagerTablet,
                ItemType.Radio,
                ItemType.Disarmer,
                ItemType.Medkit,
                ItemType.GrenadeFrag,
            };

            List<ItemType> fg = new List<ItemType>()
            {
                ItemType.GunMP7,
                ItemType.KeycardGuard,
                ItemType.WeaponManagerTablet,
                ItemType.Radio,
                ItemType.Disarmer,
                ItemType.Medkit,
                ItemType.GrenadeFrag,
                ItemType.GrenadeFlash,
            };

            Timing.CallDelayed(0.5f, () =>
            {
                switch (ev.NewRole)
                {
                    case RoleType.ClassD:
                        ev.Player.ResetInventory(Cld);
                        break;
                    case RoleType.Scientist:
                        ev.Player.ResetInventory(Slt);
                        break;
                    case RoleType.ChaosInsurgency:
                        ev.Player.ResetInventory(CI);
                        break;
                    case RoleType.NtfCommander:
                        ev.Player.ResetInventory(Ntc);
                        break;
                    case RoleType.NtfCadet:
                        ev.Player.ResetInventory(Ntct);
                        break;
                    case RoleType.NtfLieutenant:
                        ev.Player.ResetInventory(ntl);
                        break;
                    case RoleType.NtfScientist:
                        ev.Player.ResetInventory(ntl);
                        break;
                    case RoleType.FacilityGuard:
                        ev.Player.ResetInventory(fg);
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