using MEC;
using Mirror;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DreamPlugin.Game.SCP204
{
    using static RExiled.Events.Handlers.Player;

    public class SCP204
    {
        public static Player SCP204CurrentPlayer = null;
        private static Player BoundPlayer = null;
        private static CoroutineHandle coinRefreshCoroutine;
        private static CoroutineHandle infiniteAmmoTimer;
        private static bool isChangingRoleInternally = false;

        public void RegisterEvents()
        {
            Died += OnDied;
            Left += OnPlayerLeft;
            ChangedRole += OnChangeRole;
            Shooting += OnShooting;
            PickingUpItem += OnPickCoin;
            DroppingItem += OnDroppingCoin;
        }

        public void UnregisterEvents()
        {
            Died -= OnDied;
            Left -= OnPlayerLeft;
            ChangedRole -= OnChangeRole;
            Shooting -= OnShooting;
            PickingUpItem -= OnPickCoin;
            DroppingItem -= OnDroppingCoin;
        }

        public void OnRoundStart()
        {
            if (Player.List.Count() >= 15)
            {
                Timing.CallDelayed(1.5f, () =>
                {
                    var guards = Player.List.Where(p => p.Role == RoleType.FacilityGuard).ToList();
                    if (guards.Any())
                    {
                        var target = guards[Random.Range(0, guards.Count)];
                        SpawnSCP204(target);
                    }
                });
            }
        }

        public void SpawnSCP204(Player ply)
        {
            if (SCP204CurrentPlayer != null) return;

            SCP204CurrentPlayer = ply;
            SCP204CurrentPlayer.SetRole(RoleType.Tutorial, true);
            SCP204CurrentPlayer.MaxHealth = 240;
            SCP204CurrentPlayer.Health = 240;

            List<ItemType> items = new List<ItemType>
            {
                ItemType.GunMP7,
                ItemType.Medkit,
                ItemType.KeycardJanitor,
                ItemType.Coin
            };

            Timing.CallDelayed(0.3f, () =>
            {
                SCP204CurrentPlayer.ResetInventory(items);
                SCP204CurrentPlayer.Position = Map.GetRandomSpawnPoint(RoleType.FacilityGuard);
            });

            BroadcastSystem.BroadcastSystem.ShowToPlayer(SCP204CurrentPlayer, "[个人消息] 你是<color=red>SCP-204</color> <i>枪里面有无限的子弹</i>", 5);

            string currentRank = SCP204CurrentPlayer.RankName?.Trim() ?? "";
            if (string.IsNullOrEmpty(currentRank))
                SCP204CurrentPlayer.RankName = "SCP-204";
            else
                SCP204CurrentPlayer.RankName += " | SCP-204";

            StartCoinRefresh();
        }

        private static void StartCoinRefresh()
        {
            Timing.KillCoroutines(coinRefreshCoroutine);
            if (SCP204CurrentPlayer != null && BoundPlayer == null)
            {
                coinRefreshCoroutine = Timing.RunCoroutine(CoinRefreshRoutine());
            }
        }

        private static IEnumerator<float> CoinRefreshRoutine()
        {
            while (SCP204CurrentPlayer != null && BoundPlayer == null)
            {
                bool hasCoin = false;
                try
                {
                    var inventory = SCP204CurrentPlayer.ReferenceHub.inventory;
                    foreach (var item in inventory.items)
                    {
                        if (item.id == ItemType.Coin)
                        {
                            hasCoin = true;
                            break;
                        }
                    }
                }
                catch { /* ignore */ }

                if (!hasCoin)
                {
                    SCP204CurrentPlayer.AddItem(ItemType.Coin);
                    BroadcastSystem.BroadcastSystem.ShowToPlayer(SCP204CurrentPlayer, "[个人消息] 硬币已刷新", 3);
                }

                yield return Timing.WaitForSeconds(180f);
            }
        }

        public void OnPickCoin(PickingUpItemEventArgs ev)
        {
            // 如果 SCP-204 未激活，完全不干预硬币拾取逻辑
            if (SCP204CurrentPlayer == null)
                return;

            if (ev.Player == null || BoundPlayer != null)
                return;

            if (ev.Player == SCP204CurrentPlayer)
            {
                BroadcastSystem.BroadcastSystem.ShowToPlayer(SCP204CurrentPlayer, "[个人消息] 你不可以拾取硬币", 5);
                ev.IsAllowed = false;
                return;
            }

            if (ev.Pickup.ItemId != ItemType.Coin)
                return;

            BoundPlayer = ev.Player;
            var targetRole = BoundPlayer.Role;
            var pickups = UnityEngine.Object.FindObjectsOfType<Pickup>();
            foreach (var pickup in pickups)
            {
                if (pickup == null || pickup.gameObject == null) continue;
                if (pickup.Networkinfo.itemId == ItemType.Coin)
                {
                    NetworkServer.Destroy(pickup.gameObject);
                }
            }

            InfiniteAmmo.EnableForPlayer(BoundPlayer);
            infiniteAmmoTimer = Timing.CallDelayed(180f, () =>
            {
                InfiniteAmmo.DisableForPlayer(BoundPlayer);
                if (BoundPlayer != null)
                    BroadcastSystem.BroadcastSystem.ShowToPlayer(BoundPlayer, "[个人消息] 无限子弹效果已结束", 3);
            });

            isChangingRoleInternally = true;
            SCP204CurrentPlayer.SetRole(targetRole, true);
            Timing.KillCoroutines(coinRefreshCoroutine);
        }

        public void OnDroppingCoin(DroppingItemEventArgs ev)
        {
            if (ev.Player == SCP204CurrentPlayer && ev.ItemId == ItemType.Coin)
            {
                Log.Info($"[SCP-204] {ev.Player.Nickname} 丢弃了硬币");
            }
        }

        public void OnChangeRole(ChangedRoleEventArgs ev)
        {
            if (ev.Player != SCP204CurrentPlayer) return;

            if (isChangingRoleInternally)
            {
                isChangingRoleInternally = false;
                return;
            }

            CleanupSCP204();
        }

        public void OnDied(DiedEventArgs ev)
        {
            if (ev.Target == SCP204CurrentPlayer || ev.Target == BoundPlayer)
            {
                CleanupSCP204();
            }
        }

        public void OnPlayerLeft(LeftEventArgs ev)
        {
            if (ev.Player == SCP204CurrentPlayer || ev.Player == BoundPlayer)
            {
                CleanupSCP204();
            }
        }

        private static void CleanupSCP204()
        {
            if (SCP204CurrentPlayer == null) return;

            string currentRank = SCP204CurrentPlayer.RankName ?? "";
            const string SCP204Tag = "SCP-204";
            const string separatorTag = " | SCP-204";
            string newRank = currentRank;

            if (currentRank.Contains(separatorTag))
                newRank = currentRank.Replace(separatorTag, "");
            else if (currentRank == SCP204Tag)
                newRank = "";

            newRank = newRank.Trim();
            if (newRank.EndsWith(" |"))
                newRank = newRank.Substring(0, newRank.Length - 2).Trim();

            SCP204CurrentPlayer.RankName = newRank;
            SCP204CurrentPlayer = null;
            BoundPlayer = null;

            Timing.KillCoroutines(coinRefreshCoroutine);
            Timing.KillCoroutines(infiniteAmmoTimer);
            InfiniteAmmo.DisableForPlayer(BoundPlayer);
        }
    }
}