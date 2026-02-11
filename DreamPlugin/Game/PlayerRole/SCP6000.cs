using DreamPlugin.Game.CustomRole.Extensions;
using MEC;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DreamPlugin.Game.CustomRole
{
    public class Scp6000Role : CustomRole
    {
        private CoroutineHandle _coinCoroutine;

        public Scp6000Role()
        {
            Name = "SCP-6000";
            SpawnDescription = "你是<color=red>SCP-6000</color> <i>丢弃硬币可传送至随机玩家</i>";
            DiedDescription = "<color=red>SCP-6000</color>已被收容!";
            SpawnCondition = SpawnConditionType.RoundStart;
            SpawnRoleType = RoleType.ClassD;
            SpawnCapacityLimit = 6;
            SpawnRoleCapacityLimit = 1;
            RoleHealth = 100;
            RoleMaxHealth = 100;
            RoleInventory = new List<ItemType>
            {
                ItemType.KeycardJanitor, ItemType.Adrenaline,
                ItemType.Coin, ItemType.Coin, ItemType.Coin,
                ItemType.Coin, ItemType.Coin, ItemType.Coin
            };
            IsJoinSpawnQueue = true;
        }

        public override void OnSpawn()
        {
            RExiled.Events.Handlers.Player.ItemDropped += OnDropItem;
            RExiled.Events.Handlers.Player.PickingUpItem += OnPickItem;
            _coinCoroutine = Timing.RunCoroutine(CoinGiving());
        }

        public override void OnDestroy()
        {
            RExiled.Events.Handlers.Player.ItemDropped -= OnDropItem;
            RExiled.Events.Handlers.Player.PickingUpItem -= OnPickItem;
            Timing.KillCoroutines(_coinCoroutine);
        }

        private void OnDropItem(ItemDroppedEventArgs ev)
        {
            if (ev.Player == CurrentPlayer && ev.ItemId == ItemType.Coin)
                TeleportToRandom();
        }

        private void OnPickItem(PickingUpItemEventArgs ev)
        {
            if (ev.Player == CurrentPlayer && ev.Pickup?.ItemId == ItemType.Coin)
            {
                ev.IsAllowed = false;
                BroadcastSystem.BroadcastSystem.ShowToPlayer(CurrentPlayer, "[个人消息] 你不可以拾取硬币");
            }
        }

        private IEnumerator<float> CoinGiving()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(120f);
                if (CurrentPlayer == null || !CurrentPlayer.IsAlive) yield break;
                CurrentPlayer.AddItem(ItemType.Coin);
                BroadcastSystem.BroadcastSystem.ShowToPlayer(CurrentPlayer, "[个人消息] <color=yellow>获得一枚硬币！</color>", 4);
            }
        }

        private void TeleportToRandom()
        {
            var targets = Player.List.Where(p => p != CurrentPlayer && p.IsAlive).ToList();
            if (targets.Count == 0) return;
            CurrentPlayer.Position = targets[Random.Range(0, targets.Count)].Position;
        }
    }
}