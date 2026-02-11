using DreamPlugin.Game.CustomRole.Extensions;
using RExiled.API.Extensions;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using System.Collections.Generic;
using UnityEngine;

namespace DreamPlugin.Game.CustomRole
{
    public class Scp550Role : CustomRole
    {
        public Scp550Role()
        {
            Name = "SCP-550";
            SpawnDescription = "你是<color=red>SCP-550</color> <i>击杀回血</i>";
            DiedDescription = "<color=red>SCP-550</color>已被收容!";
            SpawnCondition = SpawnConditionType.RoundStart;
            SpawnRoleType = RoleType.Tutorial;
            SpawnCapacityLimit = 10;
            SpawnRoleCapacityLimit = 1;
            RoleHealth = 300;
            RoleMaxHealth = 950;
            RoleInventory = new List<ItemType>
            {
                ItemType.GunProject90,
                ItemType.GrenadeFlash,
                ItemType.KeycardNTFLieutenant
            };
            IsJoinSpawnQueue = true;
        }

        public override void OnSpawn()
        {
            CurrentPlayer.Position = Map.GetRandomSpawnPoint(RoleType.ChaosInsurgency);

            RExiled.Events.Handlers.Player.Hurting += OnHurting;
            RExiled.Events.Handlers.Player.PickingUpItem += OnPickItem;
            RExiled.Events.Handlers.Player.EnteringPocketDimension += OnPocketEnter;
            RExiled.Events.Handlers.Player.Died += OnDied;
        }

        public override void OnDestroy()
        {
            RExiled.Events.Handlers.Player.Hurting -= OnHurting;
            RExiled.Events.Handlers.Player.PickingUpItem -= OnPickItem;
            RExiled.Events.Handlers.Player.EnteringPocketDimension -= OnPocketEnter;
            RExiled.Events.Handlers.Player.Died -= OnDied;
        }

        private void OnDied(DiedEventArgs ev)
        {
            if (ev.Killer == CurrentPlayer && CurrentPlayer.IsAlive)
            {
                int heal = 30;
                CurrentPlayer.Health = Mathf.Min(CurrentPlayer.Health + heal, 950);
                BroadcastSystem.BroadcastSystem.ShowToPlayer(CurrentPlayer, $"[个人消息] 击杀玩家 <color=green>+{heal}HP</color>");
            }
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            if (ev.Target == CurrentPlayer)
            {
                if (ev.Attacker?.IsSCP == true)
                    ev.IsAllowed = false;
                if (ev.DamageType == DamageTypes.Falldown)
                    ev.IsAllowed = false;
            }

            if (ev.Attacker == CurrentPlayer && ev.Target?.IsSCP == true)
                ev.IsAllowed = false;
        }

        private void OnPickItem(PickingUpItemEventArgs ev)
        {
            if (ev.Player != CurrentPlayer) return;
            if (ev.Pickup.ItemId.IsMedical() || ev.Pickup.ItemId == ItemType.MicroHID)
            {
                ev.IsAllowed = false;
                BroadcastSystem.BroadcastSystem.ShowToPlayer(CurrentPlayer, "[个人消息] 你不可以拾取此物品");
            }
        }

        private void OnPocketEnter(EnteringPocketDimensionEventArgs ev)
        {
            if (ev.Player == CurrentPlayer)
                ev.IsAllowed = false;
        }
    }
}